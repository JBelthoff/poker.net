# Poker Hand Evaluator (.NET Core Version)

[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Developer-blue?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-5C2D91?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/sql/sql-server/)
[![Docker](https://img.shields.io/badge/Containerized-Docker-blue?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

A modern **ASP.NET Core (Razor Pages)** web app that evaluates **Texas Hold’em poker hands** using **Cactus Kev’s algorithm**, re-engineered for **.NET 8 performance**.

---

> ℹ️ **Looking for the earlier ASP.NET WebForms version?**  
> You can find the legacy implementation here:  
> 👉 **[JBelthoff/poker.johnbelthoff.com](https://github.com/JBelthoff/poker.johnbelthoff.com)**

---

A working version of this application is available at:  
👉 [https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)

---

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

## ⚡ Performance Optimized

Poker.net’s new `EvalEngine` was built from the ground up for **speed, minimal allocation, and clear architecture**.  
Benchmarks were run using **BenchmarkDotNet v0.15.4** on **.NET 8.0.21**, Windows 10 (22H2), and an **Intel Core i9-9940X** CPU.

Each full 9-player river evaluation involves **189 five-card combinations** (9 players × 21 combos each).

| Method | Mean (µs/op) | Alloc/op | Derived 5-card evals/sec* |
|:----------------------------------------------|--------------:|----------:|--------------------------:|
| **End-to-End (9 players • best-of-7)** | 9.435 | 6.1 KB | ≈ **20 million/sec** |
| **Engine-only (7-card → best-of-21)** | 1.643 | 0.9 KB | ≈ **115 million 5-card evals/sec** |

\* Derived = 189 ÷ mean seconds, where each 7-card hand is evaluated by testing all 21 possible 5-card combinations to find the best hand.  
This expresses throughput in the same unit (5-card evaluations per second) used by other poker evaluators.

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
