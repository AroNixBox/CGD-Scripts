using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [Serializable]
    public class HazardStrategy : IImpactStrategy {
        [InfoBox("<b><color=#FF0000>N</color><color=#FF7F00>I</color><color=#FFFF00>G</color><color=#00FF00>G</color><color=#0000FF>O</color><color=#8B00FF>L</color><color=#FF0000>A</color><color=#FF7F00>R</color><color=#FFFF00>S</color> <color=#00FF00>H</color><color=#0000FF>I</color><color=#8B00FF>E</color><color=#FF0000>R</color> <color=#FF7F00>D</color><color=#FFFF00>A</color><color=#00FF00>R</color><color=#0000FF>F</color><color=#8B00FF>S</color><color=#FF0000>T</color> <color=#FF7F00>D</color><color=#FFFF00>U</color></b>", InfoMessageType.Error)]
        public string NIGGOLARS = "NIGGOLARS";
        public ImpactResult OnImpact(Vector3 impactPosition) {
            throw new NotImplementedException("Niggolars muss arbeiten");
        }
    }
}