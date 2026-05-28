using _Game.Scripts.Timer;
using AssetKits.ParticleImage;
using Cysharp.Threading.Tasks;
using GameTemplate.Audio;
using GameTemplate.Events;
using GameTemplate.Gameplay.UI;
using GameTemplate.Managers;
using GameTemplate.Managers.Scene;
using GameTemplate.UI;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using GameTemplate._Game.Scripts.Match;

namespace GameTemplate.Gameplay.GameState
{
    /// <summary>
    /// State chính của gameplay scene.
    ///
    /// Đây là "trung tâm điều khiển gameplay".
    ///
    /// Nó quản lý:
    /// - Spawn level
    /// - Start timer
    /// - Win/Lose
    /// - Currency reward
    /// - UI gameplay
    /// - Chuyển scene
    /// - Audio end game
    /// </summary>
    public class GameSceneState : GameStateBehaviour
    {
        #region Variables
        public override GameState ActiveState => GameState.Game; // Xác định đây là gameplay state
        private bool _isPaused;

        public bool IsPaused => _isPaused;
        /// <summary>
        /// Event được gọi khi player chạm lần đầu tiên.
        ///
        /// Dùng để:
        /// - Start timer
        /// - Trigger gameplay start
        /// </summary>
        public static Action OnFirstTouch;
        public static Action<bool> OnPauseChanged;

        [SerializeField] private Transform _levelPrefabParent; // Parent dùng để chứa level prefab được spawn ra
        [SerializeField] private UIGameCanvas _uiGameCanvas; // Gameplay UI canvas
        [SerializeField] private EarningsUI _earningsUI; // UI tính toán phần thưởng cuối game
        [SerializeField] private TimerController _timerController; // Timer gameplay
        [SerializeField] private ParticleImage _winParticleImage; // Particle effect khi thắng
        [SerializeField] private GameObject _allLinesFilledText; // Text hiển thị khi toàn bộ line được fill

        // Wait time constants for switching to post game after the game is won or lost
        private const float k_WinDelay = 2.0f; // Delay trước khi hiện win UI
        private const float k_LoseDelay = 2.0f; // Delay trước khi hiện lose UI

        #endregion

        #region Injections

        [Inject] PersistentGameState m_PersistentGameState; // Lưu trạng thái win/lose hiện tại
        [Inject] LevelManager _levelManager; // Quản lý level
        [Inject] CurrencyManager _currencyManager; // Quản lý currency
        [Inject] SceneLoader m_SceneLoader; // Load scene
        [Inject] SoundPlayer m_SoundPlayer; // Phát âm thanh

        #endregion

        protected override void Awake()
        {
            base.Awake();

            // Subscribe event chạm đầu tiên
            // Khi player chạm lần đầu -> StartTimer()
            OnFirstTouch += StartTimer;
        }

        protected override void Start() // Khởi tạo gameplay scene
        {
            base.Start();

            m_PersistentGameState.Reset(); // Reset trạng thái win/lose

            _levelManager.SpawnLevel(_levelPrefabParent); // Spawn level prefab vào scene

            _uiGameCanvas.Initialize(_levelManager.UILevelId); // Khởi tạo gameplay UI

            LevelPrefab.OnGameFinished += OnGameFinished; // Subscribe sự kiện game finish
            TimerController.OnTimesUp += OnGameFinished; // Subscribe sự kiện hết giờ
        }

        protected override void Configure(IContainerBuilder builder) // Đăng ký component scene vào DI Container
        {
            base.Configure(builder);

            builder.RegisterComponentInHierarchy<ComboController>(); // Register ComboController trong hierarchy
            builder.RegisterComponentInHierarchy<StarController>(); // Register StarController trong hierarchy
            builder.RegisterComponentInHierarchy<TimerController>(); // Register TimerController trong hierarchy
            builder.RegisterComponentInHierarchy<UISettingsPanel>();
            builder.RegisterComponentInHierarchy<UIPauseCanvas>();
            builder.RegisterComponentInHierarchy<UIButtonSound>();
            builder.RegisterComponentInHierarchy<GameplayAudioController>();
        }

        protected override void OnDestroy() // Cleanup event khi object bị destroy
        {
            base.OnDestroy();
            LevelPrefab.OnGameFinished -= OnGameFinished; // Unsubscribe event
            TimerController.OnTimesUp -= OnGameFinished; // Unsubscribe event
            OnFirstTouch = null; // Clear static event
        }

        public void StartTimer() // Start gameplay timer
        {
            OnFirstTouch -= StartTimer; // Chỉ start timer 1 lần

            if (_levelManager.LevelId == 0) // Level đầu tiên chúng ta không dùng timer
                return;

            _timerController.StartTimer(); // Start timer
        }

        /// <summary>
        /// Được gọi khi game kết thúc
        ///
        /// Version có thêm thông tin:
        /// "toàn bộ line đã được fill hay chưa"
        /// </summary>
        public void OnGameFinished(bool isWin, bool isAllLinesFilled) // Được gọi từ event LevelPrefab.OnGameFinished
        {
            Debug.Log("GameSceneState.OnGameFinished 2 parameter");
            if (isAllLinesFilled)// Nếu tất cả line đã fill thì hiện text đặc biệt
            {
                _allLinesFilledText.SetActive(true);
            }

            // start the coroutine (Start async game over flow)
            _ = CoroGameOver(isWin ? k_WinDelay : k_LoseDelay, isWin);
        }

        public void OnGameFinished(bool isWin) // Overload đơn giản hơn, được gọi từ event TimerController.OnTimeUp
        {
            Debug.Log("GameSceneState.OnGameFinished đơn giản");
            // start the coroutine (Start async game over flow)
            _ = CoroGameOver(isWin ? k_WinDelay : k_LoseDelay, isWin);
        }

        async UniTaskVoid CoroGameOver(float wait, bool gameWon) // Coroutine xử lý toàn bộ flow game over
        {
            Debug.Log("GameSceneState.CoroGameOver");
            m_PersistentGameState.SetWinState(gameWon ? WinState.Win : WinState.Loss); // Lưu trạng thái thắng/thua
            //if (gameWon) _winParticleImage.Play(); // Nếu thắng -> play particle

            // Tắt timer music ngay khi game kết thúc
            m_SoundPlayer.StopThemeMusic();

            Debug.Log("GameSceneState.CoroGameOver -> m_PersistentGameState = " + m_PersistentGameState.WinState);
            //TODO change this game to game
            // wait for game animations to finish
            await UniTask.Delay((int)(wait * 1000)); // Chờ animation/effect hoàn thành

            CurrencyArgs args = _earningsUI.SetEarnings(); // Tính toán phần thưởng

            _currencyManager.EarnCurrency(args); // Cộng currency
            _uiGameCanvas.GameFinished(m_PersistentGameState.WinState); // Hiện win/lose UI

            Debug.Log("_levelManager.LevelId: " + _levelManager.LevelId);

            if (m_PersistentGameState.WinState == WinState.Win) // Nếu thắng
            {
                Debug.Log("PlayWinSound + SetNextLevel");
                m_SoundPlayer.PlayWinSound();
                _levelManager.SetNextLevel(); // Set level tiếp theo
                //ReloadScene();
            }
            else // Nếu thua
            {
                Debug.Log("PlayLoseSound");
                m_SoundPlayer.PlayLoseSound();
            }
        }

        public void NextButtonClick() // Button next level
        {
            //_levelManager.SetNextLevel();
            ReloadScene();

            //Debug.Log("_levelManager.LevelId: " + _levelManager.LevelId);
            //if (_levelManager.LevelId < 2) // Nếu chưa hết level
            //{
            //    m_SceneLoader.LoadSceneByType(SceneType.GamePlay); // Load gameplay scene tiếp
            //}
            //else
            //{
            //    m_SceneLoader.LoadSceneByType(SceneType.MainMenu); // Quay về menu
            //}
        }

        public void MainMenuButtonClick()
        {
            m_SceneLoader.LoadSceneByType(SceneType.MainMenu); // Quay về menu
        }
        public void ReloadScene() // Button retry level
        {
            m_SceneLoader.LoadSceneByType(SceneType.GamePlay); // Reload gameplay scene
        }

        public void SetPause(bool pause)
        {
            _isPaused = pause;

            Time.timeScale = pause ? 0f : 1f;

            OnPauseChanged?.Invoke(pause);

            //if (pause)
            //{
            //    m_SoundPlayer.PauseMusic();
            //}
            //else
            //{
            //    m_SoundPlayer.ResumeMusic();
            //}
        }
#if UNITY_EDITOR
        private void Update() // Debug shortcut chỉ dùng trong Editor
        {
            if (Input.GetKeyDown(KeyCode.N)) // N = next level
            {
                _levelManager.SetNextLevel();
                ReloadScene();
            }

            if (Input.GetKeyDown(KeyCode.P)) // P = previous level
            {
                _levelManager.SetPreviousLevel();
                ReloadScene();
            }
        }
#endif
    }
}