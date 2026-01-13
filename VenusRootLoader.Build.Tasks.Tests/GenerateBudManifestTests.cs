using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Build.Framework;
using NSubstitute;
using System.Text.Json;
using VenusRootLoader.Build.Tasks.JsonConverters;

namespace VenusRootLoader.Build.Tasks.Tests;

public sealed class GenerateBudManifestTests
{
    private readonly IBuildEngine _buildEngine = Substitute.For<IBuildEngine>();

    [Fact]
    public void Execute_ThrowsException_WhenAssemblyPathHasNoParentDirectory()
    {
        GenerateBudManifest sut = new()
        {
            AssemblyFile = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "1.0.0",
            BudAuthor = "bud author",
            BudDependencies = [],
            BudIncompatibilities = []
        };
        sut.BuildEngine = _buildEngine;

        Func<bool> execute = () => sut.Execute();

        execute.Should().Throw<Exception>().WithMessage($"{sut.AssemblyFile} has no parent directory");
    }

    [Fact]
    public void Execute_ThrowsException_WhenBudVersionIsInvalid()
    {
        GenerateBudManifest sut = new()
        {
            AssemblyFile = "bud.dll",
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "some nonsense version",
            BudAuthor = "bud author",
            BudDependencies = [],
            BudIncompatibilities = []
        };
        sut.BuildEngine = _buildEngine;

        Func<bool> execute = () => sut.Execute();

        execute.Should().Throw<Exception>().WithMessage($"{sut.BudVersion} is not a valid version");
    }

    [Fact]
    public void Execute_ThrowsException_WhenBudHasADependencyWithItself()
    {
        ITaskItem dependencyItem = Substitute.For<ITaskItem>();
        dependencyItem.ItemSpec.Returns("budId");
        dependencyItem.MetadataNames.Returns(new List<string>([nameof(BudDependency.Version)]));
        const string dependencyVersionString = "1.0.0";
        dependencyItem.GetMetadata(nameof(BudDependency.Version)).Returns(dependencyVersionString);

        GenerateBudManifest sut = new()
        {
            AssemblyFile = "bud.dll",
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "1.0.0",
            BudAuthor = "bud author",
            BudDependencies = [dependencyItem],
            BudIncompatibilities = []
        };
        sut.BuildEngine = _buildEngine;

        Func<bool> execute = () => sut.Execute();

        execute.Should().Throw<Exception>().WithMessage($"The bud cannot have a dependency with itself");
    }

    [Fact]
    public void Execute_ThrowsException_WhenBudHasAnIncompatibilityWithItself()
    {
        ITaskItem incompatibilityItem = Substitute.For<ITaskItem>();
        incompatibilityItem.ItemSpec.Returns("budId");

        GenerateBudManifest sut = new()
        {
            AssemblyFile = "bud.dll",
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "1.0.0",
            BudAuthor = "bud author",
            BudDependencies = [],
            BudIncompatibilities = [incompatibilityItem]
        };
        sut.BuildEngine = _buildEngine;

        Func<bool> execute = () => sut.Execute();

        execute.Should().Throw<Exception>().WithMessage("The bud cannot have an incompatibility with itself");
    }

    [Fact]
    public void Execute_ThrowsException_WhenBudHasADependencyWithoutVersionRange()
    {
        ITaskItem dependencyItem = Substitute.For<ITaskItem>();
        dependencyItem.ItemSpec.Returns("SomeDependency");

        GenerateBudManifest sut = new()
        {
            AssemblyFile = "bud.dll",
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "1.0.0",
            BudAuthor = "bud author",
            BudDependencies = [dependencyItem],
            BudIncompatibilities = []
        };
        sut.BuildEngine = _buildEngine;

        Func<bool> execute = () => sut.Execute();

        execute.Should().Throw<Exception>().WithMessage(
            $"The dependency {dependencyItem.ItemSpec} does not have a version which is required");
    }

    [Fact]
    public void Execute_ThrowsException_WhenBudHasADependencyWithInvalidVersionRange()
    {
        ITaskItem dependencyItem = Substitute.For<ITaskItem>();
        dependencyItem.ItemSpec.Returns("SomeDependency");
        dependencyItem.MetadataNames.Returns(new List<string>([nameof(BudDependency.Version)]));
        const string invalidDependencyVersionString = "some nonsense version";
        dependencyItem.GetMetadata(nameof(BudDependency.Version)).Returns(invalidDependencyVersionString);

        GenerateBudManifest sut = new()
        {
            AssemblyFile = "bud.dll",
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "1.0.0",
            BudAuthor = "bud author",
            BudDependencies = [dependencyItem],
            BudIncompatibilities = []
        };
        sut.BuildEngine = _buildEngine;

        Func<bool> execute = () => sut.Execute();

        execute.Should().Throw<Exception>().WithMessage(
            $"The dependency {dependencyItem.ItemSpec} has an invalid version specified: {invalidDependencyVersionString}");
    }

    [Fact]
    public void Execute_ThrowsException_WhenBudHasAnIncompatibilityWithInvalidVersionRange()
    {
        ITaskItem incompatibilityItem = Substitute.For<ITaskItem>();
        incompatibilityItem.ItemSpec.Returns("SomeIncompatibility");
        incompatibilityItem.MetadataNames.Returns(new List<string>([nameof(BudDependency.Version)]));
        const string invalidDependencyVersionString = "some nonsense version";
        incompatibilityItem.GetMetadata(nameof(BudDependency.Version)).Returns(invalidDependencyVersionString);

        GenerateBudManifest sut = new()
        {
            AssemblyFile = "bud.dll",
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "1.0.0",
            BudAuthor = "bud author",
            BudDependencies = [],
            BudIncompatibilities = [incompatibilityItem]
        };
        sut.BuildEngine = _buildEngine;

        Func<bool> execute = () => sut.Execute();

        execute.Should().Throw<Exception>().WithMessage(
            $"The incompatibility {incompatibilityItem.ItemSpec} has an invalid version specified: {invalidDependencyVersionString}");
    }

    [Fact]
    public void Execute_GeneratesManifest_WhenMetadataAreValid()
    {
        ITaskItem dependencyItem = Substitute.For<ITaskItem>();
        string dependencyBudId = "SomeDependency";
        dependencyItem.ItemSpec.Returns(dependencyBudId);
        dependencyItem.MetadataNames.Returns(new List<string>([nameof(BudDependency.Version)]));
        const string invalidDependencyVersionString = "1.0.0";
        dependencyItem.GetMetadata(nameof(BudDependency.Version)).Returns(invalidDependencyVersionString);
        dependencyItem.GetMetadata(nameof(BudDependency.Optional)).Returns("true");

        ITaskItem incompatibilityItem1 = Substitute.For<ITaskItem>();
        string incompatibilityBudId1 = "SomeIncompatibility1";
        incompatibilityItem1.ItemSpec.Returns(incompatibilityBudId1);
        incompatibilityItem1.MetadataNames.Returns(new List<string>([nameof(BudDependency.Version)]));
        const string invalidIncompatibilityVersionString = "1.0.0";
        incompatibilityItem1.GetMetadata(nameof(BudDependency.Version)).Returns(invalidIncompatibilityVersionString);

        ITaskItem incompatibilityItem2 = Substitute.For<ITaskItem>();
        string incompatibilityBudId2 = "SomeIncompatibility2";
        incompatibilityItem2.ItemSpec.Returns(incompatibilityBudId2);

        string budAssembly = Path.Combine(Path.GetTempPath(), "bud.dll");
        GenerateBudManifest sut = new()
        {
            AssemblyFile = budAssembly,
            BudId = "budId",
            BudName = "bud name",
            BudVersion = "1.0.0",
            BudAuthor = "bud author",
            BudDependencies = [dependencyItem],
            BudIncompatibilities = [incompatibilityItem1, incompatibilityItem2]
        };
        sut.BuildEngine = _buildEngine;

        bool result = sut.Execute();

        result.Should().BeTrue();
        string directory = Path.GetDirectoryName(budAssembly)!;
        string manifestPath = Path.Combine(directory, "manifest.json");
        sut.BudManifestPath.Should().Be(manifestPath);
        File.Exists(manifestPath).Should().BeTrue();

        string json = File.ReadAllText(manifestPath);
        BudManifest manifest = JsonSerializer.Deserialize<BudManifest>(
            json,
            new JsonSerializerOptions
            {
                Converters =
                {
                    NuGetVersionJsonConverter.Instance,
                    NuGetVersionRangeJsonConverter.Instance
                }
            })!;
        manifest.Should().NotBeNull();

        using AssertionScope scope = new();
        manifest.AssemblyFile.Should().Be(Path.GetFileName(sut.AssemblyFile));
        manifest.BudId.Should().Be(sut.BudId);
        manifest.BudName.Should().Be(sut.BudName);
        manifest.BudVersion.ToNormalizedString().Should().Be("1.0.0");
        manifest.BudAuthor.Should().Be(sut.BudAuthor);
        manifest.BudDependencies.Should().HaveCount(sut.BudDependencies.Length);
        manifest.BudDependencies[0].BudId.Should().Be(dependencyBudId);
        manifest.BudDependencies[0].Optional.Should().Be(true);
        manifest.BudDependencies[0].Version.ToNormalizedString().Should().Be("[1.0.0, )");
        manifest.BudIncompatibilities.Should().HaveCount(sut.BudIncompatibilities.Length);
        manifest.BudIncompatibilities[0].BudId.Should().Be(incompatibilityBudId1);
        manifest.BudIncompatibilities[0].Version!.ToNormalizedString().Should().Be("[1.0.0, )");
        manifest.BudIncompatibilities[1].BudId.Should().Be(incompatibilityBudId2);
    }
}