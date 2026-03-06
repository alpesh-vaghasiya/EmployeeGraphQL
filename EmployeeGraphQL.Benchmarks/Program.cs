using BenchmarkDotNet.Running;

namespace EmployeeGraphQL.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<EmployeeQueryBenchmark>();
    }
}