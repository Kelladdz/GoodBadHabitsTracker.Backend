using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public sealed class GroupsRepository(HabitsDbContext dbContext) : IGroupsRepository
    {
        public async Task<Group?> FindAsync(Guid id, CancellationToken cancellationToken)
            => await dbContext.Groups
                        .FindAsync(id, cancellationToken);
        public async Task<Group?> ReadByIdAsync(Guid id, CancellationToken cancellationToken)
            => await dbContext.Groups
                        .Include(x => x.Habits)
                        .AsNoTracking()
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        public async Task<IEnumerable<Group>> ReadAllAsync(Guid userId, CancellationToken cancellationToken)
            => await dbContext.Groups
                        .Where(g => g.UserId == userId)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);
        public async Task InsertAsync(Group groupToInsert, CancellationToken cancellationToken)
        {
            dbContext.Groups.Add(groupToInsert);
            await dbContext.SaveChangesAsync(cancellationToken);
        } 
        public async Task UpdateAsync(JsonPatchDocument document, Group groupToUpdate, CancellationToken cancellationToken)
        {
            document.ApplyTo(groupToUpdate);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteAsync(Group groupToDelete, CancellationToken cancellationToken)
        {
            dbContext.Groups.Remove(groupToDelete);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    } 
}
