using System;

namespace SeaMonkey.Exceptions
{
    public class InvalidTroopRequestException : Exception
    {
        public InvalidTroopRequestException(string message) : base(message)
        {
        }
    }
}