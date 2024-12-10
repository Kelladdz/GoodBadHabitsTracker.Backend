using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IGroupsRepository
    {
        Task<Group?> FindAsync(Guid id, CancellationToken cancellationToken);
        Task<Group?> ReadByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Group>> ReadAllAsync(Guid userId, CancellationToken cancellationToken);
        Task InsertAsync(Group groupToInsert, CancellationToken cancellationToken);
        Task UpdateAsync(JsonPatchDocument document, Group groupToUpdate, CancellationToken cancellationToken);
        Task DeleteAsync(Group groupToDelete, CancellationToken cancellationToken);
    }
}
