using GameTemplate.Gameplay.GameState;
using GameTemplate.Managers;
using System;
using UnityEngine;
using VContainer;

namespace GameTemplate.Gameplay.UI
{
    /// <summary>
    /// Controls the special Canvas that has the settings icon and the settings window.
    /// The window itself is controlled by UISettingsPanel; the button is controlled here.
    /// </summary>
    public class UIPauseCanvas : MonoBehaviour
    {
        private SceneLoader _SceneLoader;
        private GameSceneState _gameSceneState;

        [Inject]
        public void Construct(SceneLoader sceneLoader, GameSceneState gameSceneState)
        {
            _SceneLoader = sceneLoader;
            _gameSceneState = gameSceneState;
        }

        [SerializeField]
        private GameObject m_PausePanelRoot; // Root object của popup settings.
        [SerializeField]
        private GameObject m_ConfirmGiveUpPopup;
        void Awake()
        {
            DisablePausePanels(); // Đảm bảo panel bị ẩn khi start game
            DisableConfirmPanel();
        }

        private void DisableConfirmPanel()
        {
            if (m_ConfirmGiveUpPopup != null)
            {
                m_ConfirmGiveUpPopup.SetActive(false);
            }
        }

        void DisablePausePanels() // Ẩn toàn bộ popup panel.
        {
            m_PausePanelRoot.SetActive(false);

            ResumeGame();
        }

        /// <summary>
        /// Được gọi trực tiếp bởi Button Settings trong UI prefab.
        ///
        /// Toggle popup:
        /// - đang mở => đóng
        /// - đang đóng => mở
        /// </summary
        public void OnClickPauseButton()
        {
            Debug.Log("Before: " + m_PausePanelRoot.activeSelf);

            m_PausePanelRoot.SetActive(true);

            Debug.Log("After: " + m_PausePanelRoot.activeSelf);
            PauseGame();
        }

        private void PauseGame()
        {
            //Time.timeScale = 0f;
            _gameSceneState.SetPause(true);
        }

        private void ResumeGame()
        {
            //Time.timeScale = 1f;
            _gameSceneState.SetPause(false);
        }
        /// <summary>
        /// Được gọi trực tiếp bởi nút Quit/Close trong UI prefab.
        /// Chỉ đóng popup settings.
        /// </summary>
        public void OnClickResumeButton()
        {
            DisablePausePanels();
            Time.timeScale = 1f;
        }

        public void OnClickGiveupButton()
        {
            // Chưa làm tính năng
            //m_PausePanelRoot.SetActive(false);
            m_ConfirmGiveUpPopup.SetActive(true);
        }

        public void OnClickCancelGiveUpButton()
        {
            m_ConfirmGiveUpPopup.SetActive(false);
        }

        public void OnClickConfirmGiveUpButton()
        {
            Time.timeScale = 1f;

            // Load MainMenu scene
            _SceneLoader.LoadSceneByType(SceneType.MainMenu);
        }

        private void OnDestroy()
        {
            // Safety restore
            Time.timeScale = 1f;
        }
    }
}
