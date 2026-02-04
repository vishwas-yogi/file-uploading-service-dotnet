using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SecureLink.Core.Contracts;

namespace SecureLink.Api.Controllers;

// TODO: Add a cancellation token
[ApiController]
[Route("files")]
public class FileUploadController(
    IFileUploadService fileService,
    ILogger<FileUploadController> logger
) : ControllerBase
{
    private readonly ILogger<FileUploadController> _logger = logger;
    private readonly IFileUploadService _fileService = fileService;

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<string>> UploadFile()
    {
        _logger.LogInformation($"Controller UploadFile invoked with Request: {Request}");

        if (!Request.ContentType?.StartsWith("multipart/form-data") ?? true)
        {
            return BadRequest("The request is invalid, as it is not a multi part request");
        }

        var boundary = HeaderUtilities
            .RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary)
            .Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            return BadRequest("Boundary can't be null");
        }
        var response = await _fileService.UploadFile(boundary, Request.Body);

        if (!response.IsSuccess)
        {
            return response.Status switch
            {
                ResponseStatus.ValidationError => StatusCode(400, response.Error),
                _ => StatusCode(500, "An unexpected error occurred"),
            };
        }

        return Ok("File is located at: " + response.Data);
    }
}
