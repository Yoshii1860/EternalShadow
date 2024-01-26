using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    #region Fields

    Player player;

    #endregion

    #region Enums

    public enum PotionType
    {
        None,
        Antibiotics,
        Bandage,
        Painkillers
    }

    #endregion

    #region Unity Lifecycle Methods

    // Start is called before the first frame update
    void Start()
    {
        UpdateReferences();
    }

    #endregion

    #region Initialization Method

    /// <summary>
    /// Updates references such as the player.
    /// </summary>
    public void UpdateReferences()
    {
        player = GameManager.Instance.player;
    }

    #endregion

    #region Potion Actions

    /// <summary>
    /// Use antibiotics to cure poisoning.
    /// </summary>
    /// <param name="item">The antibiotics item.</param>
    public void Antibiotics(Item item)
    {
        if (player.isPoisoned)
        {
            player.isPoisoned = false;
            InventoryManager.Instance.RemoveItem(item);
        }
        else
        {
            Debug.Log("Not poisoned");
        }
    }

    /// <summary>
    /// Use a bandage to stop bleeding.
    /// </summary>
    /// <param name="item">The bandage item.</param>
    public void Bandage(Item item)
    {
        if (player.isBleeding)
        {
            player.isBleeding = false;
            InventoryManager.Instance.RemoveItem(item);
        }
        else
        {
            Debug.Log("Not bleeding");
        }
    }

    /// <summary>
    /// Use painkillers to alleviate dizziness.
    /// </summary>
    /// <param name="item">The painkillers item.</param>
    public void Painkillers(Item item)
    {
        if (player.isDizzy)
        {
            player.isDizzy = false;
            InventoryManager.Instance.RemoveItem(item);
        }
        else
        {
            Debug.Log("Not dizzy");
        }
    }

    #endregion
}