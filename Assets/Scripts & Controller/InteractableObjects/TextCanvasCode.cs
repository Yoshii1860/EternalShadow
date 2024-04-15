using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TextCanvasCode : MonoBehaviour
{
    // 1 header
    // 0-3 subheader
    // 1 main body
    // 0-3 subbody
    // Restrict body 1 to 12 lines (if no other body)
    // Restrict body 1 to 5 lines (if other body)
    // Restrict each other body to 5 lines
    // Restrict subBody2 to 12 lines (if twoPageBody)

    // if only mainHeader & Main body, deactivate all subheaders and bodies

    
    [SerializeField] TextMeshProUGUI mainHeader;
    string mainHeaderString;
    [SerializeField] TextMeshProUGUI mainBody;
    string mainBodyString;

    [Space(10)]

    bool twoBodies;
    [SerializeField] TextMeshProUGUI subHeader;
    string subHeaderString;
    [SerializeField] TextMeshProUGUI subBody;
    string subBodyString;

    [Space(10)]

    public string speakerClipName = "";
    public float delay = 0f;
    public bool audioPlayable = false;

    void Start()
    {
        gameObject.SetActive(false);
    }

    // function to set up text
    public void NewText(string newHeader, string newBody, string newSubHeader = "", string newSubBody = "")
    {
        mainHeaderString = newHeader;
        mainBodyString = newBody;
        subHeaderString = newSubHeader;
        subBodyString = newSubBody;

        if (subHeaderString == "" && subBodyString == "") twoBodies = false;
        else twoBodies = true;

        SetTexts();
        
        if (twoBodies)
        {
            subHeader.gameObject.SetActive(true);
            subBody.gameObject.SetActive(true);
        }
        else
        {
            subHeader.gameObject.SetActive(false);
            subBody.gameObject.SetActive(false);
        }

        RendererToggle(GameManager.Instance.fpsArms, false);
        GameManager.Instance.PickUp();
        gameObject.SetActive(true);
        GameManager.Instance.canvasActive = true;
        StartCoroutine(ToggleText());
    }

    // function to restrict text length
    void SetTexts()
    {
        mainBodyString = mainBodyString.Replace("\\n", "\n");

        //change rect transform bottom and top
        mainBody.rectTransform.offsetMin = new Vector2(mainBody.rectTransform.offsetMin.x, 90);
        mainBody.rectTransform.offsetMax = new Vector2(mainBody.rectTransform.offsetMax.x, -140);

        mainHeader.text = mainHeaderString;
        mainBody.text = mainBodyString;

        if (twoBodies)
        {
            //change rect transform bottom to 350
            mainBody.rectTransform.offsetMin = new Vector2(mainBody.rectTransform.offsetMin.x, 350);

            subBodyString = subBodyString.Replace("\\n", "\n");
            subHeader.text = subHeaderString;
            subBody.text = subBodyString;
        }

        if (mainHeaderString == "")
        {
            mainBody.rectTransform.offsetMax = new Vector2(mainBody.rectTransform.offsetMax.x, -80);
        }
    }

    public void SetAudioClip(string clipName, float delay)
    {
        speakerClipName = clipName;
        this.delay = delay;
        audioPlayable = true;
    }

    // function to run text toggle
    IEnumerator ToggleText()
    {
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Default);
        if (audioPlayable)
        {
            AudioManager.Instance.PlayOneShotWithDelay(AudioManager.Instance.playerSpeaker2, speakerClipName, delay);
            audioPlayable = false;
        }
        gameObject.SetActive(false);
        RendererToggle(GameManager.Instance.fpsArms, true);
    }

    // function to toggle players renderer
    public virtual void RendererToggle(GameObject go, bool active)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Renderer renderer = go.transform.GetChild(i).gameObject.GetComponent<Renderer>();
            if (renderer != null && renderer.gameObject.activeSelf) renderer.enabled = active;
            if (go.transform.GetChild(i).childCount > 0) RendererToggle(go.transform.GetChild(i).gameObject, active);
        }
    }
}
