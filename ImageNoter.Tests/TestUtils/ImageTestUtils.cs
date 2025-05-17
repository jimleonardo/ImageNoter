using System;
using System.IO;
using System.Text;
using SkiaSharp;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;

namespace ImageNoter.Tests.TestUtils;

public static class ImageTestUtils
{
    public static string CreateImageWithExif(
        string title = "Test Image",
        string cameraModel = "Test Camera",
        string aperture = "f/2.8",
        string shutterSpeed = "1/100",
        int width = 100,
        int height = 100)
    {
        var testImagePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.jpg");
        
        // Create base image with content
        using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
        {
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Add some visual content
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12
            };
            canvas.DrawText(title, 10, height / 2, paint);

            // Draw some colored shapes to ensure quality differences are visible
            using var shapePaint = new SKPaint { Color = SKColors.Blue };
            canvas.DrawCircle(width / 2, height / 2, 30, shapePaint);
            
            shapePaint.Color = SKColors.Red;
            canvas.DrawRect(new SKRect(10, 10, 40, 40), shapePaint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            File.WriteAllBytes(testImagePath, data.ToArray());
        }

        return testImagePath;
    }

    public static string CreateCorruptedImage()
    {
        var corruptedPath = Path.Combine(Path.GetTempPath(), $"corrupted_{Guid.NewGuid()}.jpg");
        File.WriteAllText(corruptedPath, "This is not a valid JPEG file");
        return corruptedPath;
    }

    public static string CreateBasicImage(int width = 100, int height = 100)
    {
        var testImagePath = Path.Combine(Path.GetTempPath(), $"basic_{Guid.NewGuid()}.jpg");
        var imageInfo = new SKImageInfo(width, height);
        
        using (var surface = SKSurface.Create(imageInfo))
        {
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Add some colored shapes to ensure quality differences are visible
            using var paint = new SKPaint { Color = SKColors.Blue };
            canvas.DrawCircle(width / 2, height / 2, 30, paint);
            
            paint.Color = SKColors.Red;
            canvas.DrawRect(new SKRect(10, 10, 40, 40), paint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            using var stream = File.OpenWrite(testImagePath);
            data.SaveTo(stream);
        }

        return testImagePath;
    }
} 