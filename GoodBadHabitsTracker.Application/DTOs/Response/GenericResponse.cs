namespace GoodBadHabitsTracker.Application.DTOs.Response
{
    public sealed record GenericResponse<TEntity>(TEntity Entity) where TEntity : class;
}
