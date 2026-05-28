//using Codice.Client.BaseCommands;
using GameTemplate.Audio;
using GameTemplate.Managers.Scene;
using System;
using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Game.Scripts.Timer
{
    /// <summary>
    /// =========================================================
    /// TimerController
    /// =========================================================
    ///
    /// Vai trò:
    /// - Quản lý countdown timer của gameplay.
    /// - Update UI thời gian.
    /// - Phát hiệu ứng khi gần hết giờ.
    /// - Phát âm thanh timer.
    /// - Trigger sự kiện hết giờ.
    ///
    /// Đây là:
    /// - Gameplay Flow Controller
    /// - Điều phối logic thời gian của màn chơi.
    ///
    /// =========================================================
    /// FLOW HOẠT ĐỘNG
    /// =========================================================
    ///
    /// SetTimer()
    ///     ↓
    /// StartTimer()
    ///     ↓
    /// Coroutine StartTimerCor()
    ///     ↓
    /// Mỗi frame:
    ///     - giảm thời gian
    ///     - update UI
    ///     - chạy animation
    ///     - phát hiệu ứng gần hết giờ
    ///     ↓
    /// Hết giờ
    ///     ↓
    /// OnTimesUp
    ///     ↓
    /// Game Lose
    ///
    /// =========================================================
    /// DEPENDENCY INJECTION
    /// =========================================================
    ///
    /// Script này dùng VContainer để inject:
    ///
    /// - LevelManager
    /// - SoundPlayer
    ///
    /// Không cần:
    /// - FindObjectOfType
    /// - Singleton access thủ công
    ///
    /// =========================================================
    /// KIẾN TRÚC
    /// =========================================================
    ///
    /// Script này dùng:
    ///
    /// - Coroutine
    /// - Event-driven architecture
    /// - Dependency Injection
    /// - UI animation runtime
    ///
    /// =========================================================
    /// </summary>
    public class TimerController : MonoBehaviour
    {
        #region Variables

        // Public Variables
        public static Action<bool> OnTimesUp; // Event khi hết giờ
        public static Action<float> OnSetTimer; // Event dùng để set thời gian gameplay, ví dụ: TimerController.OnSetTimer?.Invoke(60); để set timer = 60 giây

        // Private Variables
        [SerializeField] private TextMeshProUGUI txtTimer; // Text UI hiển thị timer. Cần kéo TextMeshProUGUI vào Inspector.
        [SerializeField] private Color timerTextColorForFast; // Màu cảnh báo khi gần hết giờ. Cần chọn màu trong Inspector.
        [SerializeField] private float pumpEffectSineSizeMultiplier = 1.1f; // Độ scale tối đa của hiệu ứng pump.
        [SerializeField] private float pumpEffectSineSpeed = 4f; // Tốc độ animation bình thường.
        [SerializeField] private float pumpEffectSineSpeedFast = 6f; // Tốc độ animation khi gần hết giờ.

        private float _totalDurationInSeconds = 30; // Tổng thời gian gameplay.
        private float _timer; // Timer runtime hiện tại.

        private float _sineTimer; // Timer dùng cho animation sine wave. (Đồ thị sóng hình sin)
        private float _sineValue; // Giá trị sine hiện tại.

        private bool timerPaused; // Pause timer hay không.
        private bool firstTick = true; // Biến chưa dùng trong script này. Có thể là phần code cũ.

        #endregion Variables

        #region Injections

        [Inject] LevelManager _levelManager; // Inject LevelManager từ framework. Hiện chưa dùng trực tiếp trong script
        [Inject] SoundPlayer m_SoundPlayer; // Inject SoundPlayer. Dùng để phát nhạc timer và phát sound hết giờ

        #endregion

        private void Awake()
        {
            OnSetTimer += SetTimer; // Subscribe hàm SetTimer cho event OnSetTimer
            LevelPrefab.OnGameFinished += StopTimer; // Subscribe hàm StopTimer khi game kết thúc.
        }

        private void OnDestroy()
        {
            OnSetTimer -= SetTimer; // Unsubscribe
            LevelPrefab.OnGameFinished -= StopTimer; // Unsubscribe
        }

        public void SetTimer(float durationInSeconds) // Set thời gian gameplay.
        {
            _totalDurationInSeconds = durationInSeconds; // Lưu tổng thời gian.
            _timer = _totalDurationInSeconds; // Reset timer runtime.
            UpdateTimerText(); // Update UI ngay lập tức.
        }

        public void StopTimer(bool isWin, bool isAllLinesFilled) // Dừng timer khi game kết thúc.
        {
            timerPaused = true; // Pause timer.
            txtTimer.color = Color.white; // Reset màu text
            txtTimer.transform.localScale = Vector3.one; // Reset scale text.
        }

        public void StartTimer() // Bắt đầu countdown timer.
        {
            previousSecond = MathF.Floor(Math.Clamp(_timer % 60f, 0f, 59f)); // Lấy giây hiện tại bằng cách % 60, chỉ lấy phần giây.

            UpdateTimerText(); // Update UI.

            StartCoroutine(StartTimerCoroutine()); // Start coroutine countdown.
        }

        /// <summary>
        /// Coroutine countdown chính.
        ///
        /// Chạy mỗi frame:
        /// - giảm timer
        /// - update UI
        /// - update animation
        /// </summary>
        private IEnumerator StartTimerCoroutine()
        {
            m_SoundPlayer.PlayTimerMusic(true); // Phát nhạc timer.

            while (_timer > 0) // Countdown tới khi hết giờ.
            {
                if (!timerPaused) // Nếu không pause.
                {
                    _timer -= Time.deltaTime; // Giảm timer theo thời gian thực.

                    _sineTimer += Time.deltaTime * (_timer <= 5f ? pumpEffectSineSpeedFast : pumpEffectSineSpeed); // Update sine timer. Nếu thời gian còn < 5 giây thì anim chạy nhanh hơn

                    UpdateTimerText(); // Update text timer.

                    TimerSinePumpAnimation(_timer <= 5f); // Chạy animation pulse.
                }

                yield return null; // Chờ frame tiếp theo.
            }

            // Khi thua game
            txtTimer.text = "00:00"; // Fix text cuối cùng.
            m_SoundPlayer.PlayTimesUpSound(); // Phát sound hết giờ.
            OnTimesUp?.Invoke(false); // Trigger phát event TimesUp.
            txtTimer.color = Color.white; // Reset màu.
            txtTimer.transform.localScale = Vector3.one; // Reset scale.
        }

        // Biến chưa dùng đến.
        private float previousSecond;
        private bool coin0Lose, coin1Lose, coin2Lose;

        private void UpdateTimerText() // Update text timer UI.
        {
            var minutes = Mathf.Clamp(Mathf.Floor(_timer / 60f), 0, 10); // Tính phút.
            var seconds = _timer % 60f; // Tính giây.
            txtTimer.text = $"{minutes:00}:{MathF.Floor(Math.Clamp(seconds, 0f, 59f)):00}"; // Format text.
        }

        /// <summary>
        /// Animation pulse cho timer text.
        ///
        /// Dùng Cos wave để:
        /// - scale lên xuống
        /// - đổi màu khi gần hết giờ
        /// </summary>
        private void TimerSinePumpAnimation(bool fastVersion)
        {
            _sineValue = Mathf.Cos(_sineTimer) * -1 / 2f + .5f;

            txtTimer.transform.localScale =
                Vector3.Lerp(Vector3.one, Vector3.one * pumpEffectSineSizeMultiplier, _sineValue);

            if (fastVersion) // Nếu gần hết giờ thì đổi màu text
            {
                txtTimer.color = Color.Lerp(Color.white, timerTextColorForFast, _sineValue);
            }
        }
    }
}