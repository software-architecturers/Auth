using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Template.Domain.Entities;
using Template.Persistence;

namespace Template.Application.Cqrs.Items.Commands
{
    public class EditItem : IRequest<Unit>
    {
        public EditItem(IdModel id, ItemDto dto) => (Id, Dto) = (id, dto);
        public void Deconstruct(out IdModel id, out ItemDto model) => (id, model) = (Id, Dto);
        public IdModel Id { get; }
        public ItemDto Dto { get; }

        public class Handler : IRequestHandler<EditItem, Unit>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(EditItem request, CancellationToken cancellationToken)
            {
                var (idModel, dto) = request;
                if (idModel.Id != dto.Id)
                {
                    return Unit.Value;
                }

                var item = _context.Items.Find(idModel.Id);
                _mapper.Map(dto, item);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}