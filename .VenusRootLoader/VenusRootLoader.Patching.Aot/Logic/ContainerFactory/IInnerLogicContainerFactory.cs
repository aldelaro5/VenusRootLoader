namespace VenusRootLoader.Patching.Aot.Logic.ContainerFactory;

/// <summary>
/// Allows to create instances of <see cref="LogicContainer"/> that are made to be the inner part of an outer
/// <see cref="LogicContainer"/>.
/// </summary>
/// <typeparam name="TOuter">The outer container type.</typeparam>
/// <typeparam name="TInner">The inner container type.</typeparam>
public interface IInnerLogicContainerFactory<in TOuter, out TInner>
    where TOuter : LogicContainer
    where TInner : LogicContainer
{
    /// <summary>
    /// Creates an inner container given an outer one.
    /// </summary>
    /// <param name="outerContainer">The outer container.</param>
    /// <param name="gameModuleData">The game module data to use for creating the inner container.</param>
    /// <param name="parameters">The parameters the inner container will have.</param>
    /// <param name="name">The name the inner container will have.</param>
    /// <returns>An inner container made to be part of the <paramref name="outerContainer"/></returns>
    TInner Create(
        TOuter outerContainer,
        GameModuleData gameModuleData,
        List<NamedParameter> parameters,
        string name);
}