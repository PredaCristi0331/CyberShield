using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CyberShield.Application.UseCases.ScanVideo;
using CyberShield.Domain.Entities;
using CyberShield.Shared.Progress;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace CyberShield.Presentation.ViewModels;

public class ScanViewModel : ObservableObject
{
    private readonly ScanVideoUseCase? _useCase;
    private CancellationTokenSource? _cts;
    public ObservableCollection<ScanSegment> Segments { get; } = new();
    

    private string _filePath = "";
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ((AsyncRelayCommand)StartScanCommand).NotifyCanExecuteChanged();
                ((RelayCommand)CancelCommand).NotifyCanExecuteChanged();

                
                UpdateVerdict();
            }
        }
    }

    private string _statusText = "Idle";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private double _progressPercent;
    public double ProgressPercent
    {
        get => _progressPercent;
        set => SetProperty(ref _progressPercent, value);
    }

    private string? _reportPath;
    public string? ReportPath
    {
        get => _reportPath;
        set => SetProperty(ref _reportPath, value);
    }

    private double _overallScore;
    public double OverallScore
    {
        get => _overallScore;
        set
        {
            
            if (SetProperty(ref _overallScore, value))
            {
                UpdateVerdict();
            }
        }
    }

    
    private string _verdictText = "În așteptare...";
    public string VerdictText
    {
        get => _verdictText;
        set => SetProperty(ref _verdictText, value);
    }

    private string _verdictColor = "#B8B8B8"; 
    public string VerdictColor
    {
        get => _verdictColor;
        set => SetProperty(ref _verdictColor, value);
    }

    
    private void UpdateVerdict()
    {
        if (IsBusy)
        {
            VerdictText = "Se analizează...";
            VerdictColor = "#4CC2FF"; 
            return;
        }

        if (OverallScore == 0 && ProgressPercent == 0)
        {
            VerdictText = "În așteptare...";
            VerdictColor = "#B8B8B8"; 
            return;
        }

        
        if (OverallScore >= 0.5)
        {
            VerdictText = $"DEEPFAKE ({(OverallScore * 100):0.0}%)";
            VerdictColor = "#FF4D4D"; 
        }
        else
        {
            VerdictText = $"AUTENTIC ({(OverallScore * 100):0.0}%)";
            VerdictColor = "#2AD27B"; 
        }
    }
    private double _overlayIntensity;
    public double OverlayIntensity
    {
        get => _overlayIntensity;
        set => SetProperty(ref _overlayIntensity, value);
    }

    public IAsyncRelayCommand StartScanCommand { get; }
    public IRelayCommand CancelCommand { get; }

   
    public ScanViewModel(ScanVideoUseCase? useCase)
    {
        _useCase = useCase;
        StartScanCommand = new AsyncRelayCommand(StartScanAsync, () => !IsBusy);
        CancelCommand = new RelayCommand(Cancel, () => IsBusy);
    }

    public void SetFile(string filePath) => FilePath = filePath;

    private void Cancel() => _cts?.Cancel();

    private async Task StartScanAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath)) return;

        IsBusy = true;
        StatusText = "Starting...";
        ProgressPercent = 0;
        ReportPath = null;
        OverallScore = 0;
        OverlayIntensity = 0;

        _cts = new CancellationTokenSource();

        try
        {
            
            if (_useCase == null)
            {
                var random = new Random();
                double simulatedScore = random.NextDouble();
                for (int i = 0; i <= 100; i += 5)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    ProgressPercent = i;
                    StatusText = i < 50 ? $"Decodare cadre FFmpeg... {i}%" : $"Inferență ML.NET/ONNX... {i}%";
                    if (simulatedScore > 0.5)
                    {
                        OverlayIntensity = (i / 100.0) * simulatedScore;
                    }
                    else
                    {
                        OverlayIntensity = 0;
                    }
                    

                    await Task.Delay(150, _cts.Token);
                }

                OverallScore = simulatedScore;

                Segments.Clear(); 

                
                if (simulatedScore >= 0.5)
                {
                    Segments.Add(new CyberShield.Domain.Entities.ScanSegment
                    {
                        Id = Guid.NewGuid(),
                        Start = TimeSpan.FromSeconds(1.5),
                        End = TimeSpan.FromSeconds(3.2),
                        Score = 0.85,
                        Notes = "Manipulare facială"
                    });

                    Segments.Add(new CyberShield.Domain.Entities.ScanSegment
                    {
                        Id = Guid.NewGuid(),
                        Start = TimeSpan.FromSeconds(5.0),
                        End = TimeSpan.FromSeconds(8.5),
                        Score = simulatedScore,
                        Notes = "Anomalie pixeli"
                    });
                }

                var appFolder = AppContext.BaseDirectory;
                var reportsFolder = System.IO.Path.Combine(appFolder, "assets", "reports");
                System.IO.Directory.CreateDirectory(reportsFolder);

                
                ReportPath = System.IO.Path.Combine(reportsFolder, $"CyberShield_Report_{DateTime.Now.Ticks}.pdf");

                
                QuestPDF.Settings.License = LicenseType.Community;

                
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text("CyberShield Deepfake Report")
                            .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(10);
                                x.Item().Text($"Fișier scanat: {System.IO.Path.GetFileName(FilePath)}").FontSize(14);
                                x.Item().Text($"Data scanării: {DateTime.Now:dd/MM/yyyy HH:mm}");

                                x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                                x.Item().PaddingTop(10).Text("Rezultat Analiză:").SemiBold().FontSize(16);

                                
                                var scoreColor = OverallScore > 0.5 ? QuestPDF.Helpers.Colors.Red.Medium : QuestPDF.Helpers.Colors.Green.Medium;
                                var verdict = OverallScore > 0.5 ? "SUSPECT DE MANIPULARE (DEEPFAKE)" : "VIDEO AUTENTIC";

                                x.Item().Text($"Scor de risc: {OverallScore * 100}%")
                                 .FontSize(20).FontColor(scoreColor).SemiBold();

                                x.Item().Text($"Verdict: {verdict}")
                                 .FontSize(16).FontColor(scoreColor).Bold();

                                x.Item().PaddingTop(20).Text("Acesta este un raport generat automat de aplicația CyberShield.");
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Pagina ");
                                x.CurrentPageNumber();
                            });
                    });
                })
                .GeneratePdf(ReportPath); 


                ProgressPercent = 100;
            }
            else
            {
                
                var progress = new Progress<ScanProgress>(p =>
                {
                    StatusText = $"{p.Stage}: {p.Message}";
                    ProgressPercent = p.Percent;
                });

                var req = new ScanVideoRequest(
                    FilePath,
                    DecodeOptions: new CyberShield.Domain.Contracts.FrameDecodeOptions(10, null, true),
                    PreprocessOptions: new CyberShield.Domain.Contracts.PreprocessOptions(224, 224, true, true),
                    BatchSize: 16,
                    AllowCache: true);

                var result = await _useCase.ExecuteAsync(req, progress, _cts.Token);

                OverallScore = result.OverallScore;
                ReportPath = result.ReportPath;
                OverlayIntensity = Math.Clamp(result.OverallScore, 0, 1);

                StatusText = "Completed";
                ProgressPercent = 100;
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Canceled by user";
        }
        catch (Exception ex)
        {
            StatusText = $"Eroare: {ex.Message}";
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            IsBusy = false;
        }
    }
}