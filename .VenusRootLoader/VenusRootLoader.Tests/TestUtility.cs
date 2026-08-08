using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Tests;

public static class TestUtility
{
    public static void AssertErrorLogs(
        FakeLogger logger,
        int expectedErrorLogsAmount,
        string expectedExceptionMessageTemplate = "*")
    {
        using AssertionScope scope = new();
        List<FakeLogRecord> errorLogs = logger.Collector.GetSnapshot().Where(l => l.Level == LogLevel.Error).ToList();
        errorLogs.Should().HaveCount(expectedErrorLogsAmount);
        if (!scope.HasFailures())
            return;

        foreach (FakeLogRecord log in errorLogs)
        {
            TestContext.Current.TestOutputHelper!.WriteLine(log.Message);
            if (log.Exception is null)
                continue;

            log.Exception.Message.Should().MatchEquivalentOf(expectedExceptionMessageTemplate);
            TestContext.Current.TestOutputHelper!.WriteLine(log.Exception.ToString());
        }
    }

    internal static void MockRegistry<TLeaf>(
        ILeavesRegistry<TLeaf> registry,
        List<TLeaf> leaves)
        where TLeaf : Leaf
    {
        foreach (TLeaf leaf in leaves)
        {
            registry.GetByGameId(leaf.GameId).Returns(leaf);
            registry.GetByEffectiveId(leaf.EffectiveId).Returns(leaf);
            registry.Get(leaf.CreatorId, leaf.NamedId).Returns(leaf);
            registry.TryGet(leaf.CreatorId, leaf.NamedId, out Arg.Any<TLeaf?>()).Returns(x =>
            {
                x[2] = leaf;
                return true;
            });
            registry.TryGetByEffectiveId(leaf.EffectiveId, out Arg.Any<TLeaf?>()).Returns(x =>
            {
                x[1] = leaf;
                return true;
            });
        }

        registry.Count.Returns(leaves.Count);
        registry.CountBaseGame.Returns(leaves.Count(x => x.CreatorId == Constants.BaseGameCreatorId));
        // ReSharper disable once GenericEnumeratorNotDisposed
        registry.GetEnumerator().Returns(_ => leaves.GetEnumerator());
        registry.GetAll().Returns(leaves.ToList());
    }
}