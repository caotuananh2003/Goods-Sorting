//using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.UI
{
    /// <summary>
    /// Controller quản lý Bottom Navigation UI.
    ///
    /// Ví dụ:
    /// - Shop
    /// - Rank
    /// - Gameplay
    /// - Clan
    /// - Collection
    ///
    /// ====================================================
    /// CHỨC NĂNG
    /// ====================================================
    ///
    /// Script này负责:
    ///
    /// - xử lý click button navigation
    /// - đổi màu icon active/inactive
    /// - scale button active
    /// - bật/tắt panel content tương ứng
    /// - di chuyển SelectorPanel bằng DOTween
    ///
    /// ====================================================
    /// HIERARCHY ĐỀ XUẤT
    /// ====================================================
    ///
    /// BottomPanel
    /// ├── SelectorPanel
    /// └── ButtonsRoot
    ///     ├── ShopButton
    ///     ├── RankButton
    ///     ├── MainGameplayButton
    ///     ├── ClanButton
    ///     └── CollectionButton
    ///
    /// ====================================================
    /// LƯU Ý
    /// ====================================================
    ///
    /// ButtonsRoot thường gắn:
    /// - Horizontal Layout Group
    ///
    /// SelectorPanel KHÔNG nằm trong Layout Group.
    ///
    /// Vì:
    /// - Layout Group sẽ tự điều khiển vị trí button
    /// - SelectorPanel cần được tween tự do
    /// </summary>
    public class BottomNavigationUI : MonoBehaviour
    {
        /// <summary>
        /// Danh sách các button navigation.
        ///
        /// Mỗi button chứa:
        /// - Button component
        /// - Icon Image
        /// - RectTransform
        /// - Content panel tương ứng
        ///
        /// Được setup trong Inspector.
        /// </summary>
        [SerializeField]
        private NavigationButton[] buttons;

        /// <summary>
        /// Panel highlight chạy qua chạy lại giữa các button.
        ///
        /// Ví dụ:
        /// - nền sáng
        /// - underline
        /// - rounded rectangle
        ///
        /// Object này sẽ được move bằng DOTween.
        /// </summary>
        //[SerializeField]
        //private RectTransform selectorPanel;

        #region Animation Settings

        [Header("Animation")]
        [SerializeField]
        private float selectorMoveDuration = 0.5f; // Thời gian selector di chuyển.

        [SerializeField]
        private float selectedFlexibleWidth = 1.5f;

        [SerializeField]
        private float normalFlexibleWidth = 1f;

        [SerializeField]
        private float widthTweenDuration = 0.25f;

        /// <summary>
        /// Kiểu easing của animation.
        ///
        /// Ease.OutCubic:
        /// - bắt đầu nhanh
        /// - chậm dần khi kết thúc
        ///
        /// Cho cảm giác mềm mại.
        /// </summary>
        [SerializeField]
        private Ease moveEase = Ease.OutCubic;

        [Header("Panel Animation")]
        [SerializeField]
        private float panelMoveDuration = 0.35f;

        [SerializeField]
        private Ease panelEase = Ease.OutCubic;
        #endregion

        #region Color Settings

        [Header("Colors")]
        [SerializeField]
        private Color selectedColor = Color.white; // Màu icon khi button được chọn.

        /// <summary>
        /// Màu icon khi button không được chọn.
        ///
        /// Alpha = 0.4f
        /// => icon bị mờ đi.
        /// </summary>
        [SerializeField]
        private Color unselectedColor = new Color(1f, 1f, 1f, 0.4f);

        #endregion

        #region Runtime Variables
        private int currentIndex = -1; // Index button hiện tại đang được chọn. Ví dụ: 0 = Shop, 1 = Rank... -1 nghĩa là chưa có button nào active.

        //private Tween moveTween; // Tween hiện tại của selectorPanel. Dùng để kill tween cũ và tránh animation chồng lên nhau

        #endregion


        private void Start()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;

                buttons[i].button.onClick.AddListener(() =>
                {
                    Select(index);
                });
            }

            Select(2, true);
        }

        /// <summary>
        /// Chọn button navigation mới.
        ///
        /// index:
        /// - button cần chọn
        ///
        /// instant:
        /// - true  = đổi UI ngay lập tức
        /// - false = có animation
        /// 
        /// </summary>
        public void Select(int index, bool instant = false)
        {
            if (index == currentIndex)
                return;

            int oldIndex = currentIndex; // Lưu button cũ để animate transition panel

            currentIndex = index; // Update current active index

            UpdateIcons(); // Update màu icon và scale button

            //MoveSelector(buttons[index].rect, instant); // Move selector highlight. Tạm bỏ vì xấu quá

            if (instant)
            {
                InstantShow(index); // Hiện ngay không animation
            }
            else
            {
                AnimatePanels(oldIndex, index); // Slide panel có animation
            }

            currentIndex = index;
        }

        private void InstantShow(int index) // Hiện panel ngay lập tức, không có animation.
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].contentPanel.SetActive(i == index);

                RectTransform rect = buttons[i].contentPanel.GetComponent<RectTransform>();

                rect.anchoredPosition = Vector2.zero;
            }
        }

        private void AnimatePanels(int oldIndex, int newIndex) // Animate chuyển panel.
        {
            // lần đầu
            if (oldIndex == -1)
            {
                InstantShow(newIndex);
                return;
            }

            GameObject oldPanel = buttons[oldIndex].contentPanel;

            GameObject newPanel = buttons[newIndex].contentPanel;

            RectTransform oldRect = oldPanel.GetComponent<RectTransform>();

            RectTransform newRect = newPanel.GetComponent<RectTransform>();

            bool moveRight = newIndex > oldIndex;

            float width = ((RectTransform)newRect.parent).rect.width;

            Vector2 startPos = moveRight ? new Vector2(width, 0) : new Vector2(-width, 0);

            Vector2 exitPos = moveRight ? new Vector2(-width, 0) : new Vector2(width, 0);

            newPanel.SetActive(true);

            newRect.anchoredPosition = startPos;

            Sequence seq = DOTween.Sequence();

            seq.Join(newRect.DOAnchorPos(Vector2.zero, panelMoveDuration));

            seq.Join(oldRect.DOAnchorPos(exitPos, panelMoveDuration));

            seq.SetEase(panelEase);

            seq.OnComplete(() =>
            {
                oldPanel.SetActive(false);

                oldRect.anchoredPosition = Vector2.zero;
            });
        }

        /// <summary>
        /// Update trạng thái visual của button.
        ///
        /// ====================================================
        /// ACTIVE BUTTON
        /// ====================================================
        ///
        /// - icon sáng
        /// - scale lớn hơn
        ///
        /// ====================================================
        /// INACTIVE BUTTON
        /// ====================================================
        ///
        /// - icon mờ
        /// - scale bình thường
        /// </summary>
        private void UpdateIcons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                bool selected = i == currentIndex; // Check button hiện tại có được chọn không

                buttons[i].icon.DOColor(selected ? selectedColor : unselectedColor, 0.2f); // Tween màu icon

                // =========================
                // FLEXIBLE WIDTH
                // =========================

                LayoutElement layout = buttons[i].button.GetComponent<LayoutElement>();

                float targetWidth = selected ? selectedFlexibleWidth : normalFlexibleWidth;

                // Kill tween cũ
                DOTween.Kill(layout);

                // Tween flexible width
                DOTween.To(() => layout.flexibleWidth, x => layout.flexibleWidth = x, targetWidth, widthTweenDuration).SetEase(Ease.OutCubic).SetTarget(layout);
            }

            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                transform as RectTransform
            );

            //buttons[i].button.transform.DOScale(selected ? 1.1f : 1f, 0.2f); // Tween scale button

            //if (selected) // Nếu button được chọn => đưa lên render layer cao nhất
            //{
            //    buttons[i].button.transform.SetAsLastSibling();
            //}
        }

        // Tạm không dùng vì xấu quá.
        //private void MoveSelector(RectTransform target, bool instant)
        //{
        //    // Convert vị trí button sang local position của parent selector
        //    Vector2 targetPos = selectorPanel.parent.InverseTransformPoint(target.position);

        //    // Lấy width của button target
        //    float targetWidth = target.rect.width * 1.1f;

        //    // Kill tween cũ để tránh animation chồng nhau
        //    if (moveTween != null)
        //    {
        //        moveTween.Kill();
        //    }

        //    // Instant mode
        //    if (instant)
        //    {
        //        selectorPanel.localPosition = targetPos;

        //        // Update width ngay lập tức
        //        selectorPanel.sizeDelta = new Vector2(targetWidth, selectorPanel.sizeDelta.y);

        //        return;
        //    }

        //    // Tween move + resize cùng lúc
        //    Sequence seq = DOTween.Sequence();

        //    // Move selector
        //    seq.Join(selectorPanel.DOLocalMove(targetPos, selectorMoveDuration).SetEase(moveEase));

        //    // Resize width selector
        //    seq.Join(selectorPanel.DOSizeDelta(new Vector2(targetWidth, selectorPanel.sizeDelta.y), selectorMoveDuration).SetEase(moveEase));

        //    moveTween = seq;
        //}
    }
}