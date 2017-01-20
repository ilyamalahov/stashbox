﻿using Stashbox.Entity;
using Stashbox.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Stashbox.BuildUp.Resolution
{
    internal class EnumerableResolver : Resolver
    {
        private delegate object ResolverDelegate(ResolutionInfo resolutionInfo);
        private readonly IServiceRegistration[] registrationCache;
        private ResolverDelegate resolverDelegate;
        private readonly TypeInformation enumerableType;
        private readonly object syncObject = new object();

        public EnumerableResolver(IContainerContext containerContext, TypeInformation typeInfo)
            : base(containerContext, typeInfo)
        {
            this.enumerableType = new TypeInformation
            {
                Type = typeInfo.Type.GetEnumerableType() ?? typeInfo.Type.GenericTypeArguments[0],
                CustomAttributes = typeInfo.CustomAttributes,
                ParentType = typeInfo.ParentType,
                DependencyName = typeInfo.DependencyName
            };

            containerContext.RegistrationRepository.TryGetTypedRepositoryRegistrations(this.enumerableType,
                out registrationCache);

            if (registrationCache != null)
                registrationCache = base.BuilderContext.ContainerConfiguration.EnumerableOrderRule(registrationCache).ToArray();
        }

        public override object Resolve(ResolutionInfo resolutionInfo)
        {
            if (this.resolverDelegate != null) return this.resolverDelegate(resolutionInfo);
            lock (this.syncObject)
            {
                if (this.resolverDelegate != null) return this.resolverDelegate(resolutionInfo);
                var parameter = Expression.Parameter(typeof(ResolutionInfo));
                this.resolverDelegate = Expression.Lambda<ResolverDelegate>(this.GetExpression(resolutionInfo, parameter), parameter).Compile();
            }

            return this.resolverDelegate(resolutionInfo);
        }

        public override Expression GetExpression(ResolutionInfo resolutionInfo, Expression resolutionInfoExpression)
        {
            if (registrationCache == null)
                return Expression.NewArrayInit(this.enumerableType.Type);

            var length = registrationCache.Length;
            var enumerableItems = new Expression[length];
            for (var i = 0; i < length; i++)
            {
                enumerableItems[i] = registrationCache[i].GetExpression(resolutionInfo, resolutionInfoExpression, this.enumerableType);
            }

            return Expression.NewArrayInit(this.enumerableType.Type, enumerableItems);
        }

        public static bool IsAssignableToGenericType(Type type, Type genericType)
        {
            if (type == null || genericType == null) return false;

            return type == genericType
              || MapsToGenericTypeDefinition(type, genericType)
              || HasInterfaceThatMapsToGenericTypeDefinition(type, genericType)
              || IsAssignableToGenericType(type.GetTypeInfo().BaseType, genericType);
        }

        private static bool HasInterfaceThatMapsToGenericTypeDefinition(Type type, Type genericType)
        {
            return type.GetTypeInfo().ImplementedInterfaces
              .Where(it => it.GetTypeInfo().IsGenericType)
              .Any(it => it.GetGenericTypeDefinition() == genericType);
        }

        private static bool MapsToGenericTypeDefinition(Type type, Type genericType)
        {
            return genericType.GetTypeInfo().IsGenericTypeDefinition
              && type.GetTypeInfo().IsGenericType
              && type.GetGenericTypeDefinition() == genericType;
        }
    }
}
