using System;
using System.IO;
using Xunit;
using ImageNoter.Console;
using Spectre.Console;
using Spectre.Console.Testing;

namespace ImageNoter.Tests;

public class ProgramTests
{
    [Fact]
    public void DisplayHelp_WithHelpFlag_ShowsHelpText()
    {
        // Arrange
        var console = new TestConsole();
        AnsiConsole.Console = console;

        // Act
        Program.Main(new[] { "--help" });
        var output = console.Output;

        // Assert
        Assert.Contains("ImageNoter Help", output);
        Assert.Contains("Description:", output);
        Assert.Contains("Usage:", output);
        Assert.Contains("Options:", output);
        Assert.Contains("--help", output);
        Assert.Contains("--input", output);
        Assert.Contains("--output", output);
        Assert.Contains("--subfolder", output);
        Assert.Contains("--lineheight", output);
        Assert.Contains("--border", output);
        Assert.Contains("--quality", output);
        Assert.Contains("Examples:", output);
    }

    [Fact]
    public void DisplayHelp_WithShortHelpFlag_ShowsHelpText()
    {
        // Arrange
        var console = new TestConsole();
        AnsiConsole.Console = console;

        // Act
        Program.Main(new[] { "-h" });
        var output = console.Output;

        // Assert
        Assert.Contains("ImageNoter Help", output);
        Assert.Contains("Description:", output);
    }

    [Fact]
    public void Main_WithNoArguments_ShowsHelpAndExitsGracefully()
    {
        // Arrange
        var console = new TestConsole();
        AnsiConsole.Console = console;

        // Act
        Program.Main(Array.Empty<string>());
        var output = console.Output;

        // Assert
        Assert.Contains("ImageNoter Help", output);
        Assert.DoesNotContain("Error:", output);
    }
} 