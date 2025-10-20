> ℹ️ **Looking for the earlier ASP.NET WebForms version?**  
> You can find the legacy implementation here:  
> 👉 **[JBelthoff/poker.johnbelthoff.com](https://github.com/JBelthoff/poker.johnbelthoff.com)**

 # Poker Hand Evaluator

A working version of this application is now located at:  
[https://poker-calculator.johnbelthoff.com/](https://poker-calculator.johnbelthoff.com/)

## About John Belthoff’s Texas Hold’em & Poker Hand Evaluator

This project began as a personal challenge to re-create the core logic behind a Texas Hold’em Poker game — from dealing and hand evaluation to determining the winner.

The application is written in **ASP.NET Core (C#)** and relies on a **C# port of Cactus Kev’s Poker Hand Evaluator**, originally written in C++.  
Although Cactus Kev’s original webpage is no longer online, I’ve included a [copy of his article](https://poker-calculator.johnbelthoff.com/cactus_kev) for reference so you can follow along with his logic and mathematical approach.

At this stage, the program:

- Simulates up to **9 players**
- **Rotates the dealer**
- **Calculates the winning hand**
- **Displays detailed results**

There’s still room to expand the functionality into a complete, fully interactive poker experience — but the essential framework and logic are already in place.

If you have any questions, feel free to reach out.  
Otherwise, enjoy exploring the source and trying the live demo!

---

## Instructions for Local Setup

1. Create a **SQL Server** database named `PokerApp`.
2. Create a **Login** and **User** for the database.
3. Run the script **`CreateDB.sql`** (located in the `x_dBase` directory) against the `PokerApp` database.
4. Deploy the project as an **ASP.NET Core web application** using either IIS or Docker.
5. Update your connection string (via `appsettings.json` or environment variables) with your database credentials.
6. Build and run the app.
7. Open the site in your browser and start playing!

---

© 2025 John Belthoff  
[www.johnbelthoff.com](https://www.johnbelthoff.com/)
