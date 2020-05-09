using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

namespace Chorg
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        public static string VersionString { get => $"{Version.Major}.{Version.Minor}.{Version.Build}"; }
    }
}
