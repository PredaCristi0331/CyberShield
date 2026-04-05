using System;
using System.Windows;
using CyberShield.Presentation.ViewModels;

namespace CyberShield.UI.Wpf
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                

                
                var viewModel = new ScanViewModel(null!);

                
                var mainWindow = new MainWindow(viewModel);

                
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare fatală la pornire:\n\n{ex.Message}\n\n{ex.StackTrace}",
                                "Crash Aplicație",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}