using GameTemplate.Utils;
using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Dữ liệu của một level.
    ///
    /// Đây là:
    /// - level config
    /// - level info
    /// - level metadata
    ///
    /// =====================================================
    /// MỖI LevelData đại diện cho:
    /// =====================================================
    ///
    /// 1 level gameplay.
    ///
    /// Ví dụ:
    ///
    /// Level 1
    /// - timer = 60
    /// - prefab = Level_01
    ///
    /// =====================================================
    /// TẠI SAO DÙNG SCRIPTABLEOBJECT?
    /// =====================================================
    ///
    /// Vì:
    /// - dễ config trong inspector
    /// - tách data khỏi code
    /// - reusable
    /// - designer-friendly
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/Level data", order = 0)]
    public class LevelData : ScriptableObject
    {
        /// <summary>
        /// Thời gian level.
        ///
        /// Đơn vị:
        /// giây
        ///
        /// Ví dụ:
        /// 60 = 60 giây
        /// </summary>
        public int levelTimer;

        /// <summary>
        /// Prefab level gameplay.
        ///
        /// Dùng khi:
        ///
        /// levelType == Prefab
        ///
        /// Runtime:
        ///
        /// Instantiate(levelPrefab)
        /// </summary>
        public GameObject levelPrefab;

        /// <summary>
        /// Scene level gameplay.
        ///
        /// Dùng khi:
        ///
        /// levelType == Scene
        ///
        /// Runtime:
        ///
        /// SceneManager.LoadScene(levelScene)
        ///
        /// =====================================================
        /// SceneReference
        /// =====================================================
        ///
        /// Wrapper an toàn cho scene asset.
        ///
        /// Tốt hơn dùng string trực tiếp.
        /// </summary>
        public SceneReference levelScene;
    }
}