using VenusRootLoader.Patching.Resources.SpritesPatchers;
using VenusRootLoader.Unity;

namespace VenusRootLoader.LeavesInternals;

/// <summary>
/// A convenient way for <see cref="VenusRootLoader"/> to access the EnemyPortraits sprites information for concarned leaves.
/// This is mostly for <see cref="EnemyPortraitsSpriteArrayPatcher"/>.
/// </summary>
internal interface IEnemyPortraitSprite
{
    /// <summary>
    /// The sprite index of the sprite inside EnemyPortraits. A value of null means <see cref="EnemyPortraitsSpriteArrayPatcher"/> will
    /// automatically determine it.
    /// </summary>
    int? EnemyPortraitsSpriteIndex { get; set; }

    /// <summary>
    /// The sprite inside EnemyPortraits.
    /// </summary>
    WrappedSprite WrappedSprite { get; set; }
}