using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace GameTemplate.Managers.Pool
{
    /// <summary>
    /// Component đại diện cho object thuộc Pool.
    ///
    /// Gắn trên prefab được pooling.
    ///
    /// Ví dụ:
    /// - Bullet
    /// - Particle
    /// - Explosion
    /// - Coin
    /// - Enemy
    ///
    /// ====================================================
    /// NHIỆM VỤ
    /// ====================================================
    ///
    /// - biết mình thuộc pool nào
    /// - tự quay về pool khi disable
    /// - báo cho PoolManager khi parent đổi
    /// - tránh lỗi khi quit game
    /// </summary>
    public class PoolElement : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// ID pool của object này.
        ///
        /// Ví dụ:
        /// PoolID.Bullet
        /// PoolID.Explosion
        /// </summary>
        private PoolID poolId;

        private bool goBackOnDisable; // Nếu true: khi object bị disable sẽ tự quay về pool.

        /// <summary>
        /// Flag tránh callback khi app đang thoát.
        ///
        /// Vì lúc quit: Unity disable toàn bộ object.
        /// Nếu không check: object sẽ cố quay lại pool khi game đang shutdown.
        /// </summary>
        private bool isApplicationQuitting;

        public PoolID PoolId { get => poolId; set => poolId = value; } // Property public cho PoolId.

        #endregion

        /// <summary>
        /// Khởi tạo PoolElement.
        ///
        /// Được gọi bởi PoolingManager sau khi Instantiate prefab.
        /// </summary>
        public void Initialize(bool gBackOnDisable, PoolID poolId)
        {
            this.goBackOnDisable = gBackOnDisable;
            this.poolId = poolId;
        }

        /// <summary>
        /// Unity callback:
        /// gọi khi object bị disable.
        ///
        /// Nếu được config:
        /// object sẽ tự quay về pool.
        /// </summary>
        private void OnDisable()
        {
            if(!isApplicationQuitting && goBackOnDisable)
            {
                GoBackToPool();
            }
        }

        /// <summary>
        /// Unity callback:
        /// gọi khi application quit.
        ///
        /// Đánh dấu để tránh logic pooling.
        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

        /// <summary>
        /// Button Odin Inspector cho phép test trong inspector.
        ///
        /// Khi bấm: object sẽ bị disable.
        /// </summary>
        [Button("GoBackToPool")]
        public void Deactivator()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Trả object về pool.
        ///
        /// Gọi PoolingManager xử lý.
        /// </summary>
        public void GoBackToPool()
        {
            PoolingManager.Instance.GoBackToPool(this);
        }

        /// <summary>
        /// Unity callback:
        /// gọi khi object đổi parent.
        ///
        /// Nếu object không còn nằm dưới poolParent thì báo cho PoolManager.
        ///
        /// Ví dụ:
        /// particle attach (gắn) vào player
        /// bullet attach (gắn) vào weapon
        /// </summary>
        private void OnTransformParentChanged()
        {
            if(transform.parent != PoolingManager.Instance.poolParent)
            {
                PoolingManager.Instance.PoolElementParentChanged(this);
            }
        }
    }
}