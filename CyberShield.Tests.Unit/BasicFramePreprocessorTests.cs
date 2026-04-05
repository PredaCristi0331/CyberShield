using System;
using Xunit;

using CyberShield.Application.UseCases.ScanVideo;
using CyberShield.Domain.Contracts;
using CyberShield.Domain.Models; 

namespace CyberShield.Tests.Unit;

public class BasicFramePreprocessorTests
{
    [Fact]
    public void Preprocess_ConvertsToCHW_AndNormalizes()
    {
        
        var preprocessor = new BasicFramePreprocessor();

        byte[] bgraBytes = new byte[2 * 2 * 4];
        for (int i = 0; i < bgraBytes.Length; i += 4)
        {
            bgraBytes[i] = 0;     // B
            bgraBytes[i + 1] = 0;   // G
            bgraBytes[i + 2] = 255; // R
            bgraBytes[i + 3] = 255; // A
        }

        var dummyFrame = new VideoFrame
        {
            Timestamp = TimeSpan.Zero, // <--- Aceasta este linia care lipsea!
            Width = 2,
            Height = 2,
            PixelBytes = bgraBytes
        };

        var options = new PreprocessOptions(TargetWidth: 2, TargetHeight: 2, true, true);

        
        float[] result = preprocessor.Preprocess(dummyFrame, options);

        
        Assert.Equal(3 * 2 * 2, result.Length);
        Assert.Equal(1.0f, result[0]);
        Assert.Equal(1.0f, result[3]);
        Assert.Equal(0.0f, result[4]);
        Assert.Equal(0.0f, result[8]);
    }
}