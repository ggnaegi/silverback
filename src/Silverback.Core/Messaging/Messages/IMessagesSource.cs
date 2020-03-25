﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Collections.Generic;

namespace Silverback.Messaging.Messages
{
    public interface IMessagesSource
    {
        IEnumerable<object> GetMessages();

        void ClearMessages();
    }
}