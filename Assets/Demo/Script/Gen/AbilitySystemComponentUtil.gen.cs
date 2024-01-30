///////////////////////////////////
//// This is a generated file. ////
////     Do not modify it.     ////
///////////////////////////////////
using System;
using GAS.Runtime.Ability;
using GAS.Runtime.AttributeSet;
using GAS.Runtime.Tags;
namespace GAS.Runtime.Component
{
      public static class AbilitySystemComponentExtension
      {
          public static Type[] PresetAttributeSetTypes(this AbilitySystemComponent asc)
          {
              if (asc.Preset == null) return null;
              var attrSetTypes = new Type[asc.Preset.AttributeSets.Length];
              for (var i = 0; i < asc.Preset.AttributeSets.Length; i++)
                  attrSetTypes[i] = AttrSetUtil.AttrSetTypeDict[asc.Preset.AttributeSets[i]];
              return attrSetTypes;
          }
          public static AbilityInstanceInfo[] PresetAbilityInstanceInfos(this AbilitySystemComponent asc)
          {
              if (asc.Preset == null) return null;
              AbilityInstanceInfo[] infos=new AbilityInstanceInfo[asc.Preset.BaseAbilities.Length];
              for (var i = 0; i < asc.Preset.BaseAbilities.Length; i++)
              {
                  var abilityAsset = asc.Preset.BaseAbilities[i];
                  infos[i] = new AbilityInstanceInfo()
                  {
                      abilityAsset =  abilityAsset,
                      abilityType = AbilityCollection.AbilityMap[abilityAsset.UniqueName].AbilityClassType
                  };
              }
              return infos;
          }
          public static GameplayTag[] PresetBaseTags(this AbilitySystemComponent asc)
          {
              if (asc.Preset == null) return null;
              return asc.Preset.BaseTags;
          }
          public static void InitWithPreset(this AbilitySystemComponent asc,int level, AbilitySystemComponentPreset preset = null)
          {
              asc.SetLevel(level);
              if (preset != null) asc.SetPreset(preset);
              if (asc.Preset == null) return;
              asc.Init(asc.PresetBaseTags(), asc.PresetAttributeSetTypes(), asc.PresetAbilityInstanceInfos());
          }
      }
}