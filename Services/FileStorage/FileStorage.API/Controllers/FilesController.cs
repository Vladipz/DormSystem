// Controllers/FilesController.cs
using FileStorage.API.Models;
using FileStorage.API.Services;

using Microsoft.AspNetCore.Mvc;

namespace FileStorage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] FileUploadRequest request)
        {
            var metadata = await _fileService.SaveFileAsync(request.File, request.Category);

            var response = new FileUploadResponse
            {
                Id = metadata.Id,
                Url = metadata.Url,
                FileName = metadata.FileName,
                Size = metadata.Size
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(string id)
        {
            var (data, contentType, fileName) = await _fileService.GetFileAsync(id);
            return File(data, contentType, fileName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(string id)
        {
            var result = await _fileService.DeleteFileAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetFilesByCategory(string category)
        {
            var files = await _fileService.GetFilesByCategory(category);
            return Ok(files);
        }
    }
}