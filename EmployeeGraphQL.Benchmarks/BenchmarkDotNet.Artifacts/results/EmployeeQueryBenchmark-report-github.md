```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.3803/22H2/2022Update)
Intel Core i5-4590 CPU 3.30GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3


```
| Method                    | Mean       | Error     | StdDev    | Median     | Gen0   | Allocated |
|-------------------------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
| Tracking_NoPagination     |   268.0 μs |   5.34 μs |   7.99 μs |   266.0 μs | 1.4648 |   5.49 KB |
| NoTracking_NoPagination   |   256.4 μs |   4.67 μs |   7.28 μs |   254.7 μs | 1.4648 |   5.66 KB |
| Tracking_WithPagination   | 3,837.5 μs | 280.67 μs | 823.15 μs | 3,606.2 μs |      - |   8.86 KB |
| NoTracking_WithPagination | 3,529.1 μs | 202.37 μs | 587.12 μs | 3,328.1 μs |      - |   8.97 KB |
