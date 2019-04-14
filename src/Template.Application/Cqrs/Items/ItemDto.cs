using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Template.Application.Cqrs.Items
{
    public class ItemDto : ItemInputModel
    {
        public Guid Id { get; set; }


        public new class Validator : AbstractValidator<ItemDto>
        {
            public Validator(ItemInputModel.Validator baseValidator)
            {
                RuleFor(dto => dto.Id).NotEmpty();
                Include(baseValidator);
            }
        }
    }
}