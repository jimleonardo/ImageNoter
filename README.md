# ImageNoter

ImageNoter transforms your photos into storytelling masterpieces by elegantly embedding EXIF metadata directly onto your images, turning technical camera details into beautifully formatted visual elements that enhance your photography's narrative.

## Features

- 📝 Adds camera settings (aperture, shutter speed) and image title as text
- 🎨 Two border styles: bottom-only or full border
- 🔤 Dynamic font sizing with Arial font for optimal text display
- 🎯 High-quality output with configurable JPEG settings
- 📊 Preserves original EXIF data
- 📐 Smart text layout with proper margins and spacing
- 🔍 Binary search algorithm for perfect font sizing
- 📁 Batch processing with organized output folders

## Installation

Ensure you have .NET 9 installed on your system. Then:

1. Clone this repository
2. Build the project:
   ```bash
   dotnet build
   ```

## Usage

Run ImageNoter from the command line with the following options:

```bash
ImageNoter.Console --input <input_directory> [options]
```

### Options

- `--input`: Directory containing JPG images (required)
- `--output`: Output directory (default: input_directory/images with exif)
- `--subfolder`: Custom subfolder name (default: "images with exif")
- `--lineheight`: Text line height in pixels (default: 60)
- `--border`: Border style ("bottom" or "all", default: "bottom")
- `--quality`: JPEG output quality (1-100, default: 100)

### Examples

Process all images in the current directory with default settings:
```bash
ImageNoter.Console --input .
```

Process images with a full border and custom line height:
```bash
ImageNoter.Console --input ./photos --border all --lineheight 80
```

## Development

The project uses xUnit for testing. Run the tests with:
```bash
dotnet test
```

## License

Copyright (c) 2025 Jim Leonardo

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 