using AssetKits.ParticleImage;
using Codice.Client.Common;
using GameTemplate._Game.Scripts.Match;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SocialPlatforms.Impl;
using VContainer;
using VContainer.Unity;
using static UnityEngine.ParticleSystem;

namespace _Game.Scripts.Timer
{
    /// <summary> StarController
    /// Vai trò:
    /// - Spawn particle star khi player match thành công.
    /// - Particle sẽ bay về icon star trên UI.
    /// - Khi particle tới nơi:
    ///      -> cộng star
    ///      -> update text UI
    ///
    /// Đây là:
    /// - Reward feedback system
    /// - UI reward controller
    ///
    /// Script này KHÔNG:
    /// - xử lý gameplay match
    /// - kiểm tra thắng/thua
    /// - xử lý score chính
    ///
    /// Nó chỉ:
    /// - lắng nghe event match
    /// - tạo visual reward
    /// - cộng star UI
    ///
    /// =========================================================
    /// Flow hoạt động:
    ///
    /// Match thành công
    ///      ↓
    /// MatchGroup.OnMatched
    ///      ↓
    /// SpawnParticle()
    ///      ↓
    /// Spawn particle tại vị trí match
    ///      ↓
    /// Particle bay tới starIcon
    ///      ↓
    /// EarnStar()
    ///      ↓
    /// tăng số star
    ///
    /// =========================================================
    /// CÁC OBJECT CẦN KÉO THẢ TRONG UNITY
    /// =========================================================
    ///
    /// 1. _countText
    /// ---------------------------------------------------------
    /// Kéo:
    /// - TextMeshProUGUI hiển thị số sao
    ///
    /// Ví dụ:
    /// Canvas
    /// └── StarText
    ///
    ///
    /// 2. starPrefab
    /// ---------------------------------------------------------
    /// Kéo:
    /// - prefab có component ParticleImage
    ///
    /// Prefab này thường là:
    /// - particle UI
    /// - hình ngôi sao
    /// - coin fly effect
    ///
    /// QUAN TRỌNG:
    /// prefab PHẢI có:
    ///      ParticleImage component
    ///
    ///
    /// 3. starIcon
    /// ---------------------------------------------------------
    /// Kéo:
    /// - Transform của icon star UI
    ///
    /// Particle sẽ bay tới object này.
    ///
    /// Ví dụ:
    /// Canvas
    /// └── TopBar
    ///     └── StarIcon
    ///
    ///    ///* =========================================================
    /// DEPENDENCY INJECTION
    /// =========================================================
    ///
    /// ComboController được inject bởi VContainer.
    ///
    /// Không cần:
    ///      GetComponent()
    /// FindObjectOfType()
    ///
    /// VContainer tự cấp reference.
    ///
    /// </summary>
    public class StarController : MonoBehaviour, IStartable
    {
        #region Variables

        // Public Variables
        public TextMeshProUGUI _countText; // UI text hiển thị số sao hiện tại. Cần kéo TextMeshProUGUI vào Inspector.
        public GameObject starPrefab; // Prefab particle star. Cần kéo prefab chứa ParticleImage component.
        public Transform starIcon; // Target mà particle sẽ bay tới. Thường là icon ngôi sao trên UI. Cần kéo transform của icon star.

        public int StarCount // Property public chỉ đọc. Script khác có thể đọc StarCount nhưng không thể sửa trực tiếp _starCount
        {
            get { return _starCount; }
        }

        // Private Variables
        private int _starCount = 0; // Tổng số sao hiện tại.
        List<ParticleImage> particles = new List<ParticleImage>(); // Lập danh sách particle đang tồn tại, vì có thể nhiều particle spawn cùng lúc 

        #endregion

        #region Injects

        [Inject] ComboController _comboController; // Inject ComboController từ VContainer. Dùng để lấy combo count hiện tại.

        #endregion

        private void Awake() // Subscribe event match.
        {
            MatchGroup.OnMatched += SpawnParticle;
        }

        public void Start() // IStartable của VContainer. Hiện để trống.
        {
        }

        private void OnDestroy() // Unsubscribe event match.
        {
            MatchGroup.OnMatched -= SpawnParticle;
        }

        private void SpawnParticle(Vector3 point) // SpawnParticle tại vị trí point. Được gọi khi player match thành công (Event OnMatched)
        {
            //convert to screen position
            point = Camera.main.WorldToScreenPoint(point); // Convert world position thành screen position vì particle này là UI particle.

            int spawnCount = _comboController.ComboCount >= 3 ? 3 : _comboController.ComboCount + 1; // Tính số lượng particle spawn. Combo càng cao thì càng nhiều star

            //Create particle
            ParticleImage particle = Instantiate(starPrefab, transform) // Instantiate prefab particle với parent = transform hiện tại, sau đó lấy component ParticleImage.
                .GetComponent<ParticleImage>();

            particle.AddBurst(0, spawnCount); // Add burst particle. Tham số là (time, amount)
            particle.rectTransform.position = point; // Set vị trí particle.
            particle.attractorTarget = starIcon; // Target hút particle, star sẽ bay về starIcon.
            particle.onAnyParticleFinished.AddListener(EarnStar); //  Khi particle hoàn thành -> EarnStar()
            particle.onParticleStop.AddListener(DestroyParticle); // Khi particle stop -> destroy particle object.
            particle.Play(); // Bắt đầu chạy particle.
            particles.Add(particle); // Lưu particle vào list quản lý.
        }

        public void EarnStar() // Được gọi khi particle tới icon. EarnStar() sẽ tăng _starCount và update UI
        {
            _starCount++;
            _countText.text = _starCount.ToString();
        }

        private void DestroyParticle() // Cleanup particle đầu tiên trong list (remove listener, destroy gameObject, remove khỏi list)
        {
            particles[0].onAnyParticleFinished.RemoveListener(EarnStar);
            particles[0].onParticleStop.RemoveListener(DestroyParticle);
            Destroy(particles[0].gameObject);
            particles.RemoveAt(0);
        }
    }
}