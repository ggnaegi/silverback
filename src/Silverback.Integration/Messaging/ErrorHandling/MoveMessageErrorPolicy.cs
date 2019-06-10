﻿// Copyright (c) 2018-2019 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;

namespace Silverback.Messaging.ErrorHandling
{
    /// <summary>
    /// This policy moves the failed messages to the configured endpoint.
    /// </summary>
    public class MoveMessageErrorPolicy : ErrorPolicyBase
    {
        private readonly IProducer _producer;
        private readonly IEndpoint _endpoint;
        private readonly ILogger _logger;
        private readonly MessageLogger _messageLogger;

        private Func<object, Exception, object> _transformationFunction;

        public MoveMessageErrorPolicy(IBroker broker, IEndpoint endpoint, IPublisher publisher, ILogger<MoveMessageErrorPolicy> logger, MessageLogger messageLogger) 
            : base(publisher, logger, messageLogger)
        {
            _producer = broker.GetProducer(endpoint);
            _endpoint = endpoint;
            _logger = logger;
            _messageLogger = messageLogger;
        }

        public MoveMessageErrorPolicy Transform(Func<object, Exception, object> transformationFunction)
        {
            _transformationFunction = transformationFunction;
            return this;
        }

        protected override ErrorAction ApplyPolicy(FailedMessage failedMessage, Exception exception)
        {
            if (failedMessage.Message is BatchCompleteEvent batchMessage)
            {
                foreach (var singleFailedMessage in batchMessage.Messages)
                {
                    PublishToNewEndpoint(singleFailedMessage, exception);
                }
            }
            else
            {
                PublishToNewEndpoint(failedMessage, exception);
            }

            _messageLogger.LogTrace(_logger, "The failed message has been moved and will be skipped.", failedMessage, _endpoint);

            return ErrorAction.Skip;
        }

        private void PublishToNewEndpoint(object failedMessage, Exception exception)
        {
            _producer.Produce(_transformationFunction?.Invoke(failedMessage, exception) ?? failedMessage);
        }
    }
}