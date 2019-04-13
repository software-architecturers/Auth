using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Template.Domain.Entities;
using Template.Persistence;

namespace Template.Application.Cqrs.Items.Queries
{
    public class GetItem : IRequest<ItemDto>
    {
        public Guid Id { get; set; }


        public class Handler : IRequestHandler<GetItem, ItemDto>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public Task<ItemDto> Handle(GetItem request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_mapper.Map<ItemDto>(_context.Items.Find(request.Id)));
            }
        }

        public class Validator : AbstractValidator<GetItem>
        {
            public Validator()
            {
                RuleFor(item => item.Id).NotEmpty();
            }
        }
    }
}