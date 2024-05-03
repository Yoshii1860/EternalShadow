using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UniqueIDComponent))]
public class InteractableObject : MonoBehaviour
{
    #region Variables

    [Header("Object Properties")]
    [Tooltip("Does the object need the pickup canvas?")]
    public bool IsPickup = true;
    [Tooltip("Is the object active?")] // used for enable/disable objects on save/load
    public bool IsActive = false;
    // checked by ObjectHandler to determine if the object is to be put in the inventory
    [Tooltip("Is the object an item stored in the inventory?")]
    public bool InventoryItem = true;
    // checked by SimpleCameraController to determine if the object is to be highlighted
    [Tooltip("Does the object have to be highlighted?")]
    public bool UseEmission = true;
    [Tooltip("The item that the object represents")]
    public Item Item = null;
    [Space(10)]

    [Header("Rotation")]
    [Tooltip("which axis to rotate the object on (x, y or z) - empty string for no rotation")]
    [SerializeField] private string _rotationAxis;
    private Vector3 _rotationVector;
    [Space(10)]

    [Header("Audio")]
    [Tooltip("Does the object play audio after pickup?")]
    [SerializeField] bool _audioAfterPickup = false;
    [Tooltip("The name of the audio clip to play after pickup")]
    [SerializeField] string _audioClipName = "";
    [Tooltip("The delay before playing the audio clip")]
    [SerializeField] float _delay = 0f;
    [Space(10)]

    [Tooltip("The name of the audio clip to play when the object is picked up - if not set, the default is 'pickup item'")]
    public string ClipNameOnPickup;

    Coroutine _rotationCoroutine;

    #endregion




    #region Unity Methods

    protected virtual void Start()
    {
        switch (_rotationAxis.ToLower())
        {
            case "x":
                _rotationVector = Vector3.right;
                break;
            case "y":
                _rotationVector = Vector3.up;
                break;
            case "z":
                _rotationVector = Vector3.forward;
                break;
            default:
                _rotationVector = Vector3.up;
                break;
        }
    }

    #endregion




    #region Interaction

    public virtual void Interact()
    {
        Debug.Log("Interacting with the base interactable object.");

        if (IsActive) IsActive = false;

        if (!IsPickup) 
        {
            RunItemCode();
            return;
        }

        // loop through all active children and grand children of fpsArms and disable mesh renderer
        GameManager.Instance.PlayerController.ToggleArms(false);

        // Open Canvas
        GameManager.Instance.PickupCanvas.SetActive(true);

        // Pause UpdateManager
        GameManager.Instance.PickUp();

        if (ClipNameOnPickup == "") ClipNameOnPickup = "pickup item";
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, ClipNameOnPickup, 0.6f, 1f);

        DeactivateEmission();

        // Create Duplicate Object on Canvas
        GameObject newItem = Instantiate(gameObject, GameManager.Instance.PickupCanvas.transform);

        // Deactivate Collider
        Collider[] collider = newItem.GetComponents<Collider>();
        if (collider.Length > 0) 
        {
            foreach (Collider c in collider)
            {
                c.enabled = false;
            }
        }

        // Deactivate all lights from showcase object
        DeactivateLights(newItem);

        // Show Object Details on Screen
        GameManager.Instance.PickupName.text = Item.DisplayName;
        GameManager.Instance.PickupDescription.text = Item.Description;

        // Make in-scene object invisible
        DeactivateRendererAndLights(transform);

        // Run Item Code
        StartCoroutine(ItemCode(newItem));

        Transform objectPosition = null;
        foreach (Transform child in GameManager.Instance.PickupPositions.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Position" + Item.name)
            {
                objectPosition = child;
                Debug.Log("INTERACTABLE OBJECT: OBJECT POSITION FOUND");
                break;
            }
        }

        // Set world position and rotation
        newItem.transform.position = objectPosition.position;
        newItem.transform.rotation = objectPosition.rotation;
        newItem.transform.localScale = objectPosition.localScale;

        // Rotate the object
        _rotationCoroutine = StartCoroutine(RotateObject(newItem));
    }

    void DeactivateRendererAndLights(Transform objTransform)
    {
        Renderer renderer = objTransform.GetComponent<Renderer>();
        Light[] lights = objTransform.GetComponentsInChildren<Light>();

        if (renderer != null)
        {
            renderer.enabled = false;
        }
        else
        {
            Renderer[] renderers = objTransform.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }

        foreach (Light l in lights)
        {
            l.enabled = false;
        }
    }

    void DeactivateLights(GameObject obj)
    {
        Light[] lights = obj.GetComponentsInChildren<Light>();
        foreach (Light l in lights)
        {
            l.enabled = false;
        }
    }

    void DeactivateEmission()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            foreach(Material material in renderer.materials)
            {
                material.DisableKeyword("_EMISSION");
            }
        }
        else
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            if (renderers != null)
            {
                foreach (MeshRenderer rendererChild in renderers)
                {
                    foreach (Material material in rendererChild.materials)
                    {
                        material.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }

    #endregion




    #region Coroutines

    private IEnumerator ItemCode(GameObject newItemToDestroy)
    {
        // Wait for the player to return to gameplay mode
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.DEFAULT);

        // Stop coroutine to prevent errors before deleting
        StopCoroutine(_rotationCoroutine);

        // Destroy duplicated object
        Destroy(newItemToDestroy);

        // Deactivate Canvas
        GameManager.Instance.PlayerController.ToggleArms(true);
        GameManager.Instance.PickupCanvas.SetActive(false);

        // Run object-specific code
        RunItemCode();

        if (_audioAfterPickup) AudioManager.Instance.PlayClipOneShotWithDelay(AudioManager.Instance.PlayerSpeaker2, _audioClipName, _delay);

        // Destroy the interactable object
        gameObject.SetActive(false);
    }

    private IEnumerator RotateObject(GameObject itemToRotate)
    {
        float elapsedTime = 0f;
        float rotateTime = 10.0f; // Adjust this value for the rotation speed

        while (elapsedTime < rotateTime)
        {
            float angle = (360 / rotateTime) * Time.deltaTime;
            Vector3 rotation = _rotationVector * angle;

            itemToRotate.transform.rotation *= Quaternion.Euler(rotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion




    #region Item-Specific Code

    protected virtual void RunItemCode()
    {
        Debug.Log("Running base item code.");
    }

    #endregion
}