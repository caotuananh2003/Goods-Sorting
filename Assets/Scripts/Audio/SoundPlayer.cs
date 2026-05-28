using Audio;
using DG.Tweening;
using GameTemplate.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Audio
{
    // Class xử lý toàn bộ logic play âm thanh
    //[RequireComponent(typeof(AudioSource))]
    public class SoundPlayer
    {
        #region Variables
        // Bản cũ
        //// AudioSource dùng để phát âm thanh
        //public AudioSource m_source;

        // Phiên bản mới:
        private AudioSource m_MusicSource;
        private AudioSource m_SfxSource;

        // Data chứa audio
        AudioData _audioData;

        // Holder chứa object audio (parent)
        private Transform _holder;
        #endregion

        #region Injection
        // Constructor injection (VContainer)
        [Inject]
        public void Construct(AudioData audioData)
        {
            Debug.Log("Constructing Sound Player");
            _audioData = audioData;

            // Tạo GameObject chứa SoundPlayer
            _holder = new GameObject("SoundPlayer").transform;

            // Phiên bản mới
            Object.DontDestroyOnLoad(_holder.gameObject); 

            // Phiên bản mới:
            m_MusicSource = _holder.gameObject.AddComponent<AudioSource>();
            m_MusicSource.playOnAwake = false;
            m_MusicSource.loop = true;

            m_SfxSource = _holder.gameObject.AddComponent<AudioSource>();
            m_SfxSource.playOnAwake = false;
            m_SfxSource.loop = false;

            m_MusicSource.volume = UserPrefs.GetMusicVolume();
            m_SfxSource.volume = UserPrefs.GetSoundVolume();
        }

        #endregion

        #region Music
        // Phiên bản cũ
        // PlayTrack(AudioClip clip, bool looping, bool restart)
        public void PlayThemeMusic(bool restart)
        {
            PlayMusic(_audioData.GetAudio(AudioID.Music), restart);
        }

        public void PlayTimerMusic(bool restart)
        {
            PlayMusic(_audioData.GetAudio(AudioID.Ticking), restart);
        }

        /// Internal music play logic.
        private void PlayMusic(AudioClip clip, bool restart)
        {
            if (clip == null)
            {
                Debug.LogWarning("Music clip is null");
                return;
            }

            // Nếu đang play cùng clip và không muốn restart
            if (m_MusicSource.isPlaying &&
                m_MusicSource.clip == clip &&
                !restart)
            {
                return;
            }

            m_MusicSource.Stop();

            m_MusicSource.clip = clip;
            m_MusicSource.loop = true;
            m_MusicSource.time = 0;

            m_MusicSource.Play();
        }

        // Pause current music.
        public void PauseMusic()
        {
            if (m_MusicSource == null)
                return;

            m_MusicSource.Pause();
        }

        // Resume paused music.
        public void ResumeMusic()
        {
            if (m_MusicSource == null)
                return;

            m_MusicSource.UnPause();
        }

        // Stop music with fade out.
        public void StopThemeMusic()
        {
            if (m_MusicSource == null)
                return;

            m_MusicSource.DOFade(0, 1f).OnComplete(() =>
            {
                m_MusicSource.Stop();

                // restore volume
                m_MusicSource.volume = UserPrefs.GetMusicVolume();
            });
        }

        #endregion

        #region SFX
        public void PlayWinSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.Win));
        }

        public void PlayLoseSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.Lose));
        }

        public void PlayTimesUpSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.TimesUp));
        }

        public void PlayClickSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.Click));
        }

        public void PlayMatchSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.Match));
        }

        public void PlayComboEndedSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.ComboEnded));
        }

        public void PlayComboEndingSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.ComboEnding));
        }
        public void PlayDragSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.Drag));
        }

        public void PlayDropSound()
        {
            PlaySFX(_audioData.GetAudio(AudioID.Drop));
        }
        /// <summary>
        /// Internal SFX play logic.
        /// </summary>
        private void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("SFX clip is null");
                return;
            }

            m_SfxSource.PlayOneShot(clip);
        }

        #endregion

        #region Volume

        /// <summary>
        /// Set music volume runtime.
        /// </summary>
        public void SetMusicVolume(float value)
        {
            Debug.Log("SoundPlayer.SetMusicVolume");
            if (m_MusicSource == null)
                return;

            value = Mathf.Clamp01(value); // Chắc chắn volume trong khoảng từ 0 đến 1

            m_MusicSource.volume = value;

            UserPrefs.SetMusicVolume(value);
        }

        /// <summary>
        /// Set SFX volume runtime.
        /// </summary>
        public void SetSoundVolume(float value)
        {
            Debug.Log("SoundPlayer.SetSoundVolume");
            if (m_SfxSource == null)
                return;

            value = Mathf.Clamp01(value); // Chắc chắn volume trong khoảng từ 0 đến 1

            m_SfxSource.volume = value;

            UserPrefs.SetSoundVolume(value);
        }

        #endregion
    }
}