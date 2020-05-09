using System;
using System.Windows;
using Caliburn.Micro;
using Chorg.ViewModels;

namespace Chorg
{
    public class Bootstraper : BootstrapperBase
    {
        public Bootstraper()
        {
            Initialize();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            if (serviceType == typeof(MainViewModel))
                return MainViewModel.GetInstance();
            else
                return base.GetInstance(serviceType, key);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}
