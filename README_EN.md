# Dewu Content Downloader

## 语言选项 / Language Options
- [简体中文](README.md) ｜ [English](README_EN.md)

This is a Windows application for downloading content from the Dewu platform, supporting batch downloads of videos, cover images, and text content.

## Features

- **Batch Download**: Support extracting multiple URL links from multi-line text and downloading multiple contents in parallel
- **Complete Content Preservation**: Download videos, cover images (in webp format), and text content (in txt format)
- **Customizable Save Location**: Users can select the save directory
- **Real-time Progress Display**: Show progress and statistics during download
- **Completion Notification**: Display detailed success/failure statistics after download completion

## Interface Introduction

### Upper Area
- **Text with links/one per line**: Input text containing URL links in this text box, one link per line

### Lower Area
- **Save Location Settings**:
  - Text box: Displays the currently selected save directory path
  - Browse... button: Click to open folder selection dialog to customize save location
- **Save(&D) button**: Click to start the download process

## Usage

1. Paste text containing Dewu links in the upper text box (format: `XXX发布了一篇得物动态，https://dw4.co/t/A/XXXxxxXxX点开链接，快来看吧！`)
2. Confirm or modify the save location
3. Click the "Save(&D)" button to start downloading
4. Wait for the download to complete, the program will display statistics

## Technical Implementation

- **.NET 8**: Developed using C# Windows Forms
- **Multi-threaded Downloading**: Using SimpleThreadPool to implement parallel downloads for improved efficiency
- **Regular Expressions**: Extract URL links and parse webpage content
- **JSON Parsing**: Using EasyJson to parse API response data
- **HTTP Requests**: Using HttpRequestClass to send requests and get content

## Project Structure

```
dewu/
├── Form1.cs           # Main form code
├── Form1.Designer.cs  # Form designer code
├── EasyHTTP.cs        # HTTP request handling class
├── EasyJson.cs        # JSON parsing class
├── SimpleThreadPool.cs # Thread pool implementation
├── common.cs          # Common functionality class
├── dewu.csproj        # Project file
└── dewu.sln           # Solution file
```

## Notes

1. Please ensure that the URL format is correct, otherwise it may not be recognized and downloaded
2. Do not close the program window during the download process
3. Please ensure there is enough disk space and write permissions
4. Download speed depends on network conditions and server response

## Error Handling

The program will automatically handle the following error situations:
- Empty input validation
- Save directory does not exist or is inaccessible
- Network request failures
- File saving exceptions

All errors are recorded in the failure statistics and do not affect the progress of other download tasks.