# Shared.FileServiceClient

A shared client library for integrating with the FileStorage microservice across all services in the DormSystem project.

## Overview

This library provides a standardized way for microservices to interact with the FileStorage service, supporting file uploads, downloads, deletions, and URL generation. It's designed to be used across multiple microservices to ensure consistency and reduce code duplication.

## Features

- **File Upload**: Upload files with automatic categorization
- **Avatar Upload**: Specialized methods for avatar handling
- **File Download**: Retrieve file content and metadata
- **File Deletion**: Remove files from storage
- **URL Generation**: Get direct access URLs for files
- **Category Support**: Organize files by categories
- **Configuration**: Configurable base URL and settings
- **Logging**: Comprehensive logging for debugging and monitoring
- **Error Handling**: Robust error handling with detailed logging

## Installation

### 1. Add Project Reference

In your microservice project file (e.g., `YourService.BLL.csproj`):

```xml
<ProjectReference Include="..\..\Shared\Shared.FileServiceClient\Shared.FileServiceClient.csproj" />
```

### 2. Configure in Program.cs

```csharp
using Shared.FileServiceClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add shared FileServiceClient
builder.Services.AddFileServiceClient(builder.Configuration);

// ... other services

var app = builder.Build();
```

### 3. Configuration

Add FileStorage configuration to your `appsettings.json`:

```json
{
  "FileStorage": {
    "BaseUrl": "https://localhost:7081"
  }
}
```

## Usage

### Dependency Injection

Inject the `IFileServiceClient` into your services:

```csharp
public class YourService
{
    private readonly IFileServiceClient _fileServiceClient;

    public YourService(IFileServiceClient fileServiceClient)
    {
        _fileServiceClient = fileServiceClient;
    }
}
```

### File Upload Examples

#### Upload Any File

```csharp
// From IFormFile
var result = await _fileServiceClient.UploadFileAsync(formFile, "documents");

// From Stream
using var fileStream = new FileStream("path/to/file.pdf", FileMode.Open);
var result = await _fileServiceClient.UploadFileAsync(
    fileStream, 
    "document.pdf", 
    "application/pdf", 
    "documents");
```

#### Upload Avatar

```csharp
// Specialized avatar upload (automatically categorized as "avatar")
var result = await _fileServiceClient.UploadAvatarAsync(avatarFile);

// Or from Stream
using var stream = avatarFile.OpenReadStream();
var result = await _fileServiceClient.UploadAvatarAsync(
    stream, 
    avatarFile.FileName, 
    avatarFile.ContentType);
```

### File Operations

#### Get File URL

```csharp
var fileUrl = _fileServiceClient.GetFileUrl(fileId);
```

#### Download File

```csharp
var fileData = await _fileServiceClient.GetFileAsync(fileId);
if (fileData.HasValue)
{
    var (data, contentType, fileName) = fileData.Value;
    // Use the file data...
}
```

#### Delete File

```csharp
var success = await _fileServiceClient.DeleteFileAsync(fileId);
```

#### Get Files by Category

```csharp
var avatars = await _fileServiceClient.GetFilesByCategoryAsync("avatar");
var documents = await _fileServiceClient.GetFilesByCategoryAsync("documents");
```

## API Methods

### IFileServiceClient Interface

```csharp
public interface IFileServiceClient
{
    // General file upload methods
    Task<FileUploadResult?> UploadFileAsync(Stream fileStream, string fileName, string contentType, string category = "document");
    Task<FileUploadResult?> UploadFileAsync(IFormFile file, string category = "document");

    // Avatar-specific methods
    Task<FileUploadResult?> UploadAvatarAsync(Stream fileStream, string fileName, string contentType);
    Task<FileUploadResult?> UploadAvatarAsync(IFormFile avatarFile);

    // File management
    Task<bool> DeleteFileAsync(string fileId);
    string GetFileUrl(string fileId);
    Task<(byte[] data, string contentType, string fileName)?> GetFileAsync(string fileId);
    Task<IEnumerable<FileUploadResult>> GetFilesByCategoryAsync(string category);
}
```

### FileUploadResult Model

```csharp
public class FileUploadResult
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
}
```

## Configuration Options

### FileStorageSettings

```csharp
public class FileStorageSettings
{
    public const string SectionName = "FileStorage";
    public string BaseUrl { get; set; } = "https://localhost:7081";
}
```

### Alternative Configuration

You can also configure settings programmatically:

```csharp
builder.Services.AddFileServiceClient(settings =>
{
    settings.BaseUrl = "https://custom-file-storage-url.com";
});
```

## Error Handling

The client includes comprehensive error handling:

- **Null/Empty File Validation**: Automatically validates input files
- **HTTP Error Handling**: Logs and handles HTTP errors gracefully
- **Exception Logging**: All exceptions are logged with context
- **Retry Logic**: Consider implementing retry policies for production use

## Example Integration: User Avatar Service

Here's an example of how to use the FileServiceClient in a user service for avatar management:

```csharp
public class UserService : IUserService
{
    private readonly IFileServiceClient _fileServiceClient;
    private readonly UserManager<User> _userManager;

    public UserService(IFileServiceClient fileServiceClient, UserManager<User> userManager)
    {
        _fileServiceClient = fileServiceClient;
        _userManager = userManager;
    }

    public async Task<string?> UploadUserAvatarAsync(Guid userId, IFormFile avatarFile)
    {
        // Validate file (add your validation logic)
        if (avatarFile == null || avatarFile.Length == 0)
            return null;

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return null;

        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(user.AvatarId))
        {
            await _fileServiceClient.DeleteFileAsync(user.AvatarId);
        }

        // Upload new avatar
        var result = await _fileServiceClient.UploadAvatarAsync(avatarFile);
        if (result == null)
            return null;

        // Update user record
        user.AvatarId = result.Id;
        await _userManager.UpdateAsync(user);

        return result.Url;
    }

    public async Task<UserResponse> GetUserWithAvatarAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return null;

        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AvatarUrl = !string.IsNullOrEmpty(user.AvatarId) 
                ? _fileServiceClient.GetFileUrl(user.AvatarId) 
                : null
        };
    }
}
```

## Logging

The client uses Microsoft.Extensions.Logging for comprehensive logging:

- **Information**: Successful operations
- **Warning**: Non-critical failures (file not found, etc.)
- **Error**: Critical failures with full exception details

## Best Practices

1. **Always check return values**: Methods can return `null` on failure
2. **Handle exceptions**: Wrap calls in try-catch blocks for critical operations
3. **Validate files**: Check file types and sizes before uploading
4. **Clean up**: Delete old files when uploading replacements
5. **Use categories**: Organize files with meaningful categories
6. **Configure timeouts**: Set appropriate HTTP timeouts for large files
7. **Monitor logs**: Use logging to track file operations and errors

## Dependencies

- Microsoft.Extensions.Http
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options.ConfigurationExtensions
- Microsoft.AspNetCore.Http.Features

## Contributing

When contributing to this shared library:

1. Maintain backward compatibility
2. Add comprehensive logging
3. Include proper error handling
4. Update documentation
5. Add unit tests for new features
6. Follow the existing code style 