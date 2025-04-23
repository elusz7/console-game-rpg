using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEntities.Exceptions;

public class AbilityNotFoundException : Exception
{
    public AbilityNotFoundException() : base() { }
    public AbilityNotFoundException(string message) : base(message) { }
}
