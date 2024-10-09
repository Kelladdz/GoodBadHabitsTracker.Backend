using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Delete
{
    internal sealed class DeleteCommandHandler<TEntity> : IRequestHandler<DeleteCommand<TEntity>, bool>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        public DeleteCommandHandler(IGenericRepository<TEntity> genericRepository)
        {
            _genericRepository = genericRepository;
        }
        public async Task<bool> Handle(DeleteCommand<TEntity> command, CancellationToken cancellationToken)
            => await _genericRepository.DeleteAsync(command.Id, cancellationToken);
    }
}
