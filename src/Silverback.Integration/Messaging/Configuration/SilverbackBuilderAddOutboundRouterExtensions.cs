﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Outbound.Routing;
using Silverback.Util;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Adds the <c>AddSingletonOutboundRouter</c> method to the <see cref="ISilverbackBuilder" />.
    /// </summary>
    public static class SilverbackBuilderAddOutboundRouterExtensions
    {
        /// <summary>
        ///     Adds a transient outbound router of the type specified in <paramref name="routerType" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <param name="routerType">
        ///     The type of the outbound router to register and the implementation to use.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddTransientOutboundRouter(
            this ISilverbackBuilder silverbackBuilder,
            Type routerType)
        {
            Check.NotNull(silverbackBuilder, nameof(silverbackBuilder));

            silverbackBuilder.Services.AddTransient(routerType);

            return silverbackBuilder;
        }

        /// <summary>
        ///     Adds a transient outbound router of the type specified in <typeparamref name="TRouter" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TRouter">
        ///     The type of the outbound router to add.
        /// </typeparam>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddTransientOutboundRouter<TRouter>(this ISilverbackBuilder silverbackBuilder)
            where TRouter : class, IOutboundRouter =>
            AddTransientOutboundRouter(silverbackBuilder, typeof(TRouter));

        /// <summary>
        ///     Adds a transient outbound router with a factory specified in
        ///     <paramref name="implementationFactory" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <param name="implementationFactory">
        ///     The factory that creates the service.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddTransientOutboundRouter(
            this ISilverbackBuilder silverbackBuilder,
            Func<IServiceProvider, IOutboundRouter> implementationFactory)
        {
            Check.NotNull(silverbackBuilder, nameof(silverbackBuilder));

            silverbackBuilder.Services.AddTransient(implementationFactory);

            return silverbackBuilder;
        }

        /// <summary>
        ///     Adds a scoped outbound router of the type specified in <paramref name="routerType" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <param name="routerType">
        ///     The type of the outbound router to register and the implementation to use.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddScopedOutboundRouter(
            this ISilverbackBuilder silverbackBuilder,
            Type routerType)
        {
            Check.NotNull(silverbackBuilder, nameof(silverbackBuilder));

            silverbackBuilder.Services.AddScoped(routerType);

            return silverbackBuilder;
        }

        /// <summary>
        ///     Adds a scoped outbound router of the type specified in <typeparamref name="TRouter" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TRouter">
        ///     The type of the outbound router to add.
        /// </typeparam>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddScopedOutboundRouter<TRouter>(this ISilverbackBuilder silverbackBuilder)
            where TRouter : class, IOutboundRouter =>
            AddScopedOutboundRouter(silverbackBuilder, typeof(TRouter));

        /// <summary>
        ///     Adds a scoped outbound router with a factory specified in
        ///     <paramref name="implementationFactory" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <param name="implementationFactory">
        ///     The factory that creates the service.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddScopedOutboundRouter(
            this ISilverbackBuilder silverbackBuilder,
            Func<IServiceProvider, IOutboundRouter> implementationFactory)
        {
            Check.NotNull(silverbackBuilder, nameof(silverbackBuilder));

            silverbackBuilder.Services.AddScoped(implementationFactory);

            return silverbackBuilder;
        }

        /// <summary>
        ///     Adds a singleton outbound router of the type specified in <paramref name="routerType" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <param name="routerType">
        ///     The type of the outbound router to register and the implementation to use.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddSingletonOutboundRouter(
            this ISilverbackBuilder silverbackBuilder,
            Type routerType)
        {
            Check.NotNull(silverbackBuilder, nameof(silverbackBuilder));

            silverbackBuilder.Services.AddSingleton(routerType);

            return silverbackBuilder;
        }

        /// <summary>
        ///     Adds a singleton outbound router of the type specified in <typeparamref name="TRouter" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TRouter">
        ///     The type of the outbound router to add.
        /// </typeparam>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddSingletonOutboundRouter<TRouter>(this ISilverbackBuilder silverbackBuilder)
            where TRouter : class, IOutboundRouter =>
            AddSingletonOutboundRouter(silverbackBuilder, typeof(TRouter));

        /// <summary>
        ///     Adds a singleton outbound router with a factory specified in
        ///     <paramref name="implementationFactory" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <param name="implementationFactory">
        ///     The factory that creates the service.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddSingletonOutboundRouter(
            this ISilverbackBuilder silverbackBuilder,
            Func<IServiceProvider, IOutboundRouter> implementationFactory)
        {
            Check.NotNull(silverbackBuilder, nameof(silverbackBuilder));

            silverbackBuilder.Services.AddSingleton(implementationFactory);

            return silverbackBuilder;
        }

        /// <summary>
        ///     Adds a singleton outbound router with an instance specified in
        ///     <paramref name="implementationInstance" /> to the
        ///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="silverbackBuilder">
        ///     The <see cref="ISilverbackBuilder" /> that references the <see cref="IServiceCollection" /> to add
        ///     the services to.
        /// </param>
        /// <param name="implementationInstance">
        ///     The instance of the service.
        /// </param>
        /// <returns>
        ///     The <see cref="ISilverbackBuilder" /> so that additional calls can be chained.
        /// </returns>
        public static ISilverbackBuilder AddSingletonOutboundRouter(
            this ISilverbackBuilder silverbackBuilder,
            IOutboundRouter implementationInstance)
        {
            Check.NotNull(silverbackBuilder, nameof(silverbackBuilder));

            silverbackBuilder.Services.AddSingleton(implementationInstance);

            return silverbackBuilder;
        }
    }
}
