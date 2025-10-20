# Poker Hand Evaluator (.NET Core Version)

> ℹ️ **Looking for the earlier ASP.NET WebForms version?**  
> You can find the legacy implementation here:  
> 👉 **[JBelthoff/poker.johnbelthoff.com](https://github.com/JBelthoff/poker.johnbelthoff.com)**

---

A working version of this application is available at:  
👉 [https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)

## About John Belthoff’s Texas Hold’em & Poker Hand Evaluator

This project re-creates the logic and structure of a Texas Hold’em Poker game — from shuffling and dealing cards to evaluating hands and determining the winner.

The application is written in **ASP.NET Core (C#)** and uses a **C# port of Cactus Kev’s Poker Hand Evaluator**, originally developed in C++.  
Although Cactus Kev’s original site is no longer online, a [copy of his article](https://poker-calculator.johnbelthoff.com/cactus_kev) is available on the live site for reference.

At this stage, the app:

- Simulates up to **9 players**
- **Rotates the dealer**
- **Calculates the winning hand**
- **Displays detailed results**

Future updates will continue refining gameplay and add more interactive features.

If you have questions or suggestions, feel free to reach out — otherwise, enjoy exploring the source and the live demo!

---

## Instructions for Local Setup

1. Create a **SQL Server** database named `PokerApp`.
2. Create a **Login** and **User** for the database.
3. Run the script **`CreateDB.sql`** (in the `x_dBase` directory) against the `PokerApp` database.
4. Update your connection string (via `appsettings.json`, User Secrets, or environment variables).
5. Build and run the project (IIS Express, Docker, or `dotnet run`).
6. Visit the app in your browser and start playing!

---

© 2025 John Belthoff  
[www.johnbelthoff.com](https://www.johnbelthoff.com/)
