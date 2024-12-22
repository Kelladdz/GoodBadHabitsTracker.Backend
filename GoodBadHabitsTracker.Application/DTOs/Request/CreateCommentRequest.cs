using Microsoft.AspNetCore.Mvc;

namespace GoodBadHabitsTracker.Application.DTOs.Request
{
    public record CreateCommentRequest
    {
        public string Body { get; set; } = "";
        public DateOnly Date { get; set; }
    }
}
