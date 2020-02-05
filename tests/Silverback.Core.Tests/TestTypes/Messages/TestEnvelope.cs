// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using Silverback.Messaging.Messages;

namespace Silverback.Tests.Core.TestTypes.Messages
{
    public class TestEnvelope : IEnvelope
    {
        public TestEnvelope(object message)
        {
            Message = message;
        }

        public object Message { get; }
    }
}