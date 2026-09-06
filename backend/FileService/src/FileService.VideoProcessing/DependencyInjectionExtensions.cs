using FileService.Application.Abstractions;
using FileService.Application.Abstractions.Processing;
using FileService.VideoProcessing.FfmpegProcess;
using FileService.VideoProcessing.Jobs;
using FileService.VideoProcessing.Pipeline;
using FileService.VideoProcessing.Pipeline.Steps;
using FileService.VideoProcessing.Preview;
using FileService.VideoProcessing.ProcessRunner;
using FileService.VideoProcessing.Progress;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.VideoProcessing;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddVideoProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VideoProcessingOptions>(configuration.GetSection(VideoProcessingOptions.SECTION_NAME));
        services.Configure<PreviewOptions>(configuration.GetSection(PreviewOptions.SECTION_NAME));

        services.AddScoped<IProcessRunner, ProcessRunner.ProcessRunner>();
        services.AddScoped<IFfmpegProcessRunner, FfmpegProcessRunner>();
        services.AddScoped<IPreviewGenerator, PreviewGenerator>();

        services.AddScoped<IVideoProcessingService, VideoProcessingService>();
        services.AddScoped<IProcessingPipeline, ProcessingPipeline>();

        RegisterStepHandlers(services);

        services.AddScoped<IVideoProgressReporter, VideoProgressReporter>();
        services.AddScoped<IProcessingJobFactory, VideoProcessingJobFactory>();
        services.AddScoped<IProgressStreamService, ProgressStreamService>();
        services.AddTransient<VideoProcessingJob>();
        services.AddSingleton<IProgressEventQueue, InMemoryProgressQueue>();

        return services;
    }

    private static void RegisterStepHandlers(IServiceCollection services)
    {
        services.AddScoped<IProcessingStepHandler, InitializeStepHandler>();
        services.AddScoped<IProcessingStepHandler, ExtractMetadataStepHandler>();
        services.AddScoped<IProcessingStepHandler, GenerateHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, UploadHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, GeneratePreviewStepHandler>();
        services.AddScoped<IProcessingStepHandler, CleanupStepHandler>();
    }
}