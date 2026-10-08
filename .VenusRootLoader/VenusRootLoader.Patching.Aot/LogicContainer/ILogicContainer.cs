using AsmResolver.DotNet;
using VenusRootLoader.Patching.Aot.LogicExtraction;

namespace VenusRootLoader.Patching.Aot.LogicContainer;

/// <summary>
/// A construct that allows to contain logic in a method which may or may not receive a context to be initialized and
/// commited by an outer container. Containers can have part of them extracted via an <see cref="LogicExtractor{TOuter,TInner}"/>.
/// </summary>
public interface ILogicContainer
{
    /// <summary>
    /// The method that contains the logic.
    /// </summary>
    MethodDefinition ReceivingMethod { get; }

    /// <summary>
    /// Obtains a <see cref="NamedParameter"/> that represents the context of this container.
    /// </summary>
    /// <returns>A <see cref="NamedParameter"/> that represents the context of this container or null if this container
    /// has no context assigned to</returns>
    NamedParameter? GetContextParameter();
}