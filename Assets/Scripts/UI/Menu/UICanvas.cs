using System.Collections.Generic;
using GameTemplate.UI.Currencies;
using GameTemplate.Audio;
using GameTemplate.Managers;
using GameTemplate.Managers.Scene;
using GameTemplate.UI;
using GameTemplate.UI.Currency;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Gameplay.UI
{
    /// <summary>
    /// Main UI Canvas của game.
    ///
    /// Đây là controller quản lý:
    /// - Currency UI
    /// - Level text
    /// - Play button
    /// - UI khởi tạo khi vào scene
    ///
    /// ====================================================
    /// VAI TRÒ
    /// ====================================================
    ///
    /// Class này đóng vai trò làm UI Composition Root, nghĩa là:
    /// - assemble UI
    /// - spawn UI elements
    /// - kết nối UI với gameplay systems
    ///
    /// ====================================================
    /// FLOW
    /// ====================================================
    ///
    /// Scene load
    /// ↓
    /// VContainer inject dependencies
    /// ↓
    /// Construct()
    /// ↓
    /// set level text
    /// ↓
    /// Start()
    /// ↓
    /// spawn currency UI
    /// ↓
    /// player tương tác UI
    /// </summary>

    public class UICanvas : MonoBehaviour, IStartable
    {
        #region Variables

        /// <summary>
        /// Parent chứa toàn bộ Currency UI.
        ///
        /// Ví dụ hierarchy:
        ///
        /// CurrencyRoot
        /// ├── CoinUI
        /// ├── GemUI
        /// └── EnergyUI
        ///
        /// Khi Instantiate CurrencyUIPrefab:
        /// object sẽ được spawn dưới parent này.
        /// </summary>
        [SerializeField] private Transform currencyParent;

        /// <summary>
        /// Prefab UI của 1 currency.
        ///
        /// Ví dụ:
        /// - icon coin
        /// - amount text
        /// - buy button
        ///
        /// Đây thường là CurrencyUI.prefab
        /// </summary>
        [SerializeField] private GameObject CurrencyUIPrefab;

        /// <summary>
        /// Danh sách CurrencyUI đã spawn runtime.
        ///
        /// Ví dụ:
        /// currencyPanels[0] => Coin UI
        /// currencyPanels[1] => Gem UI
        ///
        /// Dùng để:
        /// - quản lý UI
        /// - update UI sau này
        /// </summary>
        public List<CurrencyUI> currencyPanels = new List<CurrencyUI>();

        #endregion

        #region Injections

        /// <summary>
        /// CurrencyManager được inject bởi VContainer.
        ///
        /// Dùng để:
        /// - lấy currency data
        /// - truy cập danh sách currencies
        /// </summary>
        [Inject] CurrencyManager _CurrencyManager;

        /// <summary>
        /// SceneLoader service.
        ///
        /// Dùng để:
        /// - load scene mới
        /// </summary>
        [Inject] SceneLoader _SceneLoader;

        /// <summary>
        /// Audio system.
        ///
        /// Dùng để:
        /// - stop music
        /// - play sound
        /// </summary>
        [Inject] SoundPlayer _SoundPlayer;

        /// <summary>
        /// LevelManager system.
        ///
        /// Dùng để:
        /// - lấy level hiện tại
        /// - hiển thị level text
        /// </summary>
        [Inject] LevelManager _LevelManager;

        #endregion

        /// <summary>
        /// Constructor Injection method.
        ///
        /// VContainer sẽ tự động gọi hàm này sau khi object được tạo.
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// VContainer resolve:
        /// - SceneLoader
        /// - SoundPlayer
        /// - LevelManager
        /// ↓
        /// gọi Construct(...)
        /// ↓
        /// inject dependencies vào class
        /// </summary>
        [Inject]
        public void Construct(SceneLoader sceneLoader, SoundPlayer SoundPlayer, LevelManager levelManager)
        {
            Debug.Log("Construct UICanvas"); // Log ra Console thử

            // Gán dependencies vào fields
            _SceneLoader = sceneLoader;
            _SoundPlayer = SoundPlayer;
            _LevelManager = levelManager;

            // ====================================================
            // SET LEVEL TEXT
            // ====================================================
            //
            // Tìm component LevelTextSetter rồi set:
            //
            // "Level 1"
            // "Level 2"
            //
            // UILevelId: level hiển thị cho player
            //
            // Ví dụ:
            // internal levelId = 0
            // UILevelId = 1
            //
            GetComponentInChildren<LevelTextSetter>().SetLevelText(_LevelManager.UILevelId);
        }

        /// <summary>
        /// Spawn toàn bộ Currency UI runtime.
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// lấy danh sách currency
        /// ↓
        /// foreach currency
        /// ↓
        /// Instantiate CurrencyUIPrefab
        /// ↓
        /// Initialize UI data
        /// </summary>
        public void Start()
        {
            //Debug.Log("UI Canvas Start");
            List<Currency> currencies = _CurrencyManager._CurrencyData.currencies; // Lấy danh sách currencies từ CurrencyManager. Ví dụ: Coin, Gem, Energy

            for (int i = 0; i < currencies.Count; i++)// Mỗi Currency → tạo 1 CurrencyUI
            {
                // Instantiate Currency UI prefab
                // Instantiate trả về GameObject, sau đó GetComponent<CurrencyUI>() để lấy script CurrencyUI.
                currencyPanels.Add(Instantiate(CurrencyUIPrefab, currencyParent).GetComponent<CurrencyUI>());

                // Đảm bảo parent đúng hierarchy (thực ra Instantiate(..., parent) đã set parent rồi nên dòng này hơi dư)
                currencyPanels[i].transform.SetParent(currencyParent);

                // truyền dữ liệu currency vào UI: icon, sign, amount, buyable
                currencyPanels[i].Initialize(currencies[i].currencyImage, currencies[i].currencySign,
                    currencies[i].currencyAmount, currencies[i].isBuyable);
            }
        }

        /// <summary>
        /// Được gọi bởi nút Play trong UI.
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// player click Play
        /// ↓
        /// stop main menu music
        /// ↓
        /// load Game scene
        ///
        /// ====================================================
        /// UI BUTTON
        /// ====================================================
        ///
        /// Hàm này thường được bind (kéo thả vào) trong: Button.OnClick()
        /// </summary>
        public void PlayButtonClick()
        {
            _SoundPlayer.StopThemeMusic(); // Stop menu music
            _SceneLoader.LoadSceneByType(SceneType.GamePlay); // Load gameplay scene
        }
    }
}