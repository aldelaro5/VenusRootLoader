using UnityEngine;

namespace VenusRootLoader.Api.Unity;

/// <summary>
/// A <see cref="MonoBehaviour"/> that has special handling by <see cref="VenusRootLoader"/> such that it will always
/// have a higher priority than the game. The relative priority between all instances is determined by the buds' loading order.
/// </summary>
public abstract class GlobalMonoBehaviour : MonoBehaviour;