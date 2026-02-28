using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SecureLink.Core.Contracts;

namespace SecureLink.Api.Controllers;

[Authorize]
// TODO: Add a cancellation token
[ApiController]
[Route("files")]
public class FilesController(IFilesService fileService, ILogger<FilesController> logger)
    : ControllerBase
{
    private readonly ILogger<FilesController> _logger = logger;
    private readonly IFilesService _fileService = fileService;

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<List<FileUploadResponse>>> Upload()
    {
        var currentUser =
            User.GetUserId()
            ?? throw new InvalidOperationException(
                "Unable to resolve the logged in user. If the error persists, kindly contact the administrator"
            );

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
        var response = await _fileService.Upload(boundary, Request.Body, currentUser);

        if (!response.IsSuccess)
        {
            return response.Status switch
            {
                ResponseStatus.ValidationError => StatusCode(400, response.Error),
                _ => StatusCode(500, "An unexpected error occurred"),
            };
        }

        return Ok(response.Data);
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
                ResponseStatus.NotFound => StatusCode(404, response.Error),
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
