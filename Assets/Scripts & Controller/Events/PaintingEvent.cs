using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingEvent : MonoBehaviour
{
    #region Fields

    [Header("Painting Event Variables")]
    // Painting order: Mind - 1, Heart - 2, Hand - 3, Foot - 4, Eye - 5
    [Tooltip("The correct order of the paintings")]
    [SerializeField] private int[] _solvedOrder = new int[5] { 4, 2, 3, 0, 1 };
    [Tooltip("List of all paintings in the room")]
    [SerializeField] private List<PaintingCode> _paintingList = new List<PaintingCode>();
    [Space(10)]

    [Tooltip("Currently marked painting")]
    public GameObject MarkedPainting;
    [Tooltip("Painting to switch with the marked painting")]
    public GameObject MarkedPaintingToSwitch;

    // Selected painting index
    private int _paintingSelector;
    private int _lastSelector;
    [Space(10)]

    [Header("Painting Event Objects")]
    [Tooltip("The pen object of the penholder that will start the event")]
    [SerializeField] private GameObject _penObject;
    [Tooltip("The drawer object that will be interacted with after the event")]
    [SerializeField] private GameObject _drawerObject;
    [Tooltip("The girl that will be activated after the event")]
    [SerializeField] private GameObject _girlObject;
    [Tooltip("The door in the room so it can be disabled during the event")]
    [SerializeField] private GameObject _doorObject;
    [Tooltip("The camera that will follow the paintings during the event")]
    [SerializeField] private Transform _vCamFollow;
    [Space(10)]

    [Tooltip("Is the painting event active?")]
    public bool IsActive = false;
    [Tooltip("Is the painting event paused?")]
    public bool IsPaused = false;
    [Tooltip("Debug mode for the painting event")]
    [SerializeField] private bool _debugMode = false;

    // The current and last selected painting
    public int PaintingSelector
    {
        get { return _paintingSelector; }
        set
        {
            _lastSelector = _paintingSelector;
            _paintingSelector = value;
            HighlightPainting();
        }
    }
        
    private bool _playOnce = false;

    #endregion




    #region Unity Methods

    // Start the painting event
    public void StartPaintingEvent()
    {
        IsPaused = true;

        StartCoroutine(StartingEvent());    
    }

    #endregion




    #region Public Methods

    // Mark a painting for switching - called by painting controller
    public void MarkPainting()
    {
        if (_debugMode) Debug.Log("Mark Painting " + PaintingSelector);

        // If no painting is marked, mark the first painting
        if (MarkedPainting == null)
        {
            MarkedPainting = GetPainting().gameObject;

            if (_debugMode) Debug.Log("First Painting " + PaintingSelector + " is marked");
        }
        else
        {
            if (_debugMode) Debug.Log("Second Painting " + PaintingSelector + " is marked");

            MarkedPaintingToSwitch = GetPainting().gameObject;

            // If two different paintings are marked, switch them
            if (MarkedPaintingToSwitch != MarkedPainting) 
            {
                StartCoroutine(SwitchPaintings());
            }
            // If the same painting is marked twice, unmark it
            else 
            {
                MarkedPaintingToSwitch = null;
                MarkedPainting.GetComponent<PaintingCode>().SpotLight.SetActive(false);
                MarkedPainting = null;
            }
        }
    }

    #endregion




    #region Private Methods

    // Check if the paintings are in the correct order
    private void CheckPaintingOrder()
    {
        if (_debugMode) Debug.Log("Check Painting Order");

        // Check if the paintings are in the correct order
        for (int i = 0; i < _solvedOrder.Length; i++)
        {
            if (_paintingList[i].PaintingNumber != _solvedOrder[i]) return;
        }

        if (_debugMode) Debug.Log("Painting Event Solved!");
        
        IsPaused = true;

        // Activate all spotlights if paintings are in correct order
        foreach (PaintingCode painting in _paintingList)
        {
            painting.SpotLight.SetActive(true);
        }

        // Start the end painting event
        StartCoroutine(EndPaintingEvent());
    }

    // Highlight the selected painting
    private void HighlightPainting()
    {
        if (_debugMode) Debug.Log("Painting " + PaintingSelector + " is highlighted");

        for (int i = 0; i < _paintingList.Count; i++)
        {
            if (i == PaintingSelector)
            {
                _paintingList[i].SpotLight.SetActive(true);
            }
            else if (i == _lastSelector && _paintingList[i].gameObject != MarkedPainting)
            {
                _paintingList[i].SpotLight.SetActive(false);
            }
        }
    }

    // Get the selected painting
    private PaintingCode GetPainting()
    {
        if (_debugMode) Debug.Log("Get Painting");

        for (int i = 0; i < _paintingList.Count; i++)
        {
            if (i == PaintingSelector)
            {
                return _paintingList[i];
            }
        }
        if (_debugMode) Debug.LogError("Painting not found");
        return null;
    }

    // Switch the positions of the marked paintings in the list
    private void SwitchListPlace()
    {
        if (_debugMode) Debug.Log("Switch List Place");

        // Find positions of marked paintings in the list and switch them
        PaintingCode tempOne = null;
        int intOne = 0;
        PaintingCode tempTwo = null;
        int intTwo = 0;

        for (int i = 0; i < _paintingList.Count; i++)
        {
            if (_paintingList[i].PaintingNumber == MarkedPainting.GetComponent<PaintingCode>().PaintingNumber)
            {
                tempOne = _paintingList[i];
                intOne = i;
            }
            else if (_paintingList[i].PaintingNumber == MarkedPaintingToSwitch.GetComponent<PaintingCode>().PaintingNumber)
            {
                tempTwo = _paintingList[i];
                intTwo = i;
            }
        }

        // Switch the positions of the marked paintings
        _paintingList.RemoveAt(intOne);
        _paintingList.Insert(intOne, tempTwo);
        _paintingList.RemoveAt(intTwo);
        _paintingList.Insert(intTwo, tempOne);

        // Reset the painting selector
        MarkedPainting = null;
        MarkedPaintingToSwitch = null;

        IsPaused = false;
    }

    #endregion




    #region Coroutines

    // Start the painting event
    private IEnumerator StartingEvent()
    {
        // Transform pens x-rotation from 0 to -25, similar to a switch
        float time = 0;
        float duration = 2f;
        Quaternion startRotation = _penObject.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(-25, 0, 0);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            _penObject.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        // Start the painting event with a sound
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "pen click", 1f, 1f);

        yield return new WaitForSeconds(0.5f);

        // Play the speaker sound once
        if (!_playOnce) 
        {
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "speaker paintings");
            _playOnce = true;
        }

        // Undo flashlight and weapon if enabled
        GameManager.Instance.PlayerAnimManager.PenholderAnimation();
        // disable the doors renderer
        _doorObject.GetComponent<Renderer>().enabled = false;

        IsPaused = false;

        // change LookAtDirection
        GameManager.Instance.PlayerController.SetFollowTarget(_vCamFollow);
        GameManager.Instance.PlayerController.ToggleArms(false);

        // Reset painting selection
        PaintingSelector = 0;

        // Adjust camera field of view
        GameManager.Instance.PlayerController.CMVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(GameManager.Instance.PlayerController.CMVirtualCamera.m_Lens.FieldOfView, 20f, Time.deltaTime * 10f);
    }

    // Switch the positions of the marked paintings
    private IEnumerator SwitchPaintings()
    {
        if (_debugMode) Debug.Log("Switch Paintings");
        IsPaused = true;
        
        // create temporary positions for the paintings
        Vector3 tempPaintingPosition = new Vector3(MarkedPainting.transform.position.x, MarkedPainting.transform.position.y, MarkedPainting.transform.position.z);
        Vector3 tempMarkedPaintingPosition = new Vector3(MarkedPaintingToSwitch.transform.position.x, MarkedPaintingToSwitch.transform.position.y, MarkedPaintingToSwitch.transform.position.z);

        // Play sound for switching paintings
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "switch painting", 1f, 1f);

        // Move the paintings to their new positions
        float time = 0;
        float duration = 0.5f;
        while (time < duration)
        {
            MarkedPainting.transform.position = Vector3.Lerp(MarkedPainting.transform.position, tempMarkedPaintingPosition, time / duration);
            MarkedPaintingToSwitch.transform.position = Vector3.Lerp(MarkedPaintingToSwitch.transform.position, tempPaintingPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Swap the positions of the paintings
        MarkedPainting.transform.position = tempMarkedPaintingPosition;
        MarkedPaintingToSwitch.transform.position = tempPaintingPosition;

        // Reset the spotlights
        MarkedPainting.GetComponent<PaintingCode>().SpotLight.SetActive(false);
        MarkedPaintingToSwitch.GetComponent<PaintingCode>().SpotLight.SetActive(false);

        // save the last selected painting
        _lastSelector = PaintingSelector;

        // Switch the positions of the paintings in the list
        SwitchListPlace();

        // Check if the paintings are in the correct order
        CheckPaintingOrder();
    }

    // End the painting event
    private IEnumerator EndPaintingEvent()
    {
        yield return new WaitForSeconds(1f);

        // Disable the pens collider to prevent further interaction
        transform.GetComponent<Collider>().enabled = false;
        // Enable the doors renderer
        _doorObject.GetComponent<Renderer>().enabled = true;

        // Reset the camera follow target, the arms and resume the game
        GameManager.Instance.PlayerController.SetFollowTarget();
        GameManager.Instance.PlayerController.ToggleArms(true);
        GameManager.Instance.ResumeGame();

        // Prepare the girl for the bathroom event
        _girlObject.GetComponent<EnemyBT>().ResetTree();
        _girlObject.SetActive(true);
        _girlObject.GetComponent<Animator>().SetTrigger("Bath");

        // Transform pens x-rotation from -25 to 0, switch the pen back
        float time = 0;
        float duration = 2f;
        Quaternion startRotation = _penObject.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            _penObject.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // Open the drawer after the event
        _drawerObject.GetComponent<Drawer>().Interact();
    }

    #endregion
}