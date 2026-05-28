using UnityEngine;
using UnityEngine.SceneManagement;
using static PlasticPipe.PlasticProtocol.Messages.Serialization.ItemHandlerMessagesSerialization;

namespace GameTemplate.Editor
{
    // Script hỗ trợ workflow khi chạy game trong Unity Editor
    // 
    // Mục đích:
    // - Tự động load scene Startup khi bấm Play
    // - Giúp game luôn khởi động đúng flow
    //
    // Thường dùng để tránh:
    // - Chạy trực tiếp gameplay scene
    // - Thiếu singleton / manager
    // - Thiếu DI initialization
    public class SceneBoot : MonoBehaviour
    {
        // Attribute đặc biệt của Unity
        //
        // Hàm này sẽ tự động được gọi:
        // - Ngay khi game bắt đầu chạy
        // - Trước cả Start/Awake của scene
        //
        // Không cần attach script vào GameObject

        // Khi bấm play trong editor thì sẽ có thứ tự sau:
        // Unity start
        //     ↓
        // RuntimeInitializeOnLoadMethod
        //     ↓
        // SceneBoot.LoadScenes()
        //     ↓
        // Load EditorGameSettings
        //     ↓
        // Check startFromGameScene
        //     ↓
        // Load Startup scene
        [RuntimeInitializeOnLoadMethod]
        public static void LoadScenes()
        {
            // Load ScriptableObject EditorGameSettings từ thư mục: Assets/Resources/Managers/
            //
            // Resources.LoadAll trả về mảng, [0] lấy phần tử đầu tiên

            EditorGameSettings gameSettings = Resources.LoadAll<EditorGameSettings>("Managers")[0];
#if UNITY_EDITOR
            // Chỉ chạy trong Unity Editor
            // Không chạy khi build game

            // Nếu bật option "startFromGameScene"
            // thì tự động load scene Startup
            if (gameSettings.startFromGameScene)
            {
                // Load scene Startup
                //
                // Startup scene thường chứa:
                // - ApplicationController
                // - DI setup
                // - Manager global
                // - Bootstrap logic
                SceneManager.LoadScene("GamePlay");
            }
#endif
        }
    }
}