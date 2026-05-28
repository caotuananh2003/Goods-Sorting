using GameTemplate.Audio;
using GameTemplate.Utils;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace GameTemplate.Gameplay.UI
{
    /// <summary>
    /// Panel quản lý setting:
    /// - sound on/off
    /// - music on/off
    ///
    /// Chức năng:
    /// - đọc setting từ UserPrefs
    /// - update UI toggle
    /// - save setting khi player thay đổi
    ///
    /// ====================================================
    /// FLOW
    /// ====================================================
    ///
    /// Panel mở
    /// ↓
    /// đọc UserPrefs
    /// ↓
    /// set trạng thái toggle
    /// ↓
    /// listen toggle change
    /// ↓
    /// player click toggle
    /// ↓
    /// save setting
    /// </summary>
    /// 
    public class UISettingsPanel : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private UISwitcher.UISwitcher m_SoundToggle; // Toggle bật/tắt sound effect.

        [SerializeField]
        private UISwitcher.UISwitcher m_MusicToggle; // Toggle bật/tắt music.

        [SerializeField]
        private Slider m_SoundSlider; // Slider

        [SerializeField]
        private Slider m_MusicSlider; // Slider
        #endregion

        private SoundPlayer m_SoundPlayer;

        [Inject]
        public void Construct(SoundPlayer soundPlayer)
        {
            Debug.Log("UISettingsPanel.Construct");
            m_SoundPlayer = soundPlayer;
        }
        private void OnEnable()
        {
            // Note that we initialize the Toggle BEFORE we listen for changes (so we don't get notified of our own change!)
            m_SoundToggle.isOn = UserPrefs.GetSoundState(); // Load trạng thái sound đã lưu
            m_SoundToggle.onValueChanged.AddListener(OnSoundToggleChanged); // Listen event thay đổi toggle

            // initialize music Toggle similarly.
            m_MusicToggle.isOn = UserPrefs.GetMusicState(); // Load trạng thái music đã lưu
            m_MusicToggle.onValueChanged.AddListener(OnMusicToggleChanged); // Listen event thay đổi toggle. Có thể kéo trong inspector

            m_SoundSlider.value = UserPrefs.GetSoundVolume();
            m_MusicSlider.value = UserPrefs.GetMusicVolume();

            m_SoundSlider.interactable = m_SoundToggle.isOn;
            m_MusicSlider.interactable = m_MusicToggle.isOn;

            m_SoundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            m_MusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        private void OnDisable() // Cleanup listeners tránh memory leak
        {
            m_SoundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
            m_MusicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
            m_SoundSlider.onValueChanged.RemoveListener(OnSoundVolumeChanged);
            m_MusicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        private void OnSoundToggleChanged(bool state) // Được gọi khi player bật/tắt sound.
        {
            //Debug.Log(state); // Có thể comment lại
            UserPrefs.SetSoundState(state); // Save setting

            m_SoundSlider.interactable = state;

            if (!state)
            {
                // lưu volume cũ
                UserPrefs.SetLastSoundVolume(m_SoundSlider.value);

                // mute
                m_SoundSlider.value = 0;
            }
            else
            {
                // restore volume cũ
                float lastVolume = UserPrefs.GetLastSoundVolume();

                m_SoundSlider.value = lastVolume;
            }
        }

        private void OnMusicToggleChanged(bool state) // Được gọi khi player bật/tắt music.
        {
            //Debug.Log(state); // Có thể comment lại
            UserPrefs.SetMusicState(state); // Save setting

            m_MusicSlider.interactable = state;

            if (!state)
            {
                UserPrefs.SetLastMusicVolume(m_MusicSlider.value);

                m_MusicSlider.value = 0;
            }
            else
            {
                float lastVolume = UserPrefs.GetLastMusicVolume();

                m_MusicSlider.value = lastVolume;
            }
        }

        private void OnSoundVolumeChanged(float value)
        {
            UserPrefs.SetSoundVolume(value);
            m_SoundPlayer.SetSoundVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            UserPrefs.SetMusicVolume(value);
            m_SoundPlayer.SetMusicVolume(value);
        }
    }

}
