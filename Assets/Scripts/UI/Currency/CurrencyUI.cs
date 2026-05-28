using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.UI.Currency
{
    /// <summary>
    /// UI hiển thị currency:
    /// - icon
    /// - amount
    /// - animation khi thay đổi tiền
    ///
    /// Ví dụ:
    /// Gold: 1500
    /// Gem: 25
    ///
    /// ====================================================
    /// NHIỆM VỤ
    /// ====================================================
    ///
    /// - update text currency
    /// - update icon
    /// - animate khi tiền thay đổi
    /// - hỗ trợ count up animation
    /// </summary>
    public class CurrencyUI : MonoBehaviour
    {
        #region Variables

        public TextMeshProUGUI currencyAmountText; // TMP text hiển thị số tiền.
        public Image currencyImage; // Icon currency.
        public GameObject AddButton; // Nút add currency/shop button.
        public Transform flyCurrencyTargetTransform; // Target để fly currency animation bay tới (Hiệu ứng tiền + vào tài khoản)

        [SerializeField] private float punch = 0.2f; // Độ mạnh scale animation. Đây là animation object phóng to nhanh rồi đàn hồi trở lại: 1.0 -> (1.0 + 0.2) -> 0.95 -> 1.0
        [SerializeField] private float punchDuration = 0.5f; // Thời gian animation punch. 0.5 tức là animation kéo dài 0.5 giây.
        [SerializeField] private int vibrato = 6; // Độ rung/lắc của animation. (rung 6 lần)

        /// <summary>
        /// Object sẽ được áp dụng punch animation.
        ///
        /// ====================================================
        /// TẠI SAO KHÔNG ANIMATE THIS.TRANSFORM?
        /// ====================================================
        ///
        /// Vì đôi khi:
        /// - chỉ muốn animate icon
        /// - chỉ muốn animate text
        /// - không muốn animate toàn bộ UI
        /// nên chúng ta dùng punchAnimationParent để chỉ định object cụ thể.
        [SerializeField] private Transform punchAnimationParent;

        private string currencySign; // Ký hiệu currency (Ví dụ như $, G, Gold để hiển thị ra 100$, 200G, 360Gold...)
        private int currencyAmount; // Giá trị currency hiện tại.
        private Coroutine incrementRoutine; // Coroutine đang chạy animation tăng/giảm tiền.

        /// <summary>
        /// Tween animation hiện tại. (Tween là cái phép nội suy)
        ///
        /// ====================================================
        /// DOTWEEN
        /// ====================================================
        ///
        /// DOTween tạo ra object Tween
        /// để đại diện animation.
        ///
        /// Ví dụ:
        ///
        /// Tween t = transform.DOMove(...);
        ///
        /// ====================================================
        /// punchTween DÙNG ĐỂ
        /// ====================================================
        ///
        /// lưu animation punch hiện tại.
        ///
        /// Nhờ vậy có thể:
        ///
        /// punchTween.Kill();
        ///
        /// để:
        /// - dừng animation cũ
        /// - tránh animation bị chồng
        ///
        /// ====================================================
        /// VÍ DỤ THỰC TẾ
        /// ====================================================
        ///
        /// Player nhận coin liên tục:
        ///
        /// +10
        /// +10
        /// +10
        ///
        /// Nếu không Kill tween cũ:
        ///
        /// animation sẽ:
        /// - rung loạn
        /// - scale lỗi
        ///
        /// Nên:
        ///
        /// punchTween?.Kill(true);
        ///
        /// sẽ:
        /// - dừng tween cũ
        /// - chạy tween mới sạch sẽ.
        /// </summary>
        private Tween punchTween; // Tween animation hiện tại.

        #endregion

        public void Initialize(Sprite currencyIcon, string currencySign, int currencyAmount, bool isBuyable) // Khởi tạo UI currency.
        {
            this.currencyImage.sprite = currencyIcon;
            this.currencySign = currencySign;
            AddButton.SetActive(isBuyable);
            //TODO Plus button click (Sẽ làm sau)

            SetCurrency(currencyAmount); // amount ban đầu
        }

        public void SetCurrency(int nextAmount) // Set currency ngay lập tức. Không có count animation.
        {
            if (currencyAmount < nextAmount) // nếu amount thay đổi thì play animation
            {
                PunchAnimation();
            }
            else if (currencyAmount > nextAmount)
            {
                PunchAnimation();
            }

            currencyAmount = nextAmount;

            /// <summary>
            /// Update text UI.
            ///
            /// NumberHelper giúp format:
            /// 1000 -> 1K
            /// 1000000 -> 1M
            /// </summary>
            currencyAmountText.text = NumberHelper.ToStringScientific(nextAmount) + "" + currencySign;
        }

        /// <summary>
        /// Set currency với animation tăng/giảm dần.
        ///
        /// Ví dụ:
        ///
        /// 100
        /// 105
        /// 110
        /// 120
        /// ...
        /// 200
        /// </summary>
        public void SetCurrencyIncremental(int nextAmount)
        {
            if (incrementRoutine != null) // nếu coroutine cũ đang chạy thì dừng nó lại
            {
                StopCoroutine(incrementRoutine);
            }

            PunchAnimation();
            incrementRoutine = StartCoroutine(MoneyChangeRoutine(nextAmount)); // Bắt đầu coroutine
        }

        public IEnumerator MoneyChangeRoutine(int nextAmount) // Coroutine animate số tiền thay đổi dần.
        {
            float lerpValue = 0;
            while (lerpValue < 1)
            {
                currencyAmount = (int)Mathf.Lerp(currencyAmount, nextAmount, lerpValue);
                currencyAmountText.text = NumberHelper.ToStringScientific(currencyAmount) + currencySign;
                lerpValue += Time.deltaTime * 2f;
                yield return null;
            }

            SetCurrency(nextAmount); // đảm bảo value cuối chính xác
        }

        /// <summary>
        /// Play punch scale animation.
        ///
        /// UI sẽ:
        /// - phóng to nhẹ
        /// - rung
        ///
        /// Dùng DOTween.
        /// </summary>
        private void PunchAnimation()
        {
            punchTween?.Kill(true); // kill tween cũ nếu còn chạy
            punchTween = punchAnimationParent.DOPunchScale(Vector3.one * punch, punchDuration, vibrato);
        }
    }
}