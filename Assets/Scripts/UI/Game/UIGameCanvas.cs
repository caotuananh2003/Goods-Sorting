using DG.Tweening;
using GameTemplate.Gameplay.GameState;
using UnityEngine;

namespace GameTemplate.UI
{
    /// <summary>
    /// UI Canvas chính của gameplay scene.
    ///
    /// Quản lý:
    /// - Top HUD
    /// - Win Panel
    /// - Lose Panel
    /// - level text
    ///
    /// ====================================================
    /// VAI TRÒ
    /// ====================================================
    ///
    /// Đây là class điều khiển:
    /// - trạng thái UI gameplay
    /// - hiển thị kết quả trận đấu
    /// - animation mở panel
    ///
    /// Nó KHÔNG xử lý gameplay logic.
    ///
    /// Chỉ chịu trách nhiệm:
    /// "gameplay state đã xảy ra"
    /// => cập nhật UI tương ứng.
    ///
    /// ====================================================
    /// GAME FLOW
    /// ====================================================
    ///
    /// Gameplay scene load
    /// ↓
    /// Initialize(levelId)
    /// ↓
    /// set text level
    /// ↓
    /// gameplay diễn ra
    /// ↓
    /// GameSceneState phát hiện WIN/LOSE
    /// ↓
    /// gọi:
    /// GameFinished()
    /// ↓
    /// mở WinPanel hoặc LosePanel
    /// </summary>
    public class UIGameCanvas : MonoBehaviour
    {
        [SerializeField]
        private GameObject TopPanel, WinPanel, LosePanel; // Các Panel

        /// <summary>
        /// Khởi tạo UI gameplay:
        /// - cập nhật toàn bộ LevelText
        /// - xử lý UI đặc biệt cho level đầu
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// GameSceneState
        /// ↓
        /// UIGameCanvas.Initialize(levelId)
        /// ↓
        /// tìm toàn bộ LevelTextSetter
        /// ↓
        /// set text:
        /// "Level X"
        /// </summary>
        /// <param name="UIlevelID">
        /// Level hiển thị cho player.
        ///
        /// Ví dụ:
        /// internal level = 0 thì sẽ hiển thị ra:
        /// UI level      = 1
        /// </param>
        public void Initialize(int UIlevelID)
        {
            /// <summary>
            /// Lấy toàn bộ component LevelTextSetter nằm trong children hierarchy.
            ///
            /// Ví dụ:
            /// - top panel level text
            /// - win panel level text
            /// - lose panel level text
            ///
            /// Tất cả sẽ được update cùng lúc.
            /// </summary>
            LevelTextSetter[] levelTextSetters = GetComponentsInChildren<LevelTextSetter>();
            foreach (var levelTextSetter in levelTextSetters) // Update text cho toàn bộ level label.
            {
                levelTextSetter.SetLevelText(UIlevelID);
            }

            /// <summary>
            /// Nếu là level đầu tiên:
            /// ẩn TopPanel.
            ///
            /// Có thể dùng cho:
            /// - tutorial level
            /// - onboarding
            /// - intro gameplay
            ///
            /// Ví dụ:
            /// level 1:
            /// không hiện timer / coin / pause.
            /// 
            /// Chỗ này có thể tự tùy chỉnh.
            /// </summary>
            //if (UIlevelID == 1)
            //{
            //    TopPanel.SetActive(false);
            //}
        }

        /// <summary>
        /// Được gọi khi game kết thúc.
        /// INPUT: gameWon:
        /// - WinState.Win
        /// - WinState.Lose
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// gameplay kết thúc
        /// ↓
        /// GameSceneState xác định:
        /// - win
        /// hoặc
        /// - lose
        /// ↓
        /// gọi UIGameCanvas.GameFinished()
        /// ↓
        /// mở panel tương ứng
        /// </summary>
        /// <param name="gameWon">
        /// Trạng thái thắng/thua.
        /// </param>
        public void GameFinished(WinState gameWon)
        {
            if (gameWon == WinState.Win)
            {
                OpenPanel(WinPanel.GetComponent<CanvasGroup>());
            }
            else
            {
                OpenPanel(LosePanel.GetComponent<CanvasGroup>());
            }
        }

        void OpenPanel(CanvasGroup group) // Mở panel UI bằng fade animation.
        {
            /// <summary>
            /// ====================================================
            /// DOTween
            /// ====================================================
            ///
            /// DOFade(1, 1) nghĩa là fade alpha tới 1 trong 1 giây. Đây là animation async của DOTween.
            /// </summary>
            group.DOFade(1, 1);
            group.interactable = true; // Cho phép UI nhận interaction.

            /// <summary>
            /// Cho phép panel chặn raycast.
            ///
            /// Nếu false click sẽ xuyên qua panel.
            ///
            /// Thường dùng để:
            /// - khóa gameplay phía sau popup
            /// - tránh click nhầm.
            /// </summary>
            group.blocksRaycasts = true; // Cho phép panel chặn raycast.
        }
    }
}