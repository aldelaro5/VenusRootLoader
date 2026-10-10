using AsmResolver.DotNet;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using VenusRootLoader.Patching.Aot.LogicExtraction;

namespace VenusRootLoader.Patching.Aot.Logic.ContainerFactory;

/// <summary>
/// Allows to create an inner <see cref="MethodLogicContainer"/> from an outer <see cref="StateMachine"/>. It will
/// have an empty body as it is meant to be filled with a <see cref="MoveNextToMethodLogicExtractor"/>.
/// </summary>
public sealed class InnerMethodFromCoroutineFactory : IInnerLogicContainerFactory<StateMachine, MethodLogicContainer>
{
    public MethodLogicContainer Create(
        StateMachine outerContainer,
        GameModuleData gameModuleData,
        List<NamedParameter> parameters,
        string name)
    {
        bool isStatic = outerContainer.EnumeratorMethod.IsStatic;
        Func<TypeSignature, IEnumerable<TypeSignature>, MethodSignature> signatureCreator = isStatic
            ? MethodSignature.CreateStatic
            : MethodSignature.CreateInstance;
        MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;
        if (isStatic)
            methodAttributes |= MethodAttributes.Static;

        MethodDefinition methodDefinition = new(
            name,
            methodAttributes,
            signatureCreator(gameModuleData.Module.CorLibTypeFactory.Void, parameters.Select(x => x.TypeSignature)));
        methodDefinition.CilMethodBody = new();

        for (int i = 0; i < parameters.Count; i++)
        {
            Parameter contextParameter = methodDefinition.Parameters[i];
            contextParameter.GetOrCreateDefinition().Name = parameters[i].Name;
        }

        outerContainer.EnumeratorMethod.DeclaringType!.Methods.Add(methodDefinition);
        return new MethodLogicContainer(methodDefinition);
    }
}