using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(UniqueIDComponent))]
public class InteractableObject : MonoBehaviour
{
    protected Animator anim;
    public bool active = false;
    [SerializeField] Transform objectPosition;
    [SerializeField] bool rotateX = false;
    [SerializeField] bool rotateY = false;
    [SerializeField] bool rotateZ = false;
    [SerializeField] TextMeshProUGUI objectName;
    [SerializeField] TextMeshProUGUI objectDescription;
    [SerializeField] string objectNameString;
    [SerializeField] string objectDescriptionString;
    Coroutine rotationCoroutine;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public virtual void Interact()
    {
        Debug.Log("Interacting with the base interactable object.");

        // Open Canvas
        GameManager.Instance.pickupCanvas.SetActive(true);

        // Pause UpdateManager
        GameManager.Instance.PickUp();

        // Create Duplicate Object on Canvas
        GameObject newItem = Instantiate(gameObject, GameManager.Instance.pickupCanvas.transform);

        // Deactivate all lights from showcase object
        Light[] lights = newItem.GetComponentsInChildren<Light>();
        foreach (Light l in lights)
        {
            l.enabled = false;
        }

        // Show Object Details on Screen
        objectName.text = objectNameString;
        objectDescription.text = objectDescriptionString;

        // Make in-scene object invisible
        Renderer renderer = transform.GetComponent<Renderer>();
        lights = transform.GetComponentsInChildren<Light>();
        if (renderer != null) 
        {
            renderer.enabled = false;
        }
        else
        {
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }
        foreach (Light l in lights)
        {
            l.enabled = false;
        }

        // Run Item Code
        StartCoroutine(ItemCode(newItem));
        
        //Deactivate Collider
        Collider collider = newItem.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // Set world position and rotation
        newItem.transform.position = objectPosition.position;
        newItem.transform.rotation = objectPosition.rotation;
        newItem.transform.localScale = objectPosition.localScale;

        rotationCoroutine = StartCoroutine(RotateObject(newItem));
    }

    IEnumerator ItemCode(GameObject newItemToDestroy)
    {
        // Wait for player to return to gameplaye mode
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Default);

        // Stop coroutine to prevent errors before deleting
        StopCoroutine(rotationCoroutine);

        // Destroy duplicated object
        Destroy(newItemToDestroy);

        // Deactivate Canvas
        GameManager.Instance.pickupCanvas.SetActive(false);

        // Run object specific code
        RunItemCode();
    }

    IEnumerator RotateObject(GameObject itemToRotate)
    {
        float elapsedTime = 0f;
        float rotateTime = 10.0f; // Adjust this value for the rotation speed

        while (elapsedTime < rotateTime)
        {
            Vector3 rotation = Vector3.zero;

            if (rotateX) rotation.x = (360 / rotateTime) * Time.deltaTime;
            if (rotateY) rotation.y = (360 / rotateTime) * Time.deltaTime;
            if (rotateZ) rotation.z = (360 / rotateTime) * Time.deltaTime;

            itemToRotate.transform.rotation *= Quaternion.Euler(rotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    protected virtual void RunItemCode()
    {
        Debug.Log("Running the base item code.");
        Debug.Log("This should destroy the main object at the end.");
    }
}