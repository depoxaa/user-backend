using Microsoft.AspNetCore.Mvc;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet("{**path}")]
    public IActionResult GetFile(string path)
    {
        var stream = _fileService.GetFileStream(path);
        
        if (stream == null)
            return NotFound();
            
        var contentType = _fileService.GetContentType(path);
        return File(stream, contentType);
    }
}
