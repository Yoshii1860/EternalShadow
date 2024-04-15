using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingEvent : MonoBehaviour
{
    #region Fields

    // Painting order: Mind - 1, Heart - 2, Hand - 3, Foot - 4, Eye - 5
    [SerializeField] int[] solvedOrder = new int[5] { 4, 2, 3, 0, 1 };

    [SerializeField] List<PaintingCode> paintingList = new List<PaintingCode>();

    // Currently marked paintings for switching
    public GameObject markedPainting;
    public GameObject markedPaintingToSwitch;

    // Selected painting index
    private int _paintingSelector;
    private int lastSelector;

    [SerializeField] GameObject pen;
    [SerializeField] GameObject drawer;

    [SerializeField] GameObject girl;
    [SerializeField] GameObject door;

    public bool active = false;
    bool playOnce = false;

    public int paintingSelector
    {
        get { return _paintingSelector; }
        set
        {
            lastSelector = _paintingSelector;
            _paintingSelector = value;
            HighlightPainting();
        }
    }

    [SerializeField] Transform vCamFollow;

    public bool pause = false;

    #endregion

    #region Painting Event Methods

    // Start the painting event
    public void StartPaintingEvent()
    {
        pause = true;

        StartCoroutine(StartingEvent());    
    }

    IEnumerator StartingEvent()
    {
        // Transform pens x-rotation from 0 to -25
        float time = 0;
        float duration = 2f;
        Quaternion startRotation = pen.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(-25, 0, 0);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            pen.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "pen click", 1f, 1f);

        yield return new WaitForSeconds(0.5f);

        if (!playOnce) 
        {
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "speaker paintings");
            playOnce = true;
        }

        // Undo flashlight and weapon if enabled
        GameManager.Instance.playerAnimController.PenholderAnimation();
        
        pause = false;

        door.GetComponent<Renderer>().enabled = false;

        // change LookAtDirection
        GameManager.Instance.playerController.SetFollowTarget(vCamFollow);
        GameManager.Instance.playerController.ToggleArms(false);
        // Reset painting selection
        paintingSelector = 0;
        // Adjust camera field of view
        GameManager.Instance.playerController.cmVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(GameManager.Instance.playerController.cmVirtualCamera.m_Lens.FieldOfView, 20f, Time.deltaTime * 10f);
    }

    // Highlight the selected painting
    private void HighlightPainting()
    {
        Debug.Log("Painting " + paintingSelector + " is highlighted");
        for (int i = 0; i < paintingList.Count; i++)
        {
            if (i == paintingSelector)
            {
                paintingList[i].spotLight.SetActive(true);
            }
            else if (i == lastSelector && paintingList[i].gameObject != markedPainting)
            {
                paintingList[i].spotLight.SetActive(false);
            }
        }
    }

    // Mark a painting for switching
    public void MarkPainting()
    {
        Debug.Log("Mark Painting " + paintingSelector);
        if (markedPainting == null)
        {
            markedPainting = GetPainting().gameObject;
            Debug.Log("First Painting " + paintingSelector + " is marked");
        }
        else
        {
            Debug.Log("Second Painting " + paintingSelector + " is marked");
            markedPaintingToSwitch = GetPainting().gameObject;
            if (markedPaintingToSwitch != markedPainting) StartCoroutine(SwitchPaintings());
            else 
            {
                markedPaintingToSwitch = null;
                markedPainting.GetComponent<PaintingCode>().spotLight.SetActive(false);
                markedPainting = null;
            }
        }
    }

    // Get the selected painting
    private PaintingCode GetPainting()
    {
        Debug.Log("Get Painting");

        for (int i = 0; i < paintingList.Count; i++)
        {
            if (i == paintingSelector)
            {
                return paintingList[i];
            }
        }
        Debug.LogError("Painting not found");
        return null;
    }

    // Switch the positions of the marked paintings
    IEnumerator SwitchPaintings()
    {
        Debug.Log("Switch Paintings");
        pause = true;
        
        Vector3 tempPaintingPosition = new Vector3(markedPainting.transform.position.x, markedPainting.transform.position.y, markedPainting.transform.position.z);
        Vector3 tempMarkedPaintingPosition = new Vector3(markedPaintingToSwitch.transform.position.x, markedPaintingToSwitch.transform.position.y, markedPaintingToSwitch.transform.position.z);

        float time = 0;
        float duration = 0.5f;

        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "switch painting", 1f, 1f);
        while (time < duration)
        {
            markedPainting.transform.position = Vector3.Lerp(markedPainting.transform.position, tempMarkedPaintingPosition, time / duration);
            markedPaintingToSwitch.transform.position = Vector3.Lerp(markedPaintingToSwitch.transform.position, tempPaintingPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        markedPainting.transform.position = tempMarkedPaintingPosition;
        markedPaintingToSwitch.transform.position = tempPaintingPosition;

        markedPainting.GetComponent<PaintingCode>().spotLight.SetActive(false);
        markedPaintingToSwitch.GetComponent<PaintingCode>().spotLight.SetActive(false);

        lastSelector = paintingSelector;

        SwitchListPlace();

        CheckPaintingOrder();
    }

    // Switch the positions of the marked paintings in the list
    private void SwitchListPlace()
    {
        Debug.Log("Switch List Place");

        // Find positions of marked paintings in the list and switch them
        PaintingCode tempOne = null;
        int intOne = 0;
        PaintingCode tempTwo = null;
        int intTwo = 0;

        for (int i = 0; i < paintingList.Count; i++)
        {
            if (paintingList[i].paintingNumber == markedPainting.GetComponent<PaintingCode>().paintingNumber)
            {
                tempOne = paintingList[i];
                intOne = i;
            }
            else if (paintingList[i].paintingNumber == markedPaintingToSwitch.GetComponent<PaintingCode>().paintingNumber)
            {
                tempTwo = paintingList[i];
                intTwo = i;
            }
        }
        paintingList.RemoveAt(intOne);
        paintingList.Insert(intOne, tempTwo);
        paintingList.RemoveAt(intTwo);
        paintingList.Insert(intTwo, tempOne);

        markedPainting = null;
        markedPaintingToSwitch = null;

        pause = false;
    }

    // Check if the paintings are in the correct order
    public void CheckPaintingOrder()
    {
        Debug.Log("Check Painting Order");

        for (int i = 0; i < solvedOrder.Length; i++)
        {
            if (paintingList[i].paintingNumber != solvedOrder[i]) return;
        }

        Debug.Log("Painting Event Solved!");
        
        pause = true;

        // Activate all spotlights if paintings are in correct order
        foreach (PaintingCode painting in paintingList)
        {
            painting.spotLight.SetActive(true);
        }

        StartCoroutine(EndPaintingEvent());
    }

    IEnumerator EndPaintingEvent()
    {
        yield return new WaitForSeconds(1f);

        transform.GetComponent<Collider>().enabled = false;

        door.GetComponent<Renderer>().enabled = true;

        GameManager.Instance.playerController.SetFollowTarget();
        GameManager.Instance.playerController.ToggleArms(true);
        GameManager.Instance.ResumeGame();

        girl.GetComponent<EnemyBT>().ResetTree();
        girl.SetActive(true);
        girl.GetComponent<Animator>().SetTrigger("Bath");

        // Transform pens x-rotation from -25 to 0
        float time = 0;
        float duration = 2f;
        Quaternion startRotation = pen.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            pen.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        drawer.GetComponent<Drawer>().Interact();
    }
    #endregion
}