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

    /// Updates references such as the player.
    public void UpdateReferences()
    {
        player = GameManager.Instance.Player;
    }

    #endregion




    #region Potion Actions

    /// Use antibiotics to cure poisoning.
    /// <param name="item">The antibiotics item.</param>
    public void Antibiotics(Item item)
    {
        if (player.IsPoisoned)
        {
            InventoryManager.Instance.DisplayMessage("The effects of the poison start to fade away.");
            player.IsPoisoned = false;
            InventoryManager.Instance.RemoveItem(item);
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "pills", 0.6f, 1f);
        }
        else
        {
            InventoryManager.Instance.DisplayMessage("You don`t have an infection.");
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "error", 0.6f, 1f);
        }
    }

    /// Use a bandage to stop bleeding.
    /// <param name="item">The bandage item.</param>
    public void Bandage(Item item)
    {
        if (player.IsBleeding || player.Health < 100)
        {
            if (player.IsBleeding) InventoryManager.Instance.DisplayMessage("You stopped the bleeding.");
            else if (player.Health + 50 < 100) InventoryManager.Instance.DisplayMessage("You feel better now.");
            else InventoryManager.Instance.DisplayMessage("You are fully healed.");

            player.IsBleeding = false;
            player.Health += 50;
            InventoryManager.Instance.RemoveItem(item);
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "bandage", 0.6f, 1f);
        }
        else
        {
            InventoryManager.Instance.DisplayMessage("You don`t have any wounds.");
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "error", 0.6f, 1f);
        }
    }

    /// Use painkillers to alleviate dizziness.
    /// <param name="item">The painkillers item.</param>
    public void Painkillers(Item item)
    {
        if (player.IsDizzy)
        {
            InventoryManager.Instance.DisplayMessage("The pain starts to fade away.");
            player.IsDizzy = false;
            InventoryManager.Instance.RemoveItem(item);
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "pills", 0.6f, 1f);
        }
        else
        {
            InventoryManager.Instance.DisplayMessage("You don`t need those pills right now.");
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "error", 0.6f, 1f);
        }
    }

    #endregion
}