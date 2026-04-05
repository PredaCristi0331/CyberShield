using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using CyberShield.Presentation.ViewModels;

namespace CyberShield.UI.Wpf;

public partial class MainWindow : Window
{
    private readonly ScanViewModel _vm;
    private bool _isDragActive;

    public MainWindow(ScanViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select a video file",
            Filter = "Video files|*.mp4;*.mkv;*.mov;*.avi|All files|*.*"
        };

        if (dlg.ShowDialog(this) == true)
            _vm.SetFile(dlg.FileName);
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_vm.FilePath)) return;
        Player.Source = new Uri(_vm.FilePath);
        Player.Play();
    }

    private void Pause_Click(object sender, RoutedEventArgs e) => Player.Pause();

    private void Stop_Click(object sender, RoutedEventArgs e) => Player.Stop();

    private void OpenReport_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_vm.ReportPath)) return;

        try
        {
            // Verificăm dacă fișierul există fizic pe disc
            if (System.IO.File.Exists(_vm.ReportPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _vm.ReportPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show($"Fișierul {_vm.ReportPath} nu există fizic pe disc.\n\nAceasta este faza de simulare UI. Raportul PDF va fi generat real odată cu integrarea QuestPDF.",
                                "Simulare Raport",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Nu am putut deschide raportul: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Drag & drop UX highlight
    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        _isDragActive = true;
        DropZone.BorderBrush = FindResource("AccentBrush") as System.Windows.Media.Brush;
        DropZone.BorderThickness = new Thickness(2);
        e.Handled = true;
    }

    private void Window_DragLeave(object sender, DragEventArgs e)
    {
        _isDragActive = false;
        DropZone.BorderBrush = FindResource("BorderBrushSoft") as System.Windows.Media.Brush;
        DropZone.BorderThickness = new Thickness(1);
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        _isDragActive = false;
        DropZone.BorderBrush = FindResource("BorderBrushSoft") as System.Windows.Media.Brush;
        DropZone.BorderThickness = new Thickness(1);

        if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
            _vm.SetFile(files[0]);
    }
}