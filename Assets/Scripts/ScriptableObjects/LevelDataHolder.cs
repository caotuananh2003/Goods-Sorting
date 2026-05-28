using GameTemplate.Managers.Scene;
using GameTemplate.Utils;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameTemplate.ScriptableObjects
{
    /// <summary>
    /// Database chứa toàn bộ level của game.
    ///
    /// Đây là:
    /// - level list
    /// - level collection
    /// - level database
    ///
    /// =====================================================
    /// NHIỆM VỤ
    /// =====================================================
    ///
    /// - lưu danh sách level
    /// - xác định level load bằng Scene hay Prefab
    /// - cung cấp dữ liệu cho LevelManager
    ///
    /// =====================================================
    /// FLOW
    /// =====================================================
    ///
    /// LevelManager
    /// ↓
    /// đọc LevelDataHolder
    /// ↓
    /// lấy current level
    /// ↓
    /// load level
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/Level data holder", order = 0)]
    public class LevelDataHolder : ScriptableObject
    {
        /// <summary>
        /// Kiểu level.
        ///
        /// Scene:
        /// - mỗi level là một scene
        ///
        /// Prefab:
        /// - level là prefab instantiate runtime
        /// </summary>
        public LevelTypes levelType;

        /// <summary>
        /// Danh sách toàn bộ level.
        ///
        /// Ví dụ:
        ///
        /// levels[0]
        /// => Level 1
        ///
        /// levels[1]
        /// => Level 2
        /// </summary>
        public LevelData[] levels;
    }
}