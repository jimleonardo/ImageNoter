# ImageNoter

A .NET 9 console application that processes JPG images by adding EXIF metadata as visible text on the image.

## Features

- Process JPG images by adding EXIF metadata as visible text
- Preserve original image quality and EXIF data
- Display up to 3 lines of EXIF information:
  - Image title/description
  - Camera info (date taken, camera model, lens model)
  - Technical shooting details (focal length, aperture, shutter speed, ISO)
- Customizable border placement (bottom or all sides)
- Configurable text formatting
- Batch processing support
- Unicode text support
- Error handling for missing EXIF data

## Requirements

- .NET 9 SDK
- Windows/Linux/macOS

## Installation

1. Clone the repository
2. Build the solution:
   ```bash
   dotnet build
   ```

## Usage

Run the application with the following command:

```bash
dotnet run --project ImageNoter.Console -- --input <input_directory> [options]
```

### Command Line Options

- `--input`: (Required) Input directory containing JPG files
- `--output`: (Optional) Output directory (default: input_directory/images with exif)
- `--subfolder`: (Optional) Name of the output subfolder (default: "images with exif")
- `--lineheight`: (Optional) Height of each text line in pixels (default: 60)
- `--border`: (Optional) Border style - "bottom" or "all" (default: bottom)

### Examples

Process all JPG files in the current directory:
```bash
dotnet run --project ImageNoter.Console -- --input .
```

Process images with custom output directory and line height:
```bash
dotnet run --project ImageNoter.Console -- --input ./photos --output ./processed --lineheight 80
```

Process images with borders on all sides:
```bash
dotnet run --project ImageNoter.Console -- --input ./photos --border all
```

## Development

### Running Tests

```bash
dotnet test
```

## License

MIT License 