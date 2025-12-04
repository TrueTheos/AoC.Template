namespace AoC.Template;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DayAttribute : Attribute
{
    public int Year { get; }
    public int DayNumber { get; }
    public DayAttribute(int year, int dayNumber)
    {
        Year = year;
        DayNumber = dayNumber;
    }
}