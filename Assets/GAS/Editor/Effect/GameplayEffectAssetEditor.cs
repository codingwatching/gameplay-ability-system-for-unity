﻿using System;
using System.Collections.Generic;
using System.Linq;
using GAS.Editor.General;
using GAS.Editor.Tags;
using GAS.Runtime.Cue;
using GAS.Runtime.Effects;
using GAS.Runtime.Effects.Modifier;
using GAS.Runtime.Tags;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor.Effect
{
    [CustomEditor(typeof(GameplayEffectAsset))]
    public class GameplayEffectAssetEditor : UnityEditor.Editor
    {
        private ScriptableObjectReorderableList<GameplayCue> cueOnAddList;
        private ScriptableObjectReorderableList<GameplayCue> cueOnExecuteList;
        private ScriptableObjectReorderableList<GameplayCue> cueOnRemoveList;

        private CustomReorderableList<GameplayEffectModifier> modifierList;

        private Vector2 scrollPos;

        private List<GameplayTag> tagChoices = new List<GameplayTag>();

        private readonly ArraySetFromChoicesAsset<GameplayTag>[] tagGroupAssets =
            new ArraySetFromChoicesAsset<GameplayTag>[5];

        private readonly string[] tagGroupNames = new string[5]
        {
            "Asset Tags",
            "Granted Tags",
            "Application Required Tags",
            "Ongoing Required Tags",
            "Remove GameplayEffects With Tags"
        };

        private GameplayEffectAsset Asset => (GameplayEffectAsset)target;

        private void OnEnable()
        {
            tagChoices = TagEditorUntil.GetTagChoice();

            for (var i = 0; i < tagGroupAssets.Length; i++)
            {
                var initData = i switch
                {
                    0 => Asset.AssetTags,
                    1 => Asset.GrantedTags,
                    2 => Asset.ApplicationRequiredTags,
                    3 => Asset.OngoingRequiredTags,
                    4 => Asset.RemoveGameplayEffectsWithTags,
                    _ => Array.Empty<GameplayTag>()
                };
                tagGroupAssets[i] =
                    new ArraySetFromChoicesAsset<GameplayTag>(initData, tagChoices, tagGroupNames[i], null);
            }

            modifierList = CustomReorderableList<GameplayEffectModifier>.Create(Asset.Modifiers,
                rect => { EditorGUI.LabelField(rect, "Modifiers", EditorStyles.boldLabel); }, 
                OnEditModifier,
                OnModifierDrawGUI, GetModifierElementHeight, null);

            cueOnExecuteList = new ScriptableObjectReorderableList<GameplayCue>(Asset.CueOnExecute, "Cue On Execute");
            cueOnAddList = new ScriptableObjectReorderableList<GameplayCue>(Asset.CueOnAdd, "Cue On Add");
            cueOnRemoveList = new ScriptableObjectReorderableList<GameplayCue>(Asset.CueOnRemove, "Cue On Remove");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Name", GUILayout.Width(100));
                Asset.Name = EditorGUILayout.TextField("", Asset.Name);
            }

            EditorGUILayout.Space(5f);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Description", GUILayout.Width(100));
                Asset.Description = EditorGUILayout.TextField("", Asset.Description);
            }

            ConfigErrorTip();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            DurationPolicyGroup();
            EditorGUILayout.Space(5f);
            TagContainerGroup();
            EditorGUILayout.Space(5f);
            ModifierGroup();
            EditorGUILayout.Space(5f);
            CueGroup();
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save")) Save();

            EditorGUILayout.EndVertical();
        }

        private void DurationPolicyGroup()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField("Apply Policy", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Duration Policy", GUILayout.Width(100));
                Asset.DurationPolicy = (EffectsDurationPolicy)EditorGUILayout.EnumPopup("", Asset.DurationPolicy,
                    GUILayout.Width(70));

                if (Asset.DurationPolicy == EffectsDurationPolicy.Duration)
                {
                    EditorGUILayout.LabelField("-->", EditorStyles.miniBoldLabel, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Duration", GUILayout.Width(60));
                    Asset.Duration = EditorGUILayout.FloatField("", Asset.Duration, GUILayout.Width(70));
                }
            }

            EditorGUILayout.Space(5);

            if (Asset.DurationPolicy == EffectsDurationPolicy.Duration ||
                Asset.DurationPolicy == EffectsDurationPolicy.Infinite)
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Period", GUILayout.Width(100));
                    Asset.Period = EditorGUILayout.FloatField("", Asset.Period, GUILayout.Width(70));

                    if (Asset.Period > 0)
                    {
                        EditorGUILayout.LabelField("-->", GUILayout.Width(20));

                        EditorGUILayout.LabelField("Period GE", GUILayout.Width(60));
                        Asset.PeriodExecution =
                            (GameplayEffectAsset)EditorGUILayout.ObjectField("", Asset.PeriodExecution,
                                typeof(GameplayEffectAsset), false);
                    }
                }

            EditorGUILayout.EndVertical();
        }

        private void TagContainerGroup()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField("GameplayEffect Tags", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("  ", GUILayout.Width(10));

                EditorGUILayout.BeginVertical();
                foreach (var t in tagGroupAssets)
                {
                    t.OnGUI();
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void CueGroup()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField("Gameplay Cues", EditorStyles.boldLabel);

            if (Asset.DurationPolicy == EffectsDurationPolicy.Instant)
            {
                cueOnExecuteList.OnGUI();
            }
            else
            {
                cueOnAddList.OnGUI();
                EditorGUILayout.Space(5);
                cueOnRemoveList.OnGUI();
            }

            EditorGUILayout.EndVertical();
        }

        private void ModifierGroup()
        {
            modifierList.OnGUI();
        }

        private void Save()
        {
            // Save Tag
            Asset.AssetTags = tagGroupAssets[0].GetItemList().ToArray();
            Asset.GrantedTags = tagGroupAssets[1].GetItemList().ToArray();
            Asset.ApplicationRequiredTags = tagGroupAssets[2].GetItemList().ToArray();
            Asset.OngoingRequiredTags = tagGroupAssets[3].GetItemList().ToArray();
            Asset.RemoveGameplayEffectsWithTags = tagGroupAssets[4].GetItemList().ToArray();

            // Save Modifier
            Asset.Modifiers = modifierList.GetItemList().ToArray();

            // Save Cue
            Asset.CueOnExecute = cueOnExecuteList.GetItemList().ToArray();
            Asset.CueOnAdd = cueOnAddList.GetItemList().ToArray();
            Asset.CueOnRemove = cueOnRemoveList.GetItemList().ToArray();

            EditorUtility.SetDirty(Asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnEditModifier(int index, GameplayEffectModifier mod)
        {
            ModifierConfigEditor.OpenWindow(mod, modifier => modifierList.UpdateItem(index, modifier));
        }

        private float GetModifierElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 10;
        }

        private void OnModifierDrawGUI(Rect rect, GameplayEffectModifier mod, int index)
        {
            var attributeName = string.IsNullOrEmpty(mod.AttributeName) ? "None" : mod.AttributeName;
            var operation = mod.Operation.ToString();
            var value = mod.ModiferMagnitude;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, 500, EditorGUIUtility.singleLineHeight),
                $"Attribute :{attributeName} | Operation:{operation} | Value:{value}");

            var mmcType = "Empty!!!";
            if (mod.MMC != null)
            {
                var path = AssetDatabase.GetAssetPath(mod.MMC);
                mmcType = path.Split('/').Last();
            }
            var mmcInfo = $"MMC:{mmcType}";

            EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 5,
                500, EditorGUIUtility.singleLineHeight), mmcInfo);
        }

        private void ConfigErrorTip()
        {
            switch (Asset.DurationPolicy)
            {
                case EffectsDurationPolicy.None:
                {
                    using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                    {
                        EditorGUILayout.LabelField(
                            "<size=16><b><color=red>GameplayEffect DurationPolicy can't be None!!!!</color></b></size>",
                            new GUIStyle(GUI.skin.label) { richText = true });
                    }

                    break;
                }
                case EffectsDurationPolicy.Duration when Asset.Duration <= 0:
                {
                    using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                    {
                        EditorGUILayout.LabelField(
                            "<size=16><b><color=red>Durational GameplayEffect's Duration must larger than 0 !!!!</color></b></size>",
                            new GUIStyle(GUI.skin.label) { richText = true });
                    }

                    break;
                }
            }

            if ((Asset.DurationPolicy == EffectsDurationPolicy.Infinite ||
                 Asset.DurationPolicy == EffectsDurationPolicy.Duration) &&
                Asset.Period > 0 && Asset.PeriodExecution == null)
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField(
                        "<size=16><b><color=red>Periodic GameplayEffect's PeriodExecution can't be None !!!!</color></b></size>",
                        new GUIStyle(GUI.skin.label) { richText = true });
                }
        }
    }
}