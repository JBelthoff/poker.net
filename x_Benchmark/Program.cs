using BenchmarkDotNet.Running; 

namespace PokerBenchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Run the full suite so you can publish both numbers in README:
            // - EndToEnd_FullFinalCalculation_9Players   (refactored DoRiver flow)
            // - EngineOnly_SevenCardBestOf21_9Players    (apples-to-apples vs LUT libraries)
            BenchmarkRunner.Run<FinalRiverBench>();
        }
    }
}
