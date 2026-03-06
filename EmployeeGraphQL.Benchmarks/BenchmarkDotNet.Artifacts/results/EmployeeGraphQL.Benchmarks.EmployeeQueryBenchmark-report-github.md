```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.3803/22H2/2022Update)
Intel Core i5-4590 CPU 3.30GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 8.0.15 (8.0.15, 8.0.1525.16413), X64 RyuJIT x86-64-v3


```
| Method            | Mean     | Error    | StdDev   | Median   | Gen0      | Gen1     | Gen2     | Allocated |
|------------------ |---------:|---------:|---------:|---------:|----------:|---------:|---------:|----------:|
| TrackingQuery     | 10.93 ms | 0.233 ms | 0.662 ms | 10.73 ms | 1656.2500 |  93.7500 |  46.8750 |    5.2 MB |
| AsNoTrackingQuery | 22.15 ms | 1.075 ms | 3.049 ms | 21.35 ms | 1093.7500 | 531.2500 | 187.5000 |   6.57 MB |
