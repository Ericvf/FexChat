﻿using System.Windows;
using System.Windows.Threading;

namespace RaboChat.WpfClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "An error occured.", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
