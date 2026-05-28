using System;
using System.Collections.Generic;

namespace GameTemplate.Infrastructure
{
    /// <summary>
    /// Group chứa nhiều IDisposable.
    ///
    /// Dùng để dispose hàng loạt.
    ///
    /// Chủ yếu dùng cho:
    /// - subscriptions
    /// - event handles
    /// - cleanup logic
    /// </summary>
    public class DisposableGroup : IDisposable
    {

        readonly List<IDisposable> m_Disposables = new List<IDisposable>(); // Danh sách IDisposable đang được quản lý

        public void Dispose() // Gọi hàm Dispose() cho toàn bộ object trong group
        {
            foreach (var disposable in m_Disposables)
            {
                disposable.Dispose();
            }

            m_Disposables.Clear(); // Clear list sau khi dispose
        }

        public void Add(IDisposable disposable) // Thêm IDisposable vào group
        {
            m_Disposables.Add(disposable);
        }
    }

    /// <summary> Ví dụ dùng:
    ///var group = new DisposableGroup();
    ///
    ///group.Add(subscription1);
    ///group.Add(subscription2);
    ///
    ///group.Dispose();
    /// -> sẽ unsubscribe toàn bộ.
    /// </summary>

}
