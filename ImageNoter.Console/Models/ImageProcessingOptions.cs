namespace ImageNoter.Console.Models;

public class ImageAnnotationOptions
{
    public string InputDirectory { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public string SubfolderName { get; set; } = "images with exif";
    public int LineHeight { get; set; } = 60;
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Bottom;
    public bool PreserveExif { get; set; } = true;
    public int OutputQuality { get; set; } = 100;  // Default to highest quality for output
}

public enum BorderStyle
{
    Bottom,
    All
} 