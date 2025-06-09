namespace ConsoleGame.Managers.Interfaces;

public interface IOutputManager
{
    public void Clear();
    public void Display();
    public void Write(string message, ConsoleColor color = ConsoleColor.White);
    public void WriteLine(string message, ConsoleColor color = ConsoleColor.White);
    public void WriteLine();
}
