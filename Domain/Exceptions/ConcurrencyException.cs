using System;

namespace ConcurrencyApi.Domain.Exceptions;

public class ConcurrencyException : Exception
{
    public ConcurrencyException() : base("A concurrency conflict occurred. The entity has been modified by another process.")
    {
    }

    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public object? CurrentEntity { get; set; }
    public object? DatabaseEntity { get; set; }
}
