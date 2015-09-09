﻿using Stashbox.Entity;
using Stashbox.Infrastructure;
using Stashbox.Overrides;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stashbox.MetaInfo
{
    internal class MetaInfoProvider : IMetaInfoProvider
    {
        private readonly IBuilderContext builderContext;
        private readonly IResolverSelector resolverSelector;
        private readonly MetaInfoCache metaInfoCache;

        public Type TypeTo => this.metaInfoCache.TypeTo;

        public MetaInfoProvider(IBuilderContext builderContext, IResolverSelector resolverSelector, Type typeTo)
        {
            this.builderContext = builderContext;
            this.resolverSelector = resolverSelector;
            this.metaInfoCache = new MetaInfoCache(typeTo);
        }

        public bool TryChooseConstructor(out ResolutionConstructor resolutionConstructor, OverrideManager overrideManager = null)
        {
            return this.TryGetBestConstructor(out resolutionConstructor, overrideManager);
        }

        private bool TryGetBestConstructor(out ResolutionConstructor resolutionConstructor, OverrideManager overrideManager = null)
        {
            return this.TryGetConstructor(this.metaInfoCache.Constructors.Where(constructor => constructor.HasInjectionAttribute), out resolutionConstructor, overrideManager) ||
                this.TryGetConstructor(this.metaInfoCache.Constructors.Where(constructor => !constructor.HasInjectionAttribute), out resolutionConstructor, overrideManager);
        }

        private bool TryGetConstructor(IEnumerable<ConstructorInformation> constructors, out ResolutionConstructor resolutionConstructor, OverrideManager overrideManager = null)
        {
            var usableConstructors = this.GetUsableConstructors(constructors, overrideManager).ToArray();

            if (usableConstructors.Any())
            {
                resolutionConstructor = this.CreateResolutionConstructor(this.SelectBestConstructor(usableConstructors));
                return true;
            }

            resolutionConstructor = null;
            return false;
        }

        private IEnumerable<ConstructorInformation> GetUsableConstructors(IEnumerable<ConstructorInformation> constructors, OverrideManager overrideManager = null)
        {
            if (overrideManager == null)
                return constructors
                    .Where(constructor => constructor.Parameters
                    .All(parameter => this.resolverSelector.CanResolve(this.builderContext, parameter)));

            return constructors
                .Where(constructor => constructor.Parameters
                    .All(parameter => this.resolverSelector.CanResolve(this.builderContext, parameter) ||
                                      overrideManager.ContainsValue(parameter)));
        }

        private ResolutionConstructor CreateResolutionConstructor(ConstructorInformation constructorInformation)
        {
            return new ResolutionConstructor
            {
                Constructor = constructorInformation,
                Parameters = constructorInformation.Parameters.Select(parameter =>
                {
                    Resolver resolver;
                    this.resolverSelector.TryChooseResolver(this.builderContext, parameter, out resolver);
                    return new ResolutionParameter
                    {
                        ParameterInfo = parameter,
                        Resolver = resolver
                    };
                }).ToArray()
            };
        }

        private ConstructorInformation SelectBestConstructor(IEnumerable<ConstructorInformation> constructors)
        {
            return constructors.OrderBy(constructor => constructor.Parameters.Count()).First();
        }
    }
}