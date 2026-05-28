using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameTemplate.Managers.Scene;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Managers
{
    /// <summary>
    /// Enum định nghĩa các loại scene trong game.
    ///
    /// Thay vì dùng string:
    ///
    /// "MainMenu"
    /// "Game"
    ///
    /// ta dùng enum để:
    /// - tránh typo
    /// - dễ maintain
    /// - autocomplete tốt hơn
    /// </summary>
    public enum SceneType
    {
        Startup,
        MainMenu,
        GamePlay
    }

    /// <summary>
    /// Manager chịu trách nhiệm load scene.
    ///
    /// Đây là:
    /// - Scene Management Service
    /// - Scene Loader System
    ///
    /// Nhiệm vụ:
    /// - load scene
    /// - invoke event trước/sau khi load
    /// - quản lý flow chuyển scene
    ///
    /// Class này KHÔNG kế thừa MonoBehaviour.
    ///
    /// Nó được quản lý bởi:
    /// VContainer DI.
    /// </summary>
    public class SceneLoader
    {
        #region Variables
        /// <summary>
        /// Event gọi TRƯỚC khi load scene.
        ///
        /// Dùng cho:
        /// - loading screen
        /// - fade out
        /// - save data
        /// - stop input
        ///
        /// Ví dụ:
        ///
        /// LoadingScreen.Show()
        /// </summary>
        public static event Action OnBeforeSceneLoad = delegate { };

        /// <summary>
        /// Event gọi SAU khi scene load xong.
        ///
        /// Dùng cho:
        /// - fade in
        /// - hide loading screen
        /// - initialize UI
        /// - analytics
        /// </summary>
        public static event Action OnSceneLoaded = delegate { };

        /// <summary>
        /// ScriptableObject chứa danh sách scene.
        ///
        /// Ví dụ:
        /// MainMenu => "MainMenuScene"
        /// Game => "GameScene"
        /// </summary>
        private SceneData _sceneData;

        #endregion

        /// <summary>
        /// Inject SceneData từ DI container.
        ///
        /// VContainer sẽ:
        /// - resolve SceneData
        /// - tự gọi Construct()
        /// </summary>
        /// <param name="sceneData">
        /// ScriptableObject chứa scene config
        /// </param>
        [Inject]
        public void Construct(SceneData sceneData)
        {
            Debug.Log("Constructing SceneLoader");
            _sceneData = sceneData;
        }

        /// <summary>
        /// Load scene bằng enum SceneType. Đây là wrapper tiện lợi.
        /// 
        /// Ví dụ:
        /// LoadSceneByType(SceneType.Game);
        /// => tự động lấy: /// _sceneData.scenes[SceneType.Game] rồi load scene đó.
        /// </summary>
        /// <param name="sceneType">
        /// Loại scene cần load
        /// </param>
        /// <param name="mode">
        /// Kiểu load scene:
        ///
        /// Single:
        /// - unload scene cũ
        ///
        /// Additive:
        /// - load chồng scene
        /// </param>
        public UniTask LoadSceneByType(SceneType sceneType, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return LoadScene(new SceneLoadData(_sceneData.scenes[sceneType], mode, true, true)); // tạo dữ liệu load scene
        }

        // SceneLoader.Instance.LoadScene("MainMenu"); // Code cũ, không dùng tới, không nên dùng.

        /// <summary>
        /// Load scene async.
        ///
        /// Dùng UniTask thay vì Coroutine.
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// 1. Invoke OnBeforeSceneLoad
        ///
        /// 2. Load scene async
        ///
        /// 3. Chờ load hoàn tất
        ///
        /// 4. Invoke OnSceneLoaded
        ///
        /// ====================================================
        /// VÍ DỤ
        /// ====================================================
        ///
        /// UI:
        /// OnBeforeSceneLoad
        /// => show loading screen
        ///
        /// OnSceneLoaded
        /// => hide loading screen
        /// </summary>
        /// <param name="sceneLoadData">
        /// Dữ liệu load scene
        /// </param>
        public async UniTask LoadScene(SceneLoadData sceneLoadData)
        {
            OnBeforeSceneLoad?.Invoke(); // notify toàn hệ thống: chuẩn bị load scene
            //Debug.Log("OnBeforeSceneLoad");

            // Load using SceneManager. Load scene bất đồng bộ
            // await: chờ tới khi load xong
            await SceneManager.LoadSceneAsync(sceneLoadData._sceneName, sceneLoadData._sceneMode).ToUniTask();

            OnSceneLoaded?.Invoke(); // notify toàn hệ thống: scene đã load xong
            //Debug.Log("OnSceneLoaded");
        }
    }
}