using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCheck : MonoBehaviour, ICustomUpdatable
{
    #region Unity Methods

    void Start()
    {
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(this);
    }

    #endregion




    #region Custom Update

    public void CustomUpdate(float deltaTime)
    {
        CheckDoor();
    }

    #endregion




    #region Private Methods

    private void CheckDoor()
    {
        // Check with a sphere if the enemy is close to a door
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Interact"));
        // make it visible for debugging
        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Door door = hitColliders[i].GetComponent<Door>();
                if (door != null)
                {
                    if (!door.DoorState() && !door.IsLocked)
                    {
                        door.OpenDoor();
                    }
                }
            }
        }
    }

    #endregion
}
