using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameTemplate.Managers.Pool
{
    /// <summary>
    /// =========================================================
    /// SCRIPTABLE POOLING DATABASE
    /// =========================================================
    ///
    /// ScriptableObject chứa toàn bộ cấu hình pool của game. Đây là:
    /// - database
    /// - config asset
    /// - pooling settings container
    ///
    /// =========================================================
    /// NHIỆM VỤ
    /// =========================================================
    ///
    /// Chứa danh sách: PoolObject[], mỗi PoolObject mô tả:
    /// - prefab gì
    /// - preload bao nhiêu
    /// - pool tên gì
    /// - auto return hay không
    ///
    /// =========================================================
    /// ĐƯỢC DÙNG BỞI
    /// =========================================================
    ///
    /// PoolingManager.Initialize()
    ///
    /// PoolingManager sẽ:
    /// - đọc dữ liệu từ ScriptablePooling
    /// - tạo queue
    /// - instantiate object
    /// - build object pool
    ///
    /// =========================================================
    /// VÍ DỤ
    /// =========================================================
    ///
    /// ScriptablePooling asset có:
    /// - Explosion
    /// - HitEffect
    /// - Bullet
    /// PoolingManager sẽ preload:
    /// - Explosion x20
    /// - Bullet x50
    /// - HitEffect x30
    /// </summary>
    [CreateAssetMenu(fileName = "PoolingManager", menuName = "Scriptable Objects/Pooling")] // Cho phép tạo asset từ: Right Click → Create → Scriptable Objects → Pooling
    public class ScriptablePooling : ScriptableObject
    {
        /// <summary>
        /// =====================================================
        /// DANH SÁCH TOÀN BỘ POOL CONFIG
        /// =====================================================
        ///
        /// Mỗi phần tử mô tả một loại pool object. Ví dụ:
        ///
        /// [0] Explosion
        /// [1] Bullet
        /// [2] Coin
        ///
        /// ĐƯỢC DÙNG TRONG PoolingManager.Initialize() để:
        /// - preload object
        /// - tạo Queue
        /// - mapping PoolID
        ///
        /// =====================================================
        /// FLOW
        /// =====================================================
        ///
        /// PoolingManager
        /// ↓
        /// đọc poolObjects
        /// ↓
        /// Instantiate prefab
        /// ↓
        /// SetActive(false)
        /// ↓
        /// enqueue vào Queue
        /// </summary>
        public PoolObject[] poolObjects;

#if UNITY_EDITOR
        /// <summary>
        /// Button editor dùng để TỰ ĐỘNG generate file PoolId.cs
        ///
        /// =====================================================
        /// BUTTON ATTRIBUTE
        /// =====================================================
        ///
        /// [Button("Apply Pool")] sẽ giúp Odin Inspector sẽ hiển thị button "Apply Pool" trong Inspector.
        /// Khi bấm: button này thì Generate() sẽ được gọi.
        ///
        /// Mục đích để Tự động đồng bộ poolObjects và PoolID enum
        ///
        /// =====================================================
        /// VÍ DỤ
        /// =====================================================
        ///
        /// Nếu poolObjects có: Explosion, Bullet, Coin thì Generate() sẽ tạo:
        ///
        /// public enum PoolID
        /// {
        ///     Explosion,
        ///     Bullet,
        ///     Coin
        /// }
        ///
        /// =====================================================
        /// LỢI ÍCH
        /// =====================================================
        ///
        /// Không cần:
        /// - tự viết enum
        /// - tự sync tên
        /// - sửa tay
        ///
        /// tránh:
        /// - typo
        /// - mismatch
        /// - thiếu enum
        /// </summary>
        [Button("Apply Pool")]
        public void Generate()
        {
            /// <summary>
            /// =================================================
            /// ĐƯỜNG DẪN FILE ENUM SẼ ĐƯỢC GENERATE
            /// =================================================
            ///
            /// Kết quả:
            /// File Assets/Scripts/Managers/Pool/PoolId.cs sẽ bị ghi đè mỗi lần Generate()
            ///
            /// </summary>
            string filePathAndName = "Assets/Scripts/Managers/Pool/PoolId.cs"; //The folder Scripts/Enums/ is expected to exist

            /// <summary>
            /// =================================================
            /// STREAM WRITER
            /// =================================================
            ///
            /// Mở file để ghi text.
            ///
            /// using:
            /// - tự động close file
            /// - tự dispose stream
            /// </summary>
            using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
            {
                streamWriter.WriteLine("public enum PoolID");
                streamWriter.WriteLine("{");
                for (int i = 0; i < poolObjects.Length; i++)
                {
                    streamWriter.WriteLine("\t" + poolObjects[i].poolName + ",");
                }
                streamWriter.WriteLine("}");
            }
            /// <summary>
            /// =================================================
            /// REFRESH UNITY ASSET DATABASE
            /// =================================================
            ///
            /// Bắt Unity:
            /// - detect file mới
            /// - reimport asset
            /// - compile lại script
            ///
            /// Nếu không gọi thì Unity sẽ chưa thấy PoolId.cs mới ngay.
            /// </summary>
            AssetDatabase.Refresh();
        }
#endif
    }
}