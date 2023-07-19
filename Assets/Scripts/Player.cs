using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Tooltip("The health of the player.")]
    public int health = 100;
    [Tooltip("The stamina of the player.")]
    public int stamina = 100;
    [Tooltip("The regeneration in seconds of the player.")]
    public int regeneration = 10;
    [Tooltip("If the player is bleeding - reduces health over time.")]
    public bool isBleeding = false;
    [Tooltip("If the player is poisoned - reduces health and stamina over time.")]
    public bool isPoisoned = false;
    [Tooltip("If the player is dizzy - stops at least once a minute for 3 seconds.")]
    public bool isDizzy = false;

    private int maxHealth = 100;
    private int maxStamina = 100;
}