using MediatR;
using GoodBadHabitsTracker.Core.Interfaces;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Update
{
    internal sealed class UpdateCommandHandler<TEntity> : IRequestHandler<UpdateCommand<TEntity>, bool>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        public UpdateCommandHandler(
            IGenericRepository<TEntity> genericRepository)
        {
            _genericRepository = genericRepository;
        
        }
        public async Task<bool> Handle(UpdateCommand<TEntity> request, CancellationToken cancellationToken)
        {
            var habitId = request.Id;
            var document = request.Request;

            return await _genericRepository.UpdateAsync(document, habitId, cancellationToken);
        }
    }
}
