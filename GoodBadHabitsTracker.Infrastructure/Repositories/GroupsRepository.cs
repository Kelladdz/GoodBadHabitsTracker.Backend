using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using static Amazon.S3.Util.S3EventNotification;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public sealed class GroupsRepository(HabitsDbContext dbContext) : IGroupsRepository
    {
        public async Task<Group?> ReadByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var group = await dbContext.Groups
                    .Include(x => x.Habits)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

            return group;
        }
        public async Task<IEnumerable<Group>> ReadAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            var groups = await dbContext.Groups
                    .Where(g => g.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

            return groups;
        }
        public async Task<Group?> InsertAsync(Group groupToInsert, Guid userId, CancellationToken cancellationToken)
        {
            var newGroup = new Group
            {
                Name = groupToInsert.Name,
                UserId = userId,
            };

            dbContext.Groups.Add(newGroup);
            return await dbContext.SaveChangesAsync(cancellationToken) > 0
                ? newGroup : null;
        } 
        public async Task<bool> UpdateAsync(JsonPatchDocument<Group> document, Guid id, CancellationToken cancellationToken)
        {
            var groupToUpdate = await dbContext.Groups
                .FindAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Group not found");

            document.ApplyTo(groupToUpdate);

            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var groupToRemove = await dbContext.Groups.FindAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Group not found");

            dbContext.Groups.Remove(groupToRemove);

            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
