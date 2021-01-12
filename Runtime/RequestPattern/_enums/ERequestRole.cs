namespace HexCS.Networking
{
    public enum ERequestRole
    {
        // A Message asking for some kind of response
        REQUEST = 0,

        // The response to a request
        RESPONSE = 1,

        // The result of the response
        RESULT = 2
    }
}
