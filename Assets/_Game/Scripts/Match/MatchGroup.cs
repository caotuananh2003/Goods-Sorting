using System;
using System.Collections.Generic;
using System.Linq;
using GameTemplate.Managers.Scene;
using UnityEngine;

namespace GameTemplate._Game.Scripts.Match
{
    /// <summary>
    /// =========================================================
    /// MatchGroup
    /// =========================================================
    ///
    /// Đại diện cho 1 frame gồm nhiều SingleGroup (Ở đây là 3)
    ///
    /// Cấu trúc:
    /// MatchGroup
    /// ├── SingleGroup
    /// ├── SingleGroup
    /// └── SingleGroup
    ///
    /// Nếu object đầu tiên trong list QueueObject của tất cả SingleGroup cùng ItemId thì sẽ MATCH
    ///
    /// Script này gắn vào: GameObject cha chứa nhiều SingleGroup.
    ///
    /// SingleGroups kéo toàn bộ SingleGroup con vào list.
    ///
    /// HasBlocker đánh dấu line này có blocker hay không.
    /// </summary>
    public class MatchGroup : MonoBehaviour
    {
        #region Variables

        public List<SingleGroup> SingleGroups = new List<SingleGroup>(); // Danh sách các cột trong hàng match.

        public bool HasBlocker = false; // Line (Cái khung chứa các object) có blocker hay không.

        /// <summary>
        /// Event global khi match thành công.
        ///
        /// Listener:
        /// - ComboController
        /// - StarController
        /// - PopEffectController
        /// </summary>
        public static event Action<Vector3> OnMatched;

        #endregion

        /// <summary>
        /// Kiểm tra:
        /// - line có match không
        /// - line có empty không
        /// </summary>
        public void CheckMatchAndEmpty()
        {
            if (IsAllEmpty()) // Nếu toàn bộ object đầu tiên đều empty
            {
                DestroyFirstRow(); // Xóa đi phần tử QueueObject đầu tiên trong list của các SingleGroup
            }
            else
            {
                bool isMatched = true; // Flag đánh dấu là đã match thành công để chạy vòng lặp check logic
                ItemType currentType = SingleGroups[0].GetFirstObject(); // Lấy ra QueueObject đầu tiên để so sánh
                if (currentType == null) // Nếu như phần tử đầu là rỗng thì đương nhiên không match.
                {
                    isMatched = false;
                }
                else
                {
                    foreach (var singleGroup in SingleGroups)
                    {
                        if (currentType.itemId != singleGroup.GetFirstObjectType()) // Nếu có 1 phần tử khác loại thì không match
                        {
                            isMatched = false;
                            break;
                        }
                    }
                }

                if (isMatched) // Nếu match thì phát event
                {
                    //Listeners
                    //StarController
                    //ComboController
                    OnMatched?.Invoke(transform.position);

                    PopFirstRow(); // Pop các QueueObject ở hàng đầu tiên (Đầu danh sách QueueObject của các SingleGroup)
                    _ = GetComponentInParent<LevelPrefab>().CheckLevelOver(); // Check xem đã kết thúc game chưa
                }
            }
        }

        private void DestroyFirstRow() // Xóa đi phần tử QueueObject đầu tiên trong list của các SingleGroup
        {
            foreach (var singleGroup in SingleGroups)
            {
                singleGroup.DestroyFirstObject();
            }
        }

        private void PopFirstRow() // Pop các QueueObject ở hàng đầu tiên (Đầu danh sách QueueObject của các SingleGroup)
        {
            foreach (var singleGroup in SingleGroups)
            {
                singleGroup.PopFirstObject();
            }
        }

        public bool IsAllEmpty() // Trả về xem toàn bộ object đầu tiên có đều empty không
        {
            bool isEmptyLine = true;
            foreach (var singleGroup in SingleGroups)
            {
                if (!singleGroup.IsFirstEmpty())
                {
                    isEmptyLine = false;
                    break;
                }
            }

            //Debug.Log(gameObject + " // "+ isEmptyLine);
            return isEmptyLine;
        }

        public bool IsAllFirstFilled() // Kiểm tra xem còn SingleGroup nào trống slot đầu không. (Tức là kiểm tra xem trong MatchGroup còn trống slot nào không)
        {
            bool isAllFilled = true;
            foreach (var singleGroup in SingleGroups)
            {
                if (singleGroup.IsFirstEmpty())
                {
                    isAllFilled = false;
                    break;
                }
            }

            return isAllFilled;
        }

        public void BlockerDeactivated() // Gọi khi blocker bị phá để mở interact cho toàn bộ QueueObject.
        {
            HasBlocker = false;
            List<QueueObject> childInteractables = GetComponentsInChildren<QueueObject>().ToList();
            foreach (var childInteractable in childInteractables)
            {
                childInteractable.SetInteractState();
            }
        }

//#if UNITY_EDITOR
//        public void CloseAllInteractables() // Disable drag toàn bộ QueueObject.
//        {
//            List<QueueObject> childInteractables = transform.GetComponentsInChildren<QueueObject>().ToList();

//            foreach (var childInteractable in childInteractables)
//            {
//                childInteractable.SetInteractState(false);
//            }
//        }
        
//        public void SpawnRow() // Spawn thêm 1 queue object cho mỗi SingleGroup. (Spawn thêm 3 item làm thành 1 hàng ở trong cái frame)
//        {
//            foreach (var singleGroup in SingleGroups)
//            {
//                singleGroup.AddQueue();
//            }
//        }
//#endif
    }
}