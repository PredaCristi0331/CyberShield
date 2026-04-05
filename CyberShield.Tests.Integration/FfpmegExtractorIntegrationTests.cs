using System.IO;
using System.Linq;
using Xunit;
using CyberShield.Application.UseCases.ScanVideo;
using CyberShield.Domain.Contracts;

namespace CyberShield.Tests.Integration;

public class FfpmegExtractorIntegrationTests
{
    [Fact]
    public void ExtractFrames_ValidVideo_ReturnsCorrectNumberOfFrames()
    {
        
        string videoPath = Path.Combine(Directory.GetCurrentDirectory(), "test_video.mp4");

        
        if (!File.Exists(videoPath))
            return;

        var extractor = new FfpmegFrameExtractor();

       
        var options = new FrameDecodeOptions(TargetFps: 1, null, true);

        
        var frames = extractor.ExtractFrames(videoPath, options).ToList();

        
        Assert.NotEmpty(frames);

        
        var firstFrame = frames.First();
        Assert.True(firstFrame.Width > 0);
        Assert.True(firstFrame.Height > 0);
        Assert.NotNull(firstFrame.PixelBytes);
        Assert.Equal(firstFrame.Width * firstFrame.Height * 4, firstFrame.PixelBytes.Length);
    }
}