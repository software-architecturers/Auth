using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;
using Template.Persistence;

namespace Template.Application.Cqrs.Items.Queries
{
    public class GetAllItems : IRequest<IEnumerable<ItemDto>>
    {
        public class Handler : IRequestHandler<GetAllItems, IEnumerable<ItemDto>>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<IEnumerable<ItemDto>> Handle(GetAllItems request, CancellationToken cancellationToken)
            {
               return Task.FromResult(_mapper.ProjectTo<ItemDto>(_context.Items.AsNoTracking()).AsEnumerable());
            }
        }
    }
}