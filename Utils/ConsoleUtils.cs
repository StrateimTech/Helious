namespace Helious.Utils;

public static class ConsoleUtils
{
    public static void WriteLine(string value)
    {            
        Console.WriteLine($" > {value}");
    }

    public static void WriteOnLine()
    {
        Console.WriteLine(" > ");
        Console.SetCursorPosition(3, Console.CursorTop-1);
    }

    public static void WriteCentered(string value)
    {
        int length = Console.WindowWidth - value.Length;
        if (length < 2)
        {
            Console.WriteLine(value);
            return;
        }

        Console.SetCursorPosition(length / 2, Console.CursorTop);
        Console.WriteLine(value);
    }
}