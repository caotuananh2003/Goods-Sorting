using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameTemplate.Managers.Pool
{
    /// <summary>
    /// =========================================================
    /// POOL OBJECT DATA
    /// =========================================================
    ///
    /// Class chứa dữ liệu cấu hình cho một loại object pool.
    ///
    /// Đây KHÔNG phải gameplay object runtime.
    ///
    /// Nó chỉ là:
    /// - data config
    /// - thông tin setup pool
    ///
    /// =========================================================
    /// VÍ DỤ
    /// =========================================================
    ///
    /// Một PoolObject có thể đại diện cho:
    ///
    /// - Bullet
    /// - HitEffect
    /// - Explosion
    /// - Coin
    /// - DamageText
    ///
    /// =========================================================
    /// POOLING FLOW
    /// =========================================================
    ///
    /// ScriptablePooling chứa nhiều PoolObject
    ///
    /// PoolingManager.Initialize() đọc từng PoolObject
    ///
    /// rồi:
    /// - instantiate prefab
    /// - tạo queue
    /// - build pool
    ///
    /// =========================================================
    /// VÍ DỤ CONFIG
    /// =========================================================
    ///
    /// objectPrefab     = ExplosionEffect
    /// poolName         = Explosion
    /// objectCount      = 20
    /// goBackOnDisable  = true
    ///
    /// nghĩa là:
    ///
    /// - pool loại Explosion
    /// - preload 20 object
    /// - khi SetActive(false)
    ///   sẽ tự return về pool
    /// </summary>
    [System.Serializable]
    public class PoolObject
    {
        /// <summary>
        /// =====================================================
        /// PREFAB DÙNG CHO POOL
        /// =====================================================
        ///
        /// Prefab sẽ được instantiate để tạo object pool.
        ///
        /// Ví dụ:
        /// - Bullet prefab
        /// - Explosion prefab
        /// - Coin prefab
        ///
        /// =====================================================
        /// ODIN INSPECTOR
        /// =====================================================
        ///
        /// [OnValueChanged("OnPrefabSet")]
        ///
        /// Khi thay đổi prefab trong Inspector:
        /// → tự động gọi:
        ///
        /// OnPrefabSet()
        ///
        /// =====================================================
        /// MỤC ĐÍCH
        /// =====================================================
        ///
        /// Đảm bảo prefab luôn có:
        ///
        /// PoolElement component
        ///
        /// vì PoolingManager cần component này
        /// để:
        ///
        /// - biết PoolId
        /// - return object về pool
        /// - detect parent change
        /// - auto return on disable
        ///
        /// =====================================================
        /// NẾU KHÔNG CÓ PoolElement
        /// =====================================================
        ///
        /// PoolingManager sẽ lỗi:
        ///
        /// GetComponent<PoolElement>()
        ///
        /// => null
        /// </summary>
        [OnValueChanged("OnPrefabSet")]
        public GameObject objectPrefab;

        /// <summary>
        /// =====================================================
        /// TÊN CỦA POOL
        /// =====================================================
        ///
        /// Tên này dùng để generate: PoolID enum
        ///
        /// =====================================================
        /// VÍ DỤ
        /// =====================================================
        ///
        /// poolName = Explosion -> sẽ generate:
        ///
        /// public enum PoolID
        /// {
        ///     Explosion
        /// }
        ///
        /// =====================================================
        /// QUAN TRỌNG
        /// =====================================================
        ///
        /// poolName phải là độc nhất.
        ///
        /// Nếu trùng:
        /// - enum lỗi
        /// - lookup sai
        /// - pool sai object
        /// </summary>
        public string poolName;

        /// <summary>
        /// =====================================================
        /// SỐ LƯỢNG OBJECT PRELOAD
        /// =====================================================
        ///
        /// PoolingManager.Initialize()
        /// sẽ instantiate sẵn số lượng object này.
        ///
        /// =====================================================
        /// VÍ DỤ
        /// =====================================================
        ///
        /// objectCount = 20
        ///
        /// => tạo sẵn 20 object inactive.
        ///
        /// =====================================================
        /// LỢI ÍCH
        /// =====================================================
        ///
        /// Giảm:
        /// - Instantiate runtime
        /// - GC spike
        /// - lag
        /// - stutter
        ///
        /// =====================================================
        /// NẾU KHÔNG ĐỦ OBJECT
        /// =====================================================
        ///
        /// PoolingManager vẫn có thể Instantiate thêm runtime nhưng sẽ tốn performance hơn.
        /// </summary>
        public int objectCount;

        /// <summary>
        /// =====================================================
        /// AUTO RETURN TO POOL
        /// =====================================================
        ///
        /// Nếu true: khi object gọi gameObject.SetActive(false) thì PoolElement sẽ tự GoBackToPool()
        ///
        /// =====================================================
        /// DÙNG CHO
        /// =====================================================
        ///
        /// - particle effect
        /// - temporary object
        /// - popup text
        /// - projectile
        ///
        /// =====================================================
        /// FLOW
        /// =====================================================
        ///
        /// effect.SetActive(false)
        /// ↓
        /// PoolElement.OnDisable()
        /// ↓
        /// GoBackToPool()
        /// ↓
        /// enqueue lại vào Queue
        ///
        /// =====================================================
        /// NẾU FALSE
        /// =====================================================
        ///
        /// Object sẽ không tự return. Dev phải tự gọi:
        /// PoolingManager.Instance.GoBackToPool(...)
        /// </summary>
        public bool goBackOnDisable;

        /// <summary>
        /// =====================================================
        /// AUTO SETUP PREFAB
        /// =====================================================
        ///
        /// Hàm được gọi tự động bởi: [OnValueChanged] khi objectPrefab thay đổi. OnValueChanged là Attribute của Odin Inspector
        ///
        /// =====================================================
        /// MỤC ĐÍCH
        /// =====================================================
        ///
        /// Đảm bảo prefab có: PoolElement component
        ///
        /// =====================================================
        /// FLOW
        /// =====================================================
        ///
        /// 1. User drag prefab vào Inspector
        /// 2. Odin gọi: OnPrefabSet()
        /// 3. Kiểm tra prefab có PoolElement chưa
        /// 4. Nếu chưa có:
        ///    - AddComponent<PoolElement>()
        ///    - save asset
        ///
        /// =====================================================
        /// LỢI ÍCH
        /// =====================================================
        ///
        /// - tránh quên add component
        /// - giảm lỗi runtime
        /// - setup tự động
        /// - workflow nhanh hơn
        ///
        /// =====================================================
        /// UNITY_EDITOR
        /// =====================================================
        ///
        /// Chỉ chạy trong Unity Editor.
        ///
        /// Build game sẽ không compile đoạn này.
        /// </summary>
        public void OnPrefabSet()
        {
#if UNITY_EDITOR
            PoolElement poolElement; // Biến dùng để nhận PoolElement nếu prefab có component này.

            /// <summary>
            /// TryGetComponent: kiểm tra prefab có PoolElement chưa.
            /// Nếu chưa có:
            /// => return false
            /// </summary>
            if (!objectPrefab.TryGetComponent<PoolElement>(out poolElement))
            {
                objectPrefab.AddComponent<PoolElement>(); // Tự động add PoolElement vào prefab.

                EditorUtility.SetDirty(objectPrefab); // Đánh dấu prefab đã thay đổi. Unity Editor cần điều này để biết asset cần save.

                AssetDatabase.SaveAssets(); // Save prefab asset xuống disk.
            }
#endif
        }
    }
}