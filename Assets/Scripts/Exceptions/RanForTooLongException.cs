using System;

public class RanForTooLongException : Exception
{
    public RanForTooLongException(string message) : base(message)
    {
    }
}
