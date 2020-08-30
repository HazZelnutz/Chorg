using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.IO;
using System.Data;

namespace Chorg.Models.Database
{
    public class DBClient
    {
        #region DEFINES
        static readonly string connectionString = "Data Source=Chorg.db;Version=3;foreign_keys=True";
        enum COLUMNS_AIRPORTS { ID = 0, ICAO = 1, NAME = 2}
        enum COLUMNS_CHARTS { ID = 0, AIRPORT = 1, PAGE = 2, IDENTIFIER = 3, DESCRIPTION = 4, CONTENT = 5, PDF = 6, KEYWORDS = 7, PDFSIZE = 8 }
        #endregion

        readonly SQLiteConnection dbCon = new SQLiteConnection(connectionString);

        /// <summary>
        /// Creates a new DB Client and connects to the Chorg.db
        /// </summary>
        public DBClient(bool open)
        {
            // Create Database if not found
            if (!File.Exists("Chorg.db"))
                CreateDatabase();

            if (open)
                Open();
        }

        /// <summary>
        /// Returns a collection of all availabe airports with charts
        /// </summary>
        /// <returns>The Airports</returns>
        public ICollection<Airport> GetAirports()
        {
            List<Airport> result = new List<Airport>();

            // Get Airports
            var airportReader = new SQLiteCommand(Properties.Resources.sql_readAllAirports, dbCon).ExecuteReader(CommandBehavior.KeyInfo);
            while (airportReader.Read())
            {
                int id = airportReader.GetInt32((int)COLUMNS_AIRPORTS.ID);
                string icao = airportReader.GetString((int)COLUMNS_AIRPORTS.ICAO);
                string name = airportReader[COLUMNS_AIRPORTS.NAME.ToString()] == DBNull.Value ? null : airportReader.GetString((int)COLUMNS_AIRPORTS.NAME);

                result.Add(new Airport(icao)
                {
                    Id = id,
                    Name = name,
                    Charts = new List<Chart>()
                });
            }

            // Get Charts
            foreach (Airport airport in result)
            {
                using (var chartCmd = new SQLiteCommand(Properties.Resources.sql_readAllChartsFromAirport, dbCon))
                {
                    chartCmd.Parameters.Add(new SQLiteParameter(DbType.String) { Value = airport.ICAO });
                    var chartReader = chartCmd.ExecuteReader(CommandBehavior.KeyInfo);

                    while (chartReader.Read())
                    {
                        int id = chartReader.GetInt32((int)COLUMNS_CHARTS.ID);
                        int page = chartReader.GetInt32((int)COLUMNS_CHARTS.PAGE);
                        string identifier = chartReader[COLUMNS_CHARTS.IDENTIFIER.ToString()] == DBNull.Value ? null : chartReader.GetString((int)COLUMNS_CHARTS.IDENTIFIER);
                        string description = chartReader[COLUMNS_CHARTS.DESCRIPTION.ToString()] == DBNull.Value ? null : chartReader.GetString((int)COLUMNS_CHARTS.DESCRIPTION);
                        Enum.TryParse(chartReader.GetString((int)COLUMNS_CHARTS.CONTENT), out ContentType content);
                        string keywordsRaw = chartReader[COLUMNS_CHARTS.KEYWORDS.ToString()] == DBNull.Value ? null : chartReader.GetString((int)COLUMNS_CHARTS.KEYWORDS);
                        int size = chartReader.GetInt32((int)COLUMNS_CHARTS.PDFSIZE);

                        airport.Charts.Add(new Chart(page, size)
                        {
                            Id = id,
                            Identifier = identifier,
                            Description = description,
                            Content = content,
                            Keywords = keywordsRaw == null ? new List<string>() : keywordsRaw.Split('\n').ToList()
                        });
                    }
                }
            }
            return result;
        }

        public byte[] GetChartRaw(Chart chart)
        {
            using (var cmd = new SQLiteCommand("SELECT *, LENGTH(PDF) AS PDFSIZE FROM CHARTS WHERE ID = ?", dbCon))
            {
                cmd.Parameters.Add(new SQLiteParameter(DbType.Int32) { Value = chart.Id });
                var chartReader = cmd.ExecuteReader(CommandBehavior.KeyInfo);

                if (chartReader.Read())
                {
                    SQLiteBlob blob = chartReader.GetBlob((int)COLUMNS_CHARTS.PDF, true);
                    int size = chartReader.GetInt32((int)COLUMNS_CHARTS.PDFSIZE);

                    byte[] result = new byte[size];
                    blob.Read(result, size, 0);
                    return result;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Adds the airport
        /// </summary>
        /// <param name="airport">The airport</param>
        /// <returns>The inserted airport with DB ID</returns>
        public void AddAirport(Airport airport)
        {
            if (AirportExists(airport))
                throw new DuplicateNameException("Airport is already in Database. Delete it first!");

            // Add airport
            var addAirportCmd = new SQLiteCommand(Properties.Resources.sql_addAirport, dbCon);
            addAirportCmd.Parameters.Add(new SQLiteParameter(DbType.String) { Value = airport.ICAO });
            addAirportCmd.Parameters.Add(new SQLiteParameter(DbType.String) { Value = airport.Name });
            addAirportCmd.ExecuteNonQuery();

            // Get ID from inserted Airport
            var airportIdReader = new SQLiteCommand(Properties.Resources.sql_readLastRowIdAirport, dbCon).ExecuteReader();
            airportIdReader.Read();
            int airportId = airportIdReader.GetInt32(0);

            // Set Id in airport object
            airport.Id = airportId;  
        }

        /// <summary>
        /// Adds the chart to the given airport
        /// </summary>
        /// <param name="chart">Chart to add</param>
        /// <param name="airport">Airport to add to</param>
        public void AddChartToAirport(Chart chart, Airport airport)
        {
            using (var addChartCmd = new SQLiteCommand(Properties.Resources.sql_addChart, dbCon))
            {
                addChartCmd.Parameters.Clear();
                addChartCmd.Parameters.Add(new SQLiteParameter("@airport", DbType.Int32) { Value = airport.Id });
                addChartCmd.Parameters.Add(new SQLiteParameter("@page", DbType.Int32) { Value = chart.Page });
                addChartCmd.Parameters.Add(new SQLiteParameter("@identifier", DbType.String) { Value = chart.Identifier });
                addChartCmd.Parameters.Add(new SQLiteParameter("@description", DbType.String) { Value = chart.Description });
                addChartCmd.Parameters.Add(new SQLiteParameter("@content", DbType.String) { Value = chart.Content.ToString() });
                addChartCmd.Parameters.Add(new SQLiteParameter("@pdf", DbType.Object) { Value = chart.GetChart() });
                addChartCmd.Parameters.Add(new SQLiteParameter("@keywords", DbType.String)
                {
                    Value = chart.Keywords.Count > 0 ? chart.Keywords.Aggregate(String.Empty, (acc, current) => acc += current + "\n").Trim('\n') : null
                });
                addChartCmd.ExecuteNonQuery();
            }

            // Get inserted ID
            using (var chartIdCmd = new SQLiteCommand(Properties.Resources.sql_readLastRowIdChart, dbCon))
            {
                var chartIdReader = chartIdCmd.ExecuteReader();
                chartIdReader.Read();
                chart.Id = chartIdReader.GetInt32(0);
            }

            // Free Bytes
            chart.FreeRawPdf();
        }

        /// <summary>
        /// Updates the given chart in the DB
        /// </summary>
        /// <param name="chart">The chart</param>
        public void UpdateChart(Chart chart)
        {
            if (!ChartExists(chart))
                throw new Exception("This Chart does not exist in DB!");

            var updateChartCmd = new SQLiteCommand(Properties.Resources.sql_updateChart, dbCon);
            updateChartCmd.Parameters.Add(new SQLiteParameter("@identifier", DbType.String) { Value = chart.Identifier });
            updateChartCmd.Parameters.Add(new SQLiteParameter("@description", DbType.String) { Value = chart.Description });
            updateChartCmd.Parameters.Add(new SQLiteParameter("@content", DbType.String) { Value = chart.Content.ToString() });
            updateChartCmd.Parameters.Add(new SQLiteParameter("@id", DbType.Int32) { Value = chart.Id });
            updateChartCmd.Parameters.Add(new SQLiteParameter("@keywords", DbType.String)
            {
                Value = chart.Keywords.Count > 0 ? chart.Keywords.Aggregate(String.Empty, (acc, current) => acc += current + "\n").Trim('\n') : null
            });

            updateChartCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Update the given airport in the DB
        /// </summary>
        /// <param name="chart">The airport</param>
        public void UpdateAirport(Airport airport)
        {
            if (!AirportExists(airport))
                throw new Exception("This Airport does not exist in DB!");

            var updateAirportCmd = new SQLiteCommand(Properties.Resources.sql_updateAirport, dbCon);
            updateAirportCmd.Parameters.Add(new SQLiteParameter(DbType.String) { Value = airport.Name });
            updateAirportCmd.Parameters.Add(new SQLiteParameter(DbType.String) { Value = airport.ICAO });

            updateAirportCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes the given airport
        /// </summary>
        /// <param name="airport">The airport</param>
        public void DeleteAirport(Airport airport)
        {
            var deleteAirportCmd = new SQLiteCommand(Properties.Resources.sql_deleteAirport, dbCon);
            deleteAirportCmd.Parameters.Add(new SQLiteParameter(DbType.String) { Value = airport.ICAO });
            deleteAirportCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes the given chart
        /// </summary>
        /// <param name="chart">The chart</param>
        public void DeleteChart(Chart chart)
        {
            var deleteChartCmd = new SQLiteCommand(Properties.Resources.sql_deleteChart, dbCon);
            deleteChartCmd.Parameters.Add(new SQLiteParameter(DbType.Int32) { Value = chart.Id });
            deleteChartCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns if the given airport exists
        /// </summary>
        /// <param name="airport">The airport</param>
        /// <returns></returns>
        bool AirportExists(Airport airport)
        {
            var cmd = new SQLiteCommand(Properties.Resources.sql_readAirportExists, dbCon);
            cmd.Parameters.Add(new SQLiteParameter(DbType.String) { Value = airport.ICAO });
            var reader = cmd.ExecuteReader();
            
            reader.Read();
            return reader.GetBoolean(0);
        }

        /// <summary>
        /// Returns if the given Chart exists
        /// </summary>
        /// <param name="chart">The chart</param>
        /// <returns></returns>
        bool ChartExists(Chart chart)
        {
            var cmd = new SQLiteCommand(Properties.Resources.sql_readChartExists, dbCon);
            cmd.Parameters.Add(new SQLiteParameter(DbType.Int32) { Value = chart.Id });
            var reader = cmd.ExecuteReader();

            reader.Read();
            return reader.GetBoolean(0);
        }

        /// <summary>
        /// Opens the DB Connection
        /// </summary>
        public void Open()
        {
            dbCon.Open();

            // Enable Vacuuming
            new SQLiteCommand(Properties.Resources.sql_enableVacuum, dbCon).ExecuteNonQuery();
        }

        /// <summary>
        /// Closes the DB Connection
        /// </summary>
        public void Close() => dbCon.Close();

        /// <summary>
        /// Creates a new 'ChorgDatabase.sqlite' DB with the correct schema
        /// </summary>
        void CreateDatabase()
        {
            SQLiteConnection.CreateFile("Chorg.db");
            SQLiteCommand.Execute(Properties.Resources.sql_createDB, SQLiteExecuteType.NonQuery, connectionString);
        }
    }
}
