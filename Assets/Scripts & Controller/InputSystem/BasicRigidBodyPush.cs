using UnityEngine;

public class BasicRigidBodyPush : MonoBehaviour
{
    #region Fields

    [Header("Push Settings")]
    [Tooltip("Layers to consider for pushing")]
    public LayerMask pushLayers;

    [Tooltip("Toggle to enable or disable pushing")]
    public bool canPush;

    [Range(0.5f, 5f)]
    [Tooltip("Strength of the push")]
    public float strength = 1.1f;

    #endregion

    #region Unity Callbacks

    // Called when a controller hits a collider while performing a Move operation
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (canPush)
        {
            // Perform rigid body pushing if enabled
            PushRigidBodies(hit);
        }
    }

    #endregion

    #region Private Methods

    // Push rigid bodies based on collision information
    private void PushRigidBodies(ControllerColliderHit hit)
    {
        // Ensure the hit object has a non-kinematic rigidbody
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
        {
            // Ignore if the body is null or kinematic
            return;
        }

        // Check if the hit object's layer is included in the pushLayers
        var bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & pushLayers.value) == 0)
        {
            // Ignore if the layer is not included in pushLayers
            return;
        }

        // Avoid pushing objects below the controller
        if (hit.moveDirection.y < -0.3f)
        {
            // Ignore if the hit is from below
            return;
        }

        // Calculate the push direction (horizontal motion only)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        // Apply the push force, taking strength into account
        body.AddForce(pushDir * strength, ForceMode.Impulse);
    }

    #endregion
}