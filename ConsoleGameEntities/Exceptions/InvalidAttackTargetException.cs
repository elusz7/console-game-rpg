using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEntities.Exceptions;

public class InvalidAttackTargetException : Exception
{
    public InvalidAttackTargetException() : base() { }
    public InvalidAttackTargetException(string message) : base(message) { }
}
