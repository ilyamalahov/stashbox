﻿using Stashbox.Entity;
using Stashbox.Utils;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Stashbox.Resolution
{
    internal class ResolutionStrategy : IResolutionStrategy
    {
        private readonly IResolverSelector resolverSelector;

        internal ResolutionStrategy(IResolverSelector resolverSelector)
        {
            this.resolverSelector = resolverSelector;
        }

        public Expression BuildResolutionExpression(IContainerContext containerContext, ResolutionContext resolutionContext, TypeInformation typeInformation,
            InjectionParameter[] injectionParameters)
        {
            if (typeInformation.Type == Constants.ResolverType)
                return resolutionContext.CurrentScopeParameter.ConvertTo(Constants.ResolverType);

            if (resolutionContext.ParameterExpressions.Length > 0)
            {
                var length = resolutionContext.ParameterExpressions.Length;
                for (var i = length; i-- > 0;)
                {
                    var parameters = resolutionContext.ParameterExpressions[i].WhereOrDefault(p => p.Value.Type == typeInformation.Type ||
                                                                                                   p.Value.Type.Implements(typeInformation.Type));

                    if (parameters == null) continue;
                    var selected = parameters.Repository.FirstOrDefault(parameter => !parameter.Key) ?? parameters.Repository.Last();
                    selected.Key = true;
                    return selected.Value;
                }
            }

            var matchingParam = injectionParameters?.FirstOrDefault(param => param.Name == typeInformation.ParameterName);
            if (matchingParam != null)
                return matchingParam.Value.GetType() == typeInformation.Type
                    ? matchingParam.Value.AsConstant()
                    : matchingParam.Value.AsConstant().ConvertTo(typeInformation.Type);

            var exprOverride = resolutionContext.GetExpressionOverrideOrDefault(typeInformation.Type);
            if (exprOverride != null)
                return exprOverride;

            var registration = containerContext.RegistrationRepository.GetRegistrationOrDefault(typeInformation, resolutionContext);
            return registration != null ? registration.GetExpression(resolutionContext.ChildContext ?? containerContext, resolutionContext, typeInformation.Type) :
                this.resolverSelector.GetResolverExpression(containerContext, typeInformation, resolutionContext);
        }

        public Expression[] BuildResolutionExpressions(IContainerContext containerContext, ResolutionContext resolutionContext, TypeInformation typeInformation)
        {
            var registrations = containerContext.RegistrationRepository.GetRegistrationsOrDefault(typeInformation.Type, resolutionContext)?.CastToArray();
            if (registrations == null)
                return this.resolverSelector.GetResolverExpressions(containerContext, typeInformation, resolutionContext);

            var lenght = registrations.Length;
            var expressions = new Expression[lenght];
            for (var i = 0; i < lenght; i++)
                expressions[i] = registrations[i].Value.GetExpression(resolutionContext.ChildContext ?? containerContext, resolutionContext, typeInformation.Type);

            return expressions;
        }
    }
}
