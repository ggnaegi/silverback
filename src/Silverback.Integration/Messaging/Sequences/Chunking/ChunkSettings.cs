﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Diagnostics.CodeAnalysis;
using Silverback.Messaging.Configuration;

namespace Silverback.Messaging.Sequences.Chunking
{
    /// <summary>
    ///     The chunking settings. To enable chunking just set the <c>Size</c> property to the desired (maximum)
    ///     chunk size.
    /// </summary>
    public sealed class ChunkSettings : IEquatable<ChunkSettings>, IValidatableEndpointSettings
    {
        /// <summary>
        ///     Gets or sets the size in bytes of each chunk. The default is <see cref="int.MaxValue" />, meaning that
        ///     chunking is disabled.
        /// </summary>
        public int Size { get; set; } = int.MaxValue;

        /// <summary>
        ///     Gets or sets a value indicating whether the <c>x-chunk-index</c> and related headers have to be added
        ///     to the produced message in any case, even if its size doesn't exceed the single chunk size. The
        ///     default is <c>true</c>. This setting is ignored if chunking is disabled (<see cref="Size" /> is not
        ///     set).
        /// </summary>
        public bool AlwaysAddHeaders { get; set; } = true;

        /// <inheritdoc cref="IValidatableEndpointSettings.Validate" />
        public void Validate()
        {
            if (Size < 1)
                throw new EndpointConfigurationException("Chunk.Size must be greater or equal to 1.");
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)" />
        public bool Equals(ChunkSettings? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Size == other.Size;
        }

        /// <inheritdoc cref="object.Equals(object)" />
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((ChunkSettings)obj);
        }

        /// <inheritdoc cref="object.GetHashCode" />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = Justifications.Settings)]
        public override int GetHashCode() => HashCode.Combine(Size);
    }
}
