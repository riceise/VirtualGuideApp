using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Guide.Data.Models;
using Guide.Api.Services;
using Guide.Data.Models.TourDTOs;
using System.Security.Claims;


namespace Guide.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentsService _commentsService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ICommentsService commentsService, ILogger<CommentsController> logger)
    {
        _commentsService = commentsService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("tours/{tourId}")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid tourId, AddCommentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var comment = await _commentsService.AddCommentAsync(
                tourId, 
                Guid.Parse(userId), 
                request.Text,
                request.Rating
            );
            
            return CreatedAtAction(nameof(GetTourComments), new { tourId }, comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении комментария для тура {TourId}", tourId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("tours/{tourId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetTourComments(Guid tourId)
    {
        try
        {
            var comments = await _commentsService.GetTourCommentsAsync(tourId);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении комментариев для тура {TourId}", tourId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _commentsService.DeleteCommentAsync(commentId, Guid.Parse(userId));
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении комментария {CommentId}", commentId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}