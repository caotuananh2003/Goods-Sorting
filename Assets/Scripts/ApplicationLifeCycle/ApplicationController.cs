using Audio;
using Cysharp.Threading.Tasks;
using GameTemplate.ApplicationLifecycle;
using GameTemplate.Audio;
using GameTemplate.Gameplay.GameState;
using GameTemplate.Infrastructure;
using GameTemplate.Managers;
using GameTemplate.Managers.Scene;
using GameTemplate.ScriptableObjects;
using GameTemplate.UI.Currency;
using ScriptableObjects;
using System;
using TextMateSharp.Grammars;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.ApplicationLifeCycle
{
    /// <summary>
    /// Entry point của toàn bộ application.
    /// Nơi:
    /// - Khởi tạo Dependency Injection (DI)
    /// - Đăng ký các system, manager, data
    /// - Setup các cấu hình ban đầu
    /// - Load scene đầu tiên
    /// An entry point to the application, where we bind all the common dependencies to the root DI scope.
    /// </summary>
    public class ApplicationController : LifetimeScope
    {
        #region Variables

        // Dùng để quản lý các subscription (event listener)
        IDisposable m_Subscriptions;

        // Các ScriptableObject chứa data
        public AudioData audioData;
        public CurrencyData currencyData;
        public SceneData sceneData;
        public LevelDataHolder levelDataHolder;

        #endregion

        /// <summary>
        /// Hàm cấu hình DI container (Dependency Injection container)
        /// Được gọi tự động khi khởi tạo LifetimeScope
        /// </summary>
        protected override void Configure(IContainerBuilder builder) // IContainerBuilder là của Assets/Plugins/VContainer 
        {
            base.Configure(builder);

            // Đăng ký các instance data (ScriptableObject)
            builder.RegisterInstance(audioData);
            builder.RegisterInstance(currencyData);
            builder.RegisterInstance(sceneData);
            builder.RegisterInstance(levelDataHolder);

            // Đăng ký các manager dạng Singleton
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<CurrencyManager>(Lifetime.Singleton);
            builder.Register<SoundPlayer>(Lifetime.Singleton);
            builder.Register<LevelManager>(Lifetime.Singleton);

            // Game state global (không bị destroy khi đổi scene)
            builder.Register<PersistentGameState>(Lifetime.Singleton);

            // Đăng ký MessageChannel cho QuitApplicationMessage cho phép publish / subscribe event này
            builder.RegisterInstance(new MessageChannel<QuitApplicationMessage>()).AsImplementedInterfaces();
        }

        /// <summary>
        /// Hàm Start của Unity
        /// Được gọi khi scene bắt đầu
        /// </summary>
        public void Start()
        {
            // Lấy subscriber để lắng nghe QuitApplicationMessage
            var quitApplicationSub = Container.Resolve<ISubscriber<QuitApplicationMessage>>();

            // Nhóm các subscription lại để dễ dispose
            var subHandles = new DisposableGroup();

            // Subscribe vào event QuitApplicationMessage
            // Khi có event → gọi hàm QuitGame
            subHandles.Add(quitApplicationSub.Subscribe(QuitGame));
            m_Subscriptions = subHandles;

            // Giữ object này không bị destroy khi load scene mới
            DontDestroyOnLoad(gameObject);

            // Giới hạn FPS (Cái này có thể bỏ qua)
            Application.targetFrameRate = 60;

            // Load scene Main Menu đầu tiên
            //SceneManager.LoadScene(sceneData.scenes[SceneType.MainMenu]);
            //_sceneLoader.LoadSceneByType(SceneType.MainMenu).Forget();
            Container.Resolve<SceneLoader>().LoadSceneByType(SceneType.MainMenu).Forget();
        }

        /// <summary>
        /// Khi object bị destroy
        /// → cần unsubscribe tất cả event để tránh memory leak
        /// </summary>
        protected override void OnDestroy()
        {
            if (m_Subscriptions != null)
            {
                m_Subscriptions.Dispose();
            }

            base.OnDestroy();
        }

        /// <summary>
        /// Hàm xử lý khi nhận event QuitApplicationMessage
        /// </summary>
        private void QuitGame(QuitApplicationMessage msg)
        {
#if UNITY_EDITOR
            // Nếu đang chạy trong Unity Editor → dừng play mode
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // Nếu build game → thoát ứng dụng
            Application.Quit();
#endif
        }
    }
}