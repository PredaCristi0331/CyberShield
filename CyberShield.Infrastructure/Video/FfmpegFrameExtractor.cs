using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using CyberShield.Domain.Contracts;
using CyberShield.Domain.Models;

namespace CyberShield.Application.UseCases.ScanVideo; 

public unsafe class FfpmegFrameExtractor
{
    public IEnumerable<VideoFrame> ExtractFrames(string videoPath, FrameDecodeOptions options)
    {
        
        var extractedFrames = new List<VideoFrame>();

        AVFormatContext* formatContext = ffmpeg.avformat_alloc_context();
        if (ffmpeg.avformat_open_input(&formatContext, videoPath, null, null) != 0)
            throw new Exception("Nu am putut deschide fișierul video.");

        if (ffmpeg.avformat_find_stream_info(formatContext, null) < 0)
            throw new Exception("Nu am putut găsi informații despre stream.");

        AVCodec* codec = null;
        int videoStreamIndex = ffmpeg.av_find_best_stream(formatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0);
        if (videoStreamIndex < 0)
            throw new Exception("Nu am găsit un stream video valid.");

        AVStream* videoStream = formatContext->streams[videoStreamIndex];
        AVCodecContext* codecContext = ffmpeg.avcodec_alloc_context3(codec);
        ffmpeg.avcodec_parameters_to_context(codecContext, videoStream->codecpar);

        if (ffmpeg.avcodec_open2(codecContext, codec, null) < 0)
            throw new Exception("Nu am putut deschide codecul.");

        AVFrame* frame = ffmpeg.av_frame_alloc();
        AVFrame* rgbFrame = ffmpeg.av_frame_alloc();
        AVPacket* packet = ffmpeg.av_packet_alloc();

        int width = codecContext->width;
        int height = codecContext->height;
        AVPixelFormat targetFormat = AVPixelFormat.AV_PIX_FMT_BGRA;

        int numBytes = ffmpeg.av_image_get_buffer_size(targetFormat, width, height, 1);
        IntPtr buffer = Marshal.AllocHGlobal(numBytes);
        byte_ptrArray4 rgbData = new byte_ptrArray4 { [0] = (byte*)buffer };
        int_array4 rgbLinesize = new int_array4 { [0] = width * 4 };

        
        SwsContext* swsContext = ffmpeg.sws_getContext(
            width, height, codecContext->pix_fmt,
            width, height, targetFormat,
            2, null, null, null);

        double timeBase = videoStream->time_base.num / (double)videoStream->time_base.den;
        double frameInterval = 1.0 / (options.TargetFps > 0 ? options.TargetFps : 10);
        double nextFrameTime = 0.0;

        try
        {
            while (ffmpeg.av_read_frame(formatContext, packet) >= 0)
            {
                if (packet->stream_index == videoStreamIndex)
                {
                    ffmpeg.avcodec_send_packet(codecContext, packet);

                    while (ffmpeg.avcodec_receive_frame(codecContext, frame) == 0)
                    {
                        double currentFrameTime = frame->pts * timeBase;

                        if (currentFrameTime >= nextFrameTime)
                        {
                            nextFrameTime += frameInterval;

                            ffmpeg.sws_scale(swsContext,
                                frame->data, frame->linesize, 0, height,
                                rgbData, rgbLinesize);

                            byte[] pixelBytes = new byte[numBytes];
                            Marshal.Copy(buffer, pixelBytes, 0, numBytes);

                            
                            extractedFrames.Add(new VideoFrame
                            {
                                Timestamp = TimeSpan.FromSeconds(currentFrameTime),
                                PixelBytes = pixelBytes,
                                Width = width,
                                Height = height
                            });
                        }
                    }
                }
                ffmpeg.av_packet_unref(packet);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
            ffmpeg.sws_freeContext(swsContext);
            ffmpeg.av_frame_free(&rgbFrame);
            ffmpeg.av_frame_free(&frame);
            ffmpeg.av_packet_free(&packet);
            ffmpeg.avcodec_free_context(&codecContext);

            AVFormatContext* pFormatContext = formatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }

        
        return extractedFrames;
    }
}