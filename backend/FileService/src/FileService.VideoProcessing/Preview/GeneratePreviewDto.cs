namespace FileService.VideoProcessing.Preview;

public record GeneratePreviewDto(IEnumerable<string> PreviewPaths, string? SpriteSheetPath);