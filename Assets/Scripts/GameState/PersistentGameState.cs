namespace GameTemplate.Gameplay.GameState
{
    /// <summary>
    /// Trạng thái kết quả game.
    /// </summary>
    public enum WinState
    {
        Invalid,
        Win,
        Loss
    }
    /// <summary>
    /// Class lưu trạng thái win/lose hiện tại.
    ///
    /// Được inject như singleton.
    /// </summary>
    /// 
    /// <summary>
    /// Class containing some data that needs to be passed between ServerBossRoomState and PostGameState to represent the game session's win state.
    /// </summary>
    public class PersistentGameState
    {
        /// <summary>
        /// Trạng thái thắng/thua hiện tại
        /// </summary>
        public WinState WinState { get; private set; }

        /// <summary>
        /// Set trạng thái game
        /// </summary>
        public void SetWinState(WinState winState)
        {
            WinState = winState;
        }

        /// <summary>
        /// Reset state
        /// </summary>
        public void Reset()
        {
            WinState = WinState.Invalid;
        }
    }
}
