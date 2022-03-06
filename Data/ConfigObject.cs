namespace Console_GettingAPIData.Data
{
    /// <summary>
    /// App configuration model (all properties).
    /// </summary>
    internal class ConfigObject
    {
        public Serilog Serilog { get; set; }
        public string InitConfig { get; set; }
        public string LogFolderPath { get; set; }
        public string ApiKey { get; set; }
        public string UserName { get; set; }
        public string HomeCity { get; set; }
    }

    internal class Serilog
    {
        public MinimumLevel MinimumLevel { get; set; }
    }

    internal class MinimumLevel
    {
        public string Default { get; set; }

        public Override Override { get; set; }
    }

    internal class Override
    {
        public string Microsoft { get; set; }
        public string System { get; set; }
    }
}
