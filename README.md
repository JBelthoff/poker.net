# Poker Hand Evaluator (.NET 10 Version)
_Optimized for **.NET 10** using modern C# performance techniques, low-allocation patterns, and high-throughput evaluation pipelines._

> A high-performance **Texas Hold’em Poker Hand Evaluator** built with **ASP.NET Core Razor Pages**, faithfully derived from [**Cactus Kev’s classic algorithm**](https://github.com/suffecool/pokerlib) and re-engineered in **pure, allocation-free C#** for deterministic speed and modern clarity.

[![Live Demo](https://img.shields.io/badge/demo-live-brightgreen)](https://poker-calculator.johnbelthoff.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Developer-blue?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-5C2D91?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/sql/sql-server/)
[![Docker](https://img.shields.io/badge/Containerized-Docker-blue?logo=docker)](https://www.docker.com/)

[![Throughput](https://img.shields.io/badge/Throughput-~189M%20hands%2Fs-brightgreen)]()

A modern **.NET 10 implementation** of the legendary [**Cactus Kev Poker Evaluator**](https://github.com/suffecool/pokerlib), rebuilt from the ground up for clarity, determinism, and high-throughput execution.

This version achieves **near-native C++ performance** through deep algorithmic refactoring, zero-allocation evaluation paths, aggressive loop flattening, and end-to-end BenchmarkDotNet validation (see *Performance* section for verified data).

**This repository now showcases a fully optimized .NET 10 Poker Hand Evaluation Engine**, implemented entirely in **pure C#** with no lookup tables, no unsafe code, and no GC pressure in the hot path. Derived five-card throughput from the latest BenchmarkDotNet runs measures roughly **180–210 million evaluations per second**, depending on workload characteristics and thread scheduling (see *Performance* for logs).

_All benchmarks were performed on an Intel Core i9-9940X (14-core, 28-thread) running Windows 10 (22H2) with .NET 10.0 in High-Performance mode._

Ideal for developers exploring **algorithmic optimization**, **combinatorial search performance**, or **advanced .NET JIT/runtime behavior** in tight computational loops.

A working version of this application is available at:  
👉 [https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)


---

### Summary of Verified Performance

| Language | Environment | Representative Throughput | Verification Source |
|---------|-------------|---------------------------|---------------------|
| **C++** | MSVC /O2 AVX2 64-bit | ~188–191 M 7-card hands/sec (≈ 5.3 ns/hand) | [Results-C++.txt](https://github.com/JBelthoff/PokerBenchmarks/blob/master/Results/Results-C++.txt) |
| **C# (.NET 10)** | RyuJIT 64-bit / Concurrent GC | **943.7 ns** (engine-only) · **1.255 µs** (full 9-player evaluation) · **Derived ~180–205 M five-card eval/sec** | [Results-FinalRiverBench.txt](https://github.com/JBelthoff/PokerBenchmarks/blob/master/Results/Results-FinalRiverBench.txt) |

**Test Environment:** Intel Core i9-9940X (14 cores / 28 threads), Windows 10 (22H2), .NET 10.0.0 (Release x64), High-Performance power plan.  

All values above are **directly derived from benchmark logs** with **no extrapolation beyond standard five-card conversion.**  
Any further comparison (e.g., relative speed-ups or ratios) should be recalculated from these verified numbers.

---


## About the Project

This project re-creates the full logic and flow of a **Texas Hold’em Poker** game — from shuffling and dealing cards to evaluating all hands and determining the winner.

The application is built with **ASP.NET Core (C#)** and uses a fully modernized, allocation-free C# reimplementation of [Cactus Kev’s Poker Hand Evaluator](https://poker-calculator.johnbelthoff.com/cactus_kev).

The evaluator preserves the core ranking model while employing contemporary techniques such as loop flattening, value-type pipelines, and zero-allocation execution tuned for .NET 10.

At its current stage, the app:

- Simulates **9 players**
- **Rotates the dealer**
- **Evaluates all hands deterministically**
- **Displays detailed, structured results**

Under the hood, the evaluation engine is optimized for **high-throughput, predictable performance** using the latest .NET 10 JIT improvements, ensuring fully managed execution without lookup tables, unsafe code, or runtime allocations.

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

All benchmarks were executed on **Windows 10 (22H2)** with an **Intel Core i9-9940X (14 cores / 28 threads)** using **.NET 10.0.0** and **BenchmarkDotNet v0.15.6**.  
Power plan: *High Performance*.  
Garbage Collector: *Concurrent Workstation*.

C++ reference results were gathered from the same system using a **Visual Studio x64 Release build** of [**bwedding/PokerEvalMultiThread**](https://github.com/bwedding/PokerEvalMultiThread) (*MSVC /O2 optimization, AVX2-capable CPU*).

---

### Verified Results (.NET 10)

| Benchmark (C# /.NET 10.0.0)                         | Mean (ns / op) | Mean (µs / op)      | Description                                      |
|-----------------------------------------------------|---------------:|--------------------:|--------------------------------------------------|
| **Optimized core evaluator (values-only, no allocs)** | 944 ns         | 0.944 µs            | Core evaluation loop (values-only, zero allocations) |
| **Full 9-player evaluation (EvalEngine pipeline)**    | 1 255 ns       | 1.255 µs            | Complete 9-player best-hand evaluation          |
| **Parallel.For batched (values-only)**               | —              | 730.955 ms total    | Parallel batch throughput test across multiple cores |

Each full 9-player river evaluation covers **189 five-card combinations (9 × 21)**.

Allocation notes:
- The **optimized values-only path** shows no Gen0 collections, confirming a zero-allocation inner loop.
- The **full 9-player evaluation** introduces minimal per-operation allocations, as reflected in GC/Allocated stats from BenchmarkDotNet.

> **Note:** “Values-only” benchmarks measure the pure numeric evaluation core. “Full” benchmarks include ranking and best-hand reconstruction for display.

---


### Verified Results (C++ Reference)

| Benchmark (C++ Reference / bwedding port) | Hands Tested | Time (s) | Hands / sec (M) | ns / hand |
|--------------------------------------------|--------------:|---------:|----------------:|----------:|
| **10 M 7-card hands** | 10,000,000 | 0.0524 | 190.72 M | 5.24 ns |
| **50 M 7-card hands** | 50,000,000 | 0.2651 | 188.60 M | 5.30 ns |
| **100 M 7-card hands** | 100,000,000 | 0.5308 | 188.39 M | 5.31 ns |

Checksums match across all C++ runs, confirming deterministic evaluation.

---

### Observations

- The **optimized .NET 10 evaluator** achieves **~944 ns per operation (0.944 µs)** in its tightest values-only path, and **≈ 1.255 µs** for full 9-player evaluation.
- The **C++ reference** still achieves **~188–191 million 7-card hands/sec (~5.3 ns/hand)** under optimized `/O2 AVX2` builds.
- These results show the **.NET 10 implementation maintaining near–microsecond performance in the core evaluator** and delivering end-to-end throughput very close to the theoretical limits of managed execution.


---


### 📚 Algorithm Lineage and Faithfulness

This evaluator is a modern **C# translation** of [Cactus Kev’s Poker Hand Evaluator](https://github.com/suffecool/pokerlib) —  
the classic C implementation that popularized **prime-number-based hand evaluation**.

All core logic is faithfully preserved:
- Flush and straight detection tables  
- Perfect-hash prime product encoding  
- Rank thresholds identical to Kev’s original  

Where Kev used C arrays, macros, and pointer arithmetic, **Poker.net** employs managed data structures, `Span<T>` buffers, and modern .NET 10 JIT optimizations to achieve equivalent throughput — **without huge in-memory lookup tables or unsafe code**.

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
source/PokerBenchmarks/bin/Release/net10.0/BenchmarkDotNet.Artifacts/results/
```

---


## SQL Server Setup

> Skip this section if you're running in **No-DB (Static)** mode.

1. Create a SQL Server database named `PokerApp`.  
2. Create a Login and User for the database.  
3. Run the appropriate initialization script from the [`x_dBase`](x_dBase/) folder:
   - [`CreateDB-2019.sql`](x_dBase/CreateDB-2019.sql) — for **SQL Server 2019 or earlier**  
   - [`CreateDB-2022.sql`](x_dBase/CreateDB-2022.sql) — for **SQL Server 2022 and newer** (supports `STRING_SPLIT(@s, '|', 1)` and other modern features)  
4. Update your connection string (via `appsettings.json`, User Secrets, or environment variables).  
5. Set `"UseSqlServer": true` in your configuration.  
6. Build and run the project (`dotnet run`, Docker, or IIS Express).  
7. Visit the app in your browser and start playing!

---

>*The 2019 script maintains full backward compatibility down to SQL Server 2008 by using `datetime2` and the legacy `DelimitedSplit8K` function.  
The 2022 script reflects modern best practices per Davide Mauri (@yorek), including transaction safety (`XACT_ABORT ON`) and native `STRING_SPLIT` with ordinal output.*

---



## 📚 Further Reading

- **[C# vs C Poker Evaluator Benchmark](https://gist.github.com/JBelthoff/4fcd73edd04fd5d239a194e7112810f4)**  
  Deep dive into optimizing and benchmarking the Poker Evaluator engine across .NET 8 → 10 and native C (MSVC /O2).  
  *Example output shows native C at 3.76 M evals/sec vs .NET 8 at ~3.08 M evals/sec (~82 % of C speed).*

- **[SQL Insert Benchmark (.NET 9) — Pipe vs JSON vs TVP](https://gist.github.com/JBelthoff/adca6398585e14c6b27378c6ca171d61)**  
  Microbenchmark of common SQL Server insert strategies (Pipe-delimited, JSON, and TVP).  
  *Pipe wins for tiny inserts, TVP scales best, JSON wins for maintainability.*

---


## Contact

💼 Interested in performance engineering or .NET optimization work?  
Contact me via [LinkedIn](https://www.linkedin.com/in/john-belthoff/) or visit [johnbelthoff.com](https://www.johnbelthoff.com/).  
  
---

© 2025 **John Belthoff**  
[www.johnbelthoff.com](https://www.johnbelthoff.com/)
