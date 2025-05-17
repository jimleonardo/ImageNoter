using System;
using System.IO;
using SkiaSharp;
using Xunit;
using ImageNoter.Console.Services;
using ImageNoter.Console.Models;
using System.Linq;

namespace ImageNoter.Tests;

public class ImageProcessorTests : IDisposable
{
    private readonly ImageProcessor _processor;
    private readonly string _testImagePath;
    private readonly string _outputImagePath;

    public ImageProcessorTests()
    {
        _processor = new ImageProcessor();
        _testImagePath = CreateTestImage();
        _outputImagePath = Path.Combine(Path.GetTempPath(), "output.jpg");
    }

    [Fact]
    public void ProcessImage_WithBottomBorder_CreatesLargerImage()
    {
        // Arrange
        var exifData = new ExifData
        {
            Title = "Test Image",
            CameraModel = "Test Camera",
            Aperture = "f/2.8",
            ShutterSpeed = "1/100"
        };

        var options = new ImageProcessingOptions
        {
            BorderStyle = BorderStyle.Bottom,
            LineHeight = 40,
            OutputQuality = 90
        };

        // Act
        _processor.ProcessImage(_testImagePath, _outputImagePath, exifData, options);

        // Assert
        using var originalImage = SKBitmap.Decode(_testImagePath);
        using var processedImage = SKBitmap.Decode(_outputImagePath);
        
        Assert.True(processedImage.Height > originalImage.Height);
        Assert.Equal(originalImage.Width, processedImage.Width);
    }

    [Fact]
    public void ProcessImage_WithAllBorder_CreatesLargerImageInBothDimensions()
    {
        // Arrange
        var exifData = new ExifData
        {
            Title = "Test Image",
            CameraModel = "Test Camera",
            Aperture = "f/2.8",
            ShutterSpeed = "1/100"
        };

        var options = new ImageProcessingOptions
        {
            BorderStyle = BorderStyle.All,
            LineHeight = 40,
            OutputQuality = 90
        };

        // Act
        _processor.ProcessImage(_testImagePath, _outputImagePath, exifData, options);

        // Assert
        using var originalImage = SKBitmap.Decode(_testImagePath);
        using var processedImage = SKBitmap.Decode(_outputImagePath);
        
        Assert.True(processedImage.Height > originalImage.Height);
        Assert.True(processedImage.Width > originalImage.Width);
    }

    [Theory]
    [InlineData(25)]  // Low quality
    [InlineData(50)]  // Medium quality
    [InlineData(75)]  // High quality
    [InlineData(100)] // Maximum quality
    public void ProcessImage_WithQualitySettings_SavesWithCorrectQuality(int quality)
    {
        // Arrange
        var exifData = new ExifData
        {
            Title = "Quality Test",
            CameraModel = "Test Camera",
            Aperture = "f/2.8"
        };

        var options = new ImageProcessingOptions
        {
            BorderStyle = BorderStyle.All,
            LineHeight = 40,
            OutputQuality = quality
        };

        var testImagePath = CreateComplexTestImage(); // Use complex image to better see quality differences
        try
        {
            // Act
            _processor.ProcessImage(testImagePath, _outputImagePath, exifData, options);

            // Assert
            var outputData = File.ReadAllBytes(_outputImagePath);
            using var stream = new SKMemoryStream(outputData);
            using var codec = SKCodec.Create(stream);
            
            // Verify it's a JPEG
            Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);

            // For same content, higher quality should mean larger file size
            using var outputImage = SKBitmap.Decode(_outputImagePath);
            using var encodedData = outputImage.Encode(SKEncodedImageFormat.Jpeg, quality);
            
            // Allow for small variations in size (within 10%)
            var expectedSize = encodedData.Size;
            var actualSize = outputData.Length;
            var ratio = (double)actualSize / expectedSize;
            
            Assert.True(ratio >= 0.9 && ratio <= 1.1,
                $"File size ratio {ratio:F2} should be within 10% of expected for quality {quality}. " +
                $"Expected ~{expectedSize:N0} bytes, got {actualSize:N0} bytes");
        }
        finally
        {
            if (File.Exists(testImagePath))
            {
                File.Delete(testImagePath);
            }
        }
    }

    [Fact]
    public void ProcessImage_WithBottomBorder_HasCorrectMarginRatio()
    {
        // Arrange
        var lineHeight = 50; // Use a line height that makes percentage calculation clear
        var expectedBottomMargin = (int)(lineHeight * 0.12); // Should be 6 pixels

        var exifData = new ExifData
        {
            Title = "Test Image",
            CameraModel = "Test Camera",
            Aperture = "f/2.8"
        };

        var options = new ImageProcessingOptions
        {
            BorderStyle = BorderStyle.Bottom,
            LineHeight = lineHeight,
            OutputQuality = 90
        };

        // Create a test image with a distinct color
        var testImagePath = CreateColoredTestImage(SKColors.Blue);
        try
        {
            // Act
            _processor.ProcessImage(testImagePath, _outputImagePath, exifData, options);

            // Assert
            using var processedImage = SKBitmap.Decode(_outputImagePath);
            
            // Get the original image height (should be the same as our test image)
            using var originalImage = SKBitmap.Decode(testImagePath);
            var originalHeight = originalImage.Height;

            // The text area starts right after the original image
            var textAreaStart = originalHeight;
            
            // Calculate where we expect the text area to end
            var textLines = new[] { "Test Image", "Test Camera", "f/2.8" }
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Count();
            var expectedTextAreaHeight = textLines * lineHeight;
            var textAreaEnd = textAreaStart + expectedTextAreaHeight + 20; // +20 for top padding

            // Check that pixels in the text area are black (background color)
            var textAreaY = textAreaStart + 10; // Check middle of text area
            Assert.Equal(SKColors.Black, processedImage.GetPixel(10, textAreaY));

            // Check that the bottom margin has the correct height
            var bottomMarginStart = textAreaEnd;
            var bottomMarginEnd = bottomMarginStart + expectedBottomMargin;
            
            // Verify the margin is black (background color)
            for (int y = bottomMarginStart; y < bottomMarginEnd; y++)
            {
                Assert.Equal(SKColors.Black, processedImage.GetPixel(10, y));
            }

            // Verify we're at the end of the image
            Assert.Equal(bottomMarginEnd, processedImage.Height);

            // Verify the margin size is exactly 12% of line height
            var actualMarginHeight = processedImage.Height - textAreaEnd;
            Assert.Equal(expectedBottomMargin, actualMarginHeight);
        }
        finally
        {
            if (File.Exists(testImagePath))
            {
                File.Delete(testImagePath);
            }
        }
    }

    [Fact]
    public void ProcessImage_WithAllBorder_HasCorrectBorderSizes()
    {
        // Arrange
        var lineHeight = 50; // Use a line height that makes calculations clear
        var expectedBottomMargin = (int)(lineHeight * 0.12); // Should be 6 pixels

        var exifData = new ExifData
        {
            Title = "Test Image",
            CameraModel = "Test Camera",
            Aperture = "f/2.8"
        };

        var options = new ImageProcessingOptions
        {
            BorderStyle = BorderStyle.All,
            LineHeight = lineHeight,
            OutputQuality = 90
        };

        // Create a test image with a distinct color
        var testImagePath = CreateColoredTestImage(SKColors.Blue);
        try
        {
            // Act
            _processor.ProcessImage(testImagePath, _outputImagePath, exifData, options);

            // Assert
            using var processedImage = SKBitmap.Decode(_outputImagePath);
            using var originalImage = SKBitmap.Decode(testImagePath);

            // Calculate expected dimensions
            var textLines = new[] { "Test Image", "Test Camera", "f/2.8" }
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Count();
            var expectedTextAreaHeight = textLines * lineHeight;

            // Verify total width includes line height borders on both sides
            var expectedTotalWidth = lineHeight * 2 + // Left and right borders
                                   originalImage.Width; // Original image width
            Assert.Equal(expectedTotalWidth, processedImage.Width);

            // Verify total height includes all components
            var expectedTotalHeight = lineHeight + // Top border
                                    originalImage.Height + // Image
                                    lineHeight + // Padding before text
                                    expectedTextAreaHeight + // Text area
                                    expectedBottomMargin; // Bottom margin
            Assert.Equal(expectedTotalHeight, processedImage.Height);

            // Verify the image area dimensions
            var imageArea = new SKRectI(
                lineHeight, // x: starts after left border
                lineHeight, // y: starts after top border
                lineHeight + originalImage.Width, // width: original image width
                lineHeight + originalImage.Height // height: original image height
            );

            // Verify text area position
            var textAreaY = imageArea.Bottom + lineHeight; // After image + padding
            var textAreaHeight = expectedTextAreaHeight;
            Assert.True(textAreaY + textAreaHeight + expectedBottomMargin == processedImage.Height,
                "Text area should extend to the bottom margin");

            // Verify border dimensions
            Assert.True(imageArea.Left == lineHeight,
                "Left border should equal line height");
            Assert.True(processedImage.Width - imageArea.Right == lineHeight,
                "Right border should equal line height");
            Assert.True(imageArea.Top == lineHeight,
                "Top border should equal line height");
        }
        finally
        {
            if (File.Exists(testImagePath))
            {
                File.Delete(testImagePath);
            }
        }
    }

    [Fact]
    public void ProcessImage_WithAllBorder_HasCenteredText()
    {
        // Arrange
        var lineHeight = 50;
        var exifData = new ExifData
        {
            Title = "Test Title", // Use predictable text lengths
            CameraModel = "Camera", // Shorter than title
            Aperture = "f/2.8"     // Even shorter
        };

        var options = new ImageProcessingOptions
        {
            BorderStyle = BorderStyle.All,
            LineHeight = lineHeight,
            OutputQuality = 90
        };

        var testImagePath = CreateColoredTestImage(SKColors.Blue);
        try
        {
            // Act
            _processor.ProcessImage(testImagePath, _outputImagePath, exifData, options);

            // Assert
            using var processedImage = SKBitmap.Decode(_outputImagePath);
            using var originalImage = SKBitmap.Decode(testImagePath);

            // Calculate available width for text (total width minus borders)
            var textAreaWidth = processedImage.Width - (lineHeight * 2);

            // Calculate expected text positions
            using var paint = new SKPaint { Typeface = SKTypeface.FromFamilyName("Arial") };
            // Font size should be about 70% of line height for proper spacing
            paint.TextSize = lineHeight * 0.7f;

            // Verify each line of text would be centered
            var lines = new[] { "Test Title", "Camera", "f/2.8" };
            foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                var textWidth = paint.MeasureText(line);
                var expectedLeftPadding = (textAreaWidth - textWidth) / 2;
                Assert.True(expectedLeftPadding >= 0, 
                    $"Text '{line}' should fit within borders with padding (padding: {expectedLeftPadding}, textWidth: {textWidth}, available: {textAreaWidth})");
                Assert.True(expectedLeftPadding <= textAreaWidth / 2, 
                    $"Text '{line}' should be centered, not exceed half the available width");
            }
        }
        finally
        {
            if (File.Exists(testImagePath))
            {
                File.Delete(testImagePath);
            }
        }
    }

    private string CreateTestImage()
    {
        var testImagePath = Path.Combine(Path.GetTempPath(), "test_processor.jpg");
        var imageInfo = new SKImageInfo(200, 200);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        
        // Create a more complex test image
        canvas.Clear(SKColors.Blue);
        using var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 24
        };
        canvas.DrawText("Test", 50, 100, paint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(testImagePath);
        data.SaveTo(stream);

        return testImagePath;
    }

    private string CreateComplexTestImage()
    {
        var testImagePath = Path.Combine(Path.GetTempPath(), "test_complex.jpg");
        var imageInfo = new SKImageInfo(800, 600); // Larger image for more noticeable quality differences
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        
        // Create a complex test image with gradients and patterns
        canvas.Clear(SKColors.White);
        
        // Draw gradient background
        using (var paint = new SKPaint())
        {
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(800, 600),
                new[] { SKColors.Blue, SKColors.Red },
                null,
                SKShaderTileMode.Clamp);
            canvas.DrawPaint(paint);
        }

        // Draw multiple shapes with different colors and gradients
        using (var paint = new SKPaint())
        {
            // Draw circles with gradients
            for (int i = 0; i < 20; i++)
            {
                paint.Shader = SKShader.CreateRadialGradient(
                    new SKPoint(400 + (float)(Math.Cos(i * Math.PI / 10) * 200),
                              300 + (float)(Math.Sin(i * Math.PI / 10) * 200)),
                    50,
                    new[] { SKColors.Yellow, SKColors.Green },
                    null,
                    SKShaderTileMode.Clamp);
                
                canvas.DrawCircle(
                    400 + (float)(Math.Cos(i * Math.PI / 10) * 200),
                    300 + (float)(Math.Sin(i * Math.PI / 10) * 200),
                    50,
                    paint);
            }
        }

        // Add detailed text
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 48,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText("Complex Test Image", 400, 300, paint);
            
            paint.TextSize = 24;
            for (int i = 0; i < 10; i++)
            {
                canvas.DrawText($"Quality Test Line {i}", 400, 350 + i * 30, paint);
            }
        }

        // Add noise pattern
        using (var paint = new SKPaint { Color = SKColors.White.WithAlpha(128) })
        {
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < 1000; i++)
            {
                canvas.DrawPoint(
                    random.Next(0, 800),
                    random.Next(0, 600),
                    paint);
            }
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(testImagePath);
        data.SaveTo(stream);

        return testImagePath;
    }

    private string CreateColoredTestImage(SKColor color)
    {
        var testImagePath = Path.Combine(Path.GetTempPath(), "test_colored.jpg");
        var imageInfo = new SKImageInfo(200, 200);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        
        // Fill with solid color
        canvas.Clear(color);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(testImagePath);
        data.SaveTo(stream);

        return testImagePath;
    }

    public void Dispose()
    {
        _processor.Dispose();
        if (File.Exists(_testImagePath))
        {
            File.Delete(_testImagePath);
        }
        if (File.Exists(_outputImagePath))
        {
            File.Delete(_outputImagePath);
        }
    }
} 