using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer.Unity;

namespace Audio
{
    // ScriptableObject chứa toàn bộ dữ liệu âm thanh
    // Dùng để mapping giữa AudioID và AudioClip
    [CreateAssetMenu(fileName = "AudioData", menuName = "Scriptable Objects/Audio data", order = 0)] // Là thao tác Right click chọn Create/ScriptableObjects...
    public class AudioData : SerializedScriptableObject
    {
        // Prefab chứa AudioSource (dùng để play âm thanh)
        public GameObject audioObject;

        // Dictionary map:
        // AudioID -> AudioClip
        // Ví dụ: Music -> nhạc nền, Win -> âm thanh thắng
        [DictionaryDrawerSettings(KeyLabel = "AudioID", ValueLabel = "AudioClip")]
        public Dictionary<AudioID, AudioClip> AudioClips = new Dictionary<AudioID, AudioClip>();

        // Lấy AudioClip theo ID
        public AudioClip GetAudio(AudioID timesUp)
        {
            return AudioClips[timesUp];
        }
    }
}