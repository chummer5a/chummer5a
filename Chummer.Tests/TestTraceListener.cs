using System.Diagnostics;
using System.Text;

namespace Chummer.Tests;

public class TestTraceListener : TraceListener
{
    private StringBuilder Log { get; set; } = new StringBuilder();

    public override void Write(string? message)
    {
        Log.Append(message);
    }

    public override void WriteLine(string? message)
    {
        Log.AppendLine(message);
    }

    public string GetContentAndClear()
    {
        var currentValue = Log.ToString();
        Log = new StringBuilder(); // Clear the builder
        return currentValue;
    }
}
