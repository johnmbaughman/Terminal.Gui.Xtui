```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.6060/23H2/2023Update/SunValley3)
Intel Core Ultra 7 155U, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.308
  [Host]   : .NET 8.0.22 (8.0.2225.52707), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.22 (8.0.2225.52707), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0  

```
| Method                              | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------ |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| &#39;Literal Integer&#39;                   |  32.47 ns |  0.692 ns |  1.283 ns |  1.00 |    0.00 | 0.0216 |      - |     136 B |        1.00 |
| Percentage                          | 215.70 ns |  4.253 ns |  6.366 ns |  6.65 |    0.36 | 0.1938 | 0.0002 |    1216 B |        8.94 |
| &#39;Named Method - Center&#39;             | 504.89 ns | 10.008 ns | 20.216 ns | 15.59 |    0.79 | 0.2308 |      - |    1448 B |       10.65 |
| &#39;Named Method - AnchorEnd&#39;          | 515.17 ns | 10.249 ns | 16.255 ns | 15.91 |    0.84 | 0.2308 |      - |    1448 B |       10.65 |
| &#39;Method with Argument&#39;              | 649.70 ns | 12.593 ns | 27.642 ns | 20.22 |    1.18 | 0.2785 |      - |    1752 B |       12.88 |
| &#39;Operator Expression - Addition&#39;    | 462.55 ns |  9.300 ns | 20.020 ns | 14.21 |    0.83 | 0.3014 |      - |    1896 B |       13.94 |
| &#39;Operator Expression - Subtraction&#39; | 450.67 ns |  9.012 ns | 17.578 ns | 13.90 |    0.90 | 0.3014 |      - |    1896 B |       13.94 |
| &#39;Dim Fill&#39;                          | 508.35 ns | 10.186 ns | 17.840 ns | 15.70 |    0.66 | 0.2289 |      - |    1440 B |       10.59 |
| &#39;Dim Auto&#39;                          | 509.80 ns | 10.143 ns | 22.050 ns | 15.63 |    0.93 | 0.2289 |      - |    1440 B |       10.59 |
| &#39;Dim Fill with Operator&#39;            | 449.22 ns |  8.922 ns | 20.500 ns | 13.83 |    0.87 | 0.2995 |      - |    1880 B |       13.82 |
| &#39;String Literal&#39;                    |  73.12 ns |  1.508 ns |  3.046 ns |  2.26 |    0.13 | 0.0293 |      - |     184 B |        1.35 |
| &#39;Boolean True&#39;                      |  22.54 ns |  0.493 ns |  0.825 ns |  0.70 |    0.04 | 0.0127 |      - |      80 B |        0.59 |
| &#39;Boolean False&#39;                     |  22.21 ns |  0.481 ns |  0.854 ns |  0.69 |    0.03 | 0.0127 |      - |      80 B |        0.59 |
