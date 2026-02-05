using Sirenix.OdinInspector;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Runtime {
    public class LoadingScreenController : MonoBehaviour {
        [SerializeField, Required] LoadingScreenView view;

        bool IsSceneInBuild(string sceneName) {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, sceneName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public async UniTaskVoid LoadSceneAsync(string sceneName) {
            const float minTotalDuration = 2.0f;
            const float minFadeDuration = 0.5f;
            if (!IsSceneInBuild(sceneName)) {
                Debug.LogError($"Scene `{sceneName}` not found in Build Settings.");
                return;
            }

            view.Show();
            view.SetProgress(0f);

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            float startTime = Time.realtimeSinceStartup;
            float lastProgress = 0f;
            
            while (op.progress < 0.9f) {
                lastProgress = Mathf.Clamp01(op.progress / 0.9f);
                view.SetProgress(lastProgress);
                await UniTask.Yield();
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            float fakeTime = Mathf.Max(minFadeDuration, minTotalDuration - elapsed);

            float t = 0f;
            while (t < fakeTime) {
                t += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(t / fakeTime);
                float eased = Mathf.SmoothStep(0f, 1f, normalized);
                float p = Mathf.Lerp(lastProgress, 1f, eased);
                view.SetProgress(p);
                await UniTask.Yield();
            }

            view.SetProgress(1f);

            op.allowSceneActivation = true;
        }


        public void Hide() => view.Hide();
    }
}