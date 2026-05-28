using System;

namespace GameTemplate.Infrastructure
{
    /// <summary>
    /// Đại diện cho một Message Channel subscription đang active.
    ///
    /// Khi Dispose():
    /// - tự unsubscribe khỏi MessageChannel
    /// </summary>
    /// 
    /// <typeparam name="T"></typeparam>
    /// 
    public class DisposableSubscription<T> : IDisposable
    {
        Action<T> m_Handler; // Callback lại handler
        bool m_IsDisposed; // Xem đã dispose chưa
        IMessageChannel<T> m_MessageChannel; // Message channel đang subscribe

        public DisposableSubscription(IMessageChannel<T> messageChannel, Action<T> handler)
        {
            m_MessageChannel = messageChannel;
            m_Handler = handler;
        }

        /// <summary>
        /// Dispose subscription
        ///
        /// => unsubscribe khỏi channel
        /// </summary>
        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                if (!m_MessageChannel.IsDisposed) // Nếu channel chưa dispose
                {
                    m_MessageChannel.Unsubscribe(m_Handler); // unsubscribe callback (hủy đăng ký để không nhận event m_Handler nữa)
                }

                // cleanup references
                m_Handler = null;
                m_MessageChannel = null;
            }
        }
    }
}
