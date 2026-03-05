using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VenusRootLoader.Api;
using VenusRootLoader.BudLoading;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using VenusRootLoader.Unity.CustomAudioClip;

namespace VenusRootLoader.Tests.BudLoading;

public sealed class VenusFactoryTests
{
    private readonly IGlobalMonoBehaviourExecution _globalMonoBehaviourExecution =
        Substitute.For<IGlobalMonoBehaviourExecution>();

    private readonly ICustomAudioClipProvider _customAudioClipProvider =
        Substitute.For<ICustomAudioClipProvider>();

    private readonly IRegistryResolver _registryResolver = Substitute.For<IRegistryResolver>();
    private readonly ILogger<Venus> _logger = Substitute.For<ILogger<Venus>>();

    private readonly VenusFactory _sut;

    public VenusFactoryTests() => _sut = new(
        _registryResolver,
        _globalMonoBehaviourExecution,
        _customAudioClipProvider,
        _logger);

    [Fact]
    public void CreateVenusForBud_CreatesVenusWithCorrectConfiguration_WhenCalled()
    {
        string budId = "SomeBudId";

        Venus venus = _sut.CreateVenusForBud(budId);

        venus.BudId.Should().Be(budId);
        venus.GlobalMonoBehaviourExecution.Should().Be(_globalMonoBehaviourExecution);
        venus.RegistryResolver.Should().Be(_registryResolver);
        venus.Logger.Should().Be(_logger);
    }
}