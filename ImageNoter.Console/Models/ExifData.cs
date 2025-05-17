using System;

namespace ImageNoter.Console.Models;

public class ExifData
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DateTaken { get; set; }
    public string CameraModel { get; set; } = string.Empty;
    public string LensModel { get; set; } = string.Empty;
    public string FocalLength { get; set; } = string.Empty;
    public string Aperture { get; set; } = string.Empty;
    public string ShutterSpeed { get; set; } = string.Empty;
    public string ISO { get; set; } = string.Empty;

    public string GetTitleLine()
        => string.IsNullOrWhiteSpace(Title) ? Description : Title;

    public string GetCameraLine()
        => $"{DateTaken?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown Date"} | {CameraModel} | {LensModel}".Trim();

    public string GetTechnicalLine()
        => $"{FocalLength} | {Aperture} | {ShutterSpeed} | ISO {ISO}".Trim();
} 