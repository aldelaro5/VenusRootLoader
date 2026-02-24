using VenusRootLoader.Unity;

namespace VenusRootLoader.LeavesInternals;

internal interface IEnemyPortraitSprite
{
    int? EnemyPortraitsSpriteIndex { get; set; }
    WrappedSprite WrappedSprite { get; set; }
}