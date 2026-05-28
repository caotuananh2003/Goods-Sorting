using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace GameTemplate.Infrastructure
{
    /// <summary>
    /// MessageChannel là trung tâm của hệ thống Pub/Sub.
    ///
    /// Nó hoạt động giống:
    /// - Event Bus
    /// - Message Bus
    /// - Observer Hub
    ///
    /// Nhiệm vụ chính:
    /// - cho phép object subscribe message
    /// - publish message tới subscriber
    /// - unsubscribe callback
    /// - quản lý lifecycle subscription
    ///
    /// Generic T:
    /// - là kiểu message được gửi.
    ///
    /// Ví dụ:
    ///
    /// MessageChannel<PlayerDeadMessage>
    /// MessageChannel<QuitApplicationMessage>
    ///
    /// Khi Publish(message):
    /// - toàn bộ subscriber của kiểu T
    ///   sẽ được notify.
    ///
    /// Đây là implementation của Observer Pattern
    /// theo kiểu Pub/Sub architecture.
    /// </summary>
    /// <typeparam name="T">
    /// Kiểu dữ liệu message
    /// </typeparam>
    public class MessageChannel<T> : IMessageChannel<T>
    {
        /// <summary>
        /// Danh sách callback đang subscribe.
        ///
        /// Mỗi Action<T> (Action<T> là 1 delegate, nó là 1 kiểu dữ liệu để trỏ tới function khác) là:
        /// - một function
        /// - một listener
        /// - một subscriber callback
        ///
        /// Ví dụ:
        ///
        /// void OnPlayerDead(PlayerDeadMessage msg)
        ///
        /// sẽ được lưu trong list này.
        ///
        /// Khi Publish():
        /// - toàn bộ callback trong list
        ///   sẽ được invoke.
        /// </summary>
        readonly List<Action<T>> m_MessageHandlers = new List<Action<T>>();

        /// /// <summary>
        /// Danh sách thay đổi "tạm thời" (pending).
        ///
        /// Dictionary:
        ///
        /// key   = callback handler
        /// value = trạng thái add/remove
        ///
        /// true:
        /// - callback sẽ được ADD
        ///
        /// false:
        /// - callback sẽ được REMOVE
        ///
        /// ====================================================
        /// TẠI SAO CẦN PENDING HANDLERS?
        /// ====================================================
        ///
        /// Vì không an toàn khi modify List
        /// trong lúc đang foreach.
        ///
        /// Ví dụ nguy hiểm:
        ///
        /// foreach(handler in handlers)
        /// {
        ///     handler();
        /// }
        ///
        /// Nhưng bên trong handler:
        ///
        /// channel.Unsubscribe(thisHandler);
        ///
        /// => List bị modify trong lúc foreach
        ///
        /// => Exception:
        /// Collection was modified
        ///
        /// ====================================================
        /// GIẢI PHÁP
        /// ====================================================
        ///
        /// Không add/remove ngay lập tức.
        ///
        /// Thay vào đó:
        /// - lưu thay đổi vào m_PendingHandlers
        ///
        /// Sau đó:
        /// - apply toàn bộ thay đổi
        ///   trước khi Publish thật sự.
        ///
        /// Đây là kỹ thuật:
        /// Deferred Modification
        /// </summary>
        readonly Dictionary<Action<T>, bool> m_PendingHandlers = new Dictionary<Action<T>, bool>();

        /// <summary>
        /// Channel đã dispose chưa.
        ///
        /// true:
        /// - channel đã cleanup
        /// - không nên dùng nữa
        ///
        /// false:
        /// - channel còn hoạt động
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Dispose channel.
        ///
        /// Cleanup:
        /// - toàn bộ subscribers
        /// - toàn bộ pending handlers
        ///
        /// Sau khi dispose:
        /// - channel không còn hoạt động
        /// - không nên Publish/Subscribe nữa
        ///
        /// Thường dùng khi:
        /// - scene unload
        /// - game shutdown
        /// - destroy system
        /// </summary>
        public virtual void Dispose()
        {
            if (!IsDisposed) // tránh dispose nhiều lần
            {
                IsDisposed = true;

                m_MessageHandlers.Clear(); // xóa toàn bộ subscribers

                m_PendingHandlers.Clear(); // xóa pending changes
            }
        }

        /// <summary>
        /// Publish message tới toàn bộ subscriber.
        ///
        /// Đây là "notify" trong Observer Pattern.
        ///
        /// ====================================================
        /// FLOW
        /// ====================================================
        ///
        /// 1. Apply toàn bộ pending add/remove
        ///
        /// 2. Invoke toàn bộ callback subscriber
        ///
        /// 3. Truyền message vào callback
        ///
        /// ====================================================
        /// VÍ DỤ
        /// ====================================================
        ///
        /// channel.Publish(new PlayerDeadMessage());
        ///
        /// =>
        ///
        /// OnUIPlayerDead(message)
        /// OnAudioPlayerDead(message)
        /// OnAnalyticsPlayerDead(message)
        ///
        /// sẽ được gọi.
        /// </summary>
        /// <param name="message">
        /// Message cần gửi
        /// </param>
        public virtual void Publish(T message)
        {
            foreach (var handler in m_PendingHandlers.Keys) // Apply toàn bộ add/remove đang pending
            {
                if (m_PendingHandlers[handler]) // true = add subscriber
                {
                    m_MessageHandlers.Add(handler);
                }
                else // false = remove subscriber
                {
                    m_MessageHandlers.Remove(handler);
                }
            }
            m_PendingHandlers.Clear(); // clear pending queue

            foreach (var messageHandler in m_MessageHandlers) // invoke toàn bộ subscriber
            {
                if (messageHandler != null)
                {
                    messageHandler.Invoke(message);
                }
            }
        }

        /// <summary>
        /// Subscribe callback mới.
        ///
        /// Callback sẽ được gọi
        /// mỗi khi Publish(message).
        ///
        /// ====================================================
        /// KHÔNG ADD TRỰC TIẾP
        /// ====================================================
        ///
        /// Callback sẽ được:
        /// - đưa vào pending queue
        ///
        /// Sau đó:
        /// - Publish() sẽ apply sau
        ///
        /// ====================================================
        /// RETURN IDisposable
        /// ====================================================
        ///
        /// IDisposable dùng để:
        /// - unsubscribe dễ dàng
        /// - cleanup an toàn
        ///
        /// Ví dụ:
        ///
        /// var sub = channel.Subscribe(OnDead);
        ///
        /// sub.Dispose();
        ///
        /// =>
        ///
        /// tự động unsubscribe.
        /// </summary>
        /// <param name="handler">
        /// Callback cần subscribe
        /// </param>
        /// <returns>
        /// Disposable handle cho subscription
        /// </returns>
        public virtual IDisposable Subscribe(Action<T> handler)
        {
            Assert.IsTrue(!IsSubscribed(handler), "Attempting to subscribe with the same handler more than once"); // Không cho subscribe trùng callback

            if (m_PendingHandlers.ContainsKey(handler)) // Nếu callback đang pending remove
            {
                if (!m_PendingHandlers[handler]) // false = pending remove
                {
                    m_PendingHandlers.Remove(handler); // hủy remove
                }
            }
            else // Đánh dấu. Callback sẽ được add
            {
                m_PendingHandlers[handler] = true;
            }

            var subscription = new DisposableSubscription<T>(this, handler); // tạo disposable subscription
            return subscription;
        }

        /// <summary>
        /// Hủy đăng ký callback.
        ///
        /// Callback sẽ không nhận message nữa.
        ///
        /// ====================================================
        /// KHÔNG REMOVE NGAY
        /// ====================================================
        ///
        /// Chỉ đánh dấu pending remove.
        ///
        /// Publish() sẽ remove thật sự sau.
        ///
        /// Điều này giúp:
        /// - tránh modify collection khi foreach
        /// </summary>
        /// <param name="handler">
        /// Callback cần unsubscribe
        /// </param>
        public void Unsubscribe(Action<T> handler)
        {
            if (IsSubscribed(handler)) // chỉ unsubscribe nếu đang subscribe
            {
                if (m_PendingHandlers.ContainsKey(handler)) // nếu callback đã có trong pending
                {
                    if (m_PendingHandlers[handler]) // true = pending add
                    {
                        m_PendingHandlers.Remove(handler); // hủy add
                    }
                }
                else
                {
                    m_PendingHandlers[handler] = false; // đánh dấu pending remove
                }
            }
        }

        /// <summary>
        /// Kiểm tra callback đã subscribe chưa.
        ///
        /// Hàm này phải kiểm tra:
        /// - subscriber thật sự
        /// - pending add
        /// - pending remove
        ///
        /// vì hệ thống dùng deferred modification.
        /// </summary>
        /// <param name="handler">
        /// Callback cần kiểm tra
        /// </param>
        /// <returns>
        /// true:
        /// - đã subscribe
        ///
        /// false:
        /// - chưa subscribe
        /// </returns>
        bool IsSubscribed(Action<T> handler)
        {
            var isPendingRemoval = m_PendingHandlers.ContainsKey(handler) && !m_PendingHandlers[handler]; // callback đang chờ remove?
            var isPendingAdding = m_PendingHandlers.ContainsKey(handler) && m_PendingHandlers[handler]; // callback đang chờ add?

            // đã tồn tại trong active handlers và không pending remove
            //
            // HOẶC
            //
            // đang pending add
            return m_MessageHandlers.Contains(handler) && !isPendingRemoval || isPendingAdding;
        }
    }
}
