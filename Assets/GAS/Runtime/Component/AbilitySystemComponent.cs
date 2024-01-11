using System;
using System.Collections.Generic;
using GAS.Core;
using GAS.Runtime.Ability;
using GAS.Runtime.AttributeSet;
using GAS.Runtime.Effects;
using GAS.Runtime.Effects.Modifier;
using GAS.Runtime.Tags;
using UnityEngine;

namespace GAS.Runtime.Component
{
    public class AbilitySystemComponent : MonoBehaviour, IAbilitySystemComponent
    {
        [SerializeField] private AbilitySystemComponentPreset preset;

        public int Level { get; }

        public GameplayEffectContainer GameplayEffectContainer { get; } = new GameplayEffectContainer();

        public GameplayTagAggregator GameplayTagAggregator { get; } = new GameplayTagAggregator();

        public AbilityContainer AbilityContainer { get; } = new AbilityContainer();

        public AttributeSetContainer AttributeSetContainer { get; } = new AttributeSetContainer();

        private void Awake()
        {
            AbilityContainer.SetOwner(this);
            GameplayEffectContainer.SetOwner(this);
            AttributeSetContainer.SetOwner(this);
            GameplayTagAggregator.SetOwner(this);
            Init(preset);
        }

        private void OnEnable()
        {
            GameplayAbilitySystem.GAS.Register(this);
            GameplayTagAggregator.OnEnable();
        }

        private void OnDisable()
        {
            GameplayAbilitySystem.GAS.Unregister(this);
            GameplayTagAggregator.OnDisable();
        }

        public void DefaultInit()
        {
            Init(preset);
        }

        public void Init(AbilitySystemComponentPreset ascPreset)
        {
            if (ascPreset != null)
            {
                // Tag
                if (ascPreset.BaseTags != null) GameplayTagAggregator.Init(ascPreset.BaseTags);

                // AttributeSet
                if (ascPreset.AttributeSets != null)
                    foreach (var attributeSet in ascPreset.AttributeSets)
                    {
                        var attrSetTypeName = GasDefine.GAS_ATTRIBUTESET_CLASS_TYPE_PREFIX + attributeSet;
                        var attrSetType = Type.GetType($"{attrSetTypeName}, Assembly-CSharp");
                        if (attrSetType != null) AttributeSetContainer.AddAttributeSet(attrSetType);
                    }

                // Ability
                if (ascPreset.BaseAbilities != null)
                    foreach (var abilityAsset in ascPreset.BaseAbilities)
                    {
                        var abilityType = Type.GetType($"{abilityAsset.InstanceAbilityClassFullName}, Assembly-CSharp");
                        if (abilityType != null)
                        {
                            var ability = Activator.CreateInstance(abilityType, args: abilityAsset) as AbstractAbility;
                            AbilityContainer.GrantAbility(ability);
                        }
                    }
            }
        }

        public bool HasTag(GameplayTag gameplayTag)
        {
            return GameplayTagAggregator.HasTag(gameplayTag);
        }

        public bool HasAllTags(GameplayTagSet tags)
        {
            return GameplayTagAggregator.HasAllTags(tags);
        }

        public bool HasAnyTags(GameplayTagSet tags)
        {
            return GameplayTagAggregator.HasAnyTags(tags);
        }

        public void AddFixedTags(GameplayTagSet tags)
        {
            GameplayTagAggregator.AddFixedTag(tags);
        }

        public void RemoveFixedTags(GameplayTagSet tags)
        {
            GameplayTagAggregator.RemoveFixedTag(tags);
        }

        public void RemoveGameplayEffect(GameplayEffectSpec spec)
        {
            GameplayEffectContainer.RemoveGameplayEffectSpec(spec);
        }


        public GameplayEffectSpec ApplyGameplayEffectTo(GameplayEffect gameplayEffect, AbilitySystemComponent target)
        {
            return gameplayEffect.CanApplyTo(target)
                ? target.AddGameplayEffect(gameplayEffect.CreateSpec(this, target, Level))
                : null;
        }

        public GameplayEffectSpec ApplyGameplayEffectToSelf(GameplayEffect gameplayEffect)
        {
            return ApplyGameplayEffectTo(gameplayEffect, this);
        }

        public void GrantAbility(AbstractAbility ability)
        {
            AbilityContainer.GrantAbility(ability);
        }

        public void RemoveAbility(string abilityName)
        {
            AbilityContainer.RemoveAbility(abilityName);
        }

        public float? GetAttributeCurrentValue(string setName, string attributeShortName)
        {
            var value = AttributeSetContainer.GetAttributeCurrentValue(setName, attributeShortName);
            return value;
        }

        public float? GetAttributeBaseValue(string setName, string attributeShortName)
        {
            var value = AttributeSetContainer.GetAttributeBaseValue(setName, attributeShortName);
            return value;
        }

        public void Tick()
        {
            AbilityContainer.Tick();
            GameplayEffectContainer.Tick();
        }

        public Dictionary<string, float> DataSnapshot()
        {
            return AttributeSetContainer.Snapshot();
        }

        public bool TryActivateAbility(string abilityName, params object[] args)
        {
            return AbilityContainer.TryActivateAbility(abilityName, args);
        }

        public void TryEndAbility(string abilityName)
        {
            AbilityContainer.EndAbility(abilityName);
        }

        public void ApplyModFromInstantGameplayEffect(GameplayEffectSpec spec)
        {
            foreach (var modifier in spec.GameplayEffect.Modifiers)
            {
                var attributeBaseValue = GetAttributeBaseValue(modifier.AttributeSetName, modifier.AttributeShortName);
                if (attributeBaseValue == null) continue;
                var magnitude = modifier.MMC.CalculateMagnitude(spec, modifier.ModiferMagnitude);
                var baseValue = attributeBaseValue.Value;
                switch (modifier.Operation)
                {
                    case GEOperation.Add:
                        baseValue += magnitude;
                        break;
                    case GEOperation.Multiply:
                        baseValue *= magnitude;
                        break;
                    case GEOperation.Override:
                        baseValue = magnitude;
                        break;
                }

                AttributeSetContainer.Sets[modifier.AttributeSetName]
                    .ChangeAttributeBase(modifier.AttributeShortName, baseValue);
            }
        }

        public CooldownTimer CheckCooldownFromTags(GameplayTagSet tags)
        {
            return GameplayEffectContainer.CheckCooldownFromTags(tags);
        }

        public T AttrSet<T>() where T : AttributeSet.AttributeSet
        {
            AttributeSetContainer.TryGetAttributeSet<T>(out var attrSet);
            return attrSet;
        }

        public void ClearGameplayEffect()
        {
            // _abilityContainer = new AbilityContainer(this);
            // GameplayEffectContainer = new GameplayEffectContainer(this);
            // _attributeSetContainer = new AttributeSetContainer(this);
            // tagAggregator = new GameplayTagAggregator(this);

            GameplayEffectContainer.ClearGameplayEffect();
        }

        private GameplayEffectSpec AddGameplayEffect(GameplayEffectSpec spec)
        {
            var success = GameplayEffectContainer.AddGameplayEffectSpec(spec);
            return success ? spec : null;
        }
    }
}