// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Silverback.Diagnostics;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Broker.Behaviors;
using Silverback.Messaging.Messages;

namespace Silverback.Tests.Integration.TestTypes
{
    public class TestOtherProducer : Producer<TestOtherBroker, TestOtherProducerEndpoint>
    {
        public TestOtherProducer(
            TestOtherBroker broker,
            TestOtherProducerEndpoint endpoint,
            IBrokerBehaviorsProvider<IProducerBehavior> behaviorsProvider,
            IServiceProvider serviceProvider)
            : base(
                broker,
                endpoint,
                behaviorsProvider,
                serviceProvider,
                Substitute.For<ISilverbackIntegrationLogger<TestOtherProducer>>())
        {
            ProducedMessages = broker.ProducedMessages;
        }

        public IList<ProducedMessage> ProducedMessages { get; }

        protected override IBrokerMessageIdentifier? ProduceCore(IOutboundEnvelope envelope)
        {
            ProducedMessages.Add(new ProducedMessage(envelope.RawMessage, envelope.Headers, Endpoint));
            return null;
        }

        protected override Task<IBrokerMessageIdentifier?> ProduceCoreAsync(IOutboundEnvelope envelope)
        {
            Produce(envelope.RawMessage, envelope.Headers);
            return Task.FromResult<IBrokerMessageIdentifier?>(null);
        }
    }
}
