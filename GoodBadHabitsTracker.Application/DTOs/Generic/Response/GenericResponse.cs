namespace GoodBadHabitsTracker.Application.DTOs.Generic.Response
{
    public sealed record GenericResponse<TEntity>(TEntity Entity) where TEntity : class;
}
