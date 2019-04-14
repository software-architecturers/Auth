using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Template.Application.Cqrs.Items.Queries;
using Template.Domain.Entities;
using Template.Persistence;

namespace Template.Application.Cqrs.Items.Commands
{
    public class CreateItem : IRequest<ItemDto>
    {
        public CreateItem(ItemInputModel model) => Model = model;

        public ItemInputModel Model { get; }

        public class Handler : IRequestHandler<CreateItem, ItemDto>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<ItemDto> Handle(CreateItem request, CancellationToken cancellationToken)
            {
                var item = _mapper.Map<Item>(request.Model);
                _context.Items.Add(item);
                await _context.SaveChangesAsync(cancellationToken);
                return _mapper.Map<ItemDto>(item);
            }
        }
    }
}