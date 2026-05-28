using System;

namespace GameTemplate.Infrastructure
{
    /// <summary>
    /// Interface đại diện cho một object có khả năng GỬI message.
    ///
    /// Đây là phía "Publisher" trong mô hình Pub/Sub (Publish/Subscribe).
    ///
    /// Object implement interface này có thể:
    /// - phát event
    /// - broadcast message
    /// - notify các subscriber
    ///
    /// Ví dụ:
    /// - Player chết
    /// - UI button được click
    /// - Currency thay đổi
    /// - Game thắng/thua
    ///
    /// Generic T:
    /// - là kiểu dữ liệu message được gửi
    ///
    /// Ví dụ:
    /// IPublisher<QuitApplicationMessage>
    /// IPublisher<PlayerDeadMessage>
    /// </summary>
    /// <typeparam name="T">
    /// Kiểu dữ liệu message được publish
    /// </typeparam>
    public interface IPublisher<T>
    {
        /// <summary>
        /// Gửi message tới toàn bộ subscriber.
        ///
        /// Khi gọi hàm này:
        /// - toàn bộ callback đã subscribe
        ///   sẽ được invoke.
        ///
        /// Ví dụ:
        ///
        /// Publish(new QuitApplicationMessage());
        ///
        /// => toàn bộ subscriber của QuitApplicationMessage
        ///    sẽ nhận được message.
        /// </summary>
        /// <param name="message">
        /// Message cần gửi
        /// </param>
        void Publish(T message);
    }

    /// <summary>
    /// Interface đại diện cho object có khả năng NHẬN message.
    ///
    /// Đây là phía "Subscriber" trong mô hình Pub/Sub.
    ///
    /// Object implement interface này có thể:
    /// - đăng ký callback
    /// - lắng nghe event/message
    /// - unsubscribe khỏi message
    ///
    /// Ví dụ:
    ///
    /// Subscribe(OnPlayerDead);
    ///
    /// Khi PlayerDeadMessage được publish:
    ///
    /// OnPlayerDead(message)
    ///
    /// sẽ tự động được gọi.
    /// </summary>
    /// <typeparam name="T">
    /// Kiểu dữ liệu message cần subscribe
    /// </typeparam>
    public interface ISubscriber<T>
    {
        /// <summary>
        /// Đăng ký callback để nhận message.
        ///
        /// Handler:
        /// - là function sẽ được gọi
        ///   khi có message mới.
        ///
        /// Hàm này trả về IDisposable.
        ///
        /// IDisposable dùng để:
        /// - unsubscribe dễ dàng
        /// - cleanup an toàn
        ///
        /// Ví dụ:
        ///
        /// var sub = channel.Subscribe(OnQuit);
        ///
        /// sub.Dispose();
        ///
        /// => tự động unsubscribe.
        /// </summary>
        /// <param name="handler">
        /// Callback sẽ được invoke khi có message
        /// </param>
        /// <returns>
        /// Subscription handle để dispose/unsubscribe
        /// </returns>
        IDisposable Subscribe(Action<T> handler);

        /// <summary>
        /// Hủy đăng ký callback.
        ///
        /// Sau khi unsubscribe:
        /// - callback sẽ không nhận message nữa.
        ///
        /// Tương tự:
        ///
        /// event -= handler;
        ///
        /// trong C# event system.
        ///
        /// Ví dụ:
        ///
        /// channel.Unsubscribe(OnQuit);
        ///
        /// => OnQuit không còn được gọi nữa.
        /// </summary>
        /// <param name="handler">
        /// Callback cần hủy đăng ký
        /// </param>
        void Unsubscribe(Action<T> handler);
    }

    /// <summary>
    /// Interface đầy đủ của một Message Channel.
    ///
    /// Message Channel là trung tâm của hệ thống Pub/Sub.
    ///
    /// Nó:
    /// - nhận subscriber
    /// - lưu callback
    /// - publish message
    /// - invoke subscriber
    /// - cleanup resource
    ///
    /// Interface này kế thừa:
    ///
    /// IPublisher<T>
    /// => có khả năng gửi message
    ///
    /// ISubscriber<T>
    /// => có khả năng subscribe/unsubscribe
    ///
    /// IDisposable
    /// => có khả năng cleanup/dispose
    ///
    /// Đây chính là:
    /// - Event Bus
    /// - Message Bus
    /// - Observer Hub
    ///
    /// của toàn hệ thống.
    /// </summary>
    /// <typeparam name="T">
    /// Kiểu dữ liệu message
    /// </typeparam>
    public interface IMessageChannel<T> :
        IPublisher<T>,
        ISubscriber<T>,
        IDisposable
    {
        /// <summary>
        /// Kiểm tra channel đã dispose chưa.
        ///
        /// Nếu IsDisposed == true:
        /// - channel không còn hoạt động
        /// - không nên subscribe/publish nữa
        ///
        /// Dispose thường xảy ra khi:
        /// - scene unload
        /// - game shutdown
        /// - cleanup hệ thống
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Message Channel có buffer.
    ///
    /// Buffer:
    /// - lưu lại message cuối cùng.
    ///
    /// Điều này hữu ích khi:
    /// - subscriber subscribe MUỘN
    /// - nhưng vẫn cần biết state hiện tại.
    ///
    /// Ví dụ:
    ///
    /// PlayerHealthChangedMessage(50)
    ///
    /// UI subscribe sau đó,
    /// nhưng vẫn đọc được:
    ///
    /// BufferedMessage = 50
    ///
    /// Đây giống:
    /// - cached event
    /// - sticky event
    /// - retained message
    /// </summary>
    /// <typeparam name="T">
    /// Kiểu dữ liệu message được buffer
    /// </typeparam>
    public interface IBufferedMessageChannel<T> :
        IMessageChannel<T>
    {
        /// <summary>
        /// Có message buffer chưa?
        ///
        /// true:
        /// - đã có message được lưu
        ///
        /// false:
        /// - chưa có message nào
        /// </summary>
        bool HasBufferedMessage { get; }

        /// <summary>
        /// Message cuối cùng được lưu trong buffer.
        ///
        /// Subscriber mới có thể đọc message này
        /// mà không cần đợi publish mới.
        /// </summary>
        T BufferedMessage { get; }
    }
}
