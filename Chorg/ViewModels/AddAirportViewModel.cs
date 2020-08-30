using System;
using Caliburn.Micro;
using System.IO;
using MaterialDesignThemes.Wpf;
using System.Windows;
using Microsoft.Win32;

namespace Chorg.ViewModels
{
    public class AddAirportViewModel : Screen
    {

        private string _AirportICAO;
        public string AirportICAO
        {
            get => _AirportICAO;
            set {
                _AirportICAO = value;
                NotifyOfPropertyChange(() => AirportICAO);
                NotifyOfPropertyChange(() => CanSave);
            }
        }

        private string _AirportName;
        public string AirportName
        {
            get => _AirportName;
            set { _AirportName = value; NotifyOfPropertyChange(() => AirportName); }
        }

        private bool _IsBusy;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { _IsBusy = value; NotifyOfPropertyChange(() => IsBusy); }
        }

        public bool CanSave
        {
            get => AirportICAO?.Length == 4;
        }

        private bool _WasError;

        public bool WasError
        {
            get { return _WasError; }
            set { _WasError = value; NotifyOfPropertyChange(() => WasError); }
        }

        /// <summary>
        /// Called when Saving the airport
        /// </summary>
        public async void Save()
        {
            WasError = false;
            IsBusy = true;
            try
            {
                var newAirport = await Gateway.GetInstance().AddAirportAsync(AirportICAO, string.IsNullOrWhiteSpace(AirportName) ? null : AirportName);
                MainViewModel.GetInstance().Airports.Add(new AirportViewModel(newAirport));
                MainViewModel.GetInstance().TriggerSnackbar($"Added {newAirport.ICAO}", "WELCOME ABOARD");
                DialogHost.CloseDialogCommand.Execute(null, null);
            }
            catch (Exception e)
            {
                WasError = true;
                MainViewModel.GetInstance().TriggerSnackbar(e);
            }
            IsBusy = false;
            
        }
    }
}
