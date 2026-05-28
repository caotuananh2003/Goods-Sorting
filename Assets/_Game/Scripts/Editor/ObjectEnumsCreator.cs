using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameTemplate._Game.Scripts.Editor
{
    /// <summary>
    /// =========================================================
    /// ObjectEnumsCreator
    /// =========================================================
    ///
    /// Vai trò:
    /// - Tự động generate enum ObjectID từ danh sách ItemType.
    ///
    /// Đây là:
    /// - Editor Tool
    /// - Automation Tool
    /// - Code Generator
    ///
    /// Script này KHÔNG chạy trong build game.
    /// Nó chỉ dùng trong Unity Editor để giảm việc viết enum thủ công
    ///
    /// =========================================================
    /// MỤC ĐÍCH
    /// =========================================================
    ///
    /// Giả sử game có nhiều object:
    ///
    /// Apple
    /// Banana
    /// Orange
    /// Bomb
    ///
    /// Thay vì tự viết:
    ///
    /// public enum ObjectID
    /// {
    ///     Apple,
    ///     Banana,
    ///     Orange,
    ///     Bomb
    /// }
    ///
    /// Script này sẽ:
    /// - đọc danh sách object
    /// - tự generate file enum
    ///
    /// =========================================================
    /// LỢI ÍCH
    /// =========================================================
    ///
    /// - tránh viết tay enum
    /// - tránh typo
    /// - đồng bộ data tự động
    /// - dễ scale project
    ///
    /// Rất phổ biến trong:
    /// - casual game
    /// - inventory systems
    /// - item database
    /// - content pipeline
    ///
    /// =========================================================
    /// CÁCH DÙNG TRONG UNITY
    /// =========================================================
    ///
    /// 1. Tạo ScriptableObject:
    ///
    /// Right Click
    /// → Create
    /// → Scriptable Objects
    /// → Object Enums Creator
    ///
    ///
    /// 2. Kéo thả ItemType vào mảng objects (Các ItemType là các scriptableObject của ItemType, nằm trong Assets/_Game/ScriptableObjects/Objects)
    ///
    /// Ví dụ:
    /// objects[0] = Item_Apple
    /// objects[1] = Item_Banana
    /// objects[2] = Item_Orange
    ///
    ///
    /// 3. Bấm nút:
    /// "Apply Enums"
    ///
    ///
    /// 4. Script sẽ tự generate:
    ///
    /// Assets/_Game/Scripts/Objects/ObjectId.cs
    ///
    ///
    /// =========================================================
    /// KẾT QUẢ FILE GENERATE
    /// =========================================================
    ///
    /// public enum ItemId
    /// {
    ///     Apple,
    ///     Banana,
    ///     Orange
    /// }
    ///
    /// =========================================================
    /// LƯU Ý QUAN TRỌNG
    /// =========================================================
    ///
    /// Tên object phải có dạng:
    ///
    /// Item_Apple 
    /// Item_Banana (Nói chung là có dấu _, ví dụ như 1_Soda, 2_Drink...)
    ///
    /// =========================================================
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectEnumsCreator", menuName = "Scriptable Objects/Object Enums Creator")]
    public class ObjectEnumsCreator : ScriptableObject
    {
        public ItemType[] objects;

#if UNITY_EDITOR
        [Button("Apply Enums")]
        public void Generate()
        {
            /// <summary>
            /// Đường dẫn file enum sẽ được generate.
            ///
            /// File kết quả: Assets/_Game/Scripts/Objects/ItemId.cs
            ///
            /// Nếu folder không tồn tại -> lỗi.
            /// </summary>
            string filePathAndName = "Assets/_Game/Scripts/Objects/ItemId.cs"; //The folder Scripts/Enums/ is expected to exist

            using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
            {
                streamWriter.WriteLine("public enum ItemId");
                streamWriter.WriteLine("{");
                for (int i = 0; i < objects.Length; i++)
                {
                    string objectName = objects[i].name.Split("_")[1];
                    streamWriter.WriteLine("\t" + objectName + ",");
                }
                streamWriter.WriteLine("}");
            }

            /*
             * Báo cho Unity:
             * "có file mới/thay đổi".
             *
             * Unity sẽ:
             * - detect file mới
             * - reimport
             * - recompile scripts
             *
             * Nếu không gọi Refresh():
             * Unity có thể chưa nhận file ngay.
             */
            AssetDatabase.Refresh();
        }
#endif
    }
}