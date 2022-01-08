namespace Test.SpecFlow.StepDefinitions
{
    public abstract class StepDefinitions
    {
        protected ScenarioContext ScenarioContext { get; }

        protected ILogger Logger => Extensions.Logger;

        protected StepDefinitions(ScenarioContext scenarioContext)
        {
            ScenarioContext = scenarioContext;
        }
    }
}
