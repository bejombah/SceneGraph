using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SceneGraph
{
    public class LoadingUI : MonoBehaviour
    {
        public static LoadingUI Instance;
        [SerializeField] Image _loadingImage;
        float duration = 0.2f;

        Coroutine fadeIn, fadeOut;
        
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                Show();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(Fade(1));
        }

        public void Show()
        {
            StopAllCoroutines();
            StartCoroutine(Fade(0));
        }

        IEnumerator Fade(float targetAlpha)
        {
            Color color = _loadingImage.color;
            float startAlpha = color.a;

            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
            {
                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                _loadingImage.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            _loadingImage.color = color;
        }
    }
}
