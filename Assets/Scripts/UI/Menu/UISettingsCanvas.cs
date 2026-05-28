using System;
using UnityEngine;

namespace GameTemplate.Gameplay.UI
{
    /// <summary>
    /// Controls the special Canvas that has the settings icon and the settings window.
    /// The window itself is controlled by UISettingsPanel; the button is controlled here.
    /// </summary>
    public class UISettingsCanvas : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_SettingsPanelRoot; // Root object của popup settings.

        void Awake()
        {
            DisablePanels(); // Đảm bảo panel bị ẩn khi start game
        }

        void DisablePanels() // Ẩn toàn bộ popup panel.
        {
            m_SettingsPanelRoot.SetActive(false);
        }

        /// <summary>
        /// Được gọi trực tiếp bởi Button Settings trong UI prefab.
        ///
        /// Toggle popup:
        /// - đang mở => đóng
        /// - đang đóng => mở
        /// </summary
        public void OnClickSettingsButton()
        {
            m_SettingsPanelRoot.SetActive(true);
        }

        /// <summary>
        /// Được gọi trực tiếp bởi nút Quit/Close trong UI prefab.
        /// Chỉ đóng popup settings.
        /// </summary>
        public void OnClickQuitButton()
        {
            DisablePanels();
        }
    }
}
