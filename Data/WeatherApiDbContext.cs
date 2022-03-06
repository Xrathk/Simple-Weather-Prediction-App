using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Console_GettingAPIData.Data
{
    /// <summary>
    /// Db context class for weather API app.
    /// </summary>
    public class WeatherApiDbContext : DbContext
    {

        public static string connectionString;

        // Getting connection string from configuration file
        public WeatherApiDbContext(IConfiguration config)
        {
            connectionString = config.GetValue<string>("DbConnectionString");
        }


        // Configuring database
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }


        // Database tables
        public DbSet<RequestEntry> Requests { get; set; }

    }
}
