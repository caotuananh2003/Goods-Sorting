using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// ScriptableObject chứa các setting chỉ dùng trong Editor.
///
/// Mục đích:
/// - hỗ trợ debug
/// - hỗ trợ test game nhanh
/// - hỗ trợ workflow develop
///
/// File này KHÔNG chứa gameplay runtime chính.
///
/// ====================================================
/// CREATE ASSET MENU
/// ====================================================
///
/// Cho phép tạo asset trong: Chuột phải -> Create -> Scriptable Objects -> EditorGameSettings
///
/// ====================================================
/// THƯỜNG DÙNG CHO
/// ====================================================
///
/// - auto load startup scene
/// - skip menu
/// - debug mode
/// - cheat settings
/// - test settings
/// </summary>
/// 
[CreateAssetMenu(fileName = "EditorGameSettings", menuName = "Scriptable Objects/EditorGameSettings")]
public class EditorGameSettings : ScriptableObject
{
    [TabGroup("Editor Settings")]
    public bool startFromGameScene;    
}