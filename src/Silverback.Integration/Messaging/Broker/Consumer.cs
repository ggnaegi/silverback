﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silverback.Diagnostics;
using Silverback.Messaging.Broker.Behaviors;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Sequences;
using Silverback.Util;

namespace Silverback.Messaging.Broker
{
    /// <inheritdoc cref="IConsumer" />
    public abstract class Consumer : IConsumer, IDisposable
    {
        private readonly IReadOnlyList<IConsumerBehavior> _behaviors;

        private readonly IServiceProvider _serviceProvider;

        private readonly ISilverbackIntegrationLogger<Consumer> _logger;

        private readonly ConsumerStatusInfo _statusInfo = new ConsumerStatusInfo();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Consumer" /> class.
        /// </summary>
        /// <param name="broker">
        ///     The <see cref="IBroker" /> that is instantiating the consumer.
        /// </param>
        /// <param name="endpoint">
        ///     The endpoint to be consumed.
        /// </param>
        /// <param name="behaviorsProvider">
        ///     The <see cref="IBrokerBehaviorsProvider{TBehavior}" />.
        /// </param>
        /// <param name="sequenceStore">
        ///     The <see cref="ISequenceStore"/> to be used to store the pending sequences.
        /// </param>
        /// <param name="serviceProvider">
        ///     The <see cref="IServiceProvider" /> to be used to resolve the needed services.
        /// </param>
        /// <param name="logger">
        ///     The <see cref="ISilverbackIntegrationLogger" />.
        /// </param>
        protected Consumer(
            IBroker broker,
            IConsumerEndpoint endpoint,
            IBrokerBehaviorsProvider<IConsumerBehavior> behaviorsProvider,
            ISequenceStore sequenceStore,
            IServiceProvider serviceProvider,
            ISilverbackIntegrationLogger<Consumer> logger)
        {
            Broker = Check.NotNull(broker, nameof(broker));
            Endpoint = Check.NotNull(endpoint, nameof(endpoint));
            SequenceStore = Check.NotNull(sequenceStore, nameof(sequenceStore));

            _behaviors = Check.NotNull(behaviorsProvider, nameof(behaviorsProvider)).GetBehaviorsList();
            _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
            _logger = Check.NotNull(logger, nameof(logger));

            Endpoint.Validate();
        }

        /// <inheritdoc cref="IConsumer.Broker" />
        public IBroker Broker { get; }

        /// <inheritdoc cref="IConsumer.Endpoint" />
        public IConsumerEndpoint Endpoint { get; }

        /// <inheritdoc cref="IConsumer.SequenceStore" />
        public ISequenceStore SequenceStore { get; }

        /// <inheritdoc cref="IConsumer.StatusInfo" />
        public IConsumerStatusInfo StatusInfo => _statusInfo;

        /// <inheritdoc cref="IConsumer.IsConnected" />
        public bool IsConnected { get; private set; }

        /// <inheritdoc cref="IConsumer.Commit(IOffset)" />
        public Task Commit(IOffset offset) => Commit(new[] { offset });

        /// <inheritdoc cref="IConsumer.Commit(IReadOnlyCollection{IOffset})" />
        public abstract Task Commit(IReadOnlyCollection<IOffset> offsets);

        /// <inheritdoc cref="IConsumer.Rollback(IOffset)" />
        public Task Rollback(IOffset offset) => Rollback(new[] { offset });

        /// <inheritdoc cref="IConsumer.Rollback(IReadOnlyCollection{IOffset})" />
        public abstract Task Rollback(IReadOnlyCollection<IOffset> offsets);

        /// <inheritdoc cref="IConsumer.Connect" />
        public void Connect()
        {
            if (IsConnected)
                return;

            ConnectCore();

            IsConnected = true;
            _statusInfo.SetConnected();

            _logger.LogDebug(
                IntegrationEventIds.ConsumerConnected,
                "Connected consumer to endpoint {endpoint}.",
                Endpoint.Name);
        }

        /// <inheritdoc cref="IConsumer.Disconnect" />
        public void Disconnect()
        {
            if (!IsConnected)
                return;

            DisconnectCore();

            IsConnected = false;
            _statusInfo.SetDisconnected();

            _logger.LogDebug(
                IntegrationEventIds.ConsumerDisconnected,
                "Disconnected consumer from endpoint {endpoint}.",
                Endpoint.Name);
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Connects and starts consuming.
        /// </summary>
        protected abstract void ConnectCore();

        /// <summary>
        ///     Disconnects and stops consuming.
        /// </summary>
        protected abstract void DisconnectCore();

        /// <summary>
        ///     Handles the consumed message invoking each <see cref="IConsumerBehavior" /> in the pipeline.
        /// </summary>
        /// <param name="message">
        ///     The body of the consumed message.
        /// </param>
        /// <param name="headers">
        ///     The headers of the consumed message.
        /// </param>
        /// <param name="sourceEndpointName">
        ///     The name of the actual endpoint (topic) where the message has been delivered.
        /// </param>
        /// <param name="offset">
        ///     The offset of the consumed message.
        /// </param>
        /// <param name="additionalLogData">
        ///     An optional dictionary containing the broker specific data to be logged when processing the consumed
        ///     message.
        /// </param>
        /// <returns>
        ///     A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        [SuppressMessage("", "SA1011", Justification = Justifications.NullableTypesSpacingFalsePositive)]
        protected virtual async Task HandleMessage(
            byte[]? message,
            IReadOnlyCollection<MessageHeader> headers,
            string sourceEndpointName,
            IOffset offset,
            IDictionary<string, string>? additionalLogData)
        {
            var envelope = new RawInboundEnvelope(
                message,
                headers,
                Endpoint,
                sourceEndpointName,
                offset,
                additionalLogData);

            _statusInfo.RecordConsumedMessage(offset);

            var consumerPipelineContext = new ConsumerPipelineContext(envelope, this, _serviceProvider);

            await ExecutePipeline(consumerPipelineContext).ConfigureAwait(false);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///     resources.
        /// </summary>
        /// <param name="disposing">
        ///     A value indicating whether the method has been called by the <c>Dispose</c> method and not from the
        ///     finalizer.
        /// </param>
        [SuppressMessage("", "CA1031", Justification = Justifications.ExceptionLogged)]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    IntegrationEventIds.ConsumerDisposingError,
                    ex,
                    "Error occurred while disposing consumer from endpoint {endpoint}.",
                    Endpoint.Name);
            }
        }

        private async Task ExecutePipeline(
            ConsumerPipelineContext context,
            int stepIndex = 0)
        {
            if (_behaviors.Count == 0 || stepIndex >= _behaviors.Count)
                return;

            await _behaviors[stepIndex].Handle(
                    context,
                    nextContext => ExecutePipeline(nextContext, stepIndex + 1))
                .ConfigureAwait(false);
        }

        // private async Task ProcessMessageStream()
        // {
        //     await foreach (var envelope in _messageStream.ConfigureAwait(false))
        //     {
        //         await ExecutePipeline(
        //                 _behaviorsProvider.CreateStack(),
        //                 new ConsumerPipelineContext(
        //                     envelope,
        //                     this,
        //                     _serviceProvider))
        //             .ConfigureAwait(false);
        //     }
        // }
    }
}
