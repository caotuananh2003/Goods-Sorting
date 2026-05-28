
namespace GameTemplate.ApplicationLifecycle
{
    /// <summary>
    /// Message (event) dùng để yêu cầu thoát game.
    /// 
    /// Được publish thông qua MessageChannel.
    /// 
    /// Không chứa dữ liệu vì chỉ là tín hiệu "Quit".
    /// </summary>
    public struct QuitApplicationMessage
    {
    }
}
