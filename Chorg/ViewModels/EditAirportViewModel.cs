using Chorg.Models;
using System;
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;

namespace Chorg.ViewModels
{
    public class EditAirportViewModel : Screen
    {
        private Airport airportModel;

        public string ICAO { get; }

        private string _AirportName ;
        public string AirportName
        {
            get => _AirportName;
            set { _AirportName = value; CanSave = true; }
        }

        private bool _CanSave;
        public bool CanSave
        {
            get { return _CanSave; }
            set { _CanSave = value; NotifyOfPropertyChange(() => CanSave); }
        }

        private bool _ConfirmPending;
        public bool ConfirmPending
        {
            get { return _ConfirmPending; }
            set { _ConfirmPending = value; NotifyOfPropertyChange(() => ConfirmPending); }
        }

        private bool _IsBusy;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { _IsBusy = value; NotifyOfPropertyChange(() => IsBusy); }
        }

        private bool _WasError;
        public bool WasError
        {
            get { return _WasError; }
            set { _WasError = value; NotifyOfPropertyChange(() => WasError); }
        }


        public EditAirportViewModel(Airport airport)
        {
            // Model and ViewModel
            airportModel = airport;

            // Properties of Model
            ICAO = airport.ICAO;
            AirportName = airport.Name;

            CanSave = false;
        }

        /// <summary>
        /// Updates the model and returns the new one
        /// </summary>
        /// <returns>Updated model</returns>
        private Airport GetUpdatedModel()
        {
            airportModel.Name = AirportName;
            return airportModel;
        }

        /// <summary>
        /// Saves the modified airport to the DB
        /// </summary>
        public async void Save()
        {
            WasError = false;
            IsBusy = true;

            try
            {
                await Gateway.GetInstance().UpdateAirportAsync(GetUpdatedModel(), false);
                MainViewModel.GetInstance().TriggerSnackbar($"Updated {airportModel.ICAO}", "OK");
                DialogHost.CloseDialogCommand.Execute(null, null);
            }
            catch (Exception e)
            {
                WasError = true;
                MainViewModel.GetInstance().TriggerSnackbar(e);
            }

            IsBusy = false;
        }

        /// <summary>
        /// Deletes the airport from the DB
        /// </summary>
        public async void Delete()
        {
            if (!ConfirmPending)
                ConfirmPending = true;

            else
            {
                try
                {
                    await Gateway.GetInstance().DeleteAirportAsync(airportModel);
                    MainViewModel.GetInstance().TriggerSnackbar($"Deleted {airportModel.ICAO}", "BYE");
                    DialogHost.CloseDialogCommand.Execute(null, null);
                }
                catch (Exception e)
                {
                    MainViewModel.GetInstance().TriggerSnackbar(e);
                }
            }
        }

        /// <summary>
        /// Handles closings
        /// </summary>
        public async void Close()
        {
            bool sure = false;

            if (CanSave)
            {
                await DialogHost.Show(new Views.PromptDiscardChanges(), "EditAirportDialogHost", delegate (object sender, DialogClosingEventArgs e) {
                    sure = (bool)e.Parameter;
                });
            }

            if (!CanSave || sure)
                DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
