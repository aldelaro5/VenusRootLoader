using AsmResolver.DotNet;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace VenusRootLoader.Patching.Aot.LogicContainer;

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