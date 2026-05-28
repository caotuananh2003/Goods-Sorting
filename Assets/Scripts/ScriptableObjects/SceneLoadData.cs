using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameTemplate.Managers.Scene
{
    /// <summary>
    /// Struct chứa dữ liệu cần thiết để load scene. Đây là:
    /// - Data Container
    /// - Scene Load Config
    /// - DTO (Data Transfer Object)
    ///
    /// =====================================================
    /// TẠI SAO CẦN STRUCT NÀY?
    /// =====================================================
    ///
    /// Thay vì:
    /// LoadScene(
    ///     "GameScene",
    ///     LoadSceneMode.Additive,
    ///     true,
    ///     true
    /// );
    /// => khó đọc
    ///
    /// Ta gom toàn bộ thành SceneLoadData giúp:
    /// - code sạch hơn
    /// - dễ mở rộng
    /// - dễ truyền dữ liệu
    /// - tránh truyền sai parameter
    ///
    /// =====================================================
    /// STRUCT
    /// =====================================================
    ///
    /// Dùng struct vì:
    /// - dữ liệu nhỏ
    /// - lightweight
    /// - không cần inheritance
    /// - không cần heap allocation
    ///
    /// Đây là value type.
    /// </summary>
    public struct SceneLoadData
    {
        public string _sceneName; // Tên scene cần load.

        public LoadSceneMode   _sceneMode; // Chế độ load scene.

        public bool   _activateLoadingCanvas; // Có bật loading canvas không. Ví dụ:, loading screen, fade panel, spinner...

        /// <summary>
        /// Scene load xong có set thành active scene không.
        ///
        /// Unity có thể load nhiều scene cùng lúc.
        ///
        /// Active Scene là scene:
        /// - instantiate mặc định
        /// - lighting mặc định
        /// - scene gameplay chính
        ///
        /// Hiện tại project chưa dùng.
        /// </summary>
        public bool   _setActiveScene;

        public SceneLoadData(string lastLoadedLevelScene, LoadSceneMode additive, bool activeLoadingCanvas, bool setActiveScene) // Constructor khởi tạo dữ liệu load scene.
        {
            _sceneName = lastLoadedLevelScene;
            _sceneMode = additive;
            _activateLoadingCanvas = activeLoadingCanvas;
            _setActiveScene = setActiveScene;
        }
    }
}