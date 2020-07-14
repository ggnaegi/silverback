﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Diagnostics;
using Silverback.Messaging.Broker;
using Silverback.Messaging.ErrorHandling;

namespace Silverback.Messaging.Configuration
{
    internal class ErrorPolicyBuilder : IErrorPolicyBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public ErrorPolicyBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ErrorPolicyChain Chain(params ErrorPolicyBase[] policies) =>
            new ErrorPolicyChain(
                _serviceProvider,
                _serviceProvider.GetRequiredService<ISilverbackLogger<ErrorPolicyChain>>(),
                policies);

        public RetryErrorPolicy Retry(TimeSpan? initialDelay = null, TimeSpan? delayIncrement = null) =>
            new RetryErrorPolicy(
                _serviceProvider,
                _serviceProvider.GetRequiredService<ISilverbackLogger<RetryErrorPolicy>>(),
                initialDelay,
                delayIncrement);

        public SkipMessageErrorPolicy Skip() =>
            new SkipMessageErrorPolicy(
                _serviceProvider,
                _serviceProvider.GetRequiredService<ISilverbackLogger<SkipMessageErrorPolicy>>());

        public MoveMessageErrorPolicy Move(IProducerEndpoint endpoint) =>
            new MoveMessageErrorPolicy(
                _serviceProvider.GetRequiredService<IBrokerCollection>(),
                endpoint,
                _serviceProvider,
                _serviceProvider.GetRequiredService<ISilverbackLogger<MoveMessageErrorPolicy>>());
    }
}
