# Poker Hand Evaluator (.NET Core Version) | Version 2 - Improved!

> High-performance .NET 8 poker hand evaluator and calculator built with ASP.NET Core Razor Pages.


[![Live Demo](https://img.shields.io/badge/demo-live-brightgreen)](https://poker-calculator.johnbelthoff.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)


[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Developer-blue?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-5C2D91?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/sql/sql-server/)
[![Docker](https://img.shields.io/badge/Containerized-Docker-blue?logo=docker)](https://www.docker.com/)


A modern **ASP.NET Core (Razor Pages)** web app that evaluates **Texas Hold’em poker hands** based on [**Cactus Kev’s Poker Hand Evaluator**](https://github.com/suffecool/pokerlib), completely re-engineered for **.NET 8 performance**.

---

> ℹ️ **Looking for the earlier ASP.NET WebForms version?**  
> You can find the legacy implementation here:  
> 👉 **[JBelthoff/poker.johnbelthoff.com](https://github.com/JBelthoff/poker.johnbelthoff.com)**

---

A working version of this application is available at:  
👉 [https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)

---

**This repository showcases a fully optimized .NET 8 Poker Hand Evaluation Engine.**  
  
It benchmarks **≈ 178 million 5-card evaluations per second** using **BenchmarkDotNet**, written entirely in **pure C#** without lookup tables or unsafe code.  
  
Ideal for developers studying **algorithmic optimization**, **combinatorial evaluation**, or **.NET performance engineering**.




## About the Project

This project re-creates the logic and structure of a full **Texas Hold’em Poker** game —  
from shuffling and dealing cards to evaluating hands and determining the winner.

The application is written in **ASP.NET Core (C#)** and uses a modern C# port of  
[Cactus Kev’s Poker Hand Evaluator](https://poker-calculator.johnbelthoff.com/cactus_kev), the classic C implementation that popularized prime-number-based hand ranking.

At this stage, the app:

- Simulates up to **9 players**
- **Rotates the dealer**
- **Calculates the winning hand**
- **Displays detailed results**

Under the hood, the evaluation engine performs about **≈ 178 million 5-card evaluations per second (single-threaded)**  
and scales up to **≈ 2.76 billion hands per second (multi-threaded)** —  
placing it within **≈ 98 % of an AVX2-optimized C++ implementation** while preserving fully managed, allocation-free execution.

Future updates will continue refining gameplay and introduce interactive features such as  
hand histories, betting logic, and visualized probability analysis.







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

Benchmarks were measured with **BenchmarkDotNet v0.15.4** on **.NET 8.0.21**,
running **Windows 10 (22H2)** on an **Intel Core i9-9940X (14 cores / 28 threads)**
under the *High Performance* power plan.

Both **.NET 8** and **.NET 9** produce statistically identical results,
confirming consistent JIT and runtime behavior across LTS and preview builds.

The **Optimization Branch (Version 2)** introduced Span-based memory reuse, tighter hot-path loops,
and complete allocation elimination, achieving performance within **≈ 98 % of native C++**.
An upgrade to **.NET 10** is planned upon its release.

---

### Master Branch (Version 1) — Raw Numbers

| Benchmark                              | Mean (µs/op) | Alloc/op | Derived 5-card Evals/sec (≈) |
| -------------------------------------- | -----------: | -------: | ---------------------------: |
| **End-to-End (9 players • best-of-7)** |    10.922 µs |  8.26 KB |                  ≈ 20 M /sec |
| **Engine-only (7-card → best-of-21)**  |     1.643 µs |    888 B |                 ≈ 115 M /sec |

*Each full 9-player river evaluation involves 189 five-card combinations (9 × 21).*

---

### Optimization Branch (Version 2) vs Master (Version 1)

| Benchmark                              | Mean (µs/op) |    Derived Evals/sec (≈) |          Δ vs Master (%) | Notes                                     |
| -------------------------------------- | -----------: | -----------------------: | -----------------------: | ----------------------------------------- |
| **End-to-End (EvalEngine)**            |     1.066 µs | ≈ 940 M 5-card evals/sec |        **+927 % faster** | Major hot-path refactor, zero allocations |
| **Engine-only (7 → 21)**               |     1.376 µs | ≈ 145 M 5-card evals/sec |         **+27 % faster** | Loop unrolling + `Span<T>` reuse          |
| **FinalRiver (Parallel, values-only)** |            — |   **≈ 2.76 B hands/sec** | **≈ 13× faster overall** | Flattened Perm7, allocation-free          |

The Optimization Branch brought the managed engine from hundreds of millions
to **billions of evaluations per second**, closing the gap with native C.

---

### Optimization Branch (Version 2) vs Native C++

| Implementation                        | Toolchain                    | Runtime (s) / 10 M hands | Hands/sec (B) | % of C Speed |
| ------------------------------------- | ---------------------------- | -----------------------: | ------------: | -----------: |
| **Native C++ (bwedding / Suffecool)** | MSVC 19.44 / O2 + AVX2       |                 ≈ 1.25 s |        2.80 B |        100 % |
| **.NET 8 Optimized (V2)**             | RyuJIT TieredPGO + Server GC |                 ≈ 1.27 s |        2.76 B |   **≈ 98 %** |

Both implementations produced identical deterministic checksums,
confirming algorithmic parity. The managed version now performs
within statistical noise of C++ speed.  
  
> **Note:** Results reflect this specific compute-bound algorithm (poker hand evaluation)  
> under identical logic and workload; not a general .NET vs C++ comparison.


---

### Historical Reference — Original Cactus Kev

| Implementation               | Era / Toolchain           |             Evals/sec (M) | Comment                                                      |
| ---------------------------- | ------------------------- | ------------------------: | ------------------------------------------------------------ |
| **Cactus Kev’s C Evaluator** | 2000s C (VB6 / gcc -O2) | ≈ 0.12 M 7-card evals/sec | Prime-product hash logic on which all modern ports are based |

---

### Summary

* **V1 → V2:** Over 9× end-to-end speed-up with zero GC allocations  
* **Managed vs Native:** Modern C# achieves ≈ 98 % of C++ throughput on identical workloads  
* **Legacy → Modern:** Performance rose from ≈ 0.12 M to ≈ 2.76 B evaluations per second since Cactus Kev’s original























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
