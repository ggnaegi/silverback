﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Diagnostics;
using Silverback.Messaging.Broker.Behaviors;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;
using Silverback.Messaging.Sequences;
using Silverback.Messaging.Sequences.Unbounded;
using Silverback.Util;

namespace Silverback.Messaging.Inbound
{
    /// <summary>
    ///     Publishes the consumed messages to the internal bus.
    /// </summary>
    public sealed class PublisherConsumerBehavior : IConsumerBehavior, IDisposable
    {
        private readonly ISilverbackIntegrationLogger<PublisherConsumerBehavior> _logger;

        private UnboundedSequence? _unboundedSequence;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PublisherConsumerBehavior" /> class.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackIntegrationLogger{TCategoryName}" />.
        /// </param>
        public PublisherConsumerBehavior(ISilverbackIntegrationLogger<PublisherConsumerBehavior> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc cref="ISorted.SortIndex" />
        public int SortIndex => BrokerBehaviorsSortIndexes.Consumer.Publisher;

        /// <inheritdoc cref="IConsumerBehavior.Handle" />
        public async Task Handle(
            ConsumerPipelineContext context,
            ConsumerBehaviorHandler next)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(next, nameof(next));

            _logger.LogProcessing(context.Envelope);

            if (context.Sequence != null)
            {
                // If the sequence is being consumed already it means it is being used by the Serializer, the
                // BinaryFileMessage or another Behavior. In this case we still publish the envelope, ignoring the
                // sequence.
                if (context.Sequence.IsBeingConsumed || context.Sequence.IsComplete)
                    await PublishEnvelopeAsync(context).ConfigureAwait(false);
                else
                    await PublishSequenceAsync(context.Sequence, context).ConfigureAwait(false);
            }
            else
            {
                if (context.Envelope is IInboundEnvelope envelope)
                {
                    // TODO: Create only if necessary?

                    await EnsureUnboundedStreamIsPublishedAsync(context).ConfigureAwait(false);
                    await _unboundedSequence!.AddAsync(envelope).ConfigureAwait(false);

                    if (_unboundedSequence.IsAborted && _unboundedSequence.AbortException != null)
                        throw _unboundedSequence.AbortException; // TODO: Wrap into another exception?
                }

                await PublishEnvelopeAsync(context).ConfigureAwait(false);
            }

            await next(context).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _unboundedSequence?.Dispose();
        }

        private static async Task PublishEnvelopeAsync(ConsumerPipelineContext context)
        {
            var publisher = context.ServiceProvider.GetRequiredService<IPublisher>();

            // TODO: Handle ThrowIfUnhandled across single message and stream (and test it)
            await publisher.PublishAsync(context.Envelope, context.Envelope.Endpoint.ThrowIfUnhandled)
                .ConfigureAwait(false);
        }

        private static async Task PublishSequenceAsync(ISequence sequence, ConsumerPipelineContext context)
        {
            // TODO: Force throwIfUnhandled

            context.ProcessingTask = await PublishStreamProviderAsync(sequence, context).ConfigureAwait(false);

            // CheckStreamProcessing(
            //     await publisher.PublishAsync(sequence.StreamProvider)
            //         .ConfigureAwait(false),
            //     sequence);
        }

        private static async Task<Task> PublishStreamProviderAsync(ISequence sequence, ConsumerPipelineContext context)
        {
            var publisher = context.ServiceProvider.GetRequiredService<IStreamPublisher>();

            var processingTasks = await publisher.PublishAsync(sequence.StreamProvider).ConfigureAwait(false);

            return Task.Run(
                async () =>
                {
                    try
                    {
                        using var cancellationTokenSource = new CancellationTokenSource();
                        var tasks = processingTasks.Select(task => task.CancelOnException(cancellationTokenSource))
                            .ToList();

                        // TODO: Test whether an exception really cancels all tasks
                        await Task.WhenAny(
                                Task.WhenAll(tasks),
                                WhenCanceled(cancellationTokenSource.Token))
                            .ConfigureAwait(false);

                        var exception = tasks.Where(task => task.IsFaulted).Select(task => task.Exception)
                            .FirstOrDefault();
                        if (exception != null)
                        {
                            await sequence.AbortAsync(SequenceAbortReason.Error).ConfigureAwait(false);
                            sequence.Dispose();
                        }

                        // TODO: Test abort at first exception

                        await Task.WhenAll(processingTasks).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        // TODO: Log

                        await sequence.AbortAsync(SequenceAbortReason.Error, exception).ConfigureAwait(false);
                        sequence.Dispose();
                    }
                });
        }

        private static Task WhenCanceled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => { ((TaskCompletionSource<bool>)s).SetResult(true); }, tcs);
            return tcs.Task;
        }

        private async Task EnsureUnboundedStreamIsPublishedAsync(ConsumerPipelineContext context)
        {
            if (_unboundedSequence != null && _unboundedSequence.IsPending)
                return;

            _unboundedSequence = new UnboundedSequence("unbounded", context);
            await context.SequenceStore.AddAsync(_unboundedSequence).ConfigureAwait(false);

            await PublishStreamProviderAsync(_unboundedSequence, context).ConfigureAwait(false);
        }
    }
}