using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Guide.Api.Services;
using Guide.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Guide.Data.Models;
using Guide.Data.Models.TourDTOs;


namespace Guide.Api.Services
{
    public class CommentService : ICommentsService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommentDto>> GetTourCommentsAsync(Guid tourId)
        {
            return await _context.TourComments
                .Where(c => c.TourId == tourId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    Text = c.Text,
                    Rating = c.Rating,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CommentDto> AddCommentAsync(Guid tourId, Guid userId, string text, int rating)
        {
            var comment = new TourComment
            {
                CommentId = Guid.NewGuid(),
                TourId = tourId,
                UserId = userId,
                Text = text,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            _context.TourComments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new CommentDto
            {
                CommentId = comment.CommentId,
                UserId = comment.UserId,
                UserName = user?.UserName ?? "Unknown",
                Text = comment.Text,
                Rating = comment.Rating,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.TourComments
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null || comment.UserId != userId)
                return false;

            _context.TourComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}