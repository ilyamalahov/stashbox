﻿using Stashbox.Entity;
using Stashbox.Infrastructure.ContainerExtension;
using System;

namespace Stashbox.Infrastructure
{
    public interface IStashboxContainer : IDependencyRegistrator, IDependencyResolver
    {
        void RegisterExtension(IContainerExtension containerExtension);
        void RegisterResolver(Func<IBuilderContext, TypeInformation, bool> resolverPredicate, ResolverFactory factory);
    }
}