using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts;
using Cysharp.Threading.Tasks;
using GameTemplate._Game.Scripts;
using GameTemplate._Game.Scripts.Match;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace GameTemplate.Managers.Scene
{
    /// <summary>
    /// Component đại diện cho gameplay level prefab. Gắn trên root object của level.
    ///
    /// Nhiệm vụ:
    /// - kiểm tra win
    /// - kiểm tra lose
    /// - hỗ trợ generate level editor
    /// - hỗ trợ blocker system
    ///
    /// ====================================================
    /// GAME FLOW
    /// ====================================================
    ///
    /// Player move item
    /// ↓
    /// CheckLevelOver()
    /// ↓
    /// nếu hết object => WIN
    ///
    /// hoặc:
    ///
    /// CheckAllFirstFilled()
    /// ↓
    /// nếu hết move => LOSE
    /// </summary>
    public class LevelPrefab : MonoBehaviour, IDisposable
    {
        /// <summary>
        /// Danh sách QueueObject trong level.
        ///
        /// QueueObject thường là:
        /// - slot chứa item
        /// - object gameplay
        /// </summary>
        private List<QueueObject> _queueObjects;

        /// <summary>
        /// Event báo game kết thúc.
        ///
        /// bool #1:
        /// - true  = win
        /// - false = lose
        ///
        /// bool #2:
        /// - true  = all lines filled
        /// - false = normal finish
        ///
        /// ====================================================
        /// LISTENER
        /// ====================================================
        ///
        /// Trong GameSceneState đang dùng:
        ///
        /// LevelPrefab.OnGameFinished += OnGameFinished;
        /// </summary>
        public static event Action<bool, bool> OnGameFinished;

        public async UniTaskVoid CheckLevelOver() // Kiểm tra level đã hoàn thành chưa. Nếu toàn bộ QueueObject đã clear thì là WIN
        {
            await UniTask.Delay(200); // chờ effect/pop animation xong

            List<QueueObject> queueObjects = GetComponentsInChildren<QueueObject>() 
                .Where(x => x.ItemTypeAsset != null).ToList();  // lấy toàn bộ QueueObject còn item

            if (queueObjects.Count == 0) // nếu không còn object
            {
                OnGameFinished?.Invoke(true, false); // (Game Finished Win, isAllLinesFilled Text = false - Không kích hoạt)
            }
        }

        public async UniTaskVoid CheckAllFirstFilled() // Kiểm tra toàn bộ hàng đầu đã full chưa. Nếu tất cả đều full => player không còn move => LOSE
        {
            await UniTask.Delay(200);  // chờ effect/pop animation xong

            bool allFilled = true;

            List<MatchGroup> matchGroups = GetComponentsInChildren<MatchGroup>().ToList(); // chờ effect/pop animation xong

            foreach (var mg in matchGroups)
            {
                if (mg.HasBlocker) // bỏ qua blocker
                {
                    continue;
                }

                if (!mg.IsAllFirstFilled()) // nếu còn slot trống
                {
                    allFilled = false;
                }
            }

            //Debug.LogError("all filled = " + allFilled); // Thử in ra để debug
            if (allFilled) // Nếu tất cả đã full rồi thì phát event báo LOSE
            {
                OnGameFinished?.Invoke(false, true); // Game Finished LOSE
            }
        }

        /// <summary>
        /// IDisposable cleanup.
        ///
        /// SuppressFinalize: báo cho GC rằng không cần finalize nữa.
        ///
        /// Hiện tại gần như không cần thiết vì class này không có unmanaged resource.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

//#if UNITY_EDITOR
//        [Header("Editor Level creation settings")]
//        public GameObject BlockPrefab; // Prefab blocker dùng để khóa group.

//        [Space] public List<int> BlockGroupStartCounts = new List<int>(); // Số lượng block mỗi group.

//        public List<ItemType> LevelObjectTypes = new List<ItemType>(); // Danh sách item type được spawn.

//        List<ItemType> forSpawn = new List<ItemType>(); // Danh sách tạm để spawn runtime editor.

//        /// <summary>
//        /// Tool editor: tự động generate random level. Chỉ chạy trong editor.
//        /// </summary>
//        [ContextMenu("Create random level")]
//        private void CreateRandomLevel()
//        {
//            forSpawn.Clear(); // reset spawn list

//            Debug.Log("Initalize Level: " + gameObject.name);

//            for (int i = 0; i < LevelObjectTypes.Count; i++) // Tạo List Item để spawn cho mỗi loại
//            {
//                for (int j = 0; j < 3; j++) // Mỗi loại item tạo 3 object để đủ match 3
//                {
//                    forSpawn.Add(LevelObjectTypes[i]);
//                }
//            }

//            // random item order
//            forSpawn = ShuffleListWithOrderBy(forSpawn);

//            // create enough queue object for prefab
//            List<MatchGroup> matchGroups = GetComponentsInChildren<MatchGroup>().ToList(); // Tạo đủ queue object cho prefab

//            int emptyQueueCount = matchGroups.Count;
//            emptyQueueCount *= 2;
//            emptyQueueCount += forSpawn.Count;
//            emptyQueueCount -= (emptyQueueCount % 3); // đảm bảo chia hết cho 3

//            int totalSpawnCount = 0;
//            for (int j = 0; j < 10; j++) // spawn rows
//            {
//                for (int i = 0; i < matchGroups.Count; i++)
//                {
//                    matchGroups[i].SpawnRow();
//                    totalSpawnCount += 3;

//                    Debug.Log(totalSpawnCount + " // " + emptyQueueCount);
//                    if (totalSpawnCount == emptyQueueCount)
//                    {
//                        break;
//                    }
//                }

//                if (totalSpawnCount == emptyQueueCount)
//                {
//                    break;
//                }
//            }

//            _queueObjects = transform.GetComponentsInChildren<QueueObject>().ToList(); // RANDOM QUEUE OBJECT
//            _queueObjects = ShuffleListWithOrderBy(_queueObjects);

//            for (int i = 0; i < forSpawn.Count; i++) // SPAWN OBJECTS
//            {
//                _queueObjects[i].SpawnObjectEditor(forSpawn[i]);
//            }

//            FixAllInteractableStates(); //Fix all interactable states

//            // CREATE BLOCKERS
//            matchGroups = ShuffleListWithOrderBy(matchGroups);
//            for (var i = 0; i < BlockGroupStartCounts.Count; i++)
//            {
//                var blockCount = BlockGroupStartCounts[i];
//                GameObject prefab =
//                    PrefabUtility.InstantiatePrefab(BlockPrefab, matchGroups[i].transform) as GameObject;
//                prefab.GetComponent<GroupBlocker>().Initialize(blockCount);
//                matchGroups[i].HasBlocker = true;
//                matchGroups[i].CloseAllInteractables();
//            }
//        }

//        /// <summary>
//        /// Fix lại trạng thái interactable của toàn bộ QueueObject trong level.
//        ///
//        /// ====================================================
//        /// VẤN ĐỀ
//        /// ====================================================
//        ///
//        /// Sau khi:
//        /// - spawn object
//        /// - random level
//        /// - tạo blocker
//        /// - thay đổi queue
//        ///
//        /// thì trạng thái interactable có thể bị sai.
//        ///
//        /// Ví dụ:
//        /// - object đáng lẽ click được nhưng lại bị disable
//        /// - object bị che nhưng vẫn click được
//        ///
//        /// ====================================================
//        /// CÁCH HOẠT ĐỘNG
//        /// ====================================================
//        ///
//        /// 1. Lấy toàn bộ QueueObject trong level
//        ///
//        /// 2. Loop từng QueueObject
//        ///
//        /// 3. Gọi:
//        ///    SetInteractState()
//        ///
//        /// để QueueObject tự kiểm tra:
//        /// - có bị block không
//        /// - có object phía trước không
//        /// - có được phép click không
//        ///
//        /// ====================================================
//        /// THƯỜNG DÙNG SAU:
//        /// ====================================================
//        ///
//        /// - generate level editor
//        /// - clear blocker
//        /// - spawn row mới
//        /// - thay đổi queue
//        ///
//        /// ====================================================
//        /// VÍ DỤ
//        /// ====================================================
//        ///
//        /// Queue:
//        ///
//        /// [A]
//        /// [B]
//        /// [C]
//        ///
//        /// Chỉ A được click.
//        ///
//        /// Nếu A biến mất:
//        ///
//        /// [ ]
//        /// [B]
//        /// [C]
//        ///
//        /// cần update:
//        /// B => interactable
//        /// </summary>
//        public void FixAllInteractableStates()
//        {
//            Debug.Log("LevelPrefab.FixAllInteractableStates");
//            List<QueueObject> queueObjects = transform.GetComponentsInChildren<QueueObject>().ToList(); // Lấy toàn bộ QueueObject trong level

//            foreach (var queueObject in queueObjects) // Update interact state cho từng object
//            {
//                queueObject.SetInteractState();
//            }
//        }

//        /// <summary>
//        /// Tool editor dùng để xóa toàn bộ level hiện tại.
//        ///
//        /// ====================================================
//        /// CHỈ CHẠY TRONG UNITY EDITOR
//        /// ====================================================
//        ///
//        /// Hàm này KHÔNG dùng runtime.
//        ///
//        /// Dùng khi:
//        /// - regenerate level
//        /// - reset level editor
//        /// - test level generation
//        ///
//        /// ====================================================
//        /// CONTEXT MENU
//        /// ====================================================
//        ///
//        /// Vì có:
//        ///
//        /// [ContextMenu("Clear level")]
//        ///
//        /// nên trong Inspector:
//        ///
//        /// Right Click Component
//        /// hoặc nút ba chấm
//        ///
//        /// sẽ có:
//        ///
//        /// "Clear level"
//        ///
//        /// ====================================================
//        /// FLOW
//        /// ====================================================
//        ///
//        /// 1. Tìm toàn bộ QueueObject
//        ///
//        /// 2. DestroyImmediate toàn bộ
//        ///
//        /// 3. Tìm toàn bộ GroupBlocker
//        ///
//        /// 4. DestroyImmediate toàn bộ
//        ///
//        /// ====================================================
//        /// TẠI SAO DÙNG DestroyImmediate?
//        /// ====================================================
//        ///
//        /// Vì:
//        /// - đang chạy editor tool
//        /// - không phải runtime gameplay
//        ///
//        /// Destroy():
//        /// - chỉ destroy cuối frame
//        ///
//        /// DestroyImmediate():
//        /// - destroy ngay lập tức
//        ///
//        /// ====================================================
//        /// KẾT QUẢ
//        /// ====================================================
//        ///
//        /// Level trở về trạng thái rỗng.
//        ///
//        /// Không còn:
//        /// - object
//        /// - blocker
//        /// - queue content
//        /// </summary>
//        [ContextMenu("Clear level")]
//        public void ClearLevel()
//        {
//            List<QueueObject> queueObjects = transform.GetComponentsInChildren<QueueObject>().ToList(); // Lấy toàn bộ QueueObject trong level

//            foreach (var queueObject in queueObjects) // Xóa toàn bộ QueueObject ngay lập tức
//            {
//                DestroyImmediate(queueObject.gameObject);
//            }

//            List<GroupBlocker> groupBlockers = transform.GetComponentsInChildren<GroupBlocker>().ToList(); // Lấy toàn bộ blocker trong level

//            foreach (var groupBlocker in groupBlockers) // Xóa toàn bộ blocker
//            {
//                DestroyImmediate(groupBlocker.gameObject);
//            }
//        }

//        /// <summary>
//        /// Hàm generic dùng để random/shuffle List.
//        ///
//        /// ====================================================
//        /// GENERIC <T>
//        /// ====================================================
//        ///
//        /// T nghĩa là:
//        /// "kiểu dữ liệu bất kỳ"
//        ///
//        /// Có thể dùng cho:
//        ///
//        /// List<int>
//        /// List<GameObject>
//        /// List<QueueObject>
//        /// List<ItemType>
//        ///
//        /// ====================================================
//        /// CÁCH HOẠT ĐỘNG
//        /// ====================================================
//        ///
//        /// Dùng:
//        ///
//        /// LINQ OrderBy()
//        ///
//        /// với random key:
//        ///
//        /// random.Next()
//        ///
//        /// ====================================================
//        /// VÍ DỤ
//        /// ====================================================
//        ///
//        /// Trước:
//        ///
//        /// [A, B, C, D]
//        ///
//        /// Sau shuffle:
//        ///
//        /// [C, A, D, B]
//        ///
//        /// ====================================================
//        /// FLOW
//        /// ====================================================
//        ///
//        /// 1. Tạo object Random
//        ///
//        /// 2. OrderBy random.Next()
//        ///
//        /// 3. Mỗi phần tử được gán random number
//        ///
//        /// 4. Sort theo random number
//        ///
//        /// 5. Trả về list đã random
//        ///
//        /// ====================================================
//        /// LƯU Ý
//        /// ====================================================
//        ///
//        /// Đây KHÔNG phải thuật toán shuffle tối ưu.
//        ///
//        /// Với list lớn:
//        /// - Fisher-Yates shuffle sẽ tốt hơn
//        ///
//        /// Nhưng với:
//        /// - level editor
//        /// - list nhỏ
//        ///
//        /// thì cách này đủ dùng và code ngắn.
//        ///
//        /// ====================================================
//        /// VÍ DỤ DÙNG TRONG PROJECT
//        /// ====================================================
//        ///
//        /// Random item spawn:
//        ///
//        /// forSpawn = ShuffleListWithOrderBy(forSpawn);
//        ///
//        /// Random queue:
//        ///
//        /// _queueObjects = ShuffleListWithOrderBy(_queueObjects);
//        ///
//        /// Random blocker position:
//        ///
//        /// matchGroups = ShuffleListWithOrderBy(matchGroups);
//        private List<T> ShuffleListWithOrderBy<T>(List<T> list)
//        {
//            Random random = new Random(); // Random generator
//            return list.OrderBy(x => random.Next()).ToList(); // Randomize list bằng OrderBy
//        }
//#endif
    }
}