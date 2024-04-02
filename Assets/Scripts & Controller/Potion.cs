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
            InventoryManager.Instance.DisplayMessage("The effects of the poison start to fade away.");
            player.isPoisoned = false;
            InventoryManager.Instance.RemoveItem(item);
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "pills", 0.6f, 1f);
        }
        else
        {
            InventoryManager.Instance.DisplayMessage("You don`t have an infection.");
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "error", 0.6f, 1f);
        }
    }

    /// <summary>
    /// Use a bandage to stop bleeding.
    /// </summary>
    /// <param name="item">The bandage item.</param>
    public void Bandage(Item item)
    {
        if (player.isBleeding || player.health < 100)
        {
            if (player.isBleeding) InventoryManager.Instance.DisplayMessage("You stopped the bleeding.");
            else if (player.health + 50 < 100) InventoryManager.Instance.DisplayMessage("You feel better now.");
            else InventoryManager.Instance.DisplayMessage("You are fully healed.");

            player.isBleeding = false;
            player.health += 50;
            InventoryManager.Instance.RemoveItem(item);
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "bandage", 0.6f, 1f);
        }
        else
        {
            InventoryManager.Instance.DisplayMessage("You don`t have any wounds.");
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "error", 0.6f, 1f);
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
            InventoryManager.Instance.DisplayMessage("The pain starts to fade away.");
            player.isDizzy = false;
            InventoryManager.Instance.RemoveItem(item);
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "pills", 0.6f, 1f);
        }
        else
        {
            InventoryManager.Instance.DisplayMessage("You don`t need those pills right now.");
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "error", 0.6f, 1f);
        }
    }

    #endregion
}