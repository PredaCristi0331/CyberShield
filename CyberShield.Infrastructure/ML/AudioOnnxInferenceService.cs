using CyberShield.Domain.Contracts;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Options;
using NWaves.Signals;

namespace CyberShield.Infrastructure.ML;

public sealed class AudioOnnxInferenceService : IAudioInferenceService
{
    public async Task<double> AnalyzeAudioChunkAsync(byte[] audioPcmBytes, CancellationToken ct)
    {
        
        var mfccTensor = ExtractMFCC(audioPcmBytes, sampleRate: 16000);

        

        
        await Task.Delay(100, ct);
        return 0.15; 
    }

    private float[] ExtractMFCC(byte[] pcmBytes, int sampleRate)
    {
       
        float[] samples = ConvertPcm16ToFloat(pcmBytes);
        var signal = new DiscreteSignal(sampleRate, samples);


        var options = new MfccOptions
        {
            SamplingRate = sampleRate,
            FeatureCount = 13,         
            FrameDuration = 0.025,     
            HopDuration = 0.010,       
            FilterBankSize = 26,       
            PreEmphasis = 0.97         
            
        };

        var extractor = new MfccExtractor(options);

        
        var mfccFrames = extractor.ComputeFrom(signal);

       
        return mfccFrames.SelectMany(frame => frame).ToArray();
    }

   
    private float[] ConvertPcm16ToFloat(byte[] pcmBytes)
    {
        var floats = new float[pcmBytes.Length / 2];
        for (int i = 0; i < floats.Length; i++)
        {
            
            short sample = BitConverter.ToInt16(pcmBytes, i * 2);
            floats[i] = sample / 32768f;
        }
        return floats;
    }
}