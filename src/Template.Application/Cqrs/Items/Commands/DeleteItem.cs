using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Template.Persistence;

namespace Template.Application.Cqrs.Items.Commands
{
    public class DeleteItem : IRequest<Unit?>
    {
        public DeleteItem(IdModel idModel) => IdModel = idModel;

        public IdModel IdModel { get; }

        public class Handler : IRequestHandler<DeleteItem, Unit?>
        {
            private readonly ApplicationDbContext _context;

            public Handler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit?> Handle(DeleteItem request, CancellationToken cancellationToken)
            {
                var id = request.IdModel.Id;
                var item = _context.Items.Find(id);
                if (item == null) return null;

                _context.Items.Remove(item);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}