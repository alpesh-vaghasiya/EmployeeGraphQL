```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.3803/22H2/2022Update)
Intel Core i5-4590 CPU 3.30GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3


```
| Method                    | Mean     | Error   | StdDev  | Gen0   | Allocated |
|-------------------------- |---------:|--------:|--------:|-------:|----------:|
| Tracking_NoPagination     | 142.8 μs | 2.81 μs | 4.38 μs | 0.9766 |   2.99 KB |
| NoTracking_NoPagination   | 136.2 μs | 0.80 μs | 0.71 μs | 0.9766 |   3.02 KB |
| Tracking_WithPagination   | 135.7 μs | 0.56 μs | 0.49 μs | 0.9766 |   3.05 KB |
| NoTracking_WithPagination | 137.6 μs | 1.69 μs | 1.50 μs | 0.9766 |   3.07 KB |
