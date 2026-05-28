using Audio;
using GameTemplate.Audio;
using GameTemplate.Gameplay.GameState;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        private Button m_Button;

        [Inject]
        private SoundPlayer m_SoundPlayer;

        //private void Awake()
        //{
        //    m_Button = GetComponent<Button>();

        //    m_Button.onClick.AddListener(PlayClickSound);
        //}

        private void Awake()
        {
            // Inject dependencies cho object này
            LifetimeScope.Find<GameStateBehaviour>()
                .Container
                .Inject(this);

            m_Button = GetComponent<Button>();

            m_Button.onClick.AddListener(PlayClickSound);
        }

        private void PlayClickSound()
        {
            if (m_SoundPlayer == null)
            {
                Debug.LogError("SoundPlayer is NULL");
                return;
            }

            m_SoundPlayer.PlayClickSound();
        }

        private void OnDestroy()
        {
            m_Button.onClick.RemoveListener(PlayClickSound);
        }
    }
}