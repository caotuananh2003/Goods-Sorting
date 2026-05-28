using UnityEngine;
using VContainer.Unity;

namespace GameTemplate.Gameplay.GameState
{
    /// <summary>
    /// Enum định nghĩa các trạng thái game.
    ///
    /// MainMenu:
    ///     Trạng thái menu chính
    ///
    /// Game:
    ///     Trạng thái gameplay
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Game,
        Pause,
        Win,
        Lose
    }

    /// <summary>
    /// Base class đại diện cho một "Game State".
    ///
    /// Mỗi scene sẽ có một GameState riêng:
    ///
    /// Ví dụ:
    /// - MainMenuState
    /// - GameSceneState
    ///
    /// Chức năng chính:
    /// - Quản lý lifecycle của state
    /// - Quản lý DI scope riêng cho scene
    /// - Đảm bảo chỉ có 1 state active
    /// - Hỗ trợ persist state qua nhiều scene
    /// </summary>
    ///
    /// <remarks>
    /// Lưu ý quan trọng:
    ///
    /// Mỗi scene PHẢI có GameState object.
    ///
    /// Nếu không:
    /// - Persisting state cũ có thể không bị destroy
    /// - Gây duplicate state
    /// - Gây memory leak
    /// - Gây logic lỗi
    /// </remarks>
    public abstract class GameStateBehaviour : LifetimeScope
    {
        /// <summary>
        /// State này có persist (Tồn tại) qua các scene không?
        ///
        /// false:
        /// - destroy khi đổi scene
        ///
        /// true:
        /// - DontDestroyOnLoad
        /// - tồn tại xuyên scene
        /// </summary>
        public virtual bool Persists // Hàm có thể được override
        {
            get { return false; }
        }

        /// <summary>
        /// State hiện tại thuộc loại nào.
        ///
        /// class kế thừa class GameStateBehaviour phải override.
        ///
        /// Ví dụ:
        /// return GameState.MainMenu;
        /// </summary>
        public abstract GameState ActiveState { get; }

        /// <summary>
        /// Static reference tới GameObject của state đang active.
        ///
        /// static:
        /// - dùng chung toàn game
        /// - chỉ tồn tại 1 bản
        ///
        /// Dùng để đảm bảo: "chỉ có 1 GameState active"
        /// </summary>
        private static GameObject s_ActiveStateGO;

        protected override void Awake()
        {
            // Awake của LifetimeScope
            // setup DI container
            base.Awake();

            // Nếu có parent DI scope
            if (Parent != null)
            {
                // Inject dependency vào object hiện tại
                //
                // Ví dụ:
                // [Inject] LevelManager sẽ được resolve tại đây
                Parent.Container.Inject(this);
            }
        }

        /// <summary>
        /// Start chạy sau Awake.
        ///
        /// Đây là nơi quản lý:
        /// - active state
        /// - destroy state cũ
        /// - persist logic
        /// </summary>
        protected virtual void Start()
        {
            if (s_ActiveStateGO != null) // Nếu đã có state active trước đó
            {
                if (s_ActiveStateGO == gameObject) // Nếu object hiện tại chính là active state thì return
                {
                    //nothing to do here, if we're already the active state object.
                    return;
                }

                //on the host, this might return either the client or server version, but it doesn't matter which;
                //we are only curious about its type, and its persist state.

                var previousState = s_ActiveStateGO.GetComponent<GameStateBehaviour>(); // Lấy component GameStateBehaviour của state cũ

                if (previousState.Persists && previousState.ActiveState == ActiveState) // Nếu state cũ: persist/cùng loại state
                {
                    Destroy(gameObject); // Destroy state mới Vì state cũ đang tồn tại rồi và được giữ xuyên scene
                    return;
                }

                Destroy(s_ActiveStateGO); // Nếu state cũ không HOẶC khác loại => destroy state cũ
            }

            s_ActiveStateGO = gameObject; // Set object hiện tại thành active state mới
            if (Persists) // Nếu state này persist
            {
                DontDestroyOnLoad(gameObject); // Giữ object khi đổi scene
            }
        }

        protected override void OnDestroy() // Cleanup khi object bị destroy
        {
            if (!Persists) // Nếu state không persist
            {
                s_ActiveStateGO = null; // Clear active state reference
            }
        }
    }
}
