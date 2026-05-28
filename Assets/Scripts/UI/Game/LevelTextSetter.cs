using TMPro;
using UnityEngine;

namespace GameTemplate.UI
{
    /// <summary>
    /// Helper component dùng để set text hiển thị level.
    ///
    /// Script này là wrapper rất nhỏ cho:
    /// TextMeshProUGUI
    ///
    /// Nó giúp:
    /// - chuẩn hóa cách hiển thị level text
    /// - tránh viết code set text lặp lại nhiều nơi
    /// - cho phép manager/UI khác chỉ cần gọi: SetLevelText(levelId) thay vì phải: GetComponent<TextMeshProUGUI>().text = ...
    ///
    /// ====================================================
    /// THƯỜNG ĐƯỢC GẮN Ở ĐÂU?
    /// ====================================================
    ///
    /// Các object text như:
    ///
    /// - TopPanel/LevelText
    /// - WinPanel/LevelText
    /// - LosePanel/LevelText
    ///
    /// ====================================================
    /// FLOW
    /// ====================================================
    ///
    /// UIGameCanvas.Initialize()
    /// ↓
    /// tìm toàn bộ LevelTextSetter
    /// ↓
    /// gọi:
    /// SetLevelText(levelId)
    /// ↓
    /// update TMP text:
    /// "Level X"
    /// </summary>
    public class LevelTextSetter : MonoBehaviour
    {
        public void SetLevelText(int levelID)
        {
            GetComponent<TextMeshProUGUI>().text = "Level " + levelID;
        }
    }
}