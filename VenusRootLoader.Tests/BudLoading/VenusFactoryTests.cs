using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VenusRootLoader.Api;
using VenusRootLoader.BudLoading;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Tests.BudLoading;

public sealed class VenusFactoryTests
{
    private readonly IGlobalMonoBehaviourExecution _globalMonoBehaviourExecution =
        Substitute.For<IGlobalMonoBehaviourExecution>();

    private readonly IRegistryResolver _registryResolver = Substitute.For<IRegistryResolver>();
    private readonly ILogger<Venus> _logger = Substitute.For<ILogger<Venus>>();

    private readonly VenusFactory _sut;

    public VenusFactoryTests() => _sut = new(_globalMonoBehaviourExecution, _registryResolver, _logger);

    [Fact]
    public void CreateVenusForBud_CreatesVenusWithCorrectConfiguration_WhenCalled()
    {
        string budId = "SomeBudId";

        Venus venus = _sut.CreateVenusForBud(budId);

        venus._budId.Should().Be(budId);
        venus._globalMonoBehaviourExecution.Should().Be(_globalMonoBehaviourExecution);
        venus._registryResolver.Should().Be(_registryResolver);
        venus._logger.Should().Be(_logger);
    }
}