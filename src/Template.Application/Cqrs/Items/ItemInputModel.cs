using FluentValidation;

namespace Template.Application.Cqrs.Items
{
    public class ItemInputModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        
        public class Validator : AbstractValidator<ItemInputModel>
        {
            public Validator()
            {
                RuleFor(item => item.Name).NotEmpty();
            }
        }
    }
}