using UnityEngine;

/// <summary>
/// Manages the organization of scene elements, specifically enemies and bullets, within the game.
/// </summary>
/// <remarks>This class initializes and maintains static references to the root transforms for enemies and
/// bullets, allowing for centralized management of these game objects within the scene.</remarks>

public class SceneOrganizer : MonoBehaviour
{
    public static Transform enemyRoot;
    public static Transform bulletRoot;

    void Awake()
    {
        enemyRoot = new GameObject("Enemies").transform;
        bulletRoot = new GameObject("Bullets").transform;
    }
}
