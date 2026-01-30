using FileUploader.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace FileUploader.Controllers;

[ApiController]
[Route("file")]
public class FileUploadController(
    IFileManagerService fileService,
    ILogger<FileUploadController> logger
) : ControllerBase
{
    private readonly ILogger<FileUploadController> _logger = logger;
    private readonly IFileManagerService _fileService = fileService;

    [HttpPost]
    [Route("")]
    public Task<ActionResult<string>> UploadFile()
    {
        _logger.LogInformation($"Controller UploadFile invoked with Request: {Request}");

        if (!Request.ContentType?.StartsWith("multipart/form-data") ?? true)
        {
            return Task.FromResult<ActionResult<string>>(
                BadRequest("The request is invalid, as it is not a multi part request")
            );
        }

        var boundary = HeaderUtilities
            .RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary)
            .Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            return Task.FromResult<ActionResult<string>>(BadRequest("Boundary can't be null"));
        }
        var outputFilePath = _fileService.UploadFile(boundary, Request.Body);
        return Task.FromResult<ActionResult<string>>(Ok("File is located at: " + outputFilePath));
    }
}
