

#if UNITY_EDITOR
namespace GAS.Editor
{
	using GAS.Runtime;
	using System.Collections.Generic;
	using Editor;
	using UnityEditor.TreeDataModel;
	using UnityEngine;
	
	[CreateAssetMenu (fileName = "GameplayTagsAsset", menuName = "GAS/GameplayTagsAsset ", order = 1)]
	public class GameplayTagsAsset : ScriptableObject
	{
		[SerializeField] List<GameplayTagTreeElement> gameplayTagTreeElements = new List<GameplayTagTreeElement>();

		internal List<GameplayTagTreeElement> GameplayTagTreeElements => gameplayTagTreeElements;
		
		[SerializeField] public List<GameplayTag> Tags = new List<GameplayTag>();


		public void CacheTags()
		{
			Tags.Clear();
			for (int i = 0; i < gameplayTagTreeElements.Count; i++)
			{
				TreeElement tag = gameplayTagTreeElements[i];
				if (tag.Depth == -1) continue;
				string tagName = tag.Name;
				while (tag.Parent.Depth >=0)
				{
					tagName = tag.Parent.Name + "." + tagName;
					tag = tag.Parent;
				}

				Tags.Add(new GameplayTag(tagName));
			}
		}
	}
}
#endif
