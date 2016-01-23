﻿using Stashbox.Entity;
using Stashbox.Infrastructure;
using System.Linq.Expressions;

namespace Stashbox.BuildUp.Resolution
{
    internal class ContainerResolver : Resolver
    {
        private readonly IServiceRegistration registrationCache;

        public ContainerResolver(IContainerContext containerContext, TypeInformation typeInfo)
            : base(containerContext, typeInfo)
        {
            containerContext.RegistrationRepository.TryGetRegistrationWithConditions(typeInfo,
                out this.registrationCache);
        }

        public override Expression GetExpression(ResolutionInfo resolutionInfo, Expression resolutionInfoExpression)
        {
            return registrationCache.GetExpression(resolutionInfo, resolutionInfoExpression, base.TypeInfo);
        }

        public override object Resolve(ResolutionInfo resolutionInfo)
        {
            return registrationCache.GetInstance(resolutionInfo, base.TypeInfo);
        }
    }
}
