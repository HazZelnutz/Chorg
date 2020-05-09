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
        private string pdfPath;

        public string PDFName
        {
            get => Path.GetFileName(pdfPath) ?? "Click to select a PDF";
        }

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
            get => pdfPath != null && AirportICAO?.Length == 4;
        }

        private bool _WasError;

        public bool WasError
        {
            get { return _WasError; }
            set { _WasError = value; NotifyOfPropertyChange(() => WasError); }
        }

        public void LoadPDF()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "PDF Document (*.pdf)|*.pdf;*.PDF"
            };

            if ((bool)dialog.ShowDialog())
            {
                pdfPath = dialog.FileName;
                NotifyOfPropertyChange(() => PDFName);
                NotifyOfPropertyChange(() => CanSave);
            }
        }    

        /// <summary>
        /// Called when Saving the PDF
        /// </summary>
        public async void Save()
        {
            WasError = false;
            IsBusy = true;
            try
            {
                var newAirport = await Gateway.GetInstance().AddAirportAsync(AirportICAO, pdfPath, string.IsNullOrWhiteSpace(AirportName) ? null : AirportName);
                MainViewModel.GetInstance().TriggerSnackbar($"Added {newAirport.ICAO}", "WELCOME ABOARD");
                DialogHost.CloseDialogCommand.Execute(newAirport, null);
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
