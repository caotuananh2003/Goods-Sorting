using System;

namespace GameTemplate.Events
{
    /// <summary>
    /// Class chứa dữ liệu event liên quan tới thay đổi currency (tiền/tài nguyên).
    ///
    /// Kế thừa từ EventArgs để dùng chung với hệ thống event của C#.
    ///
    /// Ví dụ:
    /// - Player nhận coin
    /// - Player mất vàng
    /// - UI cần update số tiền
    /// - Play animation cộng tiền
    /// </summary>
    public class CurrencyArgs : EventArgs
    {
        /// <summary>
        /// ID của loại currency.
        ///
        /// Ví dụ:
        /// 0 = Gold
        /// 1 = Gem
        /// 2 = Energy
        /// </summary>
        public int currencyId;

        /// <summary>
        /// Số lượng thay đổi.
        ///
        /// Ví dụ:
        /// +100 coin
        /// -50 gem
        /// </summary>
        public int changeAmount;

        /// <summary>
        /// Có cộng tiền từ từ hay không.
        ///
        /// true:
        /// UI animate tăng dần:
        /// 100 -> 101 -> 102 ...
        ///
        /// false:
        /// Update ngay lập tức
        /// </summary>
        public bool addIncrementally;

        /// <summary>
        /// Constructor khởi tạo dữ liệu event.
        /// </summary>
        /// <param name="currencyId">
        /// ID loại tiền/tài nguyên
        /// </param>
        ///
        /// <param name="changeAmount">
        /// Giá trị thay đổi
        /// </param>
        ///
        /// <param name="addIncrementally">
        /// Có animate cộng dần hay không
        /// </param>
        public CurrencyArgs(
            int currencyId,
            int changeAmount,
            bool addIncrementally)
        {
            this.currencyId = currencyId;
            this.changeAmount = changeAmount;
            this.addIncrementally = addIncrementally;
        }
    }
}
