using BenchmarkDotNet.Running; 

namespace PokerBenchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<FiveCardBench>();
            //BenchmarkRunner.Run(
            //    new[] { typeof(FinalRiverBench), typeof(FiveCardBench) }
            //);
        }
    }
}
