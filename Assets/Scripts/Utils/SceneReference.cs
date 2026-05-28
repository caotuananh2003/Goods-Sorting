using System;
using System.Linq;
using UnityEngine;

// Mục đích chính

//Cho phép bạn:

//Kéo thả Scene trực tiếp trong Inspector
//Lưu reference scene an toàn
//Tránh phải gõ string tên scene thủ công
//Tự quản lý Build Settings cho scene

//Nói ngắn gọn:

//Đây là một wrapper giúp quản lý scene chuyên nghiệp hơn thay vì dùng string.
// Ví dụ, chúng ta không cần SceneManager.LoadScene("GameScene"); vì nếu gõ sai thì sẽ lỗi, hoặc khi rename scene thì cũng sẽ lỗi.
// Chúng ta có thể dùng phương pháp kéo/thả scene để ref tự động.

// Alias:
// dùng "Object" thay cho UnityEngine.Object
// để tránh trùng tên với object của C#
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
#endif

namespace GameTemplate.Utils
{
    /// <summary>
    /// A wrapper that provides the means to safely serialize Scene Asset References.
    /// </summary>

    /// <summary>
    /// =========================================================
    /// SCENE REFERENCE
    /// =========================================================
    ///
    /// Unity KHÔNG serialize SceneAsset an toàn ở runtime/build.
    ///
    /// Nếu bạn kéo thả Scene trực tiếp:
    /// - runtime có thể mất reference
    /// - build không giữ được SceneAsset editor
    ///
    /// Script này tạo wrapper:
    ///
    /// SceneAsset (Editor)
    ///        ↓
    /// scenePath string
    ///        ↓
    /// runtime dùng path để load scene
    ///
    /// =========================================================
    /// MỤC ĐÍCH
    /// =========================================================
    ///
    /// Cho phép:
    ///
    /// - kéo thả scene trong Inspector
    /// - serialize an toàn
    /// - runtime vẫn load scene được
    /// - editor tự sync SceneAsset <-> scenePath
    ///
    /// =========================================================
    /// FLOW
    /// =========================================================
    ///
    /// DEV kéo Scene vào Inspector
    /// ↓
    /// SceneReference lưu:
    /// - SceneAsset
    /// - scenePath
    /// ↓
    /// Build game
    /// ↓
    /// Runtime KHÔNG còn SceneAsset
    /// ↓
    /// runtime dùng scenePath để load scene
    ///
    /// =========================================================
    /// VÍ DỤ
    /// =========================================================
    ///
    /// levelScene:
    ///
    /// Assets/Scenes/Game.unity
    ///
    /// sceneAsset:
    /// -> reference tới file Game.unity
    ///
    /// scenePath:
    /// -> "Assets/Scenes/Game.unity"
    ///
    /// runtime:
    /// SceneManager.LoadScene(sceneReference.ScenePath)
    /// </summary>
    [Serializable]
    public class SceneReference : ISerializationCallbackReceiver
    {
        // What we use in editor to select the scene
        [SerializeField] private Object sceneAsset; // Reference tới file scene trong editor. Inspector sẽ hiện field này để kéo thả Scene.
        public string SceneName => sceneAsset.name; // Tên scene.
#if UNITY_EDITOR
        /// <summary>
        /// Kiểm tra object hiện tại có phải SceneAsset hợp lệ không.
        ///
        /// Ví dụ:
        /// ✔ Game.unity
        /// ✘ Prefab
        /// ✘ Material
        /// ✘ null
        /// </summary>
        private bool IsValidSceneAsset
        {
            get
            {
                if (!sceneAsset) return false;

                return sceneAsset is SceneAsset;
            }
        }
#endif

        /// <summary>
        /// Đường dẫn scene được serialize. Đây mới là dữ liệu runtime thực sự dùng.
        ///
        /// Ví dụ: "Assets/Scenes/Game.unity"
        ///
        /// Unity build/runtime không giữ SceneAsset, nên phải lưu path string.
        /// </summary>
        [SerializeField]
        private string scenePath = string.Empty; // This should only ever be set during serialization/deserialization!

        /// <summary>
        /// Property truy cập scene path.
        ///
        /// Editor:
        /// -> lấy path trực tiếp từ SceneAsset
        ///
        /// Runtime:
        /// -> dùng scenePath đã serialize
        /// </summary>
        public string ScenePath // Use this when you want to actually have the scene path
        {
            get
            {
#if UNITY_EDITOR
                // In editor we always use the asset's path
                return GetScenePathFromAsset(); // Trong editor, luôn lấy path mới nhất từ asset.
#else
            // At runtime we rely on the stored path value which we assume was serialized correctly at build time.
            // See OnBeforeSerialize and OnAfterDeserialize
            return scenePath; // Runtime/build: không có SceneAsset nên cần dùng scenePath đã lưu.
#endif
            }
            set
            {
                scenePath = value; // update path
#if UNITY_EDITOR
                sceneAsset = GetSceneAssetFromPath(); // editor tự load lại SceneAsset từ path mới.
#endif
            }
        }

        /// <summary>
        /// Cho phép convert SceneReference thành string tự động.
        ///
        /// Ví dụ:
        ///
        /// SceneReference sceneRef;
        ///
        /// string path = sceneRef;
        ///
        /// Unity sẽ tự gọi:
        /// sceneRef.ScenePath
        /// </summary>
        public static implicit operator string(SceneReference sceneReference)
        {
            return sceneReference.ScenePath;
        }

        /// <summary>
        /// Unity gọi trước khi serialize object.
        ///
        /// Dùng để sync:
        ///
        /// SceneAsset -> scenePath
        /// </summary>
        public void OnBeforeSerialize() // Called to prepare this data for serialization. Stubbed out when not in editor.
        {
#if UNITY_EDITOR
            HandleBeforeSerialize();
#endif
        }

        /// <summary>
        /// Unity gọi sau khi deserialize object.
        ///
        /// Dùng để recover:
        ///
        /// scenePath -> SceneAsset
        /// </summary>
        public void OnAfterDeserialize() // Called to set up data for deserialization. Stubbed out when not in editor.
        {
#if UNITY_EDITOR
            // We sadly cannot touch assetdatabase during serialization, so defer by a bit.
            // Không được đụng AssetDatabase trong deserialize, nên delay sang frame editor tiếp theo.
            EditorApplication.update += HandleAfterDeserialize;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Load SceneAsset từ scenePath.
        ///
        /// Ví dụ:
        ///
        /// "Assets/Scenes/Game.unity"
        /// ↓
        /// trả về SceneAsset Game
        /// </summary>
        private SceneAsset GetSceneAssetFromPath()
        {
            return string.IsNullOrEmpty(scenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }

        /// <summary>
        /// Lấy path từ SceneAsset.
        ///
        /// Ví dụ:
        ///
        /// Game.unity
        /// ↓
        /// "Assets/Scenes/Game.unity"
        /// </summary>
        private string GetScenePathFromAsset()
        {
            return sceneAsset == null ? string.Empty : AssetDatabase.GetAssetPath(sceneAsset);
        }

        private void HandleBeforeSerialize() // Sync dữ liệu trước serialize.
        {
            // Asset is invalid but have Path to try and recover from
            // CASE 1: asset mất nhưng còn path
            if (IsValidSceneAsset == false && string.IsNullOrEmpty(scenePath) == false)
            {
                sceneAsset = GetSceneAssetFromPath(); // cố recover asset từ path
                if (sceneAsset == null) scenePath = string.Empty; // path invalid

                EditorSceneManager.MarkAllScenesDirty();  // đánh dấu scene dirty để Unity biết cần save
            }
            // Asset takes precendence and overwrites Path
            // CASE 2: asset hợp lệ
            else
            {
                scenePath = GetScenePathFromAsset(); // asset luôn ưu tiên hơn path
            }
        }

        private void HandleAfterDeserialize() // Recover asset sau deserialize.
        {
            EditorApplication.update -= HandleAfterDeserialize; // unsubscribe

            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (IsValidSceneAsset) return; // asset còn tốt không cần recover

            // Asset is invalid but have path to try and recover from
            if (string.IsNullOrEmpty(scenePath)) return; // không có path

            sceneAsset = GetSceneAssetFromPath(); // recover asset từ path

            // No asset found, path was invalid. Make sure we don't carry over the old invalid path
            if (!sceneAsset) scenePath = string.Empty; // path invalid

            if (!Application.isPlaying) EditorSceneManager.MarkAllScenesDirty(); // mark dirty để Unity refresh editor
        }
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Display a Scene Reference object in the editor.
    /// If scene is valid, provides basic buttons to interact with the scene's role in Build Settings.
    /// </summary>
    /// /// <summary>
    /// Custom Inspector cho SceneReference.
    ///
    /// Cho phép:
    ///
    /// - cung cấp các button để tương tác với scene trong build settings
    /// - kéo thả Scene
    /// - hiển thị trạng thái Build Settings
    /// - add/remove scene khỏi build
    /// - enable/disable scene
    ///
    /// Thay vì Inspector mặc định xấu và khó dùng.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer
    {
        // The exact name of the asset Object variable in the SceneReference object
        private const string sceneAssetPropertyString = "sceneAsset"; // Tên field sceneAsset bên trong đối tượng SceneReference, dùng để tìm SerializedProperty.
        // The exact name of the scene Path variable in the SceneReference object
        private const string scenePathPropertyString = "scenePath"; // Tên field scenePath trong đối tượng SceneReference.

        // UI CONSTANTS
        private static readonly RectOffset boxPadding = EditorStyles.helpBox.padding;
        private const float PAD_SIZE = 0f;
        private const float FOOTER_HEIGHT = 0f;
        private static readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float paddedLine = lineHeight + PAD_SIZE;

        /// <summary>
        /// Draw các thuộc tính của SceneReference trong inspector
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) // Hàm Unity gọi để vẽ Inspector.
        {
            if (property.serializedObject.isEditingMultipleObjects) // Unity không hỗ trợ edit nhiều object
            {
                GUI.Label(position, "Scene multiediting not supported");
                return;
            }
            var sceneAssetProperty = GetSceneAssetProperty(property); // lấy field sceneAsset
             //label.tooltip = "The actual Scene Asset reference.\nOn serialize this is also stored as the asset's path."; // Hiển thị popup khi rê chuột vào field inspector
            var sceneControlID = GUIUtility.GetControlID(FocusType.Passive); // tạo control id cho UI
            EditorGUI.BeginChangeCheck(); // bắt đầu detect thay đổi
            {
                sceneAssetProperty.objectReferenceValue = EditorGUI.ObjectField(position, label, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false); // vẽ ObjectField kéo thả Scene
            }
            if (EditorGUI.EndChangeCheck()) // nếu user đổi scene
            {
                var buildScene = BuildUtils.GetBuildScene(sceneAssetProperty.objectReferenceValue);
                if (buildScene.scene == null) GetScenePathProperty(property).stringValue = string.Empty; // scene chưa có trong build
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) // Chiều cao Inspector field.
        {
            return lineHeight;
        }

        /// <summary>
        /// Vẽ phần thông tin scene trong Inspector.
        ///
        /// ======================================================
        /// MỤC ĐÍCH
        /// ======================================================
        ///
        /// Hiển thị:
        /// - scene có nằm trong Build Settings không
        /// - build index
        /// - scene đang enabled/disabled
        /// - các nút:
        ///     Add
        ///     Remove
        ///     Enable
        ///     Disable
        ///     Settings
        ///
        /// ======================================================
        /// VÍ DỤ UI
        /// ======================================================
        ///
        /// [SceneField]   BuildIndex: 2   [Disable] [Remove] [Settings]
        ///
        /// hoặc:
        ///
        /// [SceneField]   NOT In Build   [Add] [Settings]
        /// </summary>
        private void DrawSceneInfoGUI(Rect position, BuildUtils.BuildScene buildScene, int sceneControlID)
        {
            // Kiểm tra BuildSettings có readonly không.
            //
            // Ví dụ:
            // dùng Plastic SCM / Perforce
            // file chưa checkout
            // => không cho edit.
            var readOnly = BuildUtils.IsReadOnly();
            var readOnlyWarning = readOnly ? "\n\nWARNING: Build Settings is not checked out and so cannot be modified." : ""; // Warning hiển thị thêm trong tooltip.

            // GUIContent:
            // object chứa:
            // - text
            // - icon
            // - tooltip
            //
            // dùng để vẽ UI editor.
            var iconContent = new GUIContent();
            var labelContent = new GUIContent();

            // ======================================================
            // CASE 1:
            // Scene KHÔNG nằm trong Build Settings
            // ======================================================
            //
            // buildIndex = -1
            //
            // nghĩa là:
            // scene sẽ KHÔNG được build vào game.
            //
            // SceneManager.LoadScene()
            // sẽ fail nếu load scene này.
            if (buildScene.buildIndex == -1)
            {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_close"); // icon dấu X
                labelContent.text = "NOT In Build"; // text hiển thị
                labelContent.tooltip = "This scene is NOT in build settings.\nIt will be NOT included in builds."; // tooltip khi hover
            }

            // ======================================================
            // CASE 2:
            // Scene nằm trong Build Settings và ENABLED
            // ======================================================
            else if (buildScene.scene.enabled)
            {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_max"); // icon màu xanh/max
                labelContent.text = "BuildIndex: " + buildScene.buildIndex; // Hiển thị build index. Ví dụ: BuildIndex = 2
                labelContent.tooltip = "This scene is in build settings and ENABLED.\nIt will be included in builds." + readOnlyWarning; // Tooltip mô tả.
            }

            // ======================================================
            // CASE 3:
            // Scene nằm trong Build Settings nhưng DISABLED
            // ======================================================
            else
            {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_min"); // icon minimize
                labelContent.text = "BuildIndex: " + buildScene.buildIndex;
                labelContent.tooltip = "This scene is in build settings and DISABLED.\nIt will be NOT included in builds.";
            }

            // VẼ LABEL BÊN TRÁI
            using (new EditorGUI.DisabledScope(readOnly))
            {
                var labelRect = DrawUtils.GetLabelRect(position); // Lấy rect cho label.
                var iconRect = labelRect; // Rect riêng cho icon.
                iconRect.width = iconContent.image.width + PAD_SIZE; // Set width icon.
                labelRect.width -= iconRect.width; // Label bắt đầu sau icon.
                labelRect.x += iconRect.width;
                EditorGUI.PrefixLabel(iconRect, sceneControlID, iconContent); // Vẽ icon.
                EditorGUI.PrefixLabel(labelRect, sceneControlID, labelContent); // Vẽ text label.
            }

            // BUTTON BÊN PHẢI
            var buttonRect = DrawUtils.GetFieldRect(position); // Rect phần button.
            buttonRect.width = (buttonRect.width) / 3; // Chia thành 3 nút.

            var tooltipMsg = "";
            using (new EditorGUI.DisabledScope(readOnly))
            {
                if (buildScene.buildIndex == -1) // Scene chưa có trong Build Settings
                {
                    buttonRect.width *= 2; // Nút Add chiếm 2 phần width.
                    var addIndex = EditorBuildSettings.scenes.Length; // build index mới sẽ là cuối list.

                    tooltipMsg = "Add this scene to build settings. It will be appended to the end of the build scenes as buildIndex: " + addIndex + "." + readOnlyWarning;
                    if (DrawUtils.ButtonHelper(buttonRect, "Add...", "Add (buildIndex " + addIndex + ")", EditorStyles.miniButtonLeft, tooltipMsg)) // Nếu bấm Add
                        BuildUtils.AddBuildScene(buildScene); // Add scene vào Build Settings.
                    buttonRect.width /= 2; // Trả lại width cũ.
                    buttonRect.x += buttonRect.width; // Move sang phải.
                }
                else // Scene đã nằm trong Build Settings
                {
                    var isEnabled = buildScene.scene.enabled;
                    var stateString = isEnabled ? "Disable" : "Enable"; // Text nút là Disable hay Enable
                    tooltipMsg = stateString + " this scene in build settings.\n" + (isEnabled ? "It will no longer be included in builds" : "It will be included in builds") + "." + readOnlyWarning;

                    if (DrawUtils.ButtonHelper(buttonRect, stateString, stateString + " In Build", EditorStyles.miniButtonLeft, tooltipMsg)) // Nút Enable/Disable
                        BuildUtils.SetBuildSceneState(buildScene, !isEnabled);
                    buttonRect.x += buttonRect.width; // Move sang nút tiếp theo.

                    tooltipMsg = "Completely remove this scene from build settings.\nYou will need to add it again for it to be included in builds!" + readOnlyWarning;
                    if (DrawUtils.ButtonHelper(buttonRect, "Remove...", "Remove from Build", EditorStyles.miniButtonMid, tooltipMsg)) // Nếu bấm Remove
                        BuildUtils.RemoveBuildScene(buildScene);
                }
            }

            // ======================================================
            // Nút Settings
            // ======================================================
            buttonRect.x += buttonRect.width;

            tooltipMsg = "Open the 'Build Settings' Window for managing scenes." + readOnlyWarning;
            if (DrawUtils.ButtonHelper(buttonRect, "Settings", "Build Settings", EditorStyles.miniButtonRight, tooltipMsg))
            {
                BuildUtils.OpenBuildSettings(); // Mở Build Settings window.
            }

        }

        /// <summary>
        /// Lấy SerializedProperty của field: sceneAsset
        ///
        /// ======================================================
        /// VÍ DỤ
        /// ======================================================
        ///
        /// [SerializeField]
        /// private Object sceneAsset;
        ///
        /// Hàm này tìm property đó trong serialized object.
        /// </summary>
        private static SerializedProperty GetSceneAssetProperty(SerializedProperty property) // Tìm SerializedProperty sceneAsset.
        {
            return property.FindPropertyRelative(sceneAssetPropertyString);
        }

        /// <summary>
        /// Lấy SerializedProperty của field: scenePath
        ///
        /// ======================================================
        /// VÍ DỤ
        /// ======================================================
        ///
        /// [SerializeField]
        /// private string scenePath;
        /// Hàm này tìm property đó trong serialized object.
        /// </summary>
        private static SerializedProperty GetScenePathProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative(scenePathPropertyString);
        }

        /// <summary>
        /// Helper class hỗ trợ vẽ UI trong custom inspector.
        ///
        /// Chứa utility:
        /// - vẽ button
        /// - chia rect
        /// - tính label rect
        /// - tính field rect
        /// </summary>
        private static class DrawUtils
        {
            /// <summary>
            /// Draw a GUI button, choosing between a short and a long button text based on if it fits
            /// Vẽ GUI button, chọn giữa text button ngắn hay text button dài dựa trên việc nó có vừa không.
            /// ======================================================
            /// VÍ DỤ
            /// ======================================================
            ///
            /// đủ chỗ: [Remove from Build]
            /// thiếu chỗ: [Remove...]
            /// </summary>
            public static bool ButtonHelper(Rect position, string msgShort, string msgLong, GUIStyle style, string tooltip = null)
            {
                var content = new GUIContent(msgLong) { tooltip = tooltip };

                var longWidth = style.CalcSize(content).x; // Tính độ rộng text.
                if (longWidth > position.width) content.text = msgShort; // Nếu text dài quá thì dùng text ngắn

                return GUI.Button(position, content, style); // Vẽ button.
            }

            /// <summary>
            /// Given a position rect, get its field portion
            /// </summary>
            public static Rect GetFieldRect(Rect position)
            {
                position.width -= EditorGUIUtility.labelWidth;
                position.x += EditorGUIUtility.labelWidth;
                return position;
            }
            /// <summary>
            /// Given a position rect, get its label portion
            /// </summary>
            public static Rect GetLabelRect(Rect position)
            {
                position.width = EditorGUIUtility.labelWidth - PAD_SIZE;
                return position;
            }
        }

        /// <summary>
        /// Utility class xử lý toàn bộ logic liên quan tới:
        ///
        /// ======================================================
        /// BUILD SETTINGS
        /// ======================================================
        ///
        /// Bao gồm:
        /// - kiểm tra readonly
        /// - lấy thông tin scene trong build
        /// - add scene vào build
        /// - remove scene khỏi build
        /// - enable/disable scene
        /// - mở Build Settings window
        ///
        /// ======================================================
        /// BUILD SETTINGS LÀ GÌ?
        /// ======================================================
        ///
        /// File:
        /// File -> Build Settings
        ///
        /// chứa danh sách scene sẽ được build vào game.
        ///
        /// Ví dụ:
        ///
        /// Build Settings
        /// ├── MainMenu
        /// ├── GameScene
        /// └── LoadingScene
        ///
        /// Nếu scene KHÔNG nằm trong đây:
        /// SceneManager.LoadScene()
        /// sẽ không load được trong build game.
        /// </summary>
        private static class BuildUtils
        {
            public static float minCheckWait = 3; // time in seconds that we have to wait before we query again when IsReadOnly() is called.

            private static float lastTimeChecked; // Thời điểm cuối cùng đã query readonly state. Dùng để tính xem đã quá minCheckWait chưa
            private static bool cachedReadonlyVal = true; // true: BuildSettings readonly; false: có thể edit.

            /// <summary>
            /// Struct chứa thông tin của 1 scene trong Build Settings.
            /// </summary>
            public struct BuildScene
            {
                public int buildIndex;
                public GUID assetGUID;
                public string assetPath;
                public EditorBuildSettingsScene scene;
            }

            /// <summary>
            /// Kiểm tra BuildSettings có readonly không. 
            /// Trong team project thì BuildSettings có thể bị block/ chưa checkout/ readonly... thì sẽ không được phép edit.
            /// nếu cache chưa hết hạn thì return cache, nếu hết hạn thì query Version Control -> cache kết quả -> return
            /// Caches value and only queries state a max of every 'minCheckWait' seconds.
            /// </summary>
            public static bool IsReadOnly()
            {
                var curTime = Time.realtimeSinceStartup; // thời gian realtime hiện tại
                var timeSinceLastCheck = curTime - lastTimeChecked; // đã bao lâu từ lần check trước

                if (!(timeSinceLastCheck > minCheckWait)) return cachedReadonlyVal; // Nếu chưa quá thời gian cache thì dùng cache luôn

                lastTimeChecked = curTime; // Update thời gian check.
                cachedReadonlyVal = QueryBuildSettingsStatus(); // Query thật sự.

                return cachedReadonlyVal;
            }

            /// <summary>
            /// Query trực tiếp Version Control để kiểm tra BuildSettings có readonly không.
            /// Hàm này khá nặng, vì vậy chỉ gọi gián tiếp qua IsReadOnly() để tận dụng cache.
            /// A blocking call to the Version Control system to see if the build settings asset is readonly.
            /// Use BuildSettingsIsReadOnly for version that caches the value for better responsivenes.
            /// </summary>
            private static bool QueryBuildSettingsStatus()
            {
                // If no version control provider, assume not readonly
                if (!Provider.enabled) return false; // Nếu không dùng Version Control thì cho phép edit.

                // If we cannot checkout, then assume we are not readonly
                if (!Provider.hasCheckoutSupport) return false; //Nếu provider không hỗ trợ checkout thì cho phép edit.

                //// If offline (and are using a version control provider that requires checkout) we cannot edit.
                //if (UnityEditor.VersionControl.Provider.onlineState == UnityEditor.VersionControl.OnlineState.Offline)
                //    return true;

                // Try to get status for file
                var status = Provider.Status("ProjectSettings/EditorBuildSettings.asset", false); // Query trạng thái file BuildSettings.
                status.Wait(); // Chờ query hoàn tất.

                // If no status listed we can edit
                if (status.assetList == null || status.assetList.Count != 1) return true; // Không lấy được status thì coi như readonly.

                // If is checked out, we can edit
                return !status.assetList[0].IsState(Asset.States.CheckedOutLocal); // Nếu file đã checkout thì có thể edit.
            }

            /// <summary>
            /// Lấy thông tin scene trong Build Settings.
            /// return ra BuildScene:
            /// - buildIndex
            /// - path
            /// - guid
            /// - enabled
            ///
            /// FLOW:
            /// 
            /// scene asset
            /// ↓
            /// lấy path
            /// ↓
            /// lấy GUID
            /// ↓
            /// duyệt BuildSettings
            /// ↓
            /// tìm scene trùng GUID
            /// ↓
            /// return BuildScene
            /// </summary>
            public static BuildScene GetBuildScene(Object sceneObject)
            {
                var entry = new BuildScene // Mặc định là scene chưa nằm trong build.
                {
                    buildIndex = -1,
                    assetGUID = new GUID(string.Empty)
                };

                if (sceneObject as SceneAsset == null) return entry; // Không phải SceneAsset.

                entry.assetPath = AssetDatabase.GetAssetPath(sceneObject); // Lấy path scene.
                entry.assetGUID = new GUID(AssetDatabase.AssetPathToGUID(entry.assetPath)); // Convert path -> GUID.

                var scenes = EditorBuildSettings.scenes; // Lấy list scene trong Build Settings.
                for (var index = 0; index < scenes.Length; ++index) // Duyệt toàn bộ scene.
                {
                    if (!entry.assetGUID.Equals(scenes[index].guid)) continue; // Nếu GUID không trùng thì bỏ qua.

                    entry.scene = scenes[index]; // Khi tìm thấy.
                    entry.buildIndex = index;
                    return entry;
                }

                return entry; // Không tìm thấy thì return entry.
            }

            /// /// <summary>
            /// Enable hoặc Disable scene trong Build Settings.
            ///
            /// VÍ DỤ:
            ///
            /// enabled = true
            /// → scene được build
            ///
            /// enabled = false
            /// → scene không được build
            /// </summary>
            public static void SetBuildSceneState(BuildScene buildScene, bool enabled)
            {
                var modified = false;
                var scenesToModify = EditorBuildSettings.scenes; // Copy danh sách scene.
                foreach (var curScene in scenesToModify.Where(curScene => curScene.guid.Equals(buildScene.assetGUID))) // Duyệt scene.
                {
                    curScene.enabled = enabled; // Set enabled state.
                    modified = true;
                    break;
                }
                if (modified) EditorBuildSettings.scenes = scenesToModify; // Apply thay đổi.
            }

            /// /// <summary>
            /// Hiển thị Dialog (Hộp thoại) để add scene vào Build Settings.
            ///
            /// FLOW:
            ///
            /// show dialog
            /// ↓
            /// user chọn:
            ///     enabled
            ///     disabled
            ///     cancel
            /// ↓
            /// tạo EditorBuildSettingsScene
            /// ↓
            /// add vào list
            /// ↓
            /// apply BuildSettings
            /// </summary>
            public static void AddBuildScene(BuildScene buildScene, bool force = false, bool enabled = true)
            {
                if (force == false)
                {
                    var selection = EditorUtility.DisplayDialogComplex(
                        "Add Scene To Build",
                        "You are about to add scene at " + buildScene.assetPath + " To the Build Settings.",
                        "Add as Enabled", // option 0
                        "Add as Disabled", // option 1
                        "Cancel (do nothing)"); // option 2

                    switch (selection)
                    {
                        case 0: // enabled
                            enabled = true;
                            break;
                        case 1: // disabled
                            enabled = false;
                            break;
                        default:
                            //case 2: // cancel
                            return;
                    }
                }

                var newScene = new EditorBuildSettingsScene(buildScene.assetGUID, enabled);
                var tempScenes = EditorBuildSettings.scenes.ToList();
                tempScenes.Add(newScene);
                EditorBuildSettings.scenes = tempScenes.ToArray();
            }


            /// <summary>
            /// Hiển thị Dialog (Hộp thoại) để Remove scene khỏi Build Settings hoặc disable scene.
            ///
            /// USER CÓ THỂ:
            ///
            /// - remove hoàn toàn
            /// - chỉ disable
            /// - cancel
            ///
            /// LƯU Ý:
            ///
            /// KHÔNG xóa file scene.
            ///
            /// Chỉ xóa khỏi Build Settings.
            /// </summary>
            public static void RemoveBuildScene(BuildScene buildScene, bool force = false)
            {
                var onlyDisable = false;
                if (force == false)
                {
                    var selection = -1;

                    var title = "Remove Scene From Build";
                    var details = $"You are about to remove the following scene from build settings:\n    {buildScene.assetPath}\n    buildIndex: {buildScene.buildIndex}\n\nThis will modify build settings, but the scene asset will remain untouched.";
                    var confirm = "Remove From Build";
                    var alt = "Just Disable";
                    var cancel = "Cancel (do nothing)";

                    if (buildScene.scene.enabled)
                    {
                        details += "\n\nIf you want, you can also just disable it instead.";
                        selection = EditorUtility.DisplayDialogComplex(title, details, confirm, alt, cancel);
                    }
                    else
                    {
                        selection = EditorUtility.DisplayDialog(title, details, confirm, cancel) ? 0 : 2;
                    }

                    switch (selection)
                    {
                        case 0: // remove
                            break;
                        case 1: // disable
                            onlyDisable = true;
                            break;
                        default:
                            //case 2: // cancel
                            return;
                    }
                }

                // User chose to not remove, only disable the scene
                if (onlyDisable)
                {
                    SetBuildSceneState(buildScene, false);
                }
                // User chose to fully remove the scene from build settings
                else
                {
                    var tempScenes = EditorBuildSettings.scenes.ToList();
                    tempScenes.RemoveAll(scene => scene.guid.Equals(buildScene.assetGUID));
                    EditorBuildSettings.scenes = tempScenes.ToArray();
                }
            }

            /// /// <summary>
            /// Mở cửa sổ File -> Build Settings mặc định của Unity
            ///
            /// VÍ DỤ:
            ///
            /// dùng khi user bấm:
            /// [Settings]
            /// </summary>
            public static void OpenBuildSettings()
            {
                EditorWindow.GetWindow(typeof(BuildPlayerWindow));
            }
        }
    }

#endif
}