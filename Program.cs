using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AoC.Template;

public class Program
{
    public static readonly Dictionary<(int year, int day), Type> Days = LoadDays();

    public static void Main(string[] args)
    {
        try
        {
            bool shouldBenchmark = PromptYesNo("Do you want to benchmark?");
            int year = PromptInt("Enter the year:", y => Days.Keys.Any(k => k.year == y), "Year not found in available solutions.");
            int day = PromptInt("Enter the day:", d => Days.ContainsKey((year, d)), $"Day not found for year {year}.");
            int part = PromptInt("Enter the part (1 or 2):", p => p == 1 || p == 2, "Part must be 1 or 2.");

            if (shouldBenchmark)
            {
                RunBenchmark(year, day, part);
            }
            else
            {
                RunSolution(year, day, part);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static bool PromptYesNo(string message)
    {
        while (true)
        {
            Console.WriteLine($"{message} (y/n):");
            string input = Console.ReadLine()?.Trim().ToLower();
            
            if (input == "y") return true;
            if (input == "n") return false;
            
            Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
        }
    }

    private static int PromptInt(string message, Func<int, bool> validator = null, string errorMessage = null)
    {
        while (true)
        {
            Console.WriteLine(message);
            string input = Console.ReadLine();
            
            if (int.TryParse(input, out int result))
            {
                if (validator == null || validator(result))
                {
                    return result;
                }
                
                Console.WriteLine(errorMessage ?? "Invalid value. Please try again.");
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
    }

    private static string GetProjectRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetDirectoryName(sourceFilePath)!;
    }
    
    private static void RunBenchmark(int year, int day, int part)
    {
        BenchmarkRunnerClass.ConfigureForSpecificYearDayAndPart(year, day, part);
        BenchmarkRunner.Run<BenchmarkRunnerClass>();
    }

    private static void RunSolution(int year, int day, int part)
    {
        string input = ReadInputFile(year, day);
        
        if (Activator.CreateInstance(Days[(year, day)], input) is not IAdventDay adventDay)
        {
            throw new InvalidOperationException($"Failed to create instance for day {day}.");
        }

        if (part == 1)
            adventDay.SolvePart1();
        else
            adventDay.SolvePart2();
    }

    private static string ReadInputFile(int year, int day)
    {
        string projectRoot = GetProjectRoot();
        string yearFolder = Path.Combine(projectRoot, year.ToString());
        string inputsFolder = Path.Combine(yearFolder, "Inputs");
        string filePath = Path.Combine(inputsFolder, $"Day{day}.txt");
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Failed to find input file.");
            Console.WriteLine($"  Expected path: {filePath}");
            Console.WriteLine($"  Project root: {projectRoot}");
            Console.WriteLine($"  Year folder exists: {Directory.Exists(yearFolder)}");
            Console.WriteLine($"  Inputs folder exists: {Directory.Exists(inputsFolder)}");
            
            if (Directory.Exists(inputsFolder))
            {
                var files = Directory.GetFiles(inputsFolder, "*.txt");
                Console.WriteLine($"  Available files in Inputs folder: {(files.Length > 0 ? string.Join(", ", files.Select(Path.GetFileName)) : "none")}");
            }
            
            throw new FileNotFoundException($"Input file not found: {filePath}");
        }
        
        return File.ReadAllText(filePath);
    }

    private static Dictionary<(int year, int day), Type> LoadDays()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IAdventDay).IsAssignableFrom(t) && t.GetCustomAttribute<DayAttribute>() != null)
            .ToDictionary(t =>
            {
                var attr = t.GetCustomAttribute<DayAttribute>();
                return (attr.Year, attr.DayNumber);
            }, t => t);
    }
}

[MemoryDiagnoser(displayGenColumns: true)]
public class BenchmarkRunnerClass
{
    private readonly Dictionary<(int year, int day), Type> _days = Program.Days;

    [ParamsSource(nameof(GetFilteredYears))]
    public int Year { get; set; }

    [ParamsSource(nameof(GetFilteredDays))]
    public int Day { get; set; }

    [ParamsSource(nameof(GetFilteredParts))]
    public int Part { get; set; }

    private IAdventDay _adventDay;
    private string _input;

    [GlobalSetup]
    public void Setup()
    {
        if (_days.TryGetValue((Year, Day), out Type dayType))
        {
            string projectRoot = GetProjectRoot();
            string yearFolder = Path.Combine(projectRoot, Year.ToString());
            string inputsFolder = Path.Combine(yearFolder, "Inputs");
            string filePath = Path.Combine(inputsFolder, $"Day{Day}.txt");
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Benchmark setup failed: Input file not found.");
                Console.WriteLine($"  Expected path: {filePath}");
                Console.WriteLine($"  Project root: {projectRoot}");
                Console.WriteLine($"  Year folder exists: {Directory.Exists(yearFolder)}");
                Console.WriteLine($"  Inputs folder exists: {Directory.Exists(inputsFolder)}");
                
                if (Directory.Exists(inputsFolder))
                {
                    var files = Directory.GetFiles(inputsFolder, "*.txt");
                    Console.WriteLine($"  Available files in Inputs folder: {(files.Length > 0 ? string.Join(", ", files.Select(Path.GetFileName)) : "none")}");
                }
                
                throw new FileNotFoundException($"Input file not found: {filePath}");
            }
            
            _input = File.ReadAllText(filePath);
            
            _adventDay = Activator.CreateInstance(dayType, _input) as IAdventDay;
            
            if (_adventDay is AdventDayBase adventDayBase)
            {
                adventDayBase.EnableSilentMode();
            }
        }
    }
    
    private static string GetProjectRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetDirectoryName(sourceFilePath)!;
    }

    [Benchmark]
    public void RunDay()
    {
        if (_adventDay == null || string.IsNullOrEmpty(_input))
            throw new InvalidOperationException("Benchmark setup failed.");

        if (Part == 1)
            _adventDay.SolvePart1();
        else
            _adventDay.SolvePart2();
    }

    public static IEnumerable<int> GetFilteredYears() => new[] { YearToBenchmark };
    public static IEnumerable<int> GetFilteredDays() => new[] { DayToBenchmark };
    public static IEnumerable<int> GetFilteredParts() => new[] { PartToBenchmark };

    public static int YearToBenchmark { get; private set; }
    public static int DayToBenchmark { get; private set; }
    public static int PartToBenchmark { get; private set; }

    public static void ConfigureForSpecificYearDayAndPart(int year, int day, int part)
    {
        if (Program.Days.ContainsKey((year, day)) && (part == 1 || part == 2))
        {
            YearToBenchmark = year;
            DayToBenchmark = day;
            PartToBenchmark = part;
        }
        else
        {
            throw new ArgumentException("Invalid year, day or part.");
        }
    }
}