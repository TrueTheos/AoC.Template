namespace AoC.Template;

public interface IAdventDay
{
    void SolvePart1();
    void SolvePart2();
}

public abstract class AdventDayBase : IAdventDay
{
    private bool _silentMode = false;

    protected readonly string Input;
    protected ReadOnlySpan<char> InputSpan => Input.AsSpan();

    protected AdventDayBase(string input)
    {
        Input = input;
    }

    public abstract void SolvePart1();
    public abstract void SolvePart2();

    protected void PrintResult(object result)
    {
        if (!_silentMode)
        {
            Console.WriteLine(result.ToString());
        }
    }

    public void EnableSilentMode()
    {
        _silentMode = true;
    }
}