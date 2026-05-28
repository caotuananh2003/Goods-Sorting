using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace GameTemplate.UI.Currency
{
    /// <summary>
    /// ScriptableObject chứa toàn bộ dữ liệu currency của game.
    ///
    /// Đây là:
    /// - database currency
    /// - currency config container
    /// - nơi lưu danh sách tiền tệ
    ///
    /// ====================================================
    /// CHỨA GÌ?
    /// ====================================================
    ///
    /// Ví dụ:
    ///
    /// - Gold
    /// - Gem
    /// - Energy
    /// - Ticket
    ///
    /// Mỗi currency thường chứa:
    /// - icon
    /// - amount
    /// - sign
    /// - buyable
    /// - save/load data
    ///
    /// ====================================================
    /// TẠI SAO DÙNG SCRIPTABLEOBJECT?
    /// ====================================================
    ///
    /// Vì currency data:
    /// - là dữ liệu global
    /// - không phụ thuộc scene
    /// - dễ edit trong inspector
    /// - dùng chung cho nhiều system
    ///
    /// Ví dụ:
    ///
    /// CurrencyManager
    /// UI
    /// Shop
    /// Reward system
    /// Save system
    ///
    /// đều có thể dùng chung asset này.
    /// </summary>
    [CreateAssetMenu(fileName = "CurrencyData", menuName = "Scriptable Objects/Currency Data")]
    public class CurrencyData : ScriptableObject, IStartable
    {
        /// <summary>
        /// Danh sách toàn bộ currency trong game.
        ///
        /// Ví dụ:
        ///
        /// [0] Gold
        /// [1] Gem
        /// [2] Energy
        ///
        /// Mỗi phần tử là:
        /// GameTemplate.UI.Currencies.Currency
        /// </summary>
        public List<Currencies.Currency> currencies = new List<Currencies.Currency>();

        /// <summary>
        /// Hàm Start của VContainer, KHÔNG phải MonoBehaviour.Start().
        ///
        /// Vì ScriptableObject:
        /// - không có lifecycle Unity như MonoBehaviour
        /// - nên VContainer cung cấp IStartable
        ///
        /// Khi object được container start VContainer sẽ tự gọi hàm này.
        ///
        /// Hiện tại chỉ dùng để debug.
        public void Start()
        {
            Debug.Log("Currency Data");
        }
    }
}