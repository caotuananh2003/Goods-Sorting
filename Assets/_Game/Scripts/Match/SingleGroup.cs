using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameTemplate._Game.Scripts.Match
{
    /// <summary>
    /// =========================================================
    /// SingleGroup
    /// =========================================================
    ///
    /// Đại diện cho:
    /// - một cột queue object
    ///
    /// Ví dụ:
    ///
    /// Apple
    /// Banana
    /// Orange
    ///
    /// QueueObjects[0]
    /// là object FRONT.
    ///
    /// =========================================================
    /// Script này gắn vào:
    /// =========================================================
    ///
    /// GameObject cột.
    ///
    /// Ví dụ:
    ///
    /// SingleGroup
    /// ├── QueueObject
    /// ├── QueueObject
    /// └── QueueObject
    ///
    /// =========================================================
    /// CẦN KÉO THẢ
    /// =========================================================
    ///
    /// QueueObjectPrefab:
    /// - prefab QueueObject.
    /// </summary>
    public class SingleGroup : MonoBehaviour
    {
        #region Variables

        public GameObject QueueObjectPrefab; // Prefab QueueObject. Cần kéo thả QueueObject prefab vào.
        public List<QueueObject> QueueObjects = new List<QueueObject>(); // Danh sách queue object.

        #endregion
        private void Start()
        {
            QueueObjects = GetComponentsInChildren<QueueObject>().ToList(); // tự collect QueueObject con thành list
        }

        public bool IsFirstEmpty() // Kiểm tra front object có empty không.
        {
            return QueueObjects[0].ItemTypeAsset == null;
        }

        /// <summary>
        /// Hàm đưa object mới vào vị trí đầu.
        ///
        /// getChild: visual object được move sang.
        /// </summary>
        public void TakeThisObject(ItemType itemType, Transform getChild)
        {
            QueueObjects[0].ItemTypeAsset = itemType;
            getChild.parent = QueueObjects[0].transform;
            getChild.localPosition = Vector3.zero;
        }
        
        public ItemType GetFirstObject() // Lấy ItemType đầu tiên.
        {
            return QueueObjects[0].ItemTypeAsset;
        }

        public ItemId? GetFirstObjectType() // Lấy ItemId đầu tiên. Dấu ? tức là có thể trả về null. (enum bình thường không được phép return null)
        {
            return QueueObjects[0].ItemTypeAsset == null ? null : QueueObjects[0].ItemTypeAsset.itemId;
        }

        public void PopFirstObject() // Object đầu tiên sẽ chạy method Pop (animation rồi destroy), sau đó remove khỏi list.
        {
            QueueObjects[0].Pop();
            QueueObjects.RemoveAt(0);
            if (QueueObjects.Count == 0) // spawn queue rỗng mới nếu như hết queue
            {
                QueueObjects.Add(Instantiate(QueueObjectPrefab, transform).GetComponent<QueueObject>());
            }

            QueueObjects[0].SetInteractState(); // mở drag cho object mới phía trước
        }

        public void DestroyFirstObject() // Destroy object đầu tiên ngay lập tức.
        {
            Destroy(QueueObjects[0].gameObject);
            QueueObjects.RemoveAt(0);
            if (QueueObjects.Count == 0)
            {
                QueueObjects.Add(Instantiate(QueueObjectPrefab, transform).GetComponent<QueueObject>());
            }

            //open interactable for new front object
            QueueObjects[0].SetInteractState(); // Cho object đầu tiên trong list có thể drag
        }

#if UNITY_EDITOR
        public void AddQueue() // Spawn thêm QueueObject.
        {
            PrefabUtility.InstantiatePrefab(QueueObjectPrefab, transform);
        }
#endif
    }
}