using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Core.Models;
namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IHabitsRepository
    {
        Task<Habit?> FindAsync(Guid id, CancellationToken cancellationToken);
        Task<Habit?> ReadByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Habit>> ReadAllAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<Habit>> SearchAsync(string? term, DateOnly date, Guid userId, CancellationToken cancellationToken);
        Task InsertAsync(Habit habitToInsert, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(JsonPatchDocument document, Habit habit, CancellationToken cancellationToken);
        Task UpdateAllAsync();
        Task DeleteAllProgressAsync(Guid userId, CancellationToken cancellationToken);

        
        Task DeleteAsync(Habit habitToDelete, CancellationToken cancellationToken);
        Task DeleteAllAsync(IEnumerable<Habit> allHabits, CancellationToken cancellationToken);
    }
}
