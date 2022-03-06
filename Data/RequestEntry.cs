namespace Console_GettingAPIData.Data
{
    /// <summary>
    /// Weather API request database object. Unique ID is the city name.
    /// </summary>
    public class RequestEntry
    {
        public long Id { get; set; }

        public string CityName { get; set; }

        public double LastRecordedTemperature { get; set; }

        public int TimesRequested { get; set; }

        public DateTime LastRequested { get; set; }
    }
}
