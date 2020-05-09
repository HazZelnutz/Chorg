using Caliburn.Micro;
using Chorg.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace Chorg.ViewModels
{
    public class EditChartViewModel : Screen
    {
        private Chart chartModel;
        private ChartViewModel chartViewModel;

        private ContentType _Content;
        private ContentType Content
        {
            get => _Content;
            set { _Content = value; CanSave = true; }
        }

        #region ContentBools
        public bool IsGeneral
        {
            get => Content == ContentType.GENERAL;
            set { if (value) Content = ContentType.GENERAL; }
        }

        public bool IsTaxi
        {
            get => Content == ContentType.TAXI;
            set { if (value) Content = ContentType.TAXI; }
        }

        public bool IsSid
        {
            get => Content == ContentType.SID;
            set { if (value) Content = ContentType.SID; }
        }

        public bool IsStar
        {
            get => Content == ContentType.STAR;
            set { if (value) Content = ContentType.STAR; }
        }

        public bool IsApp
        {
            get => Content == ContentType.APP;
            set { if (value) Content = ContentType.APP; }
        }
        #endregion

        private string _Description;
        public string Description
        {
            get => _Description;
            set { _Description = value; CanSave = true; }
        }

        private string _Identifier;
        public string Identifier
        {
            get => _Identifier; 
            set { _Identifier = value; CanSave = true; }
        }

        public ObservableCollection<string> Keywords { get; set; } = new ObservableCollection<string>();

        public bool HasKeywords
        {
            get => Keywords.Count > 0;
        }

        private string _NewKeyword;
        public string NewKeyword
        {
            get { return _NewKeyword; }
            set { _NewKeyword = value; NotifyOfPropertyChange(() => NewKeyword); }
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

        private bool _CanSave;
        public bool CanSave
        {
            get { return _CanSave; }
            set { _CanSave = value; NotifyOfPropertyChange(() => CanSave); }
        }

        public EditChartViewModel(Chart chart, ChartViewModel chartVM)
        {
            // Model and ViewModel
            chartModel = chart;
            chartViewModel = chartVM;

            // Properties of Model
            Content = chart.Content;
            Description = chart.Description;
            Identifier = chart.Identifier;
            Keywords.Replace(chart.Keywords);

            // Update Property "HasKeywords"
            Keywords.CollectionChanged += (o, e) => { NotifyOfPropertyChange(() => HasKeywords); CanSave = true; };

            CanSave = false;
        }

        public void DeleteKeyword(string keyword)
            => Keywords.Remove(keyword);
        
        public void AddKeyword()
        {
            if (!string.IsNullOrWhiteSpace(NewKeyword))
            {
                if (!Keywords.Contains(NewKeyword))
                    Keywords.Add(NewKeyword);
            }
            NewKeyword = null;
        }

        public void EnterOnNewKeyword(KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
                AddKeyword();
        }

        /// <summary>
        /// Updates the model and returns the new one
        /// </summary>
        /// <returns>Updated model</returns>
        Chart GetUpdatedModel()
        {
            chartModel.Content = Content;
            chartModel.Identifier = Identifier;
            chartModel.Description = Description;
            chartModel.Keywords = Keywords;

            return chartModel;
        }

        /// <summary>
        /// Updates the chart in the DB
        /// </summary>
        public async void Save()
        {
            WasError = false;
            IsBusy = true;

            try
            {
                await Gateway.GetInstance().UpdateChartAsync(GetUpdatedModel());
                MainViewModel.GetInstance().TriggerSnackbar("Updated the Chart", "OK");
                PopupBox.ClosePopupCommand.Execute(null, null);
            }
            catch (Exception e)
            {
                WasError = true;
                MainViewModel.GetInstance().TriggerSnackbar(e);
            }

            IsBusy = false;
        }


        private bool _ConfirmPending;
        public bool ConfirmPending
        {
            get { return _ConfirmPending; }
            set { _ConfirmPending = value; NotifyOfPropertyChange(() => ConfirmPending); }
        }

        /// <summary>
        /// Deletes the chart from the DB
        /// </summary>
        public async void Delete()
        {
            if (!ConfirmPending)
                ConfirmPending = true;

            else
            {
                try
                {
                    await Gateway.GetInstance().DeleteChartAsync(chartModel);
                    MainViewModel.GetInstance().Charts.Remove(chartViewModel);
                    MainViewModel.GetInstance().TriggerSnackbar("Deleted the Chart", "BYE");
                    PopupBox.ClosePopupCommand.Execute(null, null);
                }
                catch (Exception e)
                {
                    MainViewModel.GetInstance().TriggerSnackbar(e);
                }
            }
        }
    }
}
