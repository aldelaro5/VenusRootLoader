using AsmResolver.DotNet;

namespace VenusRootLoader.Patching.Aot.LogicContainer;

public interface ILogicContainer
{
    MethodDefinition ReceivingMethod { get; }
}