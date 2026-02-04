using System;
using Core.Runtime.Service;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class ScreenDamageController : MonoBehaviour {
        [SerializeField, Required] Animator animator;
        [SerializeField, Required] AudioSource audioSource;
        [SerializeField, EnumToggleButtons] EInitSource initSource;
        static readonly int PulsateState = Animator.StringToHash("Pulsate");

        enum EInitSource {
            Start,
            Manual
        }

        void Awake() {
            ServiceLocator.Register(this);
        }

        void OnDisable() {
            ServiceLocator.Unregister<ScreenDamageController>();
        }

        void Start() {
            if (initSource != EInitSource.Start) return;
            PulsateAsync().Forget();
        }
        // ASDHUDUZHIHUIASDHUISHUIASdHUI
        // OXO
        public async UniTask PulsateAsync() {
            animator.Play(PulsateState);
            await UniTask.Delay(250);
            audioSource.Play();
    
            // Wait for state to stop playing
            await UniTask.WaitUntil(() => {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0);
            });
        }
    }
}
