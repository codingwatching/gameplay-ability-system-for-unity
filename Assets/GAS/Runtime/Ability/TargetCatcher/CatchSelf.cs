﻿using System.Collections.Generic;
using GAS.Runtime.Component;

namespace GAS.Runtime
{
    public class CatchSelf : TargetCatcherBase
    {
        public override List<AbilitySystemComponent> CatchTargets(AbilitySystemComponent mainTarget)
        {
            return new List<AbilitySystemComponent> { Owner };
        }
    }
}