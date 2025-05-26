using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Guide.Data.Models.TourDTOs;

namespace Guide.Api.Services
{
    public interface ICommentsService
    {
        Task<IEnumerable<CommentDto>> GetTourCommentsAsync(Guid tourId);
        Task<CommentDto> AddCommentAsync(Guid tourId, Guid userId, string text, int rating);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
    }
}