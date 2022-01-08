namespace Test.SpecFlow
{
    [Binding]
    public static class Extensions
    {
        [BeforeTestRun]
        public static void Initialization()
        {
            Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        public static ILogger Logger { get; private set; }
    }
}
