using GameTemplate.Gameplay.GameplayObjects.Audio;
using GameTemplate.Gameplay.UI;
using GameTemplate.Managers.Scene;
using GameTemplate.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Gameplay.GameState
{
    /// <summary>
    /// State dành cho Main Menu.
    ///
    /// Đây là "trạng thái game" khi người chơi đang ở menu chính.
    ///
    /// Ví dụ:
    /// - Màn hình Play
    /// - Settings
    /// - Shop
    /// - Quit
    ///
    /// Dù chưa gameplay thật sự, scene menu vẫn cần một GameState.
    ///
    /// Lý do:
    /// - Hệ thống GameStateBehaviour yêu cầu
    ///   mỗi scene phải có state
    ///
    /// - Quản lý lifecycle đồng nhất
    ///
    /// - Tạo DI scope riêng cho MainMenu
    ///
    /// - Đảm bảo chỉ có 1 state active
    /// </summary>
    public class MainMenuState : GameStateBehaviour
    {
        #region Variables

        /// <summary>
        /// Override state hiện tại.
        ///
        /// Class này đại diện cho:
        /// GameState.MainMenu
        ///
        /// Hệ thống sẽ dùng giá trị này để:
        /// - xác định loại state
        /// - so sánh state cũ/mới
        /// - quản lý persist state
        /// </summary>
        public override GameState ActiveState => GameState.MainMenu;

        #endregion

        /// <summary>
        /// Configure DI Container cho MainMenuState.
        ///
        /// Đây là nơi register:
        /// - component
        /// - manager
        /// - service
        /// thuộc riêng Main Menu scene.
        ///
        /// Hàm này được VContainer gọi tự động.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            // Tìm UICanvas trong scene rồi register vào DI Container.
            //
            // Sau khi register: [Inject] UICanvas sẽ hoạt động được.
            //
            // RegisterComponentInHierarchy:
            // - scan toàn scene
            // - tìm component cùng type
            // - register instance đó
            builder.RegisterComponentInHierarchy<UICanvas>(); 

            // Các dòng sau là tự viết thêm để thay cho thao tác kéo trong inspector.
            builder.RegisterComponentInHierarchy<MainMenuMusicStarter>();
            builder.RegisterComponentInHierarchy<UISettingsCanvas>();
            builder.RegisterComponentInHierarchy<UISettingsPanel>();
            //builder.RegisterComponentInHierarchy<UIButtonSound>();
        }

        /// <summary>
        /// Cleanup khi state bị destroy.
        ///
        /// Hiện tại chưa có logic riêng, nhưng override sẵn để:
        /// - dễ mở rộng sau này
        /// - thêm cleanup nếu cần
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}