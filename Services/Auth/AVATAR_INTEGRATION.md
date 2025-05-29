# User Avatar Integration

This document describes the integration between the Auth service and FileStorage service for user avatar functionality using the shared `Shared.FileServiceClient`.

## Overview

The avatar integration allows users to upload, update, and delete profile pictures. The system validates file types and sizes, stores files in the FileStorage service, and maintains avatar references in the User entity. This implementation uses the shared `Shared.FileServiceClient` library for standardized communication with the FileStorage microservice.

## Features

- **Upload Avatar**: Upload or update user profile pictures
- **Delete Avatar**: Remove user avatars
- **Get User with Avatar**: Retrieve user information including avatar URL
- **File Validation**: Supports JPEG, PNG, GIF, WebP formats with 5MB size limit
- **Automatic Cleanup**: Old avatars are automatically deleted when new ones are uploaded
- **Shared Client**: Uses standardized FileServiceClient for consistent file operations

## Service Architecture

### Shared FileServiceClient

The integration uses `Shared.FileServiceClient` which provides:
- Standardized communication with FileStorage service
- Built-in error handling and logging
- Support for multiple file categories including avatars
- Consistent API across all microservices

### Configuration

The FileServiceClient is configured in `Program.cs`:

```csharp
using Shared.FileServiceClient.Extensions;

// Add shared FileServiceClient
builder.Services.AddFileServiceClient(builder.Configuration);
```

Configuration in `appsettings.json`:
```json
{
  "FileStorage": {
    "BaseUrl": "https://localhost:7081"
  }
}
```

## API Endpoints

### 1. Upload User Avatar

**POST** `/api/user/{userId}/avatar`

Uploads or updates a user's avatar image.

**Parameters:**
- `userId` (path): User ID (GUID)
- `avatarFile` (form): Image file (JPEG, PNG, GIF, WebP, max 5MB)

**Request Example:**
```bash
curl -X POST "https://localhost:5001/api/user/{userId}/avatar" \
  -H "Content-Type: multipart/form-data" \
  -F "avatarFile=@profile-picture.jpg"
```

**Response (200 OK):**
```json
{
  "avatarUrl": "https://localhost:7081/api/files/abc123-def456-ghi789",
  "message": "Avatar uploaded successfully"
}
```

**Response (400 Bad Request):**
```json
{
  "errors": ["Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed"]
}
```

### 2. Delete User Avatar

**DELETE** `/api/user/{userId}/avatar`

Deletes a user's avatar.

**Parameters:**
- `userId` (path): User ID (GUID)

**Request Example:**
```bash
curl -X DELETE "https://localhost:5001/api/user/{userId}/avatar"
```

**Response (200 OK):**
```json
{
  "message": "Avatar deleted successfully"
}
```

### 3. Get User Information (with Avatar)

**GET** `/api/user/{userId}`

Retrieves user information including avatar URL.

**Parameters:**
- `userId` (path): User ID (GUID)

**Request Example:**
```bash
curl -X GET "https://localhost:5001/api/user/{userId}"
```

**Response (200 OK):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "avatarUrl": "https://localhost:7081/api/files/abc123-def456-ghi789",
  "roles": ["User"]
}
```

### 4. Get Users List (with Avatars)

**GET** `/api/user?pageNumber=1&pageSize=10`

Retrieves a paginated list of users with their avatar URLs.

**Query Parameters:**
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10, max: 100)

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@example.com",
      "avatarUrl": "https://localhost:7081/api/files/abc123-def456-ghi789",
      "roles": ["User"]
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## File Validation Rules

### Supported File Types
- **JPEG** (`image/jpeg`)
- **PNG** (`image/png`)
- **GIF** (`image/gif`)
- **WebP** (`image/webp`)

### File Size Limit
- Maximum file size: **5MB** (5,242,880 bytes)

### Validation Errors
- `"No file provided"` - When no file is uploaded
- `"Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed"` - Unsupported file format
- `"File size too large. Maximum size is 5MB"` - File exceeds size limit

## Database Changes

### User Entity Updates

The `User` entity now includes an `AvatarId` property:

```csharp
public class User : IdentityUser<Guid>
{
    // ... existing properties
    public string? AvatarId { get; set; }
    // ... existing properties
}
```

### Migration

A database migration `AddAvatarIdToUser` has been created to add the `AvatarId` column to the `AspNetUsers` table.

```sql
ALTER TABLE "AspNetUsers" ADD "AvatarId" text;
```

## Service Integration

### UserService Implementation

The `UserService` now uses the shared `IFileServiceClient`:

```csharp
public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IFileServiceClient _fileServiceClient;

    public UserService(UserManager<User> userManager, IFileServiceClient fileServiceClient)
    {
        _userManager = userManager;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<ErrorOr<string>> UploadUserAvatarAsync(Guid userId, IFormFile avatarFile, CancellationToken cancellationToken = default)
    {
        // ... validation logic

        // Upload using shared client
        var uploadResult = await _fileServiceClient.UploadAvatarAsync(avatarFile);
        
        // ... rest of implementation
    }
}
```

### Key Benefits of Shared Client

1. **Consistency**: All microservices use the same interface for file operations
2. **Maintainability**: Updates to file handling logic only need to be made in one place
3. **Error Handling**: Standardized error handling and logging across services
4. **Configuration**: Centralized configuration management
5. **Testing**: Easier to mock and test file operations

## Dependencies

### Project References

The Auth.BLL project now references:
```xml
<ProjectReference Include="..\..\Shared\Shared.FileServiceClient\Shared.FileServiceClient.csproj" />
```

### Service Registration

Services are registered using the extension method:
```csharp
builder.Services.AddFileServiceClient(builder.Configuration);
```

## Error Handling

The system uses the `ErrorOr` pattern combined with the shared client's error handling:

- **Validation Errors**: Invalid file type, size, or missing file
- **Not Found Errors**: User not found, avatar not found
- **Failure Errors**: Network issues, storage failures, database errors
- **Shared Client Errors**: Standardized error handling from the shared library

## Security Considerations

1. **File Type Validation**: Only image files are accepted
2. **File Size Limits**: Prevents large file uploads
3. **Authorization**: Consider adding authorization for avatar operations
4. **File Storage**: Files are stored securely in the FileStorage service
5. **Shared Library**: Security validations are consistent across all services

## Shared Client Features Used

- `UploadAvatarAsync(IFormFile)`: Specialized avatar upload with automatic categorization
- `DeleteFileAsync(string)`: File deletion with error handling
- `GetFileUrl(string)`: URL generation for file access
- Comprehensive logging and error handling built into the shared client

## Future Enhancements

- **Image Resizing**: Automatic thumbnail generation in FileStorage service
- **CDN Integration**: Serve images through a CDN
- **Multiple Sizes**: Store different image sizes (thumbnail, medium, full)
- **Image Optimization**: Compress images during upload
- **Batch Operations**: Delete multiple avatars at once
- **Shared Validation**: Move file validation logic to shared client

## Migration from Local Client

The previous local `FileStorageClient` has been replaced with the shared `IFileServiceClient`:

### Changes Made:
1. Removed local `FileStorageClient.cs`
2. Added reference to `Shared.FileServiceClient`
3. Updated `UserService` to use `IFileServiceClient`
4. Replaced manual HttpClient registration with `AddFileServiceClient()`
5. Updated all file operations to use shared client methods

### Benefits:
- Reduced code duplication
- Standardized error handling
- Easier maintenance
- Consistent behavior across microservices 