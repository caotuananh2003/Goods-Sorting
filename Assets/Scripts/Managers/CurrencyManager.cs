using System;
using GameTemplate.Events;
using GameTemplate.UI.Currency;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Managers
{
    /// <summary>
    /// Manager chịu trách nhiệm quản lý toàn bộ hệ thống tiền tệ/currency trong game.
    ///
    /// Ví dụ:
    /// - Gold
    /// - Coin
    /// - Gem
    /// - Energy
    ///
    /// Nhiệm vụ chính:
    /// - initialize currency
    /// - cộng tiền
    /// - trừ tiền
    /// - quản lý dữ liệu currency
    ///
    /// Đây là:
    /// - Service class
    /// - Manager pattern
    ///
    /// Class này KHÔNG kế thừa MonoBehaviour.
    ///
    /// Nó được tạo và inject bởi:
    /// VContainer DI Container.
    /// </summary>
    public class CurrencyManager
    {
        #region Variables
        /// <summary>
        /// ScriptableObject chứa toàn bộ dữ liệu currency.
        ///
        /// Ví dụ:
        /// currencies[0] = Gold
        /// currencies[1] = Gem
        ///
        /// CurrencyData thường chứa:
        /// - list currency
        /// - save/load data
        /// - current amount
        /// </summary>
        public CurrencyData _CurrencyData;

        #endregion

        /// <summary>
        /// Hàm inject dependency từ VContainer.
        ///
        /// VContainer sẽ tự động gọi Construct() và truyền CurrencyData vào.
        ///
        /// Không cần:
        /// - new CurrencyManager()
        /// - kéo thả inspector
        /// - gọi Construct thủ công
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// ApplicationController:
        ///
        /// builder.RegisterInstance(currencyData);
        /// builder.Register<CurrencyManager>(Lifetime.Singleton);
        ///
        /// =>
        ///
        /// VContainer:
        /// - tạo CurrencyManager
        /// - resolve CurrencyData
        /// - gọi Construct(currencyData)
        ///
        /// ====================================================
        /// INITIALIZE CURRENCY
        /// ====================================================
        ///
        /// Mỗi currency sẽ được:
        /// - gán ID
        /// - setup dữ liệu ban đầu
        ///
        /// Ví dụ:
        ///
        /// currencies[0] => Gold
        /// currencies[1] => Gem
        /// </summary>
        /// <param name="CurrencyData">
        /// ScriptableObject chứa dữ liệu currency
        /// </param>
        [Inject]
        public void Construct(CurrencyData CurrencyData)
        {
            // lưu reference
            Debug.Log("Constructing currency manager");
            _CurrencyData = CurrencyData;

            // initialize từng currency
            for (int i = 0; i < _CurrencyData.currencies.Count; i++)
            {
                _CurrencyData.currencies[i].Initialize(i);
            }
        }

        /// <summary>
        /// Cộng currency cho player.
        ///
        /// Ví dụ:
        /// - thắng level
        /// - nhận thưởng
        /// - xem quảng cáo
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// CurrencyArgs:
        /// - currencyId
        /// - amount
        ///
        /// =>
        ///
        /// CurrencyManager:
        /// currencies[id].Earn(amount)
        ///
        /// ====================================================
        /// VÍ DỤ
        /// ====================================================
        ///
        /// EarnCurrency(
        ///     new CurrencyArgs(0, 100, true)
        /// );
        ///
        /// =>
        ///
        /// cộng 100 Gold
        /// </summary>
        /// <param name="eventArgs">
        /// Dữ liệu currency change
        /// </param>
        public void EarnCurrency(EventArgs eventArgs)
        {
            var currencyValue = eventArgs as CurrencyArgs; // cast EventArgs về CurrencyArgs
            _CurrencyData.currencies[currencyValue.currencyId].Earn(currencyValue.changeAmount); // cộng tiền
        }

        /// <summary>
        /// Trừ currency khỏi player.
        ///
        /// Ví dụ:
        /// - mua item
        /// - unlock level
        /// - dùng booster
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// CurrencyArgs:
        /// - currencyId
        /// - amount
        ///
        /// =>
        ///
        /// currencies[id].Spend(amount)
        /// </summary>
        /// <param name="eventArgs">
        /// Dữ liệu currency change
        /// </param>
        public void SpendCurrency(EventArgs eventArgs)
        {
            var currencyValue = eventArgs as CurrencyArgs; // cast EventArgs về CurrencyArgs
            _CurrencyData.currencies[currencyValue.currencyId].Spend(currencyValue.changeAmount); // trừ tiền
        }
    }
}