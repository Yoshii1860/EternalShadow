using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NavKeypad { 
public class Keypad : MonoBehaviour
{
    #region Variables

    [Header("Door")]
    [SerializeField] private Door door;
    [Header("Combination Code (9 Numbers Max)")]
    [SerializeField] private int keypadCombo = 1234;
    [SerializeField] private GameObject musicbox;
    [SerializeField] private GameObject musicboxPrefab;

    [Header("Settings")]
    [SerializeField] private string accessGrantedText = "Granted";
    [SerializeField] private string accessDeniedText = "Denied";

    [Header("Visuals")]
    [SerializeField] private float displayResultTime = 1f;
    [Range(0,5)]
    [SerializeField] private float screenIntensity = 2.5f;
    [Header("Colors")]
    [SerializeField] private Color screenNormalColor = new (0.98f, 0.50f, 0.032f, 1f); //orangy
    [SerializeField] private Color screenDeniedColor = new(1f, 0f, 0f, 1f); //red
    [SerializeField] private Color screenGrantedColor = new(0f, 0.62f, 0.07f); //greenish
    [Header("SoundFx")]
    [SerializeField] private AudioClip buttonClickedSfx;
    [SerializeField] private AudioClip accessDeniedSfx;
    [SerializeField] private AudioClip accessGrantedSfx;
    [Header("Component References")]
    [SerializeField] private Renderer panelMesh;
    [SerializeField] private TMP_Text keypadDisplayText;
    [SerializeField] private AudioSource audioSource;

    private string currentInput;
    private bool displayingResult = false;
    private bool accessWasGranted = false;

    #endregion




    #region Unity Methods

    private void Awake()
    {
        ClearInput();
        panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);
    }

    void Start()
    {
        keypadCombo = UnityEngine.Random.Range(1000, 9999);
        musicbox.GetComponent<TextMeshPro>().text = keypadCombo.ToString();
        foreach (Transform child in musicboxPrefab.transform)
        {
            if (child.CompareTag("lid"))
            {
                child.GetComponentInChildren<TextMeshPro>().text = keypadCombo.ToString();
                return;
            }
        }
    }

    #endregion




    #region Keypad Handling

    //Gets value from pressedbutton
    public void AddInput(string input)
    {
        audioSource.PlayOneShot(buttonClickedSfx);
        if (displayingResult || accessWasGranted) return;
        switch (input)
        {
            case "enter":
                CheckCombo();
                break;
            default:
                if (currentInput != null && currentInput.Length == 9) // 9 max passcode size 
                {
                    return;
                }
                currentInput += input;
                keypadDisplayText.text = currentInput;
                break;
        }
        
    }
    public void CheckCombo()
    {
        if(int.TryParse(currentInput, out var currentKombo))
        {
            bool granted = currentKombo == keypadCombo;
            if (!displayingResult)
            {
                StartCoroutine(DisplayResultRoutine(granted));
            }
        }
        else
        {
            Debug.LogWarning("Couldn't process input for some reason..");
        }

    }

    //mainly for animations 
    private IEnumerator DisplayResultRoutine(bool granted)
    {
        GameManager.Instance.EventData.SetEvent("Keypad");
        displayingResult = true;

        if (granted) AccessGranted();
        else AccessDenied();

        yield return new WaitForSeconds(displayResultTime);
        displayingResult = false;
        if (granted) yield break;
        ClearInput();
        panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);

    }

    private void AccessDenied()
    {
        keypadDisplayText.text = accessDeniedText;
        panelMesh.material.SetVector("_EmissionColor", screenDeniedColor * screenIntensity);
        audioSource.PlayOneShot(accessDeniedSfx);
    }

    private void ClearInput()
    {
        currentInput = "";
        keypadDisplayText.text = currentInput;
    }

    private void AccessGranted()
    {
        Item item = InventoryManager.Instance.FindItem("Old Musicbox");
        if (item != null)
        {
            InventoryManager.Instance.RemoveItem(item);
        }

        accessWasGranted = true;
        keypadDisplayText.text = accessGrantedText;
        panelMesh.material.SetVector("_EmissionColor", screenGrantedColor * screenIntensity);
        audioSource.PlayOneShot(accessGrantedSfx);
        door.IsLocked = false;
        door.Interact();
        door.IsLocked = true;   
    }

    #endregion




    #region Event Method

    public void EventLoad()
    {
        if (GameManager.Instance.EventData.CheckEvent("Keypad"))
        {
            accessWasGranted = true;
            keypadDisplayText.text = accessGrantedText;
            panelMesh.material.SetVector("_EmissionColor", screenGrantedColor * screenIntensity);
            return;
        }
        else if (GameManager.Instance.EventData.CheckEvent("Musicbox"))
        {
            keypadCombo = UnityEngine.Random.Range(1000, 9999);
            musicbox.GetComponent<TextMeshPro>().text = keypadCombo.ToString();
            foreach (Transform child in musicboxPrefab.transform)
            {
                if (child.CompareTag("lid"))
                {
                    child.GetComponentInChildren<TextMeshPro>().text = keypadCombo.ToString();
                    return;
                }
            }
        }
    }

    #endregion
}
}