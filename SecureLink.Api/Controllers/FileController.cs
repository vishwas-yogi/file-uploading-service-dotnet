using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SecureLink.Core.Contracts;

namespace SecureLink.Api.Controllers;

// TODO: Add a cancellation token
[ApiController]
[Route("files")]
public class FileController(IFileService fileService, ILogger<FileController> logger)
    : ControllerBase
{
    private readonly ILogger<FileController> _logger = logger;
    private readonly IFileService _fileService = fileService;

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<string>> Upload()
    {
        _logger.LogInformation("Controller UploadFile invoked with Request: {Request}", Request);

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
        var response = await _fileService.Upload(boundary, Request.Body);

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

    [HttpGet]
    // For now keeping it to filename as there is no DB.
    // TODO: Once DB is setup change this to fileId.
    [Route("{filename}")]
    public async Task<ActionResult> Download([FromRoute] string filename)
    {
        _logger.LogInformation("Controller UploadFile invoked with Request: {Request}", Request);

        var response = await _fileService.Download(filename);

        if (!response.IsSuccess)
        {
            return response.Status switch
            {
                ResponseStatus.ValidationError => StatusCode(400, response.Error),
                ResponseStatus.NotFound => StatusCode(200, response.Error),
                _ => StatusCode(500, "An unexpected error occurred"),
            };
        }

        // For now harcoding this
        // TODO: Once DB is setup, will get it from there
        var contentType = "application/octet-stream";

        // File() already sets the Status code to 200. So need to wrap it in Ok()
        // TODO: Research and enhance this to allow user to play / pause stream
        return File(response.Data!, contentType, filename, true);
    }
}
