using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEntities.Main.Exceptions;

public class SkillCooldownException : Exception
{
    public SkillCooldownException() : base() { }
    public SkillCooldownException(string message) : base(message) { }
}
