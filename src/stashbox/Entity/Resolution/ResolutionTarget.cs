﻿using Stashbox.Infrastructure;

namespace Stashbox.Entity.Resolution
{
    public class ResolutionTarget
    {
        public TypeInformation TypeInformation { get; set; }
        public Resolver Resolver { get; set; }
        public object ResolutionTargetValue { get; set; }
    }
}