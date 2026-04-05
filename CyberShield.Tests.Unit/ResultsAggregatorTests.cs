using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CyberShield.Application.UseCases.ScanVideo;
using CyberShield.Domain.Models;

namespace CyberShield.Tests.Unit;

public class ResultsAggregatorTests
{
    [Fact]
    public void ComputeOverallScore_EmptyResults_ReturnsZero()
    {
        
        var aggregator = new ResultsAggregator();

        
        var score = aggregator.ComputeOverallScore();

        
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void BuildSegments_GroupsConsecutiveFakeFrames()
    {
        
        var aggregator = new ResultsAggregator();
        var scanId = Guid.NewGuid();

        var frames = new List<FrameResult>
        {
            new FrameResult { Timestamp = TimeSpan.FromSeconds(1), Score = 0.8 }, 
            new FrameResult { Timestamp = TimeSpan.FromSeconds(2), Score = 0.9 }, 
            new FrameResult { Timestamp = TimeSpan.FromSeconds(3), Score = 0.2 }, 
            new FrameResult { Timestamp = TimeSpan.FromSeconds(6), Score = 0.7 }  
        };

        aggregator.AddResults(scanId, frames);

        
        var segments = aggregator.BuildSegments(scanId);

        
        Assert.Equal(2, segments.Count); 

        
        Assert.Equal(TimeSpan.FromSeconds(1), segments[0].Start);
        Assert.Equal(TimeSpan.FromSeconds(2), segments[0].End);
        Assert.Equal(0.85, segments[0].Score, 3); // Media între 0.8 și 0.9

        
        Assert.Equal(TimeSpan.FromSeconds(6), segments[1].Start);
        Assert.Equal(TimeSpan.FromSeconds(6), segments[1].End);
        Assert.Equal(0.7, segments[1].Score);
    }
}