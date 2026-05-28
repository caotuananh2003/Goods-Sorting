using System;
using GameTemplate.Utils;
using UnityEngine;

namespace GameTemplate.UI.Currencies
{
    /// <summary>
    /// Class đại diện cho một loại currency trong game.
    ///
    /// Ví dụ:
    /// - Gold
    /// - Gem
    /// - Energy
    /// - Ticket
    ///
    /// ====================================================
    /// CHỨA GÌ?
    /// ====================================================
    ///
    /// - icon
    /// - ký hiệu
    /// - số lượng hiện tại
    /// - có mua được hay không
    /// - save/load PlayerPrefs
    ///
    /// ====================================================
    /// LƯU Ý
    /// ====================================================
    ///
    /// Đây KHÔNG phải MonoBehaviour.
    ///
    /// Đây chỉ là:
    /// - data class
    /// - model class
    /// - plain serializable object
    ///
    /// Nó thường nằm trong:
    /// CurrencyData ScriptableObject.
    /// </summary>
    [System.Serializable]
    public class Currency
    {
        #region Variables

        public Sprite currencyImage; // Icon currency hiển thị trên UI. Ví dụ như GoldIcon, GemIcon...
        public string currencySign; // Ký hiệu currency. Ví dụ như $, G, Gold,...
        public int currencyAmount; // Số lượng currency hiện tại.
        public bool isBuyable; // Currency có mua bằng shop được không. Nếu true thì hiện nút + (có thể buy)

        /// <summary>
        /// Giá trị tạm giữ.
        ///
        /// HideInInspector:
        /// - vẫn serialize
        /// - nhưng không hiện inspector
        ///
        /// Có thể dùng cho:
        /// - pending reward
        /// - temporary value
        /// - animation counting
        ///
        /// Hiện tại chưa thấy dùng nhiều.
        [HideInInspector] public int currencyHoldAmount;

        private int currencyId; // ID của currency.

        /// <summary>
        /// Property chỉ đọc của currencyId.
        ///
        /// Class ngoài:
        /// - đọc được
        /// - không sửa được
        /// </summary>
        public int CurrencyId
        {
            get => currencyId;
        }

        #endregion

        /// <summary>
        /// Khởi tạo currency.
        ///
        /// ====================================================
        /// NHIỆM VỤ
        /// ====================================================
        ///
        /// - set currencyId
        /// - load dữ liệu save từ PlayerPrefs
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// CurrencyManager Construct()
        /// ↓
        /// currencies[i].Initialize(i)
        /// ↓
        /// load dữ liệu save
        ///
        /// ====================================================
        /// VÍ DỤ
        /// ====================================================
        ///
        /// Currency:
        /// Gold
        ///
        /// currencyId = 0
        ///
        /// UserPrefs:
        /// Currency_0 = 5000
        ///
        /// => currencyAmount = 5000
        /// </summary>
        public void Initialize(int cId)
        {
            this.currencyId = cId;
            currencyAmount = UserPrefs.GetCurrency(currencyId, currencyAmount); // Load saved currency. Nếu chưa có save thì dùng giá trị mặc định hiện tại.
        }

        /// <summary>
        /// Reset currency về 0.
        ///
        /// Thường dùng cho:
        /// - reset save
        /// - new game
        /// - testing
        ///
        /// EventArgs được truyền vào để compatible với event system.
        public void Reset(EventArgs args)
        {
            currencyAmount = 0;
            SetPlayerPref(); // save lại PlayerPrefs
        }

        public void Spend(int spentAmount) // Trừ currency.
        {
            currencyAmount -= spentAmount;
            SetPlayerPref(); // save dữ liệu mới
        }

        public void Earn(int earningsAmount) // Cộng currency.
        {
            currencyAmount += earningsAmount;
            SetPlayerPref();  // save dữ liệu mới
        }

        /// <summary>
        /// Save currency vào PlayerPrefs.
        ///
        /// ====================================================
        /// VÍ DỤ
        /// ====================================================
        ///
        /// currencyId = 0
        /// currencyAmount = 5000
        ///
        /// =>
        ///
        /// PlayerPrefs:
        /// "Currency_0" = 5000
        ///
        /// ====================================================
        /// TẠI SAO PHẢI SAVE?
        /// ====================================================
        ///
        /// Để:
        /// - thoát game không mất dữ liệu
        /// - load lại vẫn còn tiền
        /// </summary>
        public void SetPlayerPref()
        {
            UserPrefs.SetCurrency(currencyId, currencyAmount);
        }
    }
}