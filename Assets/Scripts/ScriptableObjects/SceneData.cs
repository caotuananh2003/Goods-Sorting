using System.Collections.Generic;
using GameTemplate.Managers;
using GameTemplate.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Database chứa mapping:
    ///
    /// SceneType
    /// =>
    /// tên scene trong Unity
    ///
    /// =====================================================
    /// VÍ DỤ
    /// =====================================================
    ///
    /// MainMenu => "MainMenuScene"
    /// Game     => "GameplayScene"
    ///
    /// =====================================================
    /// TẠI SAO CẦN?
    /// =====================================================
    ///
    /// Tránh hardcode:
    ///
    /// LoadScene("GameScene");
    ///
    /// vì:
    /// - dễ typo
    /// - khó maintain
    /// - rename scene dễ lỗi
    ///
    /// Thay vào đó:
    ///
    /// LoadSceneByType(SceneType.Game); (SceneType là enum nằm trong Assets/Scripts/Managers/SceneLoader
    ///
    /// =====================================================
    /// SerializedScriptableObject
    /// =====================================================
    ///
    /// Unity mặc định KHÔNG serialize Dictionary. Odin Inspector cung cấp SerializedScriptableObject để serialize Dictionary trong Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneData", menuName = "Scriptable Objects/Scene Data", order = 0)]
    public class SceneData : SerializedScriptableObject
    {
        /// <summary>
        /// Dictionary mapping:
        ///
        /// SceneType => scene name
        ///
        /// Ví dụ:
        /// 
        /// scenes[SceneType.Game] => "GameScene"
        /// 
        /// (Cái này phải tự nhập tay giá trị scene name trong Inspector)
        /// </summary>
        public Dictionary<SceneType, string> scenes = new Dictionary<SceneType, string>();
    }
}