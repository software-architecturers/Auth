using System;
using FluentValidation;

namespace Template.Application.Cqrs
{
    public struct IdModel : IEquatable<IdModel>
    {
        public Guid Id { get; }
        public IdModel(Guid id) => Id = id;


        public bool Equals(IdModel other) => Id.Equals(other.Id);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IdModel other && Equals(other);
        }

        public static bool operator ==(IdModel first, IdModel second) => first.Equals(second);

        public static bool operator !=(IdModel first, IdModel second) => !first.Equals(second);

        public override int GetHashCode() => Id.GetHashCode();
        
        public static implicit operator IdModel(Guid id) => new IdModel(id);
        public static implicit operator Guid(IdModel model) => model.Id;
        
        
        public class Validator : AbstractValidator<IdModel>
        {
            public Validator()
            {
                RuleFor(model => model.Id).NotEmpty();
            }
        }
    }
}