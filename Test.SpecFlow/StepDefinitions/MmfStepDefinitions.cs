using System.IO.MemoryMappedFiles;
using System.Numerics;

namespace Test.SpecFlow.StepDefinitions
{
    [Binding]
    public class MmfStepDefinitions : StepDefinitions
    {
        public MmfStepDefinitions(ScenarioContext scenarioContext) : base(scenarioContext)
        {
        }

        protected string MmfFilename { get; } = $"{Guid.NewGuid():N}";

        protected int Capacity { get; set; } = 64 * 1024;

        protected MemoryMappedFile Subject { get; set; }

        protected StubData SubjectStubData { get; set; }

        [AfterScenario]
        public void AfterScenario()
        {
            Subject?
                .Dispose();
            File.Delete(MmfFilename);
        }

        [Given(@"a persistent MMF(?: with capacity (\d+))?")]
        public void GivenAPersistentMMF(int? capacity)
        {
            if (capacity.HasValue)
                Capacity = capacity.Value;
            var mmf = MemoryMappedFile.CreateFromFile(MmfFilename, FileMode.CreateNew, MmfFilename, Capacity);
            Subject = mmf;
        }

        [Given(@"a non-persistent MMF(?: with capacity (\d+))?")]
        public void GivenANon_PersistentMMF(int? capacity)
        {
            if (capacity.HasValue)
                Capacity = capacity.Value;
            var mmf = MemoryMappedFile.CreateNew(MmfFilename, Capacity);
            Subject = mmf;
        }

        [When(@"write string ""(.+)"" to the MMF")]
        public void WhenWriteStringToTheMMF(string literal)
        {
            using (var stream = Subject.CreateViewStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(literal);
            }
        }

        [Then(@"read from the MMF should be string ""(.+)""")]
        public void ThenReadFromTheMMFShouldBeString(string literal)
        {
            using (var stream = Subject.CreateViewStream())
            using (var reader = new StreamReader(stream))
            {
                reader
                    .ReadLine()
                    .Should()
                    .BeEquivalentTo(literal);
            }
        }

        [When(@"reopen the MMF with double capacity")]
        public void WhenReopenTheMMFWithDoubleCapacity()
        {
            Subject.Dispose();
            var mmf = MemoryMappedFile.CreateFromFile(MmfFilename, FileMode.Open, MmfFilename, Capacity * 2);
            Subject = mmf;
        }

        [When(@"write integer (\d+) to the MMF")]
        public void WhenWriteIntegerToTheMMF(int integer)
        {
            using (var accessor = Subject.CreateViewAccessor())
            {
                accessor
                    .Write(0, integer);
            }
        }

        [Then(@"read integer from the MMF should be (\d+)")]
        public void ThenReadIntegerFromTheMMFShouldBe(int integer)
        {
            using (var accessor = Subject.CreateViewAccessor())
            {
                accessor
                    .ReadInt32(0)
                    .Should()
                    .Be(integer);
            }
        }

        [When(@"write integer (\d+) to the position (\d+) from the begin of MMF")]
        public void WhenWriteIntegerToThePositionFromTheBeginOfMMF(int integer, int position)
        {
            using (var accessor = Subject.CreateViewAccessor(position, sizeof(int)))
            {
                accessor
                    .Write(0, integer);
            }
        }

        [Then(@"read from the position (\d+) from the begin of MMF should be (\d+)")]
        public void ThenReadFromThePositionFromTheBeginOfMMFShouldBe(int position, int integer)
        {
            using (var accessor = Subject.CreateViewAccessor(position, sizeof(int)))
            {
                accessor
                    .ReadInt32(0)
                    .Should()
                    .Be(integer);
            }
        }

        [Then(@"should occure an exception when create a random access view at position (\d+) and length (\d+)")]
        public void ThenShouldOccureAnExceptionWhenCreateARandomAccessViewAtPosition(int position, int length)
        {
            AssertionExtensions
                .Should(() => Subject.CreateViewAccessor(position, length))
                .Throw<Exception>();
        }

        public struct StubData
        {
            private static Random RandomDevice { get; } = new();

            public int integer { get; } = RandomDevice.Next();

            public float FloatNumber { get; } = RandomDevice.NextSingle();

            public double DoubleNumber { get; } = RandomDevice.NextDouble();

            public byte Byte { get; } = (byte)RandomDevice.Next(255);

            public bool Boolean { get; } = RandomDevice.Next(1) == 0;

            public char Char { get; } = (char)RandomDevice.Next(65535);
        }

        [When(@"write a user-defined object to MMF")]
        public void WhenWriteAUser_DefinedObjectToMMF()
        {
            var data = new StubData();
            using (var accessor = Subject.CreateViewAccessor())
            {
                accessor
                    .Write(0, ref data);
            }
            SubjectStubData = data;
        }

        [Then(@"read a user-defined object should be equalled")]
        public void ThenReadAUser_DefinedObjectShouldBeEqualled()
        {
            using (var accessor = Subject.CreateViewAccessor())
            {
                accessor
                    .Read<StubData>(0, out var data);
                data
                    .Should()
                    .BeEquivalentTo(SubjectStubData);
            }
        }

        public struct StubDataContainReference
        {
            public string Literal { get; } = $"{Guid.NewGuid():N}";
        }

        [Then(@"should occure an exception when write an object to MMF which contain referenece type")]
        public void ThenShouldOccureAnExceptionWhenWriteAnObjectToMMFWhichContainRefereneceType()
        {
            var data = new StubDataContainReference();
            using (var accessor = Subject.CreateViewAccessor())
            {
                AssertionExtensions
                    .Should(() => accessor.Write(0, ref data))
                    .ThrowExactly<ArgumentException>()
                    .WithMessage("The specified Type must be a struct containing no references.");
            }
        }
    }
}
