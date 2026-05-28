using GameTemplate.Audio;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Gameplay.GameplayObjects.Audio
{
    /// <summary>
    /// Simple class to play game theme on scene load
    /// </summary>
    public class MainMenuMusicStarter : MonoBehaviour,IStartable
    {
        // Có restart nhạc nếu đang phát không
        [SerializeField]
        bool m_Restart;

        // Inject SoundPlayer từ DI Container
        // Container sẽ:
        // Tìm SoundPlayer đã được đăng ký trong ApplicationController: builder.Register<SoundPlayer>(Lifetime.Singleton);
        // Khi ai cần SoundPlayer → tạo hoặc dùng instance có sẵn
        // Gán vào m_SoundPlayer
        [Inject] SoundPlayer m_SoundPlayer;

        // Được gọi khi object được start (VContainer lifecycle)
        public void Start()
        {
            m_SoundPlayer.PlayThemeMusic(m_Restart);
        }
    }
}
