using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IGroupsRepository
    {
        Task<Group?> ReadByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Group>> ReadAllAsync(Guid userId, CancellationToken cancellationToken);
        Task<Group?> InsertAsync(Group groupToInsert, Guid userId, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(JsonPatchDocument<Group> document, Guid id, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
