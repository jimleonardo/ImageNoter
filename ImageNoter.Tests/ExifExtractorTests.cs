using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using Xunit;
using ImageNoter.Console.Services;
using ImageNoter.Console.Models;
using ImageNoter.Tests.Resources;

namespace ImageNoter.Tests;

public class ExifExtractorTests : IDisposable
{
    private readonly ExifExtractor _extractor;
    private readonly string _testImagePath;
    private readonly string _testImageWithExifPath;

    public ExifExtractorTests()
    {
        _extractor = new ExifExtractor();
        _testImagePath = CreateTestImage();
        _testImageWithExifPath = TestImage.CreateTestImageWithExif();
    }

    [Fact]
    public void ExtractExifData_WithValidImage_ReturnsExifData()
    {
        // Act
        var exifData = _extractor.ExtractExifData(_testImagePath);

        // Assert
        Assert.NotNull(exifData);
        Assert.Empty(exifData.Title);
        Assert.Empty(exifData.CameraModel);
    }

    [Fact]
    public void ExtractExifData_WithExifImage_ReturnsPopulatedData()
    {
        // Act
        var exifData = _extractor.ExtractExifData(_testImageWithExifPath);

        // Assert
        Assert.NotNull(exifData);
        Assert.NotNull(exifData.CameraModel);
        Assert.NotNull(exifData.Title);
    }

    [Fact]
    public async Task ExtractExifData_WithInvalidPath_ThrowsException()
    {
        // Arrange
        var invalidPath = "nonexistent.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            Task.FromResult(_extractor.ExtractExifData(invalidPath)));
    }

    [Fact]
    public void ExtractExifData_WithCorruptedImage_ThrowsException()
    {
        // Arrange
        var corruptedPath = CreateCorruptedImage();

        // Act & Assert
        Assert.Throws<MetadataExtractor.ImageProcessingException>(() => _extractor.ExtractExifData(corruptedPath));

        // Cleanup
        if (File.Exists(corruptedPath))
        {
            File.Delete(corruptedPath);
        }
    }

    private string CreateTestImage()
    {
        var testImagePath = Path.Combine(Path.GetTempPath(), "test.jpg");
        var imageInfo = new SKImageInfo(100, 100);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(testImagePath);
        data.SaveTo(stream);

        return testImagePath;
    }

    private string CreateCorruptedImage()
    {
        var corruptedPath = Path.Combine(Path.GetTempPath(), "corrupted.jpg");
        File.WriteAllText(corruptedPath, "This is not a valid JPEG file");
        return corruptedPath;
    }

    public void Dispose()
    {
        if (File.Exists(_testImagePath))
        {
            File.Delete(_testImagePath);
        }
        if (File.Exists(_testImageWithExifPath))
        {
            File.Delete(_testImageWithExifPath);
        }
    }
} 