using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.Logic;
using VenusRootLoader.Patching.Aot.Logic.Context;

namespace VenusRootLoader.Patching.Aot.ContextSource;

/// <summary>
/// Represents a source of value for a <see cref="LogicContextField"/>.
/// </summary>
public interface IContextFieldSource
{
    /// <summary>
    /// Obtains the IL that places the value of the context field on the stack.
    /// </summary>
    /// <returns>The IL placing the value of the context field on the stack.</returns>
    List<CilInstruction> GetLoadIl();

    /// <summary>
    /// Obtains the IL that stores the value on the stack into the context field.
    /// </summary>
    /// <returns>The IL storing the value on the stack into the context field.</returns>
    List<CilInstruction> GetStoreIl();

    /// <summary>
    /// An object that uniquely identifies a specific source among the ones in a context.
    /// </summary>
    object Key { get; }

    /// <summary>
    /// The type signature of this context field.
    /// </summary>
    TypeSignature TypeSignature { get; }
}