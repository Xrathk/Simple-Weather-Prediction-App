namespace Console_GettingAPIData.Data
{
    /// <summary>
    /// Response object for successful API calls.
    /// </summary>
    internal class ValidResponseObject
    {
        public Location location { get; set; }

        public Current current { get; set; }
    }
}
