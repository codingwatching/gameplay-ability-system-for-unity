using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GAS.General;
using GAS.Runtime.Effects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Runtime
{
    public abstract class AbilityAsset : ScriptableObject
    {
        private const string GRP_BASE = "Base Info";
        private const string GRP_BASE_H = "Base Info/H";
        protected const string GRP_BASE_H_LEFT = "Base Info/H/Left";
        private const string GRP_BASE_H_RIGHT = "Base Info/H/Right";

        private const string GRP_DATA = "DATA";
        private const string GRP_DATA_H = "DATA/H";
        private const string GRP_DATA_PARAMETER = "DATA/H/Parameter";
        private const string GRP_DATA_SUMMON = "DATA/H/Summon";
        private const string GRP_DATA_TAG = "DATA/H/Tags";
        private const string GRP_CUE = "CUE";
        private const string GRP_CUE_INSTANT = "CUE/Instant";
        private const string GRP_CUE_DURATIONAL = "CUE/Durational";


        protected const int WIDTH_LABLE = 100;
        private const int WIDTH_GRP_BASE_H_LEFT = 350;


        private static IEnumerable AbilityClassChoice = new ValueDropdownList<string>();
        private static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();



        public abstract Type AbilityType();

        public string InstanceAbilityClassFullName => AbilityType() != null ? AbilityType().FullName : null;

#if UNITY_EDITOR
        [BoxGroup("Class", false, order: -1)]
        [HideLabel]
        [LabelWidth(WIDTH_LABLE)]
        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left, true)]
        [GUIColor(1, 1, 1, 1)]
        private string
            AbilityTypeFoShow {
            get
            {
                if(InstanceAbilityClassFullName==null) return "<size=18><color=red><b>Ability Class is NULL!!! Please check.</b></color></size>";
                return $"<size=15><color=white><b>Ability Class : {InstanceAbilityClassFullName}</b></color></size>";
            }
        }
#endif
        
        [BoxGroup(GRP_BASE, false)]
        [Title(GASTextDefine.ABILITY_BASEINFO, bold: true)]
        [HorizontalGroup(GRP_BASE_H, Width = WIDTH_GRP_BASE_H_LEFT)]
        [VerticalGroup(GRP_BASE_H_LEFT)]
        [InfoBox(GASTextDefine.TIP_UNAME)]
        [LabelText("U-Name")]
        [LabelWidth(WIDTH_LABLE)]
        public string UniqueName;
        
        [VerticalGroup(GRP_BASE_H_LEFT)]
        [Title("Description", bold: false)]
        [HideLabel]
        [MultiLineProperty(5)]
        public string Description;
        
        [Space]
        [Title("Gameplay Effect", bold: true)]
        [VerticalGroup(GRP_BASE_H_LEFT)]
        [LabelWidth(WIDTH_LABLE)]
        [AssetSelector]
        [LabelText(GASTextDefine.ABILITY_EFFECT_COST)]
        public GameplayEffectAsset Cost;
        
        [VerticalGroup(GRP_BASE_H_LEFT)]
        [LabelWidth(WIDTH_LABLE)]
        [AssetSelector]
        [LabelText(GASTextDefine.ABILITY_EFFECT_CD)]
        public GameplayEffectAsset Cooldown;
        
        [VerticalGroup(GRP_BASE_H_LEFT)]
        [LabelWidth(WIDTH_LABLE)]
        [LabelText(SdfIconType.ClockFill,Text = GASTextDefine.ABILITY_CD_TIME)]
        public float CooldownTime;
        
        // Tags
        [Title("Tags",bold:true)]
        [HorizontalGroup(GRP_BASE_H,PaddingLeft = 0.025f)]
        [VerticalGroup(GRP_BASE_H_RIGHT)]
        [ListDrawerSettings(Expanded = true)]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        public GameplayTag[] AssetTag;
        
        [VerticalGroup(GRP_BASE_H_RIGHT)]
        [ListDrawerSettings(Expanded = true)]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [LabelText("CancelAbility With Tags ")]
        [Space]
        public GameplayTag[] CancelAbilityTags;
        
        [VerticalGroup(GRP_BASE_H_RIGHT)]
        [ListDrawerSettings(Expanded = true)]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [LabelText("BlockAbility With Tags ")]
        [Space]
        public GameplayTag[] BlockAbilityTags;
        
        [VerticalGroup(GRP_BASE_H_RIGHT)]
        [ListDrawerSettings(Expanded = true)]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Space]
        public GameplayTag[] ActivationOwnedTag;
        
        [VerticalGroup(GRP_BASE_H_RIGHT)]
        [ListDrawerSettings(Expanded = true)]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Space]
        public GameplayTag[] ActivationRequiredTags;
        
        [VerticalGroup(GRP_BASE_H_RIGHT)]
        [ListDrawerSettings(Expanded = true)]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Space]
        public GameplayTag[] ActivationBlockedTags;
        // public GameplayTag[] SourceRequiredTags;
        // public GameplayTag[] SourceBlockedTags;
        // public GameplayTag[] TargetRequiredTags;
        // public GameplayTag[] TargetBlockedTags;


        
        
        
        private void OnEnable()
        {
            SetTagChoices();
        }
        
        private static void SetTagChoices()
        {
            Type gameplayTagSumCollectionType = Type.GetType($"GAS.Runtime.GTagLib, Assembly-CSharp");
            if(gameplayTagSumCollectionType == null)
            {
                Debug.LogError("[EX] Type 'GTagLib' not found. Please generate the TAGS CODE first!");
                TagChoices = new ValueDropdownList<GameplayTag>();
                return;
            }
            FieldInfo tagMapField = gameplayTagSumCollectionType.GetField("TagMap", BindingFlags.Public | BindingFlags.Static);

            if (tagMapField != null)
            {
                Dictionary<string, GameplayTag> tagMapValue = (Dictionary<string, GameplayTag>)tagMapField.GetValue(null);
                var tagChoices = tagMapValue.Values.ToList();
                var choices = new ValueDropdownList<GameplayTag>();
                foreach (var tag in tagChoices) choices.Add(tag.Name,tag);
                TagChoices = choices;
            }
            else
            {
                TagChoices = new ValueDropdownList<GameplayTag>();
            }
        }
    }
}