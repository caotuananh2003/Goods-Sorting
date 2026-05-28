using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.UI
{
    [System.Serializable]
    public class NavigationButton
    {
        public Button button;

        public Image icon;

        public RectTransform rect;

        public GameObject contentPanel;
    }
}