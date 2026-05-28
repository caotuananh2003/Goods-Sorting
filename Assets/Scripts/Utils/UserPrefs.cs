using UnityEngine;

namespace GameTemplate.Utils
{
    /// <summary>
    /// Singleton class which saves/loads local settings.
    /// (This is just a wrapper around the PlayerPrefs system,
    /// so that all the calls are in the same place.)
    /// </summary>
    /// /// <summary>
    /// =========================================================
    /// USER PREFS
    /// =========================================================
    ///
    /// Class static dùng để:
    /// - lưu dữ liệu local
    /// - đọc dữ liệu local
    ///
    /// Đây là wrapper quanh:
    /// Unity PlayerPrefs
    ///
    /// Thay vì gọi PlayerPrefs khắp project:
    ///
    /// PlayerPrefs.SetInt(...)
    /// PlayerPrefs.GetInt(...)
    ///
    /// ta gom tất cả vào 1 nơi:
    ///
    /// UserPrefs
    ///
    /// giúp:
    /// - dễ quản lý
    /// - dễ sửa
    /// - tránh typo key
    /// - code sạch hơn
    ///
    /// =========================================================
    /// PLAYER PREFS LÀ GÌ?
    /// =========================================================
    ///
    /// Hệ thống save local đơn giản của Unity.
    ///
    /// Dữ liệu được lưu:
    /// - trên registry (Windows)
    /// - file plist (Mac)
    /// - SharedPreferences (Android)
    ///
    /// Thường dùng để lưu:
    /// - setting
    /// - volume
    /// - level
    /// - currency
    /// - high score
    ///
    /// Không phù hợp cho:
    /// - save game lớn
    /// - inventory phức tạp
    /// - dữ liệu bảo mật
    /// </summary>
    public static class UserPrefs
    {
        #region SAVE KEYS
        const string k_SoundStateKey = "SoundState"; // Key lưu trạng thái sound effect.
        const string k_MusicStateKey = "MusicState"; // Key lưu trạng thái music.
        const string k_LevelIdKey = "LevelId"; // Key lưu level hiện tại.
        const string k_CurrencyKey = "Currency"; // Prefix key cho currency.
        const string k_MusicVolumeKey = "MusicVollumeStateKey";
        const string k_SoundVolumeKey = "SoundVollumeStateKey";
        const string k_LastMusicVolumeKey = "LastMusicVolume";
        const string k_LastSoundVolumeKey = "LastSoundVolume";
        #endregion

        public static bool GetSoundState() // Đọc trạng thái sound.
        {
            return PlayerPrefs.GetInt(k_SoundStateKey, 1) == 1;
        }

        public static void SetSoundState(bool state) // Lưu trạng thái sound.
        {
            PlayerPrefs.SetInt(k_SoundStateKey, state ? 1 : 0);
        }

        public static bool GetMusicState() // Đọc trạng thái music.
        {
            return PlayerPrefs.GetInt(k_MusicStateKey, 1) == 1;
        }

        public static void SetMusicState(bool state) // Lưu trạng thái music.
        {
            PlayerPrefs.SetInt(k_MusicStateKey, state ? 1 : 0);
        }

        public static int GetLevelId() // Đọc level hiện tại.
        {
            return PlayerPrefs.GetInt(k_LevelIdKey, 0);
        }
        
        public static void SetLevelId(int newLevelId) // Save level hiện tại.
        {
            PlayerPrefs.SetInt(k_LevelIdKey, newLevelId);
        }
        
        public static int GetCurrency(int currencyId, int currencyAmount) // Đọc currency theo id.
        {
            return PlayerPrefs.GetInt(k_CurrencyKey + currencyId, currencyAmount);
        }

        public static void SetCurrency(int currencyId, int newCurrencyAmount) // Save currency theo id.
        {
            PlayerPrefs.SetInt(k_CurrencyKey + currencyId, newCurrencyAmount);
        }
        public static float GetSoundVolume()
        {
            return PlayerPrefs.GetFloat(k_SoundVolumeKey, 1f);
        }

        public static void SetSoundVolume(float value)
        {
            PlayerPrefs.SetFloat(k_SoundVolumeKey, value);
        }

        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(k_MusicVolumeKey, 1f);
        }

        public static void SetMusicVolume(float value)
        {
            PlayerPrefs.SetFloat(k_MusicVolumeKey, value);
        }

        public static float GetLastMusicVolume()
        {
            return PlayerPrefs.GetFloat(k_LastMusicVolumeKey, 1f);
        }

        public static void SetLastMusicVolume(float value)
        {
            PlayerPrefs.SetFloat(k_LastMusicVolumeKey, value);
        }

        public static float GetLastSoundVolume()
        {
            return PlayerPrefs.GetFloat(k_LastSoundVolumeKey, 1f);
        }

        public static void SetLastSoundVolume(float value)
        {
            PlayerPrefs.SetFloat(k_LastSoundVolumeKey, value);
        }
    }
}