using System;
using System.Collections.Generic;
using Core.Runtime.Authority;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime.Data {
    [CreateAssetMenu(menuName = "Player/CharacterDatabase")]
    public class CharacterDatabase : ScriptableObject {
        [Serializable]
        public struct CharacterEntry {
            [PreviewField(Alignment = ObjectFieldAlignment.Left, Height = 50)]
            public Sprite icon;
            
            [Required, AssetsOnly]
            public AuthorityEntity prefab;
        }

        [SerializeField, TableList]
        List<CharacterEntry> characters;

        public int Count => characters?.Count ?? 0;

        public Sprite GetIconAtIndex(int index) {
            if (characters == null || index < 0 || index >= characters.Count) return null;
            return characters[index].icon;
        }
        
        public AuthorityEntity GetPrefabForIcon(Sprite icon) {
            foreach (var entry in characters) {
                if (entry.icon == icon) return entry.prefab;
            }
            return null;
        }

        public AuthorityEntity GetPrefabAtIndex(int index) {
            if (characters == null || index < 0 || index >= characters.Count) return null;
            return characters[index].prefab;
        }
    }
}

