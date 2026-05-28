//using UnityEditor;
//using UnityEngine;
//using GameTemplate.UI;

//[CustomEditor(typeof(BottomNavigationUI))]
//public class BottomNavigationUIEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        GUILayout.Space(10);

//        BottomNavigationUI nav = (BottomNavigationUI)target;

//        GUILayout.Label("Editor Preview", EditorStyles.boldLabel);

//        GUILayout.BeginHorizontal();

//        if (GUILayout.Button("Shop"))
//        {
//            nav.Select(0);
//        }

//        if (GUILayout.Button("Rank"))
//        {
//            nav.Select(1);
//        }

//        if (GUILayout.Button("Main"))
//        {
//            nav.Select(2);
//        }

//        if (GUILayout.Button("Clan"))
//        {
//            nav.Select(3);
//        }

//        if (GUILayout.Button("Collection"))
//        {
//            nav.Select(4);
//        }

//        GUILayout.EndHorizontal();

//        if (!Application.isPlaying)
//        {
//            EditorUtility.SetDirty(nav);
//        }
//    }
//}