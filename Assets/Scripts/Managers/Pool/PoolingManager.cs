using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameTemplate.Managers.Pool
{
    /// <summary>
    /// =========================================================
    /// POOLING MANAGER
    /// =========================================================
    ///
    /// Đây là core manager của hệ thống Object Pooling.
    ///
    /// Nhiệm vụ:
    /// - tạo pool
    /// - quản lý object pool
    /// - cấp object từ pool
    /// - nhận object trả về pool
    /// - tái sử dụng object
    ///
    /// =========================================================
    /// FLOW
    /// =========================================================
    ///
    /// Initialize()
    /// ↓
    /// tạo sẵn object
    /// ↓
    /// lưu vào Queue
    ///
    /// Game cần object
    /// ↓
    /// GetGameObjectById()
    /// ↓
    /// Dequeue object
    /// ↓
    /// SetActive(true)
    ///
    /// Dùng xong
    /// ↓
    /// SetActive(false)
    /// ↓
    /// PoolElement.OnDisable()
    /// ↓
    /// GoBackToPool()
    /// ↓
    /// Enqueue lại Queue
    /// </summary>
    public class PoolingManager : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// ScriptableObject chứa toàn bộ config pool.
        ///
        /// Ví dụ:
        /// - prefab nào cần pooling
        /// - số lượng object preload
        /// - auto return hay không
        /// </summary>
        public ScriptablePooling poolingData;

        /// <summary>
        /// Parent chứa toàn bộ object pool. Giúp hierarchy gọn hơn. Ví dụ:
        /// _PoolParent
        /// ├ Bullet
        /// ├ Explosion
        /// ├ Coin
        /// </summary>
        public Transform poolParent;

        public PoolID testPoolId; // Dùng để test pool trong inspector.

        /// <summary>
        /// Danh sách object đã đổi parent.
        ///
        /// Ví dụ:
        /// particle attach vào Player (particle đổi parent từ PoolParent sang Player)
        ///
        /// Khi reset pool:
        /// cần đưa object về poolParent.
        /// </summary>
        [SerializeField] private List<PoolElement> parentsChangedPoolObjects = new List<PoolElement>();

        /// <summary>
        /// Dictionary chứa toàn bộ pool.
        ///
        /// KEY: PoolID
        /// VALUE: Queue<GameObject>
        ///
        /// Ví dụ:
        /// Bullet
        /// → Queue<GameObject>
        ///
        /// Explosion
        /// → Queue<GameObject>
        /// </summary>
        private Dictionary<PoolID, Queue<GameObject>> objectPools = new Dictionary<PoolID, Queue<GameObject>>(); // Biến chính, là nơi chứa pool
        
        public static PoolingManager Instance { get; private set; }// Singleton instance. Có thể gọi: PoolingManager.Instance

        #endregion
        private void Awake() // Setup singleton.
        {
            if (Instance != null) // Nếu đã có PoolingManager khác tồn tại
            {
                throw new System.Exception("Multiple Pools!");
            }

            DontDestroyOnLoad(gameObject); // Không destroy khi đổi scene

            Instance = this; // gán singleton
        }

        /// <summary>
        /// =====================================================
        /// KHỞI TẠO TOÀN BỘ POOL
        /// =====================================================
        ///
        /// Flow:
        ///
        /// 1. đọc ScriptablePooling
        /// 2. tạo Queue cho từng PoolID
        /// 3. Instantiate object trước
        /// 4. SetActive(false)
        /// 5. đưa vào Queue
        ///
        /// Đây gọi là:
        /// PRELOAD POOL
        ///
        /// =====================================================
        /// VÍ DỤ
        /// =====================================================
        ///
        /// Explosion:
        /// objectCount = 20
        ///
        /// => tạo sẵn:
        ///
        /// Explosion(Clone) x20
        /// </summary>
        public void Initialize()
        {
            //poolParent = new GameObject("_PoolParent").transform;

            for (int i = 0; i < poolingData.poolObjects.Length; i++) // loop qua toàn bộ pool config
            {
                objectPools.Add((PoolID)i, new Queue<GameObject>()); // tạo Queue cho pool hiện tại
                for (int z = 0; z < poolingData.poolObjects[i].objectCount; z++) // preload object
                {
                    GameObject newObject = Instantiate(poolingData.poolObjects[i].objectPrefab, poolParent); // instantiate prefab
                    newObject.SetActive(false); // disable object
                    newObject.GetComponent<PoolElement>().Initialize(poolingData.poolObjects[i].goBackOnDisable, (PoolID)i); // setup PoolElement
                    objectPools[(PoolID)i].Enqueue(newObject); // đưa object vào Queue
                }
            }
        }

        /// <summary>
        /// =====================================================
        /// RESET TOÀN BỘ POOL
        /// =====================================================
        ///
        /// Dùng khi:
        /// - restart level
        /// - load scene mới
        /// - reset gameplay
        ///
        /// Flow:
        ///
        /// 1. đưa object về poolParent
        /// 2. clear tracking list
        /// 3. disable toàn bộ pool object
        /// </summary>
        public void ResetPool()
        {
            for (int i = 0; i < parentsChangedPoolObjects.Count; i++) // đưa object đã đổi parent về poolParent
            {
                parentsChangedPoolObjects[i].transform.SetParent(poolParent);
            }
            parentsChangedPoolObjects.Clear(); // clear tracking
            PoolElement[] children = poolParent.GetComponentsInChildren<PoolElement>(); // lấy toàn bộ PoolElement
            for (int i = 0; i < children.Length; i++) // disable toàn bộ object
            {
                children[i].gameObject.SetActive(false);
            }
        }
        
        public void OnPoolElementDestroyed(PoolElement destroyedPoolElement) // Callback khi object pool bị destroy. Hiện tại chỉ debug.
        {
            Debug.Log("element Destroyed : " + destroyedPoolElement);
        }

        /// <summary>
        /// =====================================================
        /// TRẢ OBJECT VỀ POOL
        /// =====================================================
        ///
        /// overload nhận GameObject.
        ///
        /// Flow:
        ///
        /// object
        /// ↓
        /// set parent về poolParent
        /// ↓
        /// enqueue vào Queue
        /// </summary>
        public void GoBackToPool(GameObject poolObject)
        {
            poolObject.transform.SetParent(poolParent); // đưa về pool parent
            objectPools[poolObject.GetComponent<PoolElement>().PoolId].Enqueue(poolObject); // enqueue vào Queue
        }

        /// <summary>
        /// =====================================================
        /// PARTICLE WRAPPER
        /// =====================================================
        ///
        /// Overload tiện lợi với các loại parameter khác nhau. (có thể scale, set transform parent...)
        /// </summary>
        public GameObject GetParticleById(PoolID poolId, Transform referance)
        {
            return GetParticleById(poolId, referance.position, Vector3.one);
        }

        public GameObject GetParticleById(PoolID poolId, Transform referance, Vector3 targetScale)
        {
            return GetParticleById(poolId, referance.position, targetScale);
        }
        public GameObject GetParticleById(PoolID poolId, Vector3 position, Vector3 targetScale, Transform parentInfo = null)
        {
            GameObject particle = GetGameObjectById(poolId, position, Quaternion.identity);
            particle.transform.localScale = targetScale;
            if (parentInfo != null)
            {
                particle.transform.SetParent(parentInfo.parent);
                parentInfo.transform.localPosition = parentInfo.localPosition;
            }
            particle.GetComponent<ParticleSystem>().Play();
            return particle;
        }

        /// <summary>
        /// =====================================================
        /// SPAWN OBJECT THEO VECTOR3.ZERO / TRANSFORM / POSITION / ROTATION / SCALE...
        /// =====================================================
        ///
        /// Overload tiện lợi với các loại parameter khác nhau.
        /// </summary>
        public GameObject GetGameObjectById(PoolID poolId)
        {
            return GetGameObjectById(poolId, Vector3.zero, Quaternion.identity);
        }

        public GameObject GetGameObjectById(PoolID poolId, Transform objectTransform)
        {
            return GetGameObjectById(poolId, objectTransform.position, objectTransform.rotation);
        }

        public GameObject GetGameObjectById(PoolID poolId, Vector3 position)
        {
            return GetGameObjectById(poolId, position, Quaternion.identity);
        }

        /// <summary> Đây là hàm quan trọng nhất. Spawn object từ pool.
        /// =====================================================
        /// FLOW
        /// =====================================================
        ///
        /// Queue còn object?
        ///
        /// YES
        /// ↓
        /// Dequeue
        /// ↓
        /// Set position/rotation
        /// ↓
        /// SetActive(true)
        /// ↓
        /// return object
        ///
        /// -----------------------------------------------------
        ///
        /// NO
        /// ↓
        /// Instantiate mới
        /// ↓
        /// return object
        /// </summary>
        public GameObject GetGameObjectById(PoolID poolId, Vector3 position, Quaternion rotation) // Logic tương tự hàm trên nhưng có thêm targetScale.
        {
            if (!objectPools.ContainsKey(poolId))
            {
                objectPools.Add(poolId, new Queue<GameObject>());
            }
            if (objectPools[poolId].Count != 0)
            {
                GameObject poolObject = objectPools[poolId].Dequeue();
                poolObject.transform.position = position;
                poolObject.transform.rotation = rotation;
                poolObject.SetActive(true);
                return poolObject;
            }
            PoolObject selectedPoolObject = poolingData.poolObjects.Where(x => x.poolName.Equals(poolId.ToString())).First();

            if (selectedPoolObject != null)
            {
                GameObject poolObject = Instantiate(selectedPoolObject.objectPrefab, position, rotation);
                poolObject.transform.SetParent(poolParent);
                poolObject.GetComponent<PoolElement>().Initialize(selectedPoolObject.goBackOnDisable, poolId);
                poolObject.SetActive(true);
                return poolObject;
            }
            return null;
        }

        public GameObject GetGameObjectById(PoolID poolId, Vector3 position, Quaternion rotation, Vector3 targetScale)
        {
            if (!objectPools.ContainsKey(poolId))
            {
                objectPools.Add(poolId, new Queue<GameObject>());
            }
            if (objectPools[poolId].Count != 0)
            {
                GameObject poolObject = objectPools[poolId].Dequeue();
                poolObject.transform.position = position;
                poolObject.transform.rotation = rotation;
                poolObject.transform.localScale = targetScale;
                poolObject.SetActive(true);
                return poolObject;
            }
            PoolObject selectedPoolObject = poolingData.poolObjects.Where(x => x.poolName.Equals(poolId.ToString())).First();

            if (selectedPoolObject != null)
            {
                GameObject poolObject = Instantiate(selectedPoolObject.objectPrefab, position, rotation);
                poolObject.transform.SetParent(poolParent);
                poolObject.GetComponent<PoolElement>().Initialize(selectedPoolObject.goBackOnDisable, poolId);
                poolObject.transform.localScale = targetScale;
                poolObject.SetActive(true);
                return poolObject;
            }
            return null;
        }

        public void GoBackToPool(PoolElement elementToGoBackToPool) // Trả PoolElement về pool. Enqueue object vào Queue.
        {
            objectPools[elementToGoBackToPool.PoolId].Enqueue(elementToGoBackToPool.gameObject);
        }

        public void GoBackToPool(PoolID poolId, GameObject objectToAddPool) // Trả object về pool bằng PoolID. Force disable trước.
        {
            objectToAddPool.SetActive(false);
            objectPools[poolId].Enqueue(objectToAddPool);
        }

        public void PoolElementParentChanged(PoolElement parentChangedObject) // Được gọi khi object đổi parent. Dùng để tracking object.
        {
            parentsChangedPoolObjects.Add(parentChangedObject);
        }

        /// <summary>
        /// Odin Inspector button.
        ///
        /// Test spawn object trong inspector.
        /// </summary>
        [Button("TestGetObject")]
        public void GetTestGameObject()
        {
            GetGameObjectById(testPoolId);
        }
    }
}