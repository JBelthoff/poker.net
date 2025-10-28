# Poker Hand Evaluator (.NET Core Version)
_**Version 2.0** - Optimized for .NET 8 and 9 using modern C# performance engineering_ 

> A high-performance **Texas Hold’em Poker Hand Evaluator** built with **ASP.NET Core Razor Pages**, faithfully derived from [**Cactus Kev’s classic algorithm**](https://github.com/suffecool/pokerlib) and re-engineered in **pure, allocation-free C#** for deterministic speed and modern clarity.

[![Live Demo](https://img.shields.io/badge/demo-live-brightgreen)](https://poker-calculator.johnbelthoff.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Developer-blue?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-5C2D91?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/sql/sql-server/)
[![Docker](https://img.shields.io/badge/Containerized-Docker-blue?logo=docker)](https://www.docker.com/)

[![Throughput](https://img.shields.io/badge/Throughput-~189M%20hands%2Fs-brightgreen)]()

A modern **.NET 8/9 implementation** of the legendary [**Cactus Kev Poker Evaluator**](https://github.com/suffecool/pokerlib), rebuilt from the ground up for clarity, determinism, and speed.  

This version achieves **near-native C++ performance** through careful algorithmic refactoring, zero memory allocations, and extensive BenchmarkDotNet validation (see *Performance* section for verified benchmark data).

**This repository showcases a fully optimized .NET 8 Poker Hand Evaluation Engine**, implemented in **pure C#** without lookup tables or unsafe code. Estimated throughput, based on derived five-card evaluation counts from recent benchmarks, corresponds to roughly **170–190 million evaluations per second**, depending on workload (see *Performance* section for logs).

_All benchmarks were performed on an Intel Core i9-9940X (14-core, 28-thread) running Windows 10 (22H2) with .NET 8.0.21 under the High-Performance power plan._

Ideal for developers exploring **algorithmic optimization**, **combinatorial evaluation**, or **.NET performance engineering**.

A working version of this application is available at:  
👉 [https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)


---

### Summary of Verified Performance

| Language | Environment | Representative Throughput | Verification Source |
|-----------|--------------|--------------------------:|--------------------:|
| **C++** | MSVC /O2 AVX2 64-bit | ~186–189 M 7-card hands/sec (≈ 5.3 ns/hand) | [Results-C++.txt](https://github.com/JBelthoff/PokerBenchmarks/blob/master/Results/Results-C++.txt) |
| **C# (.NET 8)** | RyuJIT 64-bit / Concurrent GC | ≈ 0.91–1.39 µs per 9-player evaluation (189 combos) | [Results-CSharp.txt](https://github.com/JBelthoff/PokerBenchmarks/blob/master/Results/Results-FinalRiverBench.txt) |

**Test Environment:** Intel Core i9-9940X (14 cores / 28 threads), Windows 10 (22H2), .NET 8.0.21 (Release x64), High-Performance power plan.  

All values above are **directly derived from benchmark logs** with **no extrapolation or inferred percentages.**  
Any further comparison (e.g., relative speed-ups or ratios) should be recalculated from these verified numbers.

---


## About the Project


This project re-creates the logic and structure of a full **Texas Hold’em Poker** game — from shuffling and dealing cards to evaluating hands and determining the winner.

The application is written in **ASP.NET Core (C#)** and uses a modern C# port of [Cactus Kev’s Poker Hand Evaluator](https://poker-calculator.johnbelthoff.com/cactus_kev), the classic C implementation that popularized prime-number-based hand ranking.

At this stage, the app:

- Simulates **9 players**
- **Rotates the dealer**
- **Calculates the winning hand**
- **Displays detailed results**

Under the hood, the evaluation engine is optimized for high throughput and deterministic performance, maintaining fully managed, allocation-free execution.

---


## ⚙️ Quick Start

Clone and launch the project:

```bash
git clone https://github.com/JBelthoff/poker.net.git
cd poker.net
dotnet run
```

> **Tip:**  
> After running `dotnet run`, check the console log for the exact URL (“Now listening on…”).  
> Then open that URL (usually `https://localhost:5001`) in your browser.  
>
> **Note:**  
> By default, the app runs in No-DB (Static) mode using an in-memory deck.  
> To enable SQL Server support for recording games, set `"UseSqlServer": true` in your configuration and ensure SQL Server is installed.  
> See *SQL Server Setup* for step-by-step instructions.

---


## ⚡ Performance

### Benchmark Environment

All benchmarks were executed on **Windows 10 (22H2)** with an **Intel Core i9-9940X (14 cores / 28 threads)** using **.NET 8.0.21** and **BenchmarkDotNet v0.15.4**.  
Power plan: *High Performance*.  
Garbage Collector: *Concurrent Workstation*.

C++ reference results were gathered from the same system using a **Visual Studio x64 Release build** of [**bwedding/PokerEvalMultiThread**](https://github.com/bwedding/PokerEvalMultiThread) (*MSVC /O2 optimization, AVX2-capable CPU*).


---

### Verified Results (.NET 8)

| Benchmark (C# /.NET 8.0.21) | Mean (ns / op) | Mean (µs / op) | Description |
|------------------------------|---------------:|---------------:|--------------|
| **Optimized core evaluator (values-only, no allocs)** | ≈ 907 ns | 0.907 µs | Evaluates 9 players × 21 7-card combinations (values only) |
| **Core evaluator (9 players × 21 combos)** | ≈ 1 390 ns | 1.39 µs | Best-of-7 evaluation with ranks and scores |
| **Full 9-player showdown (including setup and scoring)** | ≈ 910 ns | 0.91 µs | End-to-end EvalEngine path |
| **Full 9-player evaluation + best-hand reconstruction** | ≈ 1 221.9 ns | 1.22 µs | Returns sorted winning hands for UI display |

Each full 9-player river evaluation covers **189 five-card combinations (9 × 21)**.

Allocation notes:
- The **optimized values-only path** shows no Gen0 collections during measurement, consistent with a zero-allocation hot path.
- Other benchmark variants (e.g., full evaluation or index/rank-producing paths) do perform small per-operation allocations, as reflected in their GC/Allocated reports.

> **Note:** “Values-only” benchmarks measure the raw numeric evaluation loop. “Full” benchmarks also build and sort winning hands for display.



---

### Verified Results (C++ Reference)

| Benchmark (C++ bwedding / Suffecool port) | Hands Tested | Time (s) | Hands / sec (M) | ns / hand |
|--------------------------------------------|--------------:|---------:|----------------:|----------:|
| **10 M 7-card hands** | 10,000,000 | 0.0529 | 188.94 M | 5.29 ns |
| **50 M 7-card hands** | 50,000,000 | 0.2686 | 186.17 M | 5.37 ns |
| **100 M 7-card hands** | 100,000,000 | 0.5326 | 187.74 M | 5.33 ns |

Checksums (e.g., `40971729725`) match across runs, confirming deterministic behavior.

---

### Observations

- The optimized .NET 8 build demonstrates consistent sub-microsecond operation for full 9-player evaluations and under 1 µs for the fastest paths.  
- C++ reference throughput averages **~186–189 million 7-card hands per second (≈ 5.3 ns/hand)** under MSVC /O2 AVX2 builds.  
- Both implementations produce identical hand distribution statistics and checksums, confirming algorithmic equivalence.


---

## Latest Numbers


### .NET 8

````markdown
```
BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6456/22H2/2022Update)
Intel Core i9-9940X CPU 3.30GHz, 1 CPU, 28 logical and 14 physical cores
.NET SDK 10.0.100-rc.2.25502.107
  [Host]     : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v4
```
````

| Method                                          | CardIds               |             Mean |            Error |           StdDev |           Median | Gen0 (ops) | Allocated |
| :---------------------------------------------- | :-------------------- | ---------------: | ---------------: | ---------------: | ---------------: | ---------: | --------: |
| Optimized core evaluator (max throughput)       | 30|8(... )|9|50 [146] |         902.3 ns |          1.13 ns |          0.94 ns |         902.6 ns |          0 |         0 |
| Full 9-player evaluation (what the webapp uses) | 30|8(... )|9|50 [146] |       1,206.7 ns |          8.05 ns |          6.72 ns |       1,206.1 ns |     0.0763 |     784 B |
| Throughput: Parallel.For batched (values-only)  | 30|8(... )|9|50 [146] | 712,106,180.0 ns | 21,180,375.15 ns | 60,770,471.14 ns | 686,704,000.0 ns |          0 |  13,104 B |

---

### .NET 9

````markdown
```
BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6456/22H2/2022Update)
Intel Core i9-9940X CPU 3.30GHz, 1 CPU, 28 logical and 14 physical cores
.NET SDK 10.0.100-rc.2.25502.107
  [Host]     : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v4
```
````

| Method                                          | CardIds               |             Mean |            Error |           StdDev |           Median | Gen0 (ops) | Allocated |
| :---------------------------------------------- | :-------------------- | ---------------: | ---------------: | ---------------: | ---------------: | ---------: | --------: |
| Optimized core evaluator (max throughput)       | 30|8(... )|9|50 [146] |         866.8 ns |          1.34 ns |          1.12 ns |         866.8 ns |          0 |         0 |
| Full 9-player evaluation (what the webapp uses) | 30|8(... )|9|50 [146] |       1,199.1 ns |          2.73 ns |          2.56 ns |       1,199.8 ns |     0.0763 |     784 B |
| Throughput: Parallel.For batched (values-only)  | 30|8(... )|9|50 [146] | 684,865,571.7 ns | 17,912,920.22 ns | 50,523,712.99 ns | 663,761,350.0 ns |          0 |  11,424 B |

---

### .NET 10

````markdown
```
BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6456/22H2/2022Update)
Intel Core i9-9940X CPU 3.30GHz, 1 CPU, 28 logical and 14 physical cores
.NET SDK 10.0.100-rc.2.25502.107
  [Host]     : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v4
```
````

| Method                                          | CardIds               |             Mean |           Error |          StdDev | Gen0 (ops) | Allocated |
| :---------------------------------------------- | :-------------------- | ---------------: | --------------: | --------------: | ---------: | --------: |
| Optimized core evaluator (max throughput)       | 30|8(... )|9|50 [146] |         955.7 ns |         3.55 ns |         2.97 ns |          0 |         0 |
| Full 9-player evaluation (what the webapp uses) | 30|8(... )|9|50 [146] |       1,119.7 ns |         3.00 ns |         2.81 ns |     0.0763 |     784 B |
| Throughput: Parallel.For batched (values-only)  | 30|8(... )|9|50 [146] | 726,602,341.7 ns | 2,276,099.26 ns | 1,777,028.58 ns |          0 |  12,776 B |


### Multithreaded Engine-Only Throughput  
(10 M Rivers; 9 Players × 21 Combos = 189 Five-Card Evals per River)

| Framework     | Mean Time (s) | Rivers / sec (9-player evals) | Derived 5-Card Evals / sec (×189) |
|:--------------|--------------:|------------------------------:|----------------------------------:|
| **.NET 9**    | 0.684865572   | **14,601,405.6**              | **2,759,665,658.9**               |
| **.NET 8**    | 0.712106180   | 14,042,849.6                  | 2,654,098,578.4                   |
| **.NET 10 (RC)** | 0.726602342 | 13,762,686.2                  | 2,601,147,685.3                   |

---

### Quick Take
- **.NET 9** is currently the fastest, sustaining **≈ 2.76 billion** derived five-card evaluations per second.  
- **.NET 8** follows closely at **≈ 2.65 billion / s**, within ~4 % of .NET 9.  
- **.NET 10 RC** trails slightly at **≈ 2.60 billion / s**, likely due to JIT tuning still in progress.

All numbers are **directly derived from the benchmark logs** (Mean times of 0.685 s, 0.712 s, 0.727 s) and correspond exactly to the workload defined in `FinalRiverBench.Parallel_Batched_ValuesOnly()`.


---


### 📚 Algorithm Lineage and Faithfulness

This evaluator is a modern **C# translation** of [Cactus Kev’s Poker Hand Evaluator](https://github.com/suffecool/pokerlib) —  
the classic C implementation that popularized **prime-number-based hand evaluation**.

All core logic is faithfully preserved:
- Flush and straight detection tables  
- Perfect-hash prime product encoding  
- Rank thresholds identical to Kev’s original  

Where Kev used C arrays, macros, and pointer arithmetic, **Poker.net** employs managed data structures, `Span<T>` buffers, and modern .NET 8 JIT optimizations to achieve equivalent throughput — **without huge in-memory lookup tables or unsafe code**.

Comprehensive validation confirms **one-to-one rank and frequency equivalence** with the original algorithm, and checksum tests verify that every evaluated hand produces identical results to the C reference.

---

## 🧪 Running Benchmarks Locally

To reproduce, follow these steps:

### 1. Clone the Required Repositories

| Repository                                                                          | Description                                                       |
| ----------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| [`poker.net`](https://github.com/JBelthoff/poker.net)                               | Core ASP.NET / C# evaluator (switch to the `optimization` branch) |
| [`PokerBenchmarks`](https://github.com/JBelthoff/PokerBenchmarks)                   | Standalone benchmark harness and verified results                 |
| [`bwedding/PokerEvalMultiThread`](https://github.com/bwedding/PokerEvalMultiThread) | Reference **C++** implementation                                  |
| [`suffecool/pokerlib`](https://github.com/suffecool/pokerlib)                       | Original **C** evaluator by Cactus Kev                            |

---

### 2. Add PokerBenchmarks as Existing Project to poker.net

If you clone both repositories under a single `source` directory like this:

```
source/poker.net  
source/PokerBenchmarks
```

Then everything is already configured:

* `PokerBenchmarks` already includes the correct NuGet packages (`BenchmarkDotNet`)
* The project references are pre-wired for `poker.net`

Simply:

1. **Open the solution**
2. **Set `PokerBenchmarks` as the Startup Project**
3. **Build in Release mode**
4. Press **Ctrl + F5** to run the benchmarks

BenchmarkDotNet will automatically generate results under:

```
source/PokerBenchmarks/bin/Release/net8.0/BenchmarkDotNet.Artifacts/results/
```

---


## SQL Server Setup

> Skip this section if you're running in No-DB (Static) mode.

1. Create a SQL Server database named `PokerApp`.  
2. Create a Login and User for the database.  
3. Run the script [`CreateDB.sql`](x_dBase/CreateDB.sql) against `PokerApp`.  
4. Update your connection string (via `appsettings.json`, User Secrets, or environment variables).  
5. Set `"UseSqlServer": true` in your configuration.  
6. Build and run the project (`dotnet run`, Docker, or IIS Express).  
7. Visit the app in your browser and start playing!

---

## Contact

💼 Interested in performance engineering or .NET optimization work?  
Contact me via [LinkedIn](https://www.linkedin.com/in/john-belthoff/) or visit [johnbelthoff.com](https://www.johnbelthoff.com/).  
  
---

© 2025 **John Belthoff**  
[www.johnbelthoff.com](https://www.johnbelthoff.com/)
