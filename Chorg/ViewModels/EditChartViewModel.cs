using Caliburn.Micro;
using Chorg.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace Chorg.ViewModels
{
    public class EditChartViewModel : ChartViewModel
    {
        new private ContentType Content
        {
            get => model.Content;
            set { model.Content = value; (Parent as EditChartsViewModel).UnsavedChanges = true; }
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

        new public string Description
        {
            get => model.Description;
            set { model.Description = value; (Parent as EditChartsViewModel).UnsavedChanges = true; }
        }

        new public string Identifier
        {
            get => model.Identifier;
            set { model.Identifier = value; (Parent as EditChartsViewModel).UnsavedChanges = true; }
        }

        public ObservableCollection<string> Keywords
        {
            get => new ObservableCollection<string>(model.Keywords);
        }

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

        public EditChartViewModel(Chart chart) : base(chart)
        {
        }

        public void DeleteKeyword(string keyword)
        {
            model.Keywords.Remove(keyword);
            (Parent as EditChartsViewModel).UnsavedChanges = true;     
            NotifyOfPropertyChange(() => Keywords);
            NotifyOfPropertyChange(() => HasKeywords);
        }
        
        public void AddKeyword()
        {
            if (!string.IsNullOrWhiteSpace(NewKeyword))
            {
                if (!Keywords.Contains(NewKeyword))
                {
                    model.Keywords.Add(NewKeyword);
                    (Parent as EditChartsViewModel).UnsavedChanges = true;
                    NotifyOfPropertyChange(() => Keywords);
                    NotifyOfPropertyChange(() => HasKeywords);
                }
            }
            NewKeyword = null;
        }

        public void EnterOnNewKeyword(KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
                AddKeyword();
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
                    // Remove from Database
                    await Gateway.GetInstance().DeleteChartAsync(model);

                    // Remove from Chart Editor
                    (Parent as EditChartsViewModel).ChartThumbs.Remove(model);
                }
                catch (Exception e)
                {
                    MainViewModel.GetInstance().TriggerSnackbar(e);
                }
            }
        }
    }
}
