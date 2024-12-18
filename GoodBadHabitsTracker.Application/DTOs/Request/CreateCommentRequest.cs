using Microsoft.AspNetCore.Mvc;

namespace GoodBadHabitsTracker.Application.DTOs.Request
{
    public record CreateCommentRequest
    {
        [FromBody]
        public string? Body { get; init; }
        [FromBody]
        public DateOnly? Date { get; init; }
        [FromRoute]
        public Guid HabitId { get; init; }
    }
}
