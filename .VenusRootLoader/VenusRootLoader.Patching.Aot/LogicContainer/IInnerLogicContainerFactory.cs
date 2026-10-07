namespace VenusRootLoader.Patching.Aot.LogicContainer;

public interface IInnerLogicContainerFactory<in TOuter, out TInner>
    where TOuter : ILogicContainer
    where TInner : ILogicContainer
{
    TInner Create(
        TOuter outerContainer,
        GameModuleData gameModuleData,
        List<NamedParameter> parameters,
        string name);
}