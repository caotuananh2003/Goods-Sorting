using _Game.Scripts.Timer;
using GameTemplate.ScriptableObjects;
using GameTemplate.Utils;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Managers.Scene
{
    /// <summary>
    /// Định nghĩa kiểu level.
    ///
    /// ====================================================
    /// Scene
    /// ====================================================
    ///
    /// Mỗi level là một Unity Scene riêng.
    ///
    /// Ví dụ:
    /// Level_1.unity
    /// Level_2.unity
    ///
    /// ====================================================
    /// Prefab
    /// ====================================================
    ///
    /// Mỗi level là prefab được instantiate runtime.
    ///
    /// Ví dụ:
    /// LevelPrefab_1
    /// LevelPrefab_2
    /// </summary>
    public enum LevelTypes
    {
        None,
        Scene,
        Prefab
    }

    /// <summary>
    /// Manager chịu trách nhiệm:
    ///
    /// - quản lý level hiện tại
    /// - spawn level
    /// - load level
    /// - next level
    /// - previous level
    ///
    /// Đây là:
    /// - Service
    /// - Level Flow Manager
    ///
    /// Không kế thừa MonoBehaviour.
    ///
    /// Được tạo bởi:
    /// VContainer.
    /// </summary>
    public class LevelManager: IInitializable
    {
        #region Variables

        /// <summary>
        /// Level ID hiện tại.
        ///
        /// Dữ liệu được save/load bằng UserPrefs.
        ///
        /// ====================================================
        /// get
        /// ====================================================
        ///
        /// đọc level hiện tại
        ///
        /// ====================================================
        /// set
        /// ====================================================
        ///
        /// save level hiện tại
        /// </summary>
        private int _levelId
        {
            get => UserPrefs.GetLevelId();
            set => UserPrefs.SetLevelId(value);
        }

        #endregion

        [Inject] SceneLoader _SceneLoader; // Scene loader service.
        [Inject] LevelDataHolder _levelDataHolder; // ScriptableObject chứa toàn bộ level data.

        /// <summary>
        /// Inject dependencies từ VContainer.
        ///
        /// VContainer sẽ tự động:
        /// - resolve dependency
        /// - gọi Construct()
        /// </summary>
        [Inject]
        public void Construct(SceneLoader sceneLoader, LevelDataHolder levelDataHolder)
        {
            Debug.Log("LevelManager constructed");
            _SceneLoader = sceneLoader;
            _levelDataHolder = levelDataHolder;
        }

        public int LevelId // Level ID runtime thật.
        {
            get => _levelId;
        }

        public int UILevelId // Level ID hiển thị UI. Vì player thường thấy bắt đầu từ Level 1 thay vì từ Level 0
        {
            get => _levelId + 1;
        }

        public LevelTypes LevelType // Kiểu load level hiện tại.
        {
            get => _levelDataHolder.levelType;
        }

        private string lastLoadedLevelScene = ""; // Scene level cuối cùng đã load. Dùng để unload scene cũ.
        private GameObject lastLoadedLevelPrefab; // Prefab level cuối cùng đã instantiate.

        public void Initialize() // Callback của IInitializable. VContainer sẽ gọi sau khi object được tạo. Hiện tại chưa dùng.
        {
            
        }
        
        public void SpawnLevel(Transform levelPrefabParent) // Spawn level vào scene. Đây là entry point chính.
        {
            LoadLevel(levelPrefabParent);
        }

        /// <summary>
        /// Load level theo loại level.
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// 1. Lấy current level data
        ///
        /// 2. Kiểm tra level type
        ///
        /// 3A. Nếu Scene:
        ///     - unload scene cũ
        ///     - load additive scene mới
        ///
        /// 3B. Nếu Prefab:
        ///     - instantiate prefab
        ///     - setup timer
        /// </summary>
        public void LoadLevel(Transform levelPrefabParent)
        {
            int currentId = _levelId % _levelDataHolder.levels.Length; // loop level. Ví dụ: 12 % 10 = 2 để giúp level lặp vô hạn
            LevelData currentData = _levelDataHolder.levels[currentId]; // lấy level data hiện tại

            if (_levelDataHolder.levelType == LevelTypes.Scene) // LOAD LEVEL SCENE
            {
                if (lastLoadedLevelScene != "") // unload scene cũ
                {
                    SceneManager.UnloadSceneAsync(lastLoadedLevelScene);
                }
                lastLoadedLevelScene = currentData.levelScene;// lưu scene mới

                // load scene additive
                // additive:
                // load scene chồng lên scene hiện tại
                _SceneLoader.LoadScene(new SceneLoadData(
                    lastLoadedLevelScene, LoadSceneMode.Additive, true, true));
            }
            else
            {
                lastLoadedLevelPrefab = currentData.levelPrefab; // lấy prefab level

                lastLoadedLevelPrefab = Object.Instantiate(lastLoadedLevelPrefab, levelPrefabParent); // instantiate prefab vào parent

                TimerController.OnSetTimer.Invoke(currentData.levelTimer); // setup timer cho level
            }
        }

        public void SetNextLevel()// Sang level tiếp theo, đồng thời save lại.
        {
            _levelId++;
            UserPrefs.SetLevelId(_levelId);
        }

        public void SetPreviousLevel() // Quay về level trước, đồng thời save lại.
        {
            _levelId--;
            UserPrefs.SetLevelId(_levelId);
        }
    }
}