using Chorg.Models;
using Chorg.Models.Database;
using Chorg.Models.PDF;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chorg
{
    public class Gateway
    {
        private DBClient DBClient;

        private static Gateway instance;
        private static readonly object threadLock = new object();

        public static Gateway GetInstance()
        {
            lock (threadLock)
            {
                if (instance == null)
                    instance = new Gateway();

                return instance;
            }
        }

        private Gateway()
        {
            DBClient = new DBClient(true);
        }

        /// <summary>
        /// Generates a Airport from the given data
        /// </summary>
        /// <param name="icao">ICAO of the Airport</param>
        /// <param name="pdf">Path to PDF</param>
        /// <param name="name">Name of Airport (optional)</param>
        public async Task<Airport> AddAirportAsync(string icao, string name = null)
        {
            Airport airport = null;

            await Task.Run(() =>
            {
                airport = new Airport(icao)
                {
                    Name = name,
                    Charts = new List<Chart>()
                };

                DBClient.AddAirport(airport);
            });

            return airport;
        }

        public async Task AddChartToAirportAsync(Chart chart, Airport airport)
        {
            await Task.Run(() => DBClient.AddChartToAirport(chart, airport));
        }

        /// <summary>
        /// Gets all airports
        /// </summary>
        /// <returns>The airports</returns>
        public async Task<ICollection<Airport>> GetAirportsAsync()
        {
            ICollection<Airport> airports = null;

            await Task.Run(() => 
                {
                    airports = DBClient.GetAirports();
                });

            return airports;
        }

        /// <summary>
        /// Updates the airport in the DB
        /// </summary>
        /// <param name="airport">Airport to update</param>
        /// <param name="charts">Update charts?</param>
        public async Task UpdateAirportAsync(Airport airport, bool charts)
        {
            await Task.Run(() =>
            {
                DBClient.UpdateAirport(airport);

                if (charts)
                {
                    foreach (Chart chart in airport.Charts)
                        DBClient.UpdateChart(chart);
                }
            });
        }

        /// <summary>
        /// Updates the chart in the DB
        /// </summary>
        /// <param name="chart">Chart to update</param>
        public async Task UpdateChartAsync(Chart chart)
        {
            await Task.Run(() => DBClient.UpdateChart(chart));
        }

        /// <summary>
        /// Deletes the airport from the DB
        /// </summary>
        /// <param name="airport">Airport to delete</param>
        public async Task DeleteAirportAsync(Airport airport)
        {
            await Task.Run(() => DBClient.DeleteAirport(airport));
        }

        /// <summary>
        /// Deletes the chart from the DB
        /// </summary>
        /// <param name="chart">Chart to delete</param>
        public async Task DeleteChartAsync(Chart chart)
        {
            await Task.Run(() => DBClient.DeleteChart(chart));
        }

        public void CloseDB()
            => DBClient.Close();
    }
}
