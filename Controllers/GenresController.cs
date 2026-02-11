using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Song;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<GenreInfoDto>>>> GetAll()
    {
        var genres = await _genreService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<GenreInfoDto>>.SuccessResponse(genres));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GenreInfoDto>>> GetById(Guid id)
    {
        var genre = await _genreService.GetByIdAsync(id);
        
        if (genre == null)
            return NotFound(ApiResponse<GenreInfoDto>.ErrorResponse("Genre not found"));
            
        return Ok(ApiResponse<GenreInfoDto>.SuccessResponse(genre));
    }
}
