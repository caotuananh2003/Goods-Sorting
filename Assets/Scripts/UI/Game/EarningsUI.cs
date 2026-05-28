using GameTemplate.Events;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameTemplate.UI
{
    public class EarningsUI : MonoBehaviour
    {
        /// <summary>
        /// UI component dùng để:
        /// - tạo reward coin ngẫu nhiên trong 1 khoảng
        /// - hiển thị reward lên UI
        /// - trả về dữ liệu CurrencyArgs
        ///
        /// ====================================================
        /// MỤC ĐÍCH
        /// ====================================================
        ///
        /// Script này thường dùng trong:
        /// - Win Panel
        /// - Reward Popup
        /// - End Game Screen
        ///
        /// Khi player thắng:
        /// - random số coin nhận được
        /// - hiện text
        /// - trả về dữ liệu currency reward
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// Player win
        /// ↓
        /// gọi SetEarnings()
        /// ↓
        /// random coin
        /// ↓
        /// update UI text
        /// ↓
        /// trả về CurrencyArgs
        /// ↓
        /// CurrencyManager nhận dữ liệu
        /// ↓
        /// cộng tiền cho player
        /// </summary>
        public TextMeshProUGUI EarnedCoinText;

        public CurrencyArgs SetEarnings()
        {
            int randomEarning = Random.Range(10, 20);
            EarnedCoinText.text = "+" + randomEarning;
            return new CurrencyArgs(0, randomEarning, false);
        }
    }
}