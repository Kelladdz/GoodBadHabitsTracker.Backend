using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Core.Models;
namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IHabitsRepository
    {
        Task<Habit?> ReadByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Habit>> ReadAllAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<Habit>> SearchAsync(string? term, DateOnly date, Guid userId, CancellationToken cancellationToken);
        Task<Habit?> InsertAsync(Habit habitToInsert, Guid id, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(JsonPatchDocument<Habit> document, Guid id, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
