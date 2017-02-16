﻿using System.Linq.Expressions;
using Stashbox.Entity;
using Stashbox.Entity.Resolution;

namespace Stashbox.Infrastructure.Resolution
{
    /// <summary>
    /// Represents a resolution strategy.
    /// </summary>
    public interface IResolutionStrategy
    {
        /// <summary>
        /// Builds a <see cref="ResolutionTarget"/> for a dependency.
        /// </summary>
        /// <param name="containerContext">The <see cref="IContainerContext"/> of the <see cref="StashboxContainer"/></param>
        /// <param name="resolutionInfo">The resolution info.</param>
        /// <param name="typeInformation">The type info of the requested service.</param>
        /// <param name="injectionParameters">The injection parameters.</param>
        /// <returns>The created resolution target.</returns>
        ResolutionTarget BuildResolutionTarget(IContainerContext containerContext, ResolutionInfo resolutionInfo, TypeInformation typeInformation,
            InjectionParameter[] injectionParameters);

        /// <summary>
        /// Gets an expression for evaluating a <see cref="ResolutionTarget"/>
        /// </summary>
        /// <param name="resolutionTarget">The resolution target object.</param>
        /// <param name="resolutionInfo">The info about the actual resolution.</param>
        /// <returns>The expression.</returns>
        Expression GetExpressionForResolutionTarget(ResolutionTarget resolutionTarget, ResolutionInfo resolutionInfo);
    }
}