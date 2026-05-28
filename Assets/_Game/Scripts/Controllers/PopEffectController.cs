using GameTemplate._Game.Scripts.Match;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace _Game.Scripts.Timer
{

    /// <summary> PopEffectController có vai trò:
    /// 
    /// - Spawn hiệu ứng visual khi match thành công
    /// 
    /// Đây là:
    /// - VFX controller
    /// - chỉ xử lý visual feedback
    /// 
    /// Không xử lý:
    /// - gameplay
    /// - score
    /// - combo
    /// 
    /// </summary>
    public class PopEffectController : MonoBehaviour
    {
        public GameObject triplePopEffect; // Prefab hiệu ứng pop.

        private void Awake()
        {
            MatchGroup.OnMatched += MatchGroupOnOnMatched; // Subscribe event match.
        }

        private void OnDestroy()
        {
            MatchGroup.OnMatched -= MatchGroupOnOnMatched; // Unsubscribe event match.
        }

        private void MatchGroupOnOnMatched(Vector3 point) // Được gọi khi match thành công. Cái triplePopEffect sẽ hiện ra tại point
        {
            Debug.Log("PopEffectController.MatGroupOnOnMatched");
            //convert to screen position
            point = Camera.main.WorldToScreenPoint(point); // Convert world position -> screen position vì effect này có thể nằm trong Canvas UI.

            // Destroy sau 2 giây
            Destroy(Instantiate(triplePopEffect, point, Quaternion.identity, transform), 2); // Instantiate effect, parent = transform -> effect nằm dưới object này.
        }
    }
}