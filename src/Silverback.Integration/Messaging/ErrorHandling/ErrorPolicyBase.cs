﻿// Copyright (c) 2018-2019 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;

namespace Silverback.Messaging.ErrorHandling
{
    public abstract class ErrorPolicyBase : IErrorPolicy
    {
        private readonly IPublisher _publisher;
        private readonly ILogger<ErrorPolicyBase> _logger;
        private readonly MessageLogger _messageLogger;
        private readonly List<Type> _excludedExceptions = new List<Type>();
        private readonly List<Type> _includedExceptions = new List<Type>();
        private Func<FailedMessage, Exception, bool> _applyRule;
        private int _maxFailedAttempts = -1;

        protected ErrorPolicyBase(IPublisher publisher, ILogger<ErrorPolicyBase> logger, MessageLogger messageLogger)
        {
            _publisher = publisher;
            _logger = logger;
            _messageLogger = messageLogger;
        }

        internal Func<FailedMessage, object> MessageToPublishFactory { get; private set; }

        /// <summary>
        /// Restricts the application of this policy to the specified exception type only.
        /// It is possible to combine multiple calls to <c>ApplyTo</c> and <c>Exclude</c>.
        /// </summary>
        /// <typeparam name="T">The type of the exception to be handled.</typeparam>
        /// <returns></returns>
        public ErrorPolicyBase ApplyTo<T>() where T : Exception
        {
            ApplyTo(typeof(T));
            return this;
        }

        /// <summary>
        /// Restricts the application of this policy to the specified exception type only.
        /// It is possible to combine multiple calls to <c>ApplyTo</c> and <c>Exclude</c>.
        /// </summary>
        /// <param name="exceptionType">The type of the exception to be handled.</param>
        /// <returns></returns>
        public ErrorPolicyBase ApplyTo(Type exceptionType)
        {
            _includedExceptions.Add(exceptionType);
            return this;
        }

        /// <summary>
        /// Restricts the application of this policy to all exceptions but the specified type.
        /// It is possible to combine multiple calls to <c>ApplyTo</c> and <c>Exclude</c>.
        /// </summary>
        /// <typeparam name="T">The type of the exception to be ignored.</typeparam>
        /// <returns></returns>
        public ErrorPolicyBase Exclude<T>() where T : Exception
        {
            Exclude(typeof(T));
            return this;
        }

        /// <summary>
        /// Restricts the application of this policy to all exceptions but the specified type.
        /// It is possible to combine multiple calls to <c>ApplyTo</c> and <c>Exclude</c>.
        /// </summary>
        /// <param name="exceptionType">The type of the exception to be ignored.</param>
        /// <returns></returns>
        public ErrorPolicyBase Exclude(Type exceptionType)
        {
            _excludedExceptions.Add(exceptionType);
            return this;
        }

        /// <summary>
        /// Specifies a predicate to be used to determine whether the policy has to be applied
        /// according to the current message and exception.
        /// </summary>
        /// <param name="applyRule">The predicate.</param>
        /// <returns></returns>

        public ErrorPolicyBase ApplyWhen(Func<FailedMessage, Exception, bool> applyRule)
        {
            _applyRule = applyRule;
            return this;
        }

        /// <summary>
        /// Specifies how many times this rule can be applied to the same message. Most useful
        /// for <see cref="RetryErrorPolicy"/> and <see cref="MoveMessageErrorPolicy"/> to limit the
        /// number of iterations.
        /// If multiple policies are chained in an <see cref="ErrorPolicyChain"/> then the next policy will
        /// be triggered after the allotted amount of retries.
        /// </summary>
        /// <param name="maxFailedAttempts">The number of retries.</param>
        /// <returns></returns>
        public ErrorPolicyBase MaxFailedAttempts(int maxFailedAttempts)
        {
            _maxFailedAttempts = maxFailedAttempts;
            return this;
        }

        /// <summary>
        /// Specify a delegate to create a message to be published to the internal bus
        /// when this policy is applied. Useful to execute some custom code.
        /// </summary>
        /// <param name="factory">The factory returning the message to be published.</param>
        /// <returns></returns>
        public ErrorPolicyBase Publish(Func<FailedMessage, object> factory)
        {
            MessageToPublishFactory = factory;
            return this;
        }

        public virtual bool CanHandle(FailedMessage failedMessage, Exception exception)
        {
            if (failedMessage == null)
            {
                _logger.LogTrace($"The policy '{GetType().Name}' cannot be applied because the message is null.");
                return false;
            }

            if (_maxFailedAttempts >= 0 && failedMessage.FailedAttempts > _maxFailedAttempts)
            {
                _messageLogger.LogTrace(_logger, $"The policy '{GetType().Name}' will be skipped because the current failed attempts " +
                                 $"({failedMessage.FailedAttempts}) exceeds the configured maximum attempts " +
                                 $"({_maxFailedAttempts}).", failedMessage);

                return false;
            }

            if (_includedExceptions.Any() && _includedExceptions.All(e => !e.IsInstanceOfType(exception)))
            {
                _messageLogger.LogTrace(_logger, $"The policy '{GetType().Name}' will be skipped because the {exception.GetType().Name} " +
                                 $"is not in the list of handled exceptions.", failedMessage);

                return false;
            }

            if (_excludedExceptions.Any(e => e.IsInstanceOfType(exception)))
            {
                _messageLogger.LogTrace(_logger, $"The policy '{GetType().Name}' will be skipped because the {exception.GetType().Name} " +
                                 $"is in the list of excluded exceptions.", failedMessage);

                return false;
            }

            if (_applyRule != null && !_applyRule.Invoke(failedMessage, exception))
            {
                _messageLogger.LogTrace(_logger, $"The policy '{GetType().Name}' will be skipped because the apply rule has been " +
                                 $"evaluated and returned false.", failedMessage);
                return false;
            }

            return true;
        }

        public ErrorAction HandleError(FailedMessage failedMessage, Exception exception)
        {
            var result = ApplyPolicy(failedMessage, exception);

            if (MessageToPublishFactory != null)
            {
                _publisher.Publish(MessageToPublishFactory.Invoke(failedMessage));
            }

            return result;
        }

        protected abstract ErrorAction ApplyPolicy(FailedMessage failedMessage, Exception exception);
    }
}