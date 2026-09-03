namespace FileService.VideoProcessing.Preview;

public record PreviewOptions
{
    public const string SECTION_NAME = "PreviewOptions";

    public int Quality { get; init; } = 2;

    public int FrameWidth { get; init; } = 320;

    public int FrameHeight { get; init; } = 180;

    public string FileNamePattern { get; init; } = "preview_{index}.jpg";

    public string SpriteSheetFileName { get; init; } = "sprite_sheet.jpg";

    public int MaxPreviewCount { get; init; } = 10;

    public int SecondsPerPreview { get; init; } = 30;
}