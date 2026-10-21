using UnityEngine;

public class RaycastPusher : MonoBehaviour
{
    public Transform rayOrigin;      // camera, or an empty at chest height on the player
    public float reach = 2f;
    public float thrust = 20f;
    public LayerMask pushableMask;   // put blocks on a "Pushable" layer - you can use this later to set up 

    Rigidbody target;
    Vector3 pushDir;

    void Update()
    {
        target = null;

        if (!Input.GetKey(KeyCode.Space)){
            return;
        }

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * reach, Color.red);

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, reach, pushableMask))
        {
            if (hit.rigidbody == null){
                return;
            }

            Vector3 dir = rayOrigin.forward;
            dir.y = 0f;                          // make the y zero so we don't push downwards
            if (dir.sqrMagnitude < 0.0001f)
            {
                return; // looking straight down/up - read online this can be an issue
            }

            target = hit.rigidbody;
            pushDir = dir.normalized;
            
            // make a debug statement so we know the numbers and info coming out here
            Debug.Log($"Pushing {target.name} in direction {pushDir} with thrust {thrust}");
        }
    }

    void FixedUpdate()
    {
        if (target != null){
            target.AddForce(pushDir * thrust, ForceMode.Force);
        }
            
    }
}