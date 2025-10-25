# Poker Hand Evaluator (.NET Core Version)
_**Version 2.0** - Optimized for .NET 8 and 9 using modern C# performance engineering_

> A high-performance Texas Hold’em Poker Hand Evaluator built with **ASP.NET Core Razor Pages**,  faithfully based on [**Cactus Kev’s Poker Hand Evaluator**](https://github.com/suffecool/pokerlib)  and completely re-engineered in **pure, allocation-free C#**.


[![Live Demo](https://img.shields.io/badge/demo-live-brightgreen)](https://poker-calculator.johnbelthoff.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)


[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Developer-blue?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-5C2D91?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/sql/sql-server/)
[![Docker](https://img.shields.io/badge/Containerized-Docker-blue?logo=docker)](https://www.docker.com/)


A modern **.NET 8/9 implementation** of the legendary [**Cactus Kev Poker Evaluator**](https://github.com/suffecool/pokerlib), rebuilt from the ground up for clarity, determinism, and speed.  
  
This version achieves **near-native C++ performance** through careful algorithmic refactoring, zero memory allocations, and extensive BenchmarkDotNet validation.

---

> ℹ️ **Looking for the earlier ASP.NET WebForms version?**  
> You can find the legacy implementation here:  
> 👉 **[JBelthoff/poker.johnbelthoff.com](https://github.com/JBelthoff/poker.johnbelthoff.com)**

---

A working version of this application is available at:  
👉 [https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)

---

**This repository showcases a fully optimized .NET 8 Poker Hand Evaluation Engine**, benchmarking **≈178 million 5-card evaluations per second** in **pure C#**, without lookup tables or unsafe code.  

Ideal for developers exploring **algorithmic optimization**, **combinatorial evaluation**, or **.NET performance engineering**.




## About the Project

This project re-creates the logic and structure of a full **Texas Hold’em Poker** game — from shuffling and dealing cards to evaluating hands and determining the winner.

The application is written in **ASP.NET Core (C#)** and uses a modern C# port of [Cactus Kev’s Poker Hand Evaluator](https://poker-calculator.johnbelthoff.com/cactus_kev), the classic C implementation that popularized prime-number-based hand ranking.

At this stage, the app:

- Simulates up to **9 players**
- **Rotates the dealer**
- **Calculates the winning hand**
- **Displays detailed results**

Under the hood, the evaluation engine performs about **≈ 178 million 5-card evaluations per second (single-threaded)** and scales up to **≈ 2.76 billion hands per second (multi-threaded)** —  placing it within **≈ 98 % of an AVX2-optimized C++ implementation** while preserving fully managed, allocation-free execution.

Future updates will continue refining gameplay and introduce interactive features such as hand histories, betting logic, and visualized probability analysis.


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


### Version Notes

Benchmarks were measured with **BenchmarkDotNet v0.15.4** on **.NET 8.0.21**, running **Windows 10 (22H2)** on an **Intel Core i9-9940X (14 cores / 28 threads)** under the *High Performance* power plan.

This release introduces extensive performance optimizations, including **Span-based memory reuse**, **tightened hot-path loops**, and **complete allocation elimination**, achieving throughput within **≈ 98 % of native C++ performance**.

C++ reference benchmarks were conducted using [**bwedding/PokerEvalMultiThread**](https://github.com/bwedding/PokerEvalMultiThread), an optimized multithreaded port of [**Suffecool’s original pokerlib**](https://github.com/suffecool/pokerlib).  
  
Both implementations produced identical deterministic checksums, confirming algorithmic parity.
  
An upgrade to **.NET 10** is planned upon its release.





---








### Legacy Baseline (Version 1)

| Benchmark                              | Mean (µs/op) | Alloc/op | Derived 5-card Evals/sec (≈) |
| -------------------------------------- | -----------: | -------: | ---------------------------: |
| **End-to-End (9 players • best-of-7)** |    10.922 µs |  8.26 KB |                  ≈ 20 M /sec |
| **Engine-only (7-card → best-of-21)**  |     1.643 µs |    888 B |                 ≈ 115 M /sec |

*Each full 9-player river evaluation involves 189 five-card combinations (9 × 21).*

---

### Current Release (Version 2) vs Legacy (Version 1)

| Benchmark                              | Mean (µs/op) |    Derived Evals/sec (≈) |        Δ vs V1 (%) | Notes                                     |
| -------------------------------------- | -----------: | -----------------------: | -----------------: | ----------------------------------------- |
| **End-to-End (EvalEngine)**            |     1.066 µs |   ≈ 177–178 M / sec      | **+925 % (×10.25)** | Major hot-path refactor, zero allocations |
| **Engine-only (7 → 21)**               |     1.376 µs |        ≈ 137 M / sec     |   **+19 % (×1.19)** | Loop unrolling + `Span<T>` reuse          |
| **FinalRiver (Parallel, values-only)** |            — |     **≈ 2.76 B / sec**   |          —          | Parallel throughput; not directly comparable to per-op means |

This release elevated the managed evaluator from hundreds of millions to **billions of evaluations per second**, effectively closing the gap with native C++.

---






### Current Version (C#) vs Native C++

> **Workload:** five-card evaluation throughput (values-only).  
> **Note:** One full 9-player river evaluation performs **189** five-card evaluations.

| Implementation                         | Environment (from logs)                                               | Five-card evals/sec (B) | % of C++ |
|---------------------------------------|------------------------------------------------------------------------|------------------------:|---------:|
| **Native C++ (bwedding / Suffecool)** | MSVC 19.44 /O2 + AVX2, 64-bit build (bwedding/PokerEvalMultiThread)   | **≈ 2.8227 B**          | 100 %    |
| **.NET 8 Optimized (Current)**        | .NET 8.0.21 • X64 RyuJIT • GC = Concurrent Workstation                 | **≈ 2.78 – 2.79 B**     | ≈ 98–99 % |

**Evidence**

- **C++** run: “Hands per second: 2822698675” → **2.8227 B hands/sec**.  
- **C#** BenchmarkDotNet: Mean = 678,110,150 ns for N = 10,000,000 9-player river evals (values-only).  
  - 10 M × 189 five-card evals ÷ 0.678110 s = **≈ 2.79 B five-card evals/sec**.  
- Environment block confirms “.NET 8.0.21 (64-bit) / RyuJIT / GC = Concurrent Workstation”.

Both implementations produced identical deterministic checksums, confirming algorithmic parity.  
The managed version performs within **statistical noise** of the native C++ evaluator.





---




### Historical Reference — Original C (Suffecool) — Single-Thread Baselines

| Benchmark                                 | Hands Tested | Time (s) | Hands/sec (M) | ns / hand | Comment |
|-------------------------------------------|-------------:|---------:|--------------:|----------:|---------|
| **5-Card Random (bench5.exe)**            |   10,000,000 |   0.278223 | **35.94**     | 27.822 ns | Original C evaluator (single-thread) |
| **7-Card Random (bench7.exe)**            |   10,000,000 |   1.179530 | **8.48**      | 117.953 ns | Original C evaluator (single-thread) |
| **All 5-Card Combos (allfive.exe)**       |    2,598,960 |   0.0130   | **199.92**    | 5.002 ns  | Full enumeration of all 2,598,960 5-card hands |

> Source: `x_Benchmark/ResultsAll.txt` — single-thread C baselines using **original Suffecool pokerlib** (https://github.com/suffecool/pokerlib).






---

### Summary

- **V1 → V2 (C# only, same workload):** ≈ **10.25×** end-to-end speed-up (**+925%**, −90.24% mean time) with **zero GC allocations**.
- **Managed vs Native (same workload):** Modern C# achieves **≈98–99%** of C++ throughput on the **five-card values-only** test.
- **Legacy baselines (single-thread C):** 
  - **5-card random:** ≈ **35.9 M** hands/sec  
  - **7-card random:** ≈ **8.48 M** hands/sec
- **Modern C# throughput (current):**
  - **Derived five-card (single-thread):** ≈ **177–178 M** evals/sec (from end-to-end 9-player)  
  - **Five-card (parallel, values-only):** ≈ **2.76–2.79 B** evals/sec

























---


### 📚 Algorithm Lineage and Faithfulness

This evaluator is a modern **C# translation** of [Cactus Kev’s Poker Hand Evaluator](https://github.com/suffecool/pokerlib) —  
the classic C implementation that popularized **prime-number-based hand evaluation**.

All core logic is faithfully preserved:
- Flush and straight detection tables  
- Perfect-hash prime product encoding  
- Rank thresholds identical to Kev’s original  

Where Kev used C arrays, macros, and pointer arithmetic, **Poker.net** employs managed data structures,  
`Span<T>` buffers, and modern .NET 8 JIT optimizations to achieve equivalent throughput —  
**without huge in-memory lookup tables or unsafe code**.

Comprehensive validation confirms **one-to-one rank and frequency equivalence** with the original algorithm,  
and checksum tests verify that every evaluated hand produces identical results to the C reference.





---

## 🧪 Running Benchmarks Locally

To reproduce the same performance results using **BenchmarkDotNet**, follow these steps:

1. **Add a new project** to your existing solution:  
   - Right-click the solution → **Add → New Project** → select **C# Console App**  
   - Name it **PokerBenchmarks**  
   - ⚠️ **Important:** Make sure it is **not inside** the `poker.net` directory.

2. **Install BenchmarkDotNet**  
   - Right-click the new project → **Manage NuGet Packages**  
   - Search for `BenchmarkDotNet` and install the latest version.

3. **Replace `Program.cs`**  
   - Delete the default `Program.cs` created by Visual Studio.  
   - Right-click **PokerBenchmarks** → **Add → Existing Item**  
   - Add the two files from `poker.net/x_Benchmark` and confirm overwrites:  
     - `FinalRiverBench.cs`  
     - `Program.cs`

4. **Add a Project Reference**  
   - Right-click **PokerBenchmarks** → **Add → Project Reference**  
   - Select the `poker.net` project.

5. **Set as Startup Project**  
   - Right-click **PokerBenchmarks** → **Set as Startup Project**.

6. **Build and Run in Release Mode**  
   - Use the **Release** configuration (not Debug).  
   - Run the project to execute the benchmarks.  

**BenchmarkDotNet will generate reports in:**  

```
bin/Release/net8.0/BenchmarkDotNet.Artifacts/results/
```

**You’ll find CSV, Markdown (.md), and HTML output files such as:**  

```
PokerBenchmarks.FinalRiverBench-report.csv
PokerBenchmarks.FinalRiverBench-report-github.md
PokerBenchmarks.FinalRiverBench-report.html
```

---

### 📁 Benchmark Files

Benchmark source files are located here:
- `x_Benchmark/FinalRiverBench.cs`
- `x_Benchmark/FiveCardBench.cs`
- `x_Benchmark/Program.cs`

Benchmark results files are here:
- `x_Benchmark/ResultsAll.txt`
- `x_Benchmark/BenchmarkDotNet.Artifacts`

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
