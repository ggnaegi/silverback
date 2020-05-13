﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Threading.Tasks;
using Silverback.Messaging.Subscribers;
using Silverback.Tests.Core.TestTypes.Messages.Base;

namespace Silverback.Tests.Core.TestTypes.Subscribers
{
    public class TestCommandReplierWithWrongResponseType : ISubscriber
    {
        public int ReceivedMessagesCount { get; private set; }

        [Subscribe]
        public int OnRequestReceived(ICommand<string> message)
        {
            ReceivedMessagesCount++;

            return 1;
        }


        [Subscribe]
        public async Task<int> OnRequestReceived2(ICommand<string> message)
        {
            await Task.Delay(1);
            ReceivedMessagesCount++;

            return 2;
        }
    }
}