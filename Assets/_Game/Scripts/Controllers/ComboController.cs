using GameTemplate._Game.Scripts.Match;
using GameTemplate.Managers.Scene;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using GameTemplate.Audio;
using VContainer;

namespace _Game.Scripts.Timer
{
    /*
     * ComboController
     * -------------------------
     * Vai trò:
     * - Quản lý hệ thống combo của gameplay.
     * - Mỗi lần player match thành công:
     *      + tăng combo count
     *      + reset thời gian combo
     *      + update UI combo
     *      + chạy thanh countdown combo
     *
     * Đây là:
     * - Gameplay controller
     * - Chỉ điều phối logic combo + UI
     *
     * Không trực tiếp xử lý:
     * - match logic
     * - item logic
     * - score logic
     *
     * Nó chỉ "nghe event" từ MatchGroup.
     */
    public class ComboController : MonoBehaviour, IStartable
    {
        private SoundPlayer _soundPlayer;

        [Inject]
        public void Construct(SoundPlayer soundPlayer)
        {
            _soundPlayer = soundPlayer;
        }

        #region Variables

        private bool _playedComboEndingSound; // flag chống spam sound

        // Public Variables
        public TextMeshProUGUI ComboCountText; // Text hiển thị combo, ví dụ như X1, X2,...
        public Slider ComboSlider; // Thanh slider countdown combo. Nếu hết thời gian thì reset combo về 0

        public int ComboCount // Property public chỉ đọc. Script khác có thể đọc nhưng không thể sửa.
        {
            get { return _comboCount; }
        }

        private int _comboCount = 0; // Số combo hiện tại.
        [SerializeField] float ComboTime = 25f; // Thời gian combo cơ bản.
        private float _timer = 0f; // Timer runtime hiện tại.

        #endregion

        private void Awake() // Tạo UI và subscribe event
        {
            ComboCountText.text = string.Empty;
            ComboSlider.value = 0;

            MatchGroup.OnMatched += Combo;
            LevelPrefab.OnGameFinished += StopCombo;
        }

        public void Start() // IStartable của VContainer. VContainer sẽ gọi Start()
        {
        }

        private void OnDestroy() // UnSubscribe event 
        {
            MatchGroup.OnMatched -= Combo;
            LevelPrefab.OnGameFinished -= StopCombo;
        }

        Coroutine ComboCoroutine; // Reference tới coroutine combo hiện tại.

        /// <summary> Được gọi mỗi khi match thành công.
        ///
        /// position:
        /// - vị trí match trong world
        /// - ở script này chưa dùng
        /// 
        /// </summary>
        public void Combo(Vector3 position)
        {
            if (!gameObject.activeInHierarchy) // Nếu Object đang gắn script ComboController không được active thì không làm gì cả. gameObject là property có sẵn của MonoBehaviour.
                return;

            _comboCount++; // Tăng combo.
            ComboCountText.text = "Combo x " + _comboCount.ToString(); // Update text UI.
            _timer = Mathf.Max(10f, ComboTime - _comboCount); // Reset timer combo. Combo càng cao thì _timer càng ngắn nhưng luôn >= 10.

            _playedComboEndingSound = false;

            if (ComboCoroutine != null) StopCoroutine(ComboCoroutine); // Nếu coroutine cũ đang chạy thì stop trước.
           ComboCoroutine = StartCoroutine(StartComboCoroutine()); // Start Coroutine mới.
        }

        public void StopCombo(bool isWin, bool isAllLinesFilled) // Khi game kết thúc thì dừng ComboCoroutine
        {
            if (ComboCoroutine != null) StopCoroutine(ComboCoroutine);
        }

        /// <summary> Coroutine countdown combo.
        ///
        /// Chạy mỗi frame:
        /// - giảm timer
        /// - update slider
        ///
        /// Khi timer hết:
        /// - reset combo
        /// </summary>
        private IEnumerator StartComboCoroutine()
        {
            ComboSlider.value = _timer / (ComboTime - _comboCount); // Giá trị slider từ 0 -> 1

            while (_timer > 0)
            {
                _timer -= Time.deltaTime; // Giảm timer theo thời gian thực.
                ComboSlider.value = _timer / (ComboTime - _comboCount); // Update slider UI.

                // Combo sắp hết
                if (_timer <= 5f && !_playedComboEndingSound)
                {
                    _playedComboEndingSound = true;

                    _soundPlayer.PlayComboEndingSound();
                }

                yield return null; // Chờ frame tiếp theo.
            }

            _comboCount = 0; // Hết combo.
            ComboCountText.text = string.Empty; // Xóa text combo.

            _soundPlayer.PlayComboEndedSound();
        }
    }
}