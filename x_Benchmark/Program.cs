using BenchmarkDotNet.Running; 

namespace PokerBenchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<FinalRiverBench>();
            //BenchmarkRunner.Run<FiveCardBench>();
        }
    }
}
