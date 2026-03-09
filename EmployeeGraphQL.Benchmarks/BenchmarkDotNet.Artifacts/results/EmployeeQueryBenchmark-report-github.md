```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.3803/22H2/2022Update)
Intel Core i5-4590 CPU 3.30GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3


```
| Method                    | Mean     | Error    | StdDev   | Median   | Gen0   | Allocated |
|-------------------------- |---------:|---------:|---------:|---------:|-------:|----------:|
| Tracking_NoPagination     | 161.2 μs |  2.91 μs |  2.72 μs | 161.1 μs | 0.9766 |   2.99 KB |
| NoTracking_NoPagination   | 161.9 μs |  3.21 μs |  3.83 μs | 162.2 μs | 0.9766 |   3.02 KB |
| Tracking_WithPagination   | 166.0 μs |  3.26 μs |  3.49 μs | 164.9 μs | 0.9766 |   3.05 KB |
| NoTracking_WithPagination | 195.6 μs | 12.13 μs | 33.81 μs | 181.6 μs | 0.9766 |   3.06 KB |
