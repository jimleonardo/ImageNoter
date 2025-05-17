using System;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using ImageNoter.Console.Models;

namespace ImageNoter.Console.Services;

public class ExifExtractor
{
    public ExifData ExtractExifData(string imagePath)
    {
        var directories = ImageMetadataReader.ReadMetadata(imagePath);
        var exifData = new ExifData();

        var exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var exifDescriptionDirectory = directories.FirstOrDefault(d => d.Name == "XMP");

        if (exifIfd0Directory != null)
        {
            exifData.CameraModel = exifIfd0Directory.GetDescription(ExifDirectoryBase.TagModel) ?? string.Empty;
            exifData.Title = exifIfd0Directory.GetDescription(ExifDirectoryBase.TagImageDescription) ?? string.Empty;
        }

        if (exifSubIfdDirectory != null)
        {
            if (exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTaken))
            {
                exifData.DateTaken = dateTaken;
            }

            exifData.FocalLength = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagFocalLength) ?? string.Empty;
            exifData.Aperture = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagFNumber) ?? string.Empty;
            exifData.ShutterSpeed = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagExposureTime) ?? string.Empty;
            exifData.ISO = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagIsoSpeed) ?? string.Empty;
        }

        if (exifDescriptionDirectory != null)
        {
            exifData.Description = exifDescriptionDirectory.GetDescription(ExifDirectoryBase.TagImageDescription) ?? string.Empty;
        }

        // Try to get lens information from various possible tags
        var lensModel = directories
            .SelectMany(d => d.Tags)
            .FirstOrDefault(t => t.Name.Contains("Lens Model") || t.Name.Contains("Lens Info"))
            ?.Description ?? string.Empty;

        exifData.LensModel = lensModel;

        return exifData;
    }
} 