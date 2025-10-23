# Poker Hand Evaluator (.NET Core Version)
> High-performance .NET 8 poker hand evaluator and calculator built with ASP.NET Core Razor Pages.


[![Live Demo](https://img.shields.io/badge/demo-live-brightgreen)](https://poker-calculator.johnbelthoff.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)


[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Developer-blue?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-5C2D91?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/sql/sql-server/)
[![Docker](https://img.shields.io/badge/Containerized-Docker-blue?logo=docker)](https://www.docker.com/)


A modern **ASP.NET Core (Razor Pages)** web app that evaluates **Texas Hold’em poker hands** using [**Cactus Kev’s Poker Hand Evaluator**](https://github.com/suffecool/pokerlib), re-engineered for **.NET 8 performance**.

---

> ℹ️ **Looking for the earlier ASP.NET WebForms version?**  
> You can find the legacy implementation here:  
> 👉 **[JBelthoff/poker.johnbelthoff.com](https://github.com/JBelthoff/poker.johnbelthoff.com)**

---

A working version of this application is available at:  
👉 [https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)

---

**This repository showcases a fully optimized .NET 8 Poker Hand Evaluation Engine.**  
  
It benchmarks over **100 million 7-card evaluations per second** using **BenchmarkDotNet**, written entirely in **pure C#** without lookup tables or unsafe code.  
  
Ideal for developers studying **algorithmic optimization**, **combinatorial evaluation**, or **.NET performance engineering**.


## About the Project 

This project re-creates the logic and structure of a Texas Hold’em Poker game — from shuffling and dealing cards to evaluating hands and determining the winner.

The application is written in **ASP.NET Core (C#)** and uses a **C# port of Cactus Kev’s Poker Hand Evaluator**, originally developed in C++.  
  
Although Cactus Kev’s original site is no longer online, a [copy of his article](https://poker-calculator.johnbelthoff.com/cactus_kev) is available on the live site for reference.

At this stage, the app:

- Simulates up to **9 players**
- **Rotates the dealer**
- **Calculates the winning hand**
- **Displays detailed results**

Future updates will continue refining gameplay and add more interactive features.

---

## ⚡ Performance

### Summary

Poker.net’s new `EvalEngine` was built from the ground up for **speed, minimal allocation, and clear architecture**.  
Benchmarks were run using **BenchmarkDotNet v0.15.4** on **.NET 8.0.21**, Windows 10 (22H2), and an **Intel Core i9-9940X** CPU.

Each full 9-player river evaluation involves **189 five-card combinations** (9 players × 21 combos each).

| Method | Mean (µs/op) | Alloc/op | Derived 5-card evals/sec* |
|:----------------------------------------------|--------------:|----------:|--------------------------:|
| **End-to-End (9 players • best-of-7)** | 9.574 | 6.2 KB | ≈ **20 million/sec** |
| **Engine-only (7-card → best-of-21)** | 1.645 | 0.9 KB | ≈ **115 million/sec** |

\*Derived = 189 ÷ mean seconds, where each 7-card hand is evaluated by testing all 21 possible 5-card combinations.  
This expresses throughput in the same unit (5-card evaluations per second) used by other poker evaluators.

---

### Raw Benchmark Output

| Method                                              | Mean      | Throughput      | Allocated |
|----------------------------------------------------|----------:|----------------:|----------:|
| Engine-only: 9 × (7-card → best-of-21)             | 1.645 µs  | ~607,903 ops/s  | ~888 B  |
| End-to-End: 9 players (best-of-7) winner (EvalEngine) | 9.574 µs  | ~104,450 ops/s  | ~6,208 B  |

- **Confidence interval (99.9%)** – Engine-only [1.643 ; 1.648] µs, End-to-End [9.549 ; 9.598] µs  
- A full 9-player river involves **189 five-card evaluations**; derived throughput ≈ **115 M** (engine-only) and **~20 M** (E2E) 5-card evals/sec.



---

### 📊 How It Compares

| Evaluator | Type | Cards / Eval | Reported Speed (C#) | Memory Usage | Notes |
|------------|------|--------------|--------------------:|--------------:|-------|
| **Poker.net (EvalEngine)** | Algorithmic (computed) | 5-card | **≈ 20 M evals/sec** | ~6 KB/op | Pure .NET 8, no lookup tables |
| **SnapCall** ([platatat/SnapCall](https://github.com/platatat/SnapCall)) | Lookup table | 7-card (precomputed) | **≈ 7.5 M lookups/sec** | ~2 GB | Constant-time lookups |
| **HenryRLee/PokerHandEvaluator** | Lookup table (C++) | 7-card | ≈ 10–15 M/sec | ~2 GB | Perfect-hash table |
| **OMPEval** (C++) | Algorithmic | 7-card | ≈ 35–40 M/sec | Low | Optimized native code |
| **Cactus Kev (C)** | Algorithmic | 5-card | 10–20 M/sec | negligible | Original native C version |

Unlike table-based evaluators such as **SnapCall** or **PokerHandEvaluator** — which load multi-gigabyte precomputed data into memory —  **Poker.net computes results dynamically** in real time, yet still meets or surpasses many lookup-based speeds while using almost no memory.

> 🔥 In other words: *The engine evaluates a full 7-card hand (selecting the best 5-card combination) approximately **115 million times per second** in pure C# — no table lookups, no unsafe code, no native dependencies.*

---

### 📚 Algorithm Lineage and Faithfulness

This evaluator is a modern C# translation of [Cactus Kev’s Poker Hand Evaluator](https://github.com/suffecool/pokerlib) — the classic C implementation that popularized prime-number-based hand evaluation.  
  
All core logic is preserved: flush and straight table lookups, perfect-hash prime products, and rank thresholds identical to Kev’s original.  
  
Where Kev used C arrays, macros, and pointer arithmetic, **Poker.net** employs managed data structures, `Span<T>` buffers, and .NET 8 JIT optimizations to reach equivalent throughput without unsafe code or lookup tables.  
  
Comprehensive validation confirms one-to-one rank and frequency equivalence with the original algorithm.  

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
- `x_Benchmark/Program.cs`

---

## Local Setup

1. Create a **SQL Server** database named `PokerApp`.
2. Create a **Login** and **User** for the database.
3. Run the script **`CreateDB.sql`** (in the `x_dBase` directory) against `PokerApp`.
4. Update your connection string (via `appsettings.json`, User Secrets, or environment variables).
5. Build and run the project (`dotnet run`, Docker, or IIS Express).
6. Visit the app in your browser and start playing!

---

© 2025 **John Belthoff**  
[www.johnbelthoff.com](https://www.johnbelthoff.com/)
