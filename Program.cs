// Importing libraries
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace Console_GettingAPIData
{
    /// <summary>
    /// Main WeatherAPI project class.
    /// </summary>
    public class Program
    {
        // Command line arguments
        public static bool verboseIntro = false;
        public static string keyPath = "";
        public static bool newConfig;

        /// <summary>
        /// Main method. Program execution starts here.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static async Task Main(string[] args)
        {
            // Setting app configuration builder
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // Setting up host - Host contains all resources for an app's lifetime (configuration, dependency injection, etc).
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IMainService,MainService>();
                })
                .UseSerilog()
                .Build();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            // Setting up basic logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(configuration.GetValue<string>("LogFolderPath"), rollingInterval: RollingInterval.Day) // Change after so everything is in log folder
                .CreateLogger();
            Log.Logger.Information("WeatherAPI console app has launched."); // Initial log

            // Parsing command line arguments
            if (args.ToList().Contains("--v")) // Intro verbosity
            {
                verboseIntro = true;
            }
            if (args.ToList().Contains("--newConfig")) // Change config file values
            {
                newConfig = true;
            }
            else if (!bool.Parse(configuration.GetValue<string>("InitConfig"))) // If starting up for the first time, initialize 
            {
                newConfig = true;
            }


            // Printing intro
            Console.OutputEncoding = Encoding.UTF8; // Change encoding so copyright symbol displays in console
            UtilityMethods.PrintIntro(verboseIntro);


            // Getting new configuration
            if (newConfig)
            {
                Console.WriteLine("\nSetting up new configuration file...");
                Log.Logger.Information("Modifying configuration file...");
                SetUpConfig(configuration);
                Console.WriteLine("Configuration file updated successfully! Please restart the application.");
                Log.Logger.Information("Configuration file updated.");
                Environment.Exit(1);
            }

            // Run main weather service
            var svc = ActivatorUtilities.CreateInstance<MainService>(host.Services);
            await svc.Run();

        }

        /// <summary>
        /// Sets up connection to appsettings.json configuration file.
        /// </summary>
        /// <param name="builder">Configuration builder for program.</param>
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }

        /// <summary>
        /// Sets up or makes changes to configuration file.
        /// </summary>
        /// <param name="configuration">App configuration</param>
        static void SetUpConfig(IConfiguration configuration)
        {
            // Load up configuration file and modify it at runtime
            var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
            var json = File.ReadAllText(appSettingsPath);

            // Load file
            dynamic config = JsonConvert.DeserializeObject<Data.ConfigObject>(json);

            // Take user input
            Console.WriteLine("\nEnter your username (if you don't want to change it, press leave the input field empty):");
            string userName = Console.ReadLine();
            Console.WriteLine("\nEnter your home city (if you don't want to change it, press leave the input field empty):");
            string homeCity = Console.ReadLine();
            Console.WriteLine("\nEnter your API key (if you don't want to change it, press leave the input field empty):");
            string apiKey = Console.ReadLine();

            // Update config file
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (!String.IsNullOrEmpty(userName))
            {
                config.UserName = userName;
            }
            if (!String.IsNullOrEmpty(homeCity))
            {
                config.HomeCity = homeCity;

            }
            if (!String.IsNullOrEmpty(apiKey))
            {
                config.ApiKey = apiKey;

            }
            config.InitConfig = "true";
            #pragma warning restore CS8602 // Dereference of a possibly null reference.

            // Saving changes
            var newJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(appSettingsPath, newJson);
        }


    }
}
