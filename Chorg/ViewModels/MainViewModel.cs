using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Chorg.Models;
using Chorg.Views;
using MaterialDesignThemes.Wpf;

namespace Chorg.ViewModels
{
    public class MainViewModel : Screen
    {
        #region SINGLETON
        private static MainViewModel instance;
        private static readonly object threadLock = new object();

        public static MainViewModel GetInstance()
        {
            lock (threadLock)
            {
                if (instance == null)
                    instance = new MainViewModel();

                return instance;
            }
        }
        #endregion

        public ObservableCollection<AirportViewModel> Airports { get; private set; } = new ObservableCollection<AirportViewModel>();
        public ObservableCollection<ChartViewModel> Charts { get; private set; } = new ObservableCollection<ChartViewModel>();

        public SnackbarMessageQueue SnackbarQueue { get; private set; } = new SnackbarMessageQueue();

        #region MISC
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set {
                _Message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        private bool _CanEditCharts;

        public bool CanEditCharts
        {
            get { return _CanEditCharts; }
            set { _CanEditCharts = value; NotifyOfPropertyChange(() => CanEditCharts); }
        }

        #endregion

        #region SEARCH
        private string _AirportSearch = String.Empty;
        public string AirportSearch
        {
            get { return _AirportSearch; }
            set {
                _AirportSearch = value;
                airportsFilter.Refresh();
            }
        }

        private string _ChartSearch = String.Empty;
        public string ChartSearch
        {
            get { return _ChartSearch; }
            set {
                _ChartSearch = value;
                chartsFilter.Refresh();
            }
        }
        #endregion

        #region FILTERS
        private ICollectionView airportsFilter;
        private ICollectionView chartsFilter;

        private bool _Filter_ND = true;
        public bool Filter_ND
        {
            get => _Filter_ND;
            set { _Filter_ND = value; chartsFilter.Refresh(); }
        }

        private bool _Filter_GEN = true;
        public bool Filter_GEN
        {
            get => _Filter_GEN;
            set { _Filter_GEN = value; chartsFilter.Refresh(); }
        }

        private bool _Filter_TAXI = true;
        public bool Filter_TAXI
        {
            get => _Filter_TAXI;
            set { _Filter_TAXI = value; chartsFilter.Refresh(); }
        }

        private bool _Filter_SID = true;
        public bool Filter_SID
        {
            get => _Filter_SID;
            set { _Filter_SID = value; chartsFilter.Refresh(); }
        }

        private bool _Filter_STAR = true;
        public bool Filter_STAR
        {
            get => _Filter_STAR;
            set { _Filter_STAR = value; chartsFilter.Refresh(); }
        }

        private bool _Filter_APP = true;
        public bool Filter_APP
        {
            get => _Filter_APP;
            set { _Filter_APP = value; chartsFilter.Refresh(); }
        }

        /// <summary>
        /// Returns wheter a filter is set for the given ContentType
        /// </summary>
        /// <param name="content">ContentType</param>
        /// <returns>Is set or not</returns>
        private bool GetContentFilter(ContentType content)
        {
            switch (content)
            {
                case ContentType.UNDEFINED:
                    return Filter_ND;
                case ContentType.GENERAL:
                    return Filter_GEN;
                case ContentType.TAXI:
                    return Filter_TAXI;
                case ContentType.SID:
                    return Filter_SID;
                case ContentType.STAR:
                    return Filter_STAR;
                case ContentType.APP:
                    return Filter_APP;
                default:
                    throw new Exception();
            }
        }
        #endregion

        #region SELECTED
        private AirportViewModel _SelectedAirport;
        public AirportViewModel SelectedAirport {
            get => _SelectedAirport;
            set {
                if (value == null)
                {
                    _SelectedAirport = null;
                    Message = null;
                    CanEditCharts = false;
                    Charts.Clear();
                }
                else
                {
                    _SelectedAirport = value;
                    Message = SelectedAirport.ICAO;
                    CanEditCharts = true;
                    Charts.Replace(SelectedAirport.Charts);
                }
                NotifyOfPropertyChange(() => SelectedAirport);
            }
        }

        private ChartViewModel _SelectedChart;
        public ChartViewModel SelectedChart {
            get => _SelectedChart;
            set {
                if (value == null)
                {
                    _SelectedChart = null;
                    Message = SelectedAirport == null ? null : SelectedAirport.ICAO;
                    SelectedPDF = null;
                }
                else
                {
                    _SelectedChart = value;
                    Message = $"{SelectedAirport.ICAO} | {SelectedChart.Description}";
                    SelectedPDF = SelectedChart.PDFStream;
                }
                NotifyOfPropertyChange(() => SelectedChart);
            }
        }

        private MemoryStream _SelectedPDF;
        public MemoryStream SelectedPDF
        {
            get { return _SelectedPDF; }
            set { _SelectedPDF = value; NotifyOfPropertyChange(() => SelectedPDF); }
        }
        #endregion

        private MainViewModel()
        {
            // Filtering for Airports
            airportsFilter = CollectionViewSource.GetDefaultView(Airports);
            airportsFilter.Filter = (x) => { return (x as ITextSearch).SearchPredicate(AirportSearch); };

            // Filtering for Charts
            chartsFilter = CollectionViewSource.GetDefaultView(Charts);
            chartsFilter.Filter = (x) => {
                ChartViewModel chart = x as ChartViewModel;
                return chart.SearchPredicate(ChartSearch) && GetContentFilter(chart.Content);
            };

            // Init Load
            LoadAirportsFromDBAsync();

            // Done!
            TriggerSnackbar($"Welcome to Chorg! (Version {App.VersionString})", "Hi!");
        }

        /// <summary>
        /// Will trigger the Snackbar
        /// </summary>
        /// <param name="msg">Main message</param>
        /// <param name="btnCnt">Button content</param>
        /// <param name="time">Time in seconds</param>
        public void TriggerSnackbar(object msg, object btnCnt, int time = 3)
        {
            SnackbarQueue.Enqueue(msg, btnCnt, (x) => { }, null, false, false, TimeSpan.FromSeconds(time));
        }

        /// <summary>
        /// Will trigger the Snackbar 
        /// </summary>
        /// <param name="e">Execption to be shown</param>
        public void TriggerSnackbar(Exception e)
        {
            SnackbarQueue.Enqueue($"Whoops! ({e.Message})", "OK", (x) => { }, null, false, false, TimeSpan.FromSeconds(int.MaxValue));
        }

        /// <summary>
        /// Gets Airports from DB to ViewModel
        /// </summary>
        public async void LoadAirportsFromDBAsync()
        {
            try
            {
                var airports = await Gateway.GetInstance().GetAirportsAsync();
                Airports.Replace(airports.ToList().ConvertAll(airport => (AirportViewModel)airport));
            }
            catch (Exception e)
            {
                TriggerSnackbar(e);
            }
        }

        /// <summary>
        /// Opens the AddAirport Dialog
        /// </summary>
        public async void AddAirport()
        {
            // Bind view to viewModel by hand and pass it to the dialoghost
            var view = new AddAirportView();
            var viewModel = new AddAirportViewModel();
            ViewModelBinder.Bind(viewModel, view, null);

            var result = (Airport)await DialogHost.Show(view, "MainDialogHost");

            if(result != null)        
                Airports.Add((AirportViewModel)result);        
        }

        public async void EditCharts()
        {
            var view = new EditChartsView();
            var viewModel = new EditChartsViewModel(SelectedAirport.GetModel());

            ViewModelBinder.Bind(viewModel, view, null);

            await DialogHost.Show(view, "MainDialogHost");
            LoadAirportsFromDBAsync();
        }

        /// <summary>
        /// Display About Screen
        /// </summary>
        public void About()
        {
            DialogHost.Show(new AboutView(), "MainDialogHost");
        }

        /// <summary>
        /// Close DB on close
        /// </summary>
        /// <param name="close">Instance will be closed</param>
        protected override void OnDeactivate(bool close)
        {
            if (close)
                Gateway.GetInstance().CloseDB();
        }
    }
}
