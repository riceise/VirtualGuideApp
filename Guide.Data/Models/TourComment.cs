using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Guide.Data.Models
{

    public class TourComment
    {
        [Key] 
        public Guid CommentId { get; set; }
        public Guid TourId { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }        
        [Range(1, 5)]
        public int Rating { get; set; }
        // Навигационные свойства
        public virtual Tour Tour { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}