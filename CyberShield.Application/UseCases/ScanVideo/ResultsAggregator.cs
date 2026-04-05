using System;
using System.Collections.Generic;
using System.Linq;
using CyberShield.Domain.Models;
using CyberShield.Domain.Entities;

namespace CyberShield.Application.UseCases.ScanVideo; 

public class ResultsAggregator
{
    private readonly Dictionary<Guid, List<FrameResult>> _results = new();

    public void AddResults(Guid scanId, IReadOnlyList<FrameResult> results)
    {
        if (!_results.ContainsKey(scanId))
        {
            _results[scanId] = new List<FrameResult>();
        }

        
        foreach (var frame in results)
        {
            _results[scanId].Add(frame);
        }
    }

    
    public double ComputeOverallScore()
    {
        var allFrames = _results.Values.SelectMany(x => x).ToList();
        if (allFrames.Count == 0) return 0.0;

        return allFrames.Average(f => f.Score);
    }

    public IReadOnlyList<ScanSegment> BuildSegments(Guid scanId)
    {
        if (!_results.TryGetValue(scanId, out var frames) || frames.Count == 0)
        {
            return Array.Empty<ScanSegment>();
        }

        var orderedFrames = frames.OrderBy(f => f.Timestamp).ToList();
        var segments = new List<ScanSegment>();

        double fakeThreshold = 0.5;
        TimeSpan maxMergeGap = TimeSpan.FromSeconds(1.5);

        TimeSpan? currentStart = null;
        TimeSpan? currentEnd = null;
        List<double> segmentScores = new();

        foreach (var frame in orderedFrames)
        {
            if (frame.Score >= fakeThreshold)
            {
                if (currentStart == null)
                {
                    currentStart = frame.Timestamp;
                    currentEnd = frame.Timestamp;
                    segmentScores.Clear();
                    segmentScores.Add(frame.Score);
                }
                else
                {
                    if (frame.Timestamp - currentEnd.Value <= maxMergeGap)
                    {
                        currentEnd = frame.Timestamp;
                        segmentScores.Add(frame.Score);
                    }
                    else
                    {
                        segments.Add(new ScanSegment
                        {
                            Id = Guid.NewGuid(),
                            ScanSessionId = scanId,
                            Start = currentStart.Value,            // Mapare corectată
                            End = currentEnd.Value,              // Mapare corectată
                            Score = segmentScores.Average(),     // Mapare corectată
                            Notes = $"Peak: {segmentScores.Max() * 100:0.0}%" // Mapare corectată
                        });

                        currentStart = frame.Timestamp;
                        currentEnd = frame.Timestamp;
                        segmentScores.Clear();
                        segmentScores.Add(frame.Score);
                    }
                }
            }
        }

        if (currentStart != null && currentEnd != null)
        {
            segments.Add(new ScanSegment
            {
                Id = Guid.NewGuid(),
                ScanSessionId = scanId,
                Start = currentStart.Value,            // Mapare corectată
                End = currentEnd.Value,              // Mapare corectată
                Score = segmentScores.Average(),     // Mapare corectată
                Notes = $"Peak: {segmentScores.Max() * 100:0.0}%" // Mapare corectată
            });
        }

        return segments;
    }
}