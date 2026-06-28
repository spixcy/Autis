using UnityEngine;
using TMPro;

namespace Autis.UI
{
    public class InteractionUI : MonoBehaviour
    {
        public static InteractionUI Instance { get; private set; }

        public GameObject crosshair;
        public TextMeshProUGUI promptText;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (promptText != null) promptText.gameObject.SetActive(false);
            if (crosshair != null) crosshair.SetActive(true);
        }

        public void ShowPrompt(string message)
        {
            if (promptText != null)
            {
                promptText.text = message;
                promptText.gameObject.SetActive(true);
            }
        }

        public void HidePrompt()
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
}
