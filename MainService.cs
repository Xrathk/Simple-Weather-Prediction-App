// Importing libraries
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Console_GettingAPIData.Data;
using Newtonsoft.Json;

namespace Console_GettingAPIData
{

    /// <summary>
    /// Weather request service (main service).
    /// </summary>
    public class MainService : IMainService
    {
        // Fields
        private readonly ILogger<MainService> _log;
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor for adding logging and configuration to service.
        /// </summary>
        /// <param name="log">Adding logging</param>
        /// <param name="config">Adding configuration</param>
        public MainService(ILogger<MainService> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        /// <summary>
        /// Runs weather request service.
        /// </summary>
        public async Task Run()
        {

            // Getting user input (city)
            string city = "";
            while (String.IsNullOrEmpty(city))
            {
                Console.WriteLine("Enter the name of the city you want to get a weather forecast for:\n-------------------------------------------------------------------");
                city = Console.ReadLine();
                if (String.IsNullOrEmpty(city))
                {
                    Console.WriteLine("Error -- City name can't be empty. Try again.\n");
                }
            }

            // Getting API key
            _log.LogInformation("Request made about city -- {city}", city); // Logging city name
            string APIkey = _config.GetValue<string>("ApiKey"); // From configuration
            if (APIkey.Equals("")) // Exit program, if no API key has been found.
            {
                Console.WriteLine("Error - No API key has been found in the configuration file. Exiting program...");
                _log.LogError("Could not fetch data about city -- {city} ==== API key not found in configuration file.", city); // Logging error
                Environment.Exit(1);
            }

            // Formulating request and getting output
            Console.WriteLine("\nGetting data for " + city + "...");
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "http://api.weatherapi.com/v1/current.json?key=" + APIkey + "&q=" + city + "&aqi=no"))
                {
                    var response = await httpClient.SendAsync(request);

                    // Get status
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine("Data acquired!");

                        // Printing formatted response based on command line arguments
                        ValidResponseObject weatherData = new ValidResponseObject();
                        weatherData = JsonConvert.DeserializeObject<ValidResponseObject>(response.Content.ReadAsStringAsync().Result); // Deserialize
                        var currentTemp = weatherData.current.temp_c; // Current temperature
                        var currentWeather = weatherData.current.condition.text; // Current weather
                        Console.WriteLine("\nResponse:\n~~~~~~~~~~~~~");
                        Console.WriteLine($"The current temperature in {city} is: {currentTemp} °C");
                        Console.WriteLine($"The current weather in {city} is: {currentWeather}");
                        _log.LogInformation("Successfully received data about city -- {city} ==== Weather: {currentWeather}, Temperature: {currentTemp}", city, currentWeather, currentTemp); // Logging 

                        // Update table
                        try
                        {
                            await UpdateRequestTable(city, currentTemp);
                        }
                        catch (Exception Ex)
                        {
                            Console.WriteLine($"Error storing request data to database. Exception: {Ex}");
                            _log.LogError("Could not store request data about city -- City: {city}, Temperature: {currentTemp}", city, currentTemp);
                        }

                    }
                    else // If request unsuccessful, print appropriate error message.
                    {
                        InvalidResponseObject apiResponse = new InvalidResponseObject();
                        apiResponse = JsonConvert.DeserializeObject<InvalidResponseObject>(response.Content.ReadAsStringAsync().Result); // Deserialize error message
                        var errorCode = apiResponse.error.code;
                        var errorMessage = apiResponse.error.message;
                        var statusCode = response.StatusCode;
                        Console.WriteLine($"Could not retrieve data for city \"{city}\". Error code {errorCode} -- {errorMessage} (HTTP response status code -- {statusCode})");
                        _log.LogInformation("Could not fetch data about city -- {city} ==== Error code {errorCode} -- {errorMessage} (HTTP response status code -- {statusCode})", city, errorCode, errorMessage, statusCode); // Logging 
                    }

                }
            }

            // Getting user input (about complete request table overview)
            int choice = 0;

            while (choice != 1 && choice != 2)
            {
                Console.WriteLine("\nWhat would you like to do? (Press \"1\" or \"2\")");
                Console.WriteLine("1. Request overview and exit");
                Console.WriteLine("2. Exit");
                var input = int.TryParse(Console.ReadLine(), out choice);
                if (choice == 1)
                {
                    _log.LogInformation("Database overview requested.");
                    GetOverview();
                }
            }

            // Exiting
            Console.WriteLine("Exiting WeatherAPI app...");
            _log.LogInformation("Exiting WeatherAPI app...");

        }

        /// <summary>
        /// Updates request table.
        /// </summary>
        /// <param name="cityName">Name of city requested</param>
        /// <param name="cityName">Temperature recorded</param>
        /// <returns></returns>
        public async Task UpdateRequestTable(string cityName, double temperature)
        {
            using (var _context = new WeatherApiDbContext(_config))
            {
                // Check if city exists in table
                var correctEntry = _context.Requests.Where(x => x.CityName.Equals(cityName)).FirstOrDefault();

                // If yes, update
                if (correctEntry != null)
                {
                    correctEntry.TimesRequested++;
                    correctEntry.LastRecordedTemperature = temperature;
                    correctEntry.LastRequested = DateTime.Now;
                }
                else // If no, create new entry
                {
                    RequestEntry newEntry = new RequestEntry(){
                        CityName = cityName,
                        LastRecordedTemperature = temperature,
                        TimesRequested = 1,
                        LastRequested = DateTime.Now
                    };
                    _context.Add(newEntry);
                }

                // Store changes
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves stylized request database overview.
        /// </summary>
        public void GetOverview()
        {
            // Get context, print an entry per line
            using (var _context = new WeatherApiDbContext(_config))
            {
                var entries = _context.Requests;
                entries.OrderByDescending(x => x.LastRecordedTemperature); // Order by last requested city

                Console.WriteLine("\nDatabase overview:\n===========================");
                foreach (var entry in entries)
                {
                    Console.WriteLine($"City: {entry.CityName}, Last recorded temperature: {entry.LastRecordedTemperature} °C, Times requested: {entry.TimesRequested}, Time of last request: {entry.LastRequested}");
                }
                Console.WriteLine("===========================\n");
            }
        }

    }
}
