using System;
using System.Collections.Generic;
using GAS.Runtime;
using GAS.Runtime.Cue;

namespace GAS.Runtime
{
    [Serializable]
    public class InstantCueTrackData:TrackDataBase
    {
        public List<InstantCueMarkEvent> markEvents = new List<InstantCueMarkEvent>();
        
        public override void AddToAbilityAsset(TimelineAbilityAsset abilityAsset)
        {
            base.AddToAbilityAsset(abilityAsset);
            abilityAsset.InstantCues.Add(this);
        }
    }
    
    [Serializable]
    public class InstantCueMarkEvent:MarkEventBase
    {
        public List<GameplayCueInstant> cues = new List<GameplayCueInstant>();
    }
}