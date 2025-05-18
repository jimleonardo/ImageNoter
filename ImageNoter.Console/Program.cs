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
    private static void DisplayHelp()
    {
        AnsiConsole.Write(new Rule("[yellow]ImageNoter Help[/]").RuleStyle("grey").DoubleBorder());
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Description:[/]");
        AnsiConsole.MarkupLine("ImageNoter transforms your photos by elegantly embedding EXIF metadata directly onto your images.");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[bold]Usage:[/]");
        AnsiConsole.MarkupLine("  ImageNoter.Console --input <input_directory> [[options]]");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[bold]Options:[/]");
        AnsiConsole.MarkupLine("  [yellow]--help[/], [yellow]-h[/]    Display this help message");
        AnsiConsole.MarkupLine("  [yellow]--input[/]        Directory containing JPG images (required)");
        AnsiConsole.MarkupLine("  [yellow]--output[/]       Output directory (default: input_directory/images with exif)");
        AnsiConsole.MarkupLine("  [yellow]--subfolder[/]    Custom subfolder name (default: \"images with exif\")");
        AnsiConsole.MarkupLine("  [yellow]--lineheight[/]   Text line height in pixels (default: 60)");
        AnsiConsole.MarkupLine("  [yellow]--border[/]       Border style (\"bottom\" or \"all\", default: \"bottom\")");
        AnsiConsole.MarkupLine("  [yellow]--quality[/]      JPEG output quality (1-100, default: 100)");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[bold]Examples:[/]");
        AnsiConsole.MarkupLine("  Process all images in the current directory:");
        AnsiConsole.MarkupLine("  [grey]ImageNoter.Console --input .[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  Process images with a full border and custom line height:");
        AnsiConsole.MarkupLine("  [grey]ImageNoter.Console --input ./photos --border all --lineheight 80[/]");
    }

    public static void Main(string[] args)
    {
        try
        {
            // Check for help parameter or no arguments
            if (args.Length == 0 || args.Any(arg => arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || 
                                                   arg.Equals("-h", StringComparison.OrdinalIgnoreCase)))
            {
                DisplayHelp();
                return;
            }

            // Filter out help arguments before passing to configuration
            var configArgs = args.Where(arg => !arg.Equals("-h", StringComparison.OrdinalIgnoreCase) &&
                                             !arg.Equals("--help", StringComparison.OrdinalIgnoreCase))
                                .ToArray();

            var config = new ConfigurationBuilder()
                .AddCommandLine(configArgs)
                .Build();

            var options = new ImageProcessingOptions
            {
                InputDirectory = config["input"] ?? throw new ArgumentException("Input directory is required"),
                OutputDirectory = config["output"] ?? Path.Combine(config["input"]!, "images with exif"),
                SubfolderName = config["subfolder"] ?? "images with exif",
                LineHeight = int.TryParse(config["lineheight"], out var height) ? height : 60,
                BorderStyle = config["border"]?.ToLower() == "all" ? BorderStyle.All : BorderStyle.Bottom,
                OutputQuality = int.TryParse(config["quality"], out var quality) ? quality : 100
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
