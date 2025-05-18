using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEntities.Main.Exceptions;

public class InvalidSkillLevelException : Exception
{
    public InvalidSkillLevelException() : base() { }
    public InvalidSkillLevelException(string message) : base(message) { }
}
