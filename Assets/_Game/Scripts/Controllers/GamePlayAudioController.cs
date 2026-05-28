using GameTemplate._Game.Scripts.Match;
using GameTemplate.Gameplay.GameState;
using UnityEngine;
using VContainer;

namespace GameTemplate.Audio
{
    /// <summary>
    /// GameplayAudioController
    ///
    /// Chỉ có nhiệm vụ:
    /// - lắng nghe gameplay events
    /// - phát audio tương ứng
    /// </summary>
    public class GameplayAudioController : MonoBehaviour
    {
        private SoundPlayer _soundPlayer;

        [Inject]
        public void Construct(SoundPlayer soundPlayer)
        {
            _soundPlayer = soundPlayer;
        }

        private void Awake()
        {
            MatchGroup.OnMatched += OnMatched;

            QueueObject.OnDragStartedEvent += OnDragStarted;
            QueueObject.OnDroppedEvent += OnDropped;

            GameSceneState.OnPauseChanged += OnPauseChanged;
        }

        private void OnDestroy()
        {
            MatchGroup.OnMatched -= OnMatched;

            QueueObject.OnDragStartedEvent -= OnDragStarted;
            QueueObject.OnDroppedEvent -= OnDropped;

            GameSceneState.OnPauseChanged -= OnPauseChanged;
        }

        private void OnMatched(Vector3 point)
        {
            _soundPlayer.PlayMatchSound();
        }

        private void OnDragStarted()
        {
            _soundPlayer.PlayDragSound();
        }

        private void OnDropped()
        {
            _soundPlayer.PlayDropSound();
        }

        private void OnPauseChanged(bool paused)
        {
            if (paused)
            {
                _soundPlayer.PauseMusic();
            }
            else
            {
                _soundPlayer.ResumeMusic();
            }
        }
    }
}