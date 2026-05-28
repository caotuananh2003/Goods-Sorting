using System;
using System.Collections;
using GameTemplate.Managers;
using UnityEngine;

namespace GameTemplate
{
    /// <summary>
    /// Controller quản lý Loading Screen toàn game.
    ///
    /// Nhiệm vụ:
    /// - hiện loading screen trước khi load scene
    /// - fade out loading screen sau khi scene load xong
    /// - block raycast khi đang loading
    ///
    /// ====================================================
    /// FLOW
    /// ====================================================
    ///
    /// SceneLoader.LoadScene()
    /// ↓
    /// SceneLoader.OnBeforeSceneLoad
    /// ↓
    /// OpenLoadingScreen()
    /// ↓
    /// loading screen hiện lên
    ///
    /// Scene load xong
    /// ↓
    /// SceneLoader.OnSceneLoaded
    /// ↓
    /// CloseLoadingScreen()
    /// ↓
    /// fade out
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// CanvasGroup dùng để:
        /// - fade alpha
        /// - block raycast
        /// - enable/disable tương tác UI
        ///
        /// Alpha:
        /// 1 = hiện
        /// 0 = ẩn
        /// </summary>
        [SerializeField]
        CanvasGroup m_CanvasGroup;
        
        [SerializeField]
        float m_DelayBeforeFadeOut = 1.5f; // Delay trước khi bắt đầu fade out để tránh loading screen biến mất quá nhanh. Trước đây đặt là 0.5

        [SerializeField]
        float m_FadeOutDuration = 1f; // Thời gian fade out. Trước đây là 0.1

        bool m_LoadingScreenRunning; // Loading screen đang active hay không.

        /// <summary>
        /// Coroutine fade out hiện tại.
        ///
        /// Dùng để:
        /// - stop coroutine cũ
        /// - tránh chạy nhiều fade cùng lúc
        /// </summary>
        Coroutine m_FadeOutCoroutine; 

        #endregion
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject); // Không destroy khi đổi scene
            SceneLoader.OnBeforeSceneLoad += OpenLoadingScreen; // Subscribe event scene loading. SceneLoader nằm ở Assets/Scripts/Managers
            SceneLoader.OnSceneLoaded += CloseLoadingScreen; // Subscribe event scene loaded
        }

        private void OnDestroy() // Cleanup event subscription
        {
            SceneLoader.OnBeforeSceneLoad -= OpenLoadingScreen;
            SceneLoader.OnSceneLoaded -= CloseLoadingScreen;
        }

        public void OpenLoadingScreen() // Hiện loading screen. Được gọi trước khi load scene.
        {
            SetCanvasVisibility(true); // Hiện canvas
            m_LoadingScreenRunning = true; // Đánh dấu đang loading
            if (m_LoadingScreenRunning) // Nếu đang có fade out coroutine thì dừng nó lại
            {
                if (m_FadeOutCoroutine != null)
                {
                    Debug.Log("start loading screen");
                    StopCoroutine(m_FadeOutCoroutine);
                }
            }
        }
        
        public void CloseLoadingScreen() // Đóng loading screen. Không tắt ngay lập tức mà chạy fade out
        {
            if (m_LoadingScreenRunning)
            {
                if (m_FadeOutCoroutine != null) // stop fade cũ nếu có
                {
                    //Debug.Log("stop loading screen");
                    StopCoroutine(m_FadeOutCoroutine);
                }
                m_FadeOutCoroutine = StartCoroutine(FadeOutCoroutine()); // start fade out mới
            }
        }

        /// <summary>
        /// Set trạng thái hiển thị của canvas.
        ///
        /// visible = true:
        /// - alpha = 1
        /// - block raycast
        ///
        /// visible = false:
        /// - alpha = 0
        /// - không block raycast
        /// </summary>
        void SetCanvasVisibility(bool visible)
        {
            m_CanvasGroup.alpha = visible ? 1 : 0;
            m_CanvasGroup.blocksRaycasts = visible; // block click khi loading
        }

        /// <summary>
        /// Coroutine fade out loading screen.
        ///
        /// FLOW:
        ///
        /// wait delay
        /// ↓
        /// lerp alpha từ 1 -> 0
        /// ↓
        /// ẩn canvas
        /// </summary>
        IEnumerator FadeOutCoroutine()
        {
            yield return new WaitForSeconds(m_DelayBeforeFadeOut); // chờ trước khi fade
            m_LoadingScreenRunning = false;

            float currentTime = 0;
            while (currentTime < m_FadeOutDuration) // fade dần alpha
            {
                m_CanvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / m_FadeOutDuration);
                yield return null;
                currentTime += Time.deltaTime;
            }

            SetCanvasVisibility(false); // đảm bảo canvas bị ẩn hoàn toàn
        }
    }
}