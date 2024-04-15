using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour, ICustomUpdatable
{
    Door door;

    public void CustomUpdate(float deltaTime)
    {
        if (!door.locked && door.open)
        {
            // End the Game
            Debug.Log("Game Ended");
        }
    }

    void Start()
    {
        door = GetComponent<Door>();
    }
}

