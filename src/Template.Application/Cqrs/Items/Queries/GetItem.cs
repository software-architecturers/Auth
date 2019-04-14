using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Template.Application.Exceptions;
using Template.Domain.Entities;
using Template.Persistence;

namespace Template.Application.Cqrs.Items.Queries
{
    public class GetItem : IRequest<ItemDto>
    {
        public GetItem(IdModel idModel)
        {
            IdModel = idModel;
        }

        public IdModel IdModel { get; }


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
                var id = request.IdModel.Id;
                var item = _context.Items.Find(id);
                return item != null
                    ? Task.FromResult(_mapper.Map<ItemDto>(item))
                    : throw new NotFoundException(typeof(Item), id);
            }
        }
    }
}