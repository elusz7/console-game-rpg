using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEntities.Exceptions;

public class InvalidTargetException : Exception
{
    public InvalidTargetException() : base() { }
    public InvalidTargetException(string message) : base(message) { }
}
