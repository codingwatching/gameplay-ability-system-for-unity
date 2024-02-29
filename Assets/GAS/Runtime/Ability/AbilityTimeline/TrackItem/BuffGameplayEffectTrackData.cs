﻿using System;
using System.Collections.Generic;
using GAS.Runtime.Effects;
using UnityEngine.Serialization;

namespace GAS.Runtime.Ability.AbilityTimeline
{
    [Serializable]
    public class BuffGameplayEffectTrackData:TrackDataBase
    {
        public string trackName;
        public List<BuffGameplayEffectClipEvent> clipEvents = new List<BuffGameplayEffectClipEvent>();

        public override void AddToAbilityAsset(TimelineAbilityAsset abilityAsset)
        {
            base.AddToAbilityAsset(abilityAsset);
            abilityAsset.BuffGameplayEffects.Add(this);
        }

        public override void DefaultInit(int index)
        {
            base.DefaultInit(index);
            trackName = "BuffGameplayEffect";
        }
    }
    
    [Serializable]
    public class BuffGameplayEffectClipEvent : ClipEventBase
    {
        public BuffTarget buffTarget;
        [FormerlySerializedAs("gameplayEffects")] public GameplayEffectAsset gameplayEffect;
    }

    public enum BuffTarget
    {
        Self,
    }
}