namespace CyberShield.Shared.Progress;

public sealed record ScanProgress(
    string Stage,
    double Percent, 
    string Message);