using System;
using SkiaSharp;
using ImageNoter.Console.Models;

namespace ImageNoter.Console.Services;

public class ImageProcessor : IDisposable
{
    private readonly SKTypeface _typeface;
    private const int MinFontSize = 12;
    private const int MaxFontSize = 48;
    private const int DefaultSidePadding = 20;  // Default padding for left/right sides
    private const float BottomMarginRatio = 0.12f;  // Bottom margin as percentage of line height

    public ImageProcessor()
    {
        _typeface = SKTypeface.FromFamilyName("Arial");
    }

    public void ProcessImage(string inputPath, string outputPath, ExifData exifData, ImageProcessingOptions options)
    {
        using var originalBitmap = SKBitmap.Decode(inputPath);
        
        // Get non-empty lines only
        var textLines = new[]
        {
            exifData.GetTitleLine(),
            exifData.GetCameraLine(),
            exifData.GetTechnicalLine()
        }.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

        if (textLines.Length == 0)
        {
            // If no text to add, just save the original image
            using var originalImage = SKImage.FromBitmap(originalBitmap);
            using var encodedData = originalImage.Encode(SKEncodedImageFormat.Jpeg, options.OutputQuality);
            using var outputStream = File.OpenWrite(outputPath);
            outputStream.SetLength(0);
            encodedData.SaveTo(outputStream);
            return;
        }

        var totalHeight = options.LineHeight * textLines.Length;
        var bottomMargin = (int)(options.LineHeight * BottomMarginRatio);

        // Calculate border dimensions based on style
        int topBorder, bottomBorder, sidePadding;
        if (options.BorderStyle == BorderStyle.All)
        {
            topBorder = options.LineHeight;  // Top border equals line height
            bottomBorder = totalHeight + options.LineHeight + bottomMargin;  // Text area + padding + margin
            sidePadding = options.LineHeight;  // Side padding equals line height for consistent borders
        }
        else // BorderStyle.Bottom
        {
            topBorder = 0;
            bottomBorder = totalHeight + DefaultSidePadding + bottomMargin;
            sidePadding = DefaultSidePadding;
        }

        var newHeight = originalBitmap.Height + topBorder + bottomBorder;
        var newWidth = originalBitmap.Width + (options.BorderStyle == BorderStyle.All ? sidePadding * 2 : 0);

        using var newBitmap = new SKBitmap(newWidth, newHeight);
        using var canvas = new SKCanvas(newBitmap);

        // Fill background with black
        canvas.Clear(SKColors.Black);

        // Calculate image position
        var imageX = options.BorderStyle == BorderStyle.All ? sidePadding : 0;
        var imageY = topBorder;  // Place image after top border

        // Draw original image
        canvas.DrawBitmap(originalBitmap, imageX, imageY);

        // Calculate text position - always at bottom
        var textY = imageY + originalBitmap.Height + sidePadding;

        // Draw text lines
        using var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = _typeface
        };

        // Calculate the horizontal center position
        var centerX = newWidth / 2;

        for (int i = 0; i < textLines.Length; i++)
        {
            var fontSize = CalculateFontSize(paint, textLines[i], newWidth - (sidePadding * 2));
            paint.TextSize = fontSize;
            
            canvas.DrawText(
                textLines[i],
                centerX,
                textY + (i * options.LineHeight) + (fontSize / 2), // Add half font size for vertical centering
                paint
            );
        }

        // Save with quality settings
        using var image = SKImage.FromBitmap(newBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, Math.Max(1, Math.Min(100, options.OutputQuality)));
        using var stream = File.OpenWrite(outputPath);
        stream.SetLength(0); // Ensure the file is empty before writing
        data.SaveTo(stream);
    }

    private float CalculateFontSize(SKPaint paint, string text, int maxWidth)
    {
        for (float size = MaxFontSize; size >= MinFontSize; size--)
        {
            paint.TextSize = size;
            var textWidth = paint.MeasureText(text);
            
            if (textWidth <= maxWidth)
            {
                return size;
            }
        }

        return MinFontSize;
    }

    public void Dispose()
    {
        _typeface?.Dispose();
    }
} 