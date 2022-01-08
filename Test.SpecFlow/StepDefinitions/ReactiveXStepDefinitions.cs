using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Test.SpecFlow.StepDefinitions
{
    [Binding]
    public class ReactiveXStepDefinitions : StepDefinitions
    {
        public ReactiveXStepDefinitions(ScenarioContext scenarioContext) :
            base(scenarioContext)
        {
        }

        protected ICollection<IObservable<object>> Observables { get; } = new List<IObservable<object>>();

        protected ICollection<Subject<object>> Observers { get; } = new List<Subject<object>>();

        protected List<IConnectableObservable<object>> ConnectableObservables { get; } = new();

        protected IObservable<object> DefaultObservable
        {
            get => ScenarioContext.Get<IObservable<object>>();
            set
            {
                ScenarioContext.Set(value);
                Observables.Add(value);
            }
        }

        protected Subject<object> DefaultObserver
        {
            get => ScenarioContext.Get<Subject<object>>();
            set
            {
                ScenarioContext.Set(value);
                Observers.Add(value);
            }
        }

        [Given("an observable emmits data in (.+) seconds?")]
        public void GivenThePeriodObservable(string periodName)
        {
            var periodTable = new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase)
            {
                { "every", 1.Seconds() },
            };
            var period = periodTable[periodName];
            var observable = Observable
                .Interval(period)
                .Select(x => (object)x);
            DefaultObservable = observable;
        }

        [Given(@"an observable will emmit (\d+) publications?")]
        public void GivenTheObservableWillEmmitPublications(int numberOfPublications)
        {
            var observable = Observable
                .Range(0, numberOfPublications)
                .Select(x =>
                {
                    Logger.Information("emmiting {Item}", x);
                    return (object)x;
                });
            DefaultObservable = observable
                .Publish();
        }

        [When("subscribe the observable(?: again)?")]
        public void WhenSubscribeTheObservable()
        {
            var subject = new Subject<object>();
            DefaultObservable
                .Subscribe(subject);
            var observer = subject
                .Replay();
            observer
                .Connect();
            ConnectableObservables.Add(observer);
            subject
                .Subscribe(x =>
                {
                    Logger.Information("receive {Item}", x);
                });
            DefaultObserver = subject;
        }

        [Then(@"the (.+) observers should have same publications(?: of (\d+))?")]
        public void ThenTheSpecificObserversShouldHaveSamePublications(string typeOfObservers, int? numberOfPublications)
        {
            DefaultObservable
                .As<IConnectableObservable<object>>()
                .Connect();
            var observerTable = new Dictionary<string, Action>
            {
                {
                    "last two",
                    () =>
                    {
                        ConnectableObservables
                            .Should()
                            .HaveCountGreaterThanOrEqualTo(2);
                        var x = ConnectableObservables
                            .SkipLast(1)
                            .Last()
                            .ToEnumerable();
                        var y = ConnectableObservables
                            .Last()
                            .ToEnumerable();
                        x
                            .Should()
                            .BeEquivalentTo(y);
                        if (numberOfPublications.HasValue)
                        {
                            x
                                .Should()
                                .HaveCount(numberOfPublications.Value);
                        }
                    }
                }
            };
            observerTable
                .Should()
                .ContainKey(typeOfObservers)
                .And
                .Subject[typeOfObservers]();
        }
    }
}