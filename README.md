# 🎄 Advent of Code C# Template

A C# template for solving Advent of Code puzzles with benchmarking support.

## How to Use

### 1. Create a Solution Class

Create a new class that inherits from `AdventDayBase` and add the `[Day]` attribute. Naming doesn't matter.

```csharp
[Day(2025, 1)]
public class Day1_2025 : AdventDayBase
{
    public Day1_2025(string input) : base(input) { }

    public override void SolvePart1()
    {
        // Use Input for string-based processing
        var lines = Input.Split('\n');
        int result = lines.Length;
        PrintResult(result);
    }

    public override void SolvePart2()
    {
        // Use InputSpan for span processing
        int count = 0;
        foreach (var line in InputSpan.EnumerateLines())
        {
            count++;
        }
        PrintResult(count);
    }
}
```

### 2. Add Your Input File

Place your puzzle input in the appropriate folder:
```
2025/Inputs/Day1.txt
```

### 3. Run Your Solution

Run the program and answer the prompts:
```
Do you want to benchmark? (y/n):
n
Enter the year:
2025
Enter the day:
1
Enter the part (1 or 2):
1
```

### 4. Benchmark Your Solution (Optional)

To measure performance:
```
Do you want to benchmark? (y/n):
y
Enter the year:
2025
Enter the day:
1
Enter the part (1 or 2):
1
```
