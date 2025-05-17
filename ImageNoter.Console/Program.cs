using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using ImageNoter.Console.Models;
using ImageNoter.Console.Services;

namespace ImageNoter.Console;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var options = new ImageProcessingOptions
            {
                InputDirectory = config["input"] ?? throw new ArgumentException("Input directory is required"),
                OutputDirectory = config["output"] ?? Path.Combine(config["input"]!, "images with exif"),
                SubfolderName = config["subfolder"] ?? "images with exif",
                LineHeight = int.TryParse(config["lineheight"], out var height) ? height : 60,
                BorderStyle = config["border"]?.ToLower() == "all" ? BorderStyle.All : BorderStyle.Bottom
            };

            if (!Directory.Exists(options.InputDirectory))
            {
                AnsiConsole.MarkupLine($"[red]Error: Input directory '{options.InputDirectory}' does not exist.[/]");
                return;
            }

            // Create output directory if it doesn't exist
            Directory.CreateDirectory(options.OutputDirectory);

            var imageProcessor = new ImageProcessor();
            var exifExtractor = new ExifExtractor();
            var jpgFiles = Directory.GetFiles(options.InputDirectory, "*.jpg", SearchOption.TopDirectoryOnly);

            if (!jpgFiles.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No JPG files found in the input directory.[/]");
                return;
            }

            var table = new Table()
                .AddColumn("File")
                .AddColumn("Status");

            AnsiConsole.Live(table)
                .Start(ctx =>
                {
                    foreach (var file in jpgFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        table.AddRow(fileName, "[yellow]Processing...[/]");
                        ctx.Refresh();

                        try
                        {
                            var exifData = exifExtractor.ExtractExifData(file);
                            var outputPath = Path.Combine(
                                options.OutputDirectory,
                                $"processed_{fileName}"
                            );

                            imageProcessor.ProcessImage(file, outputPath, exifData, options);

                            table.UpdateCell(table.Rows.Count - 1, 1, "[green]Success[/]");
                        }
                        catch (Exception ex)
                        {
                            table.UpdateCell(table.Rows.Count - 1, 1, $"[red]Error: {ex.Message}[/]");
                        }

                        ctx.Refresh();
                    }
                });

            AnsiConsole.MarkupLine($"\n[green]Processing complete. Output files saved to:[/] {options.OutputDirectory}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
}
