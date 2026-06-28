using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Autis.UI
{
    public class HintUI : MonoBehaviour
    {
        private static HintUI _instance;
        private TextMeshProUGUI _hintText;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                CreateUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void CreateUI()
        {
            GameObject canvasObj = new GameObject("HintCanvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject textObj = new GameObject("HintText");
            textObj.transform.SetParent(canvasObj.transform, false);
            _hintText = textObj.AddComponent<TextMeshProUGUI>();
            _hintText.alignment = TextAlignmentOptions.Center;
            _hintText.fontSize = 36;
            _hintText.color = Color.white;
            _hintText.fontStyle = FontStyles.Bold;

            var outline = textObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            RectTransform rt = _hintText.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 150); // 150px from bottom
            rt.sizeDelta = new Vector2(800, 100);

            _canvasGroup = textObj.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        public static void Show(string message)
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("HintUIManager");
                DontDestroyOnLoad(obj);
                _instance = obj.AddComponent<HintUI>();
            }

            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.ShowCoroutine(message));
        }

        private IEnumerator ShowCoroutine(string message)
        {
            _hintText.text = message;

            // Fade in
            float t = 0;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                _canvasGroup.alpha = t / 0.1f;
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSeconds(1.5f);

            // Fade out
            t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                _canvasGroup.alpha = 1f - (t / 0.5f);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }
    }
}
