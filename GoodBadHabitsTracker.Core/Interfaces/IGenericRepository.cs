using Microsoft.AspNetCore.JsonPatch;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IGenericRepository<TEntity> 
        where TEntity : class
    { 
        Task<TEntity> ReadByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<TEntity>> ReadAllAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<TEntity>> SearchAsync(string? term, DateOnly date, Guid userId, CancellationToken cancellationToken);
        Task<TEntity> InsertAsync(TEntity entityToInsert, Guid userId, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(JsonPatchDocument<TEntity> document, Guid userId, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
