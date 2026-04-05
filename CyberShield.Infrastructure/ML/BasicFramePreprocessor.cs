using System;
using CyberShield.Domain.Contracts;
using CyberShield.Domain.Models; // Sau unde se află clasa ta de cadru brut

namespace CyberShield.Application.UseCases.ScanVideo;

public class BasicFramePreprocessor
{
    public float[] Preprocess(dynamic frame, PreprocessOptions options)
    {
        
        int targetW = options.TargetWidth;
        int targetH = options.TargetHeight;

        float[] chw = new float[3 * targetW * targetH];

        int origW = frame.Width;
        int origH = frame.Height;
        byte[] bgra = frame.PixelBytes;

        float xRatio = (float)origW / targetW;
        float yRatio = (float)origH / targetH;
        int channelStride = targetW * targetH;

        for (int y = 0; y < targetH; y++)
        {
            int sy = (int)(y * yRatio);
            if (sy >= origH) sy = origH - 1;

            for (int x = 0; x < targetW; x++)
            {
                int sx = (int)(x * xRatio);
                if (sx >= origW) sx = origW - 1;

                int origIndex = (sy * origW + sx) * 4;

                byte b = bgra[origIndex];
                byte g = bgra[origIndex + 1];
                byte r = bgra[origIndex + 2];

                int spatialIndex = y * targetW + x;

                chw[0 * channelStride + spatialIndex] = r / 255.0f;
                chw[1 * channelStride + spatialIndex] = g / 255.0f;
                chw[2 * channelStride + spatialIndex] = b / 255.0f;
            }
        }

        return chw;
    }
}