using System;

namespace Auth.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(Type type, object key)
            : base($"Entity {type.Name} with key {key} not found")
        {
        }

        public static NotFoundException OfType<T>(T instance, object key)
            => new NotFoundException(typeof(T), key);
    }
}