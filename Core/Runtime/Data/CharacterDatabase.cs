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
            [SerializeField, Required] Sprite icon;
            public Sprite Icon => icon;
            
            [PreviewField(Alignment = ObjectFieldAlignment.Left, Height = 50)]
            [SerializeField, Required, AssetsOnly] GameObject hatPrefab;
            public GameObject HatPrefab => hatPrefab;
        }

        [SerializeField, Required, AssetsOnly] AuthorityEntity playerPrefab;
        public AuthorityEntity PlayerPrefab => playerPrefab;
        
        [SerializeField, TableList]
        List<CharacterEntry> characters;

        public int Count => characters?.Count ?? 0;

        public Sprite GetIconAtIndex(int index) {
            if (characters == null || index < 0 || index >= characters.Count) return null;
            return characters[index].Icon;
        }
        
        public GameObject GetHatForIcon(Sprite icon) {
            foreach (var entry in characters) {
                if (entry.Icon == icon) return entry.HatPrefab;
            }
            return null;
        }
    }
}
