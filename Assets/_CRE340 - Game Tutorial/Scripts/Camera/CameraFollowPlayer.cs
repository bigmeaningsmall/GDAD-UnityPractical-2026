using UnityEngine;
using Unity.Cinemachine;

// Points the Cinemachine camera at the player.
// The player is spawned at runtime by the GameManager, so there's nothing to
// follow when the scene loads. We pick it up as soon as one exists, and again
// after a respawn.
public class CameraFollowPlayer : MonoBehaviour
{
    public CinemachineCamera cam;

    private void Awake()
    {
        if (cam == null)
        {
            cam = GetComponent<CinemachineCamera>();
        }
    }

    private void Update()
    {
        // Already following something that still exists - nothing to do
        if (cam.Follow != null)
        {
            return;
        }

        // Ask the GameManager. It gives us null until a player exists.
        if (GameManager.Instance.Player != null)
        {
            cam.Follow = GameManager.Instance.Player.transform;
        }
    }
}