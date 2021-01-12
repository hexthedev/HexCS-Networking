namespace HexCS.Networking
{
    /// <summary>
    /// This class an be converted into a message
    /// </summary>
    public interface IMessagable
    {
        /// <summary>
        /// Get as a message
        /// </summary>
        /// <returns></returns>
        Message AsMessage();
    }
}
