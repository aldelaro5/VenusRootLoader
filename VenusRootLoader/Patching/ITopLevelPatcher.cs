namespace VenusRootLoader.Patching;

/// <summary>
/// A patcher that runs independently of each other and will take effect early during boot.
/// </summary>
internal interface ITopLevelPatcher
{
    /// <summary>
    /// Executes the patching process of the patcher.
    /// </summary>
    void Patch();
}