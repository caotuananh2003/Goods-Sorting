Khi game start (Main Menu):
ApplicationController
    ↓ (DI inject)
SoundPlayer được tạo
    ↓
MainMenuMusicStarter.Start()
    ↓
SoundPlayer.PlayThemeMusic()
-----
Bên trong SoundPlayer:
PlayThemeMusic()
    ↓
GetAudio(AudioID.Music)
    ↓
PlayTrack()
-----
Nếu chưa có AudioSource
Instantiate(audioObject prefab)
    ↓
GetComponent<AudioSource>()
    ↓
Gán vào m_source
