using DG.Tweening;
using Flexalon;
using GameTemplate.Gameplay.GameState;
using GameTemplate.Managers.Scene;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameTemplate._Game.Scripts.Match
{
    /// <summary>
    /// =========================================================
    /// QueueObject
    /// =========================================================
    ///
    /// Đại diện cho:
    /// - 1 slot chứa item
    /// - object draggable
    ///
    /// =========================================================
    /// Script này gắn vào:
    /// =========================================================
    ///
    /// QueueObject prefab.
    ///
    /// =========================================================
    /// Hierarchy ví dụ
    /// =========================================================
    ///
    /// QueueObject
    /// └── Chips / Snack...
    ///
    /// =========================================================
    /// CẦN COMPONENT
    /// =========================================================
    ///
    /// - FlexalonInteractable. Dùng để drag
    ///
    /// =========================================================
    /// CẦN KÉO THẢ
    /// =========================================================
    ///
    /// _layerMask:
    /// - layer chứa drop target.
    /// </summary>
    public class QueueObject : MonoBehaviour
    {
        #region Action
        public static event Action OnDragStartedEvent;
        public static event Action OnDroppedEvent;
        #endregion

        #region Variables

        public ItemType ItemTypeAsset // Wrapper property itemType hiện tại của object.
        {
            get { return itemType; }
            set
            {
                itemType = value;

                SetInteractState();
            }
        }

        public LayerMask _layerMask; // Layer dùng raycast khi drop. Cần set Layer dùng raycast khi drop.

        [FormerlySerializedAs("_objectType")] [SerializeField] private ItemType itemType; // ItemType hiện tại. Setup trong Inspector
        private MatchGroup _matchGroup; // Cái khung (MatchGroup đang chứa SingleGroup chứa các Queue)
        private FlexalonInteractable _interactable; // Component drag của Flexalon.

        #endregion

        private void Start()
        {
            _matchGroup = GetComponentInParent<MatchGroup>();
            _interactable = GetComponent<FlexalonInteractable>();

            if (transform.position.z > 0) // object nằm phía sau thì disable drag
            {
                SetInteractState(false);
            }

            _interactable.DragStart.AddListener(DragStarted); // Callback khi player drag lần đầu.
        }

        void DragStarted(FlexalonInteractable arg0)
        {
            Debug.Log("QueueObject.DragStarted");

            OnDragStartedEvent?.Invoke();
            GameSceneState.OnFirstTouch?.Invoke();

            //_interactable.DragStart.RemoveListener(DragStarted);
        }

        public void TryToDropNewPlace() // Thử drop object sang vị trí mới. Được thả trong event drag end của component Flexalon Interactable.
        {
            Debug.Log("QueueObject.TryToDropNewPlace");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(MobileInput.PointerPosition());
            //Debug.DrawRay(ray.origin, ray.direction * 50, Color.red, 60);

            if (Physics.Raycast(ray, out hit, 50, _layerMask)) // raycast tìm drop target
            {
                Transform objectHit = hit.transform;
                if (objectHit.TryGetComponent(out FlexalonDragTarget target)) // check có phải drag target không
                {
                    SingleGroup singleGroup = target.GetComponent<SingleGroup>();
                    if (singleGroup.IsFirstEmpty()) // chỉ cho drop nếu front empty
                    {
                        singleGroup.TakeThisObject(itemType, transform.GetChild(0)); // Move item sang queue object mới trong singleGroup
                        OnDroppedEvent?.Invoke();
                        ItemTypeAsset = null; // clear current slot
                        singleGroup.GetComponentInParent<MatchGroup>().CheckMatchAndEmpty(); // Check match
                        _matchGroup.CheckMatchAndEmpty();
                        
                        _ = GetComponentInParent<LevelPrefab>().CheckAllFirstFilled(); // check fail state
                    }
                }
            }
        }

        public void Pop() // Pop animation rồi destroy.
        {
            Debug.Log("QueueObject.Pop");
            PunchScaleY();
            Destroy(gameObject,.15f);
        }

        void PunchScaleY() // DOTween punch scale effect.
        {
            Debug.Log("QueueObject.PunchScaleY");
            transform.DOPunchScale(new Vector3(0, .1f, 0), .1f, 1);
        }

        public void SetInteractState(bool state = true) // Bật/tắt drag.
        {
            if (!state)
            {
                _interactable.Draggable = state;
                return;
            }
            
            if (_interactable == null)
            {
                _interactable = GetComponent<FlexalonInteractable>();
            }

            _interactable.Draggable = itemType != null; // chỉ draggable nếu có item
        }

//#if UNITY_EDITOR
//        public void SpawnObjectEditor(ItemType itemType) // Spawn visual object trong editor.
//        {
//            _interactable = GetComponent<FlexalonInteractable>();
//            ItemTypeAsset = itemType;
//            PrefabUtility.InstantiatePrefab(this.itemType.prefab, transform);
//        }
//#endif
    }
}