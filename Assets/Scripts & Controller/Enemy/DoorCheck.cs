using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCheck : MonoBehaviour, ICustomUpdatable
{
    void Start()
    {
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(this);
    }

    public void CustomUpdate(float deltaTime)
    {
        CheckDoor();
    }

    private void CheckDoor()
    {
        // Check with a sphere if the mannequin is close to a door
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Interact"));
        // make it visible for debugging
        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Door door = hitColliders[i].GetComponent<Door>();
                if (door != null)
                {
                    if (!door.DoorState() && !door.locked)
                    {
                        door.OpenDoor();
                    }
                }
            }
        }
    }
}
