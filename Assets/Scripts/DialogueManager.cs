using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

// partial code credit to Brackeys on dialogue systems
public class DialogueManager : MonoBehaviour
{
    // First in First out
    private Queue<string> sentences;
    public Text nameText;
    public Text dialogueText;
    public Text nextText;
    private bool pressed;
    private bool isRunning;
    public List<GameObject> panelImages = new List<GameObject>();
    private int currentPanel = -1;
    public GameObject skipButton;
    public string nextSceneName;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
        EventSystem.current.SetSelectedGameObject(skipButton);

        #if UNITY_WSA
            //xbox version of text
            nextText.text = "press A";
        #endif
    }

    private void Update()
    {
        // dialogue moves faster is space is pressed
        if (Input.GetAxisRaw("Action1") > 0)
        {
            pressed = true;
            DisplayNextSentence();
        }
        else
        {
            pressed = false;
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        nameText.text = dialogue.name;
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (!isRunning)
        {
            if (sentences.Count == 0)
            {
                EndDialogue();
                return;
            }

            Managers.AudioManager.PlaySound("ui_button_press");
            string sentence = sentences.Dequeue();
            //dialogueText.text = sentence;
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence));
            DisplayNextPanel();
        }
    }

    public void DisplayNextPanel()
    {
        currentPanel++;
        if (currentPanel >= panelImages.Count)
        {
            currentPanel = panelImages.Count - 1;
        }
        panelImages[currentPanel].SetActive(true);
        for (int i = 0; i < panelImages.Count; i++)
        {
            if (i != currentPanel)
            {
                panelImages[i].SetActive(false);
            }
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isRunning = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            if (pressed == true)
            {
                //Debug.Log("check");
                yield return new WaitForSeconds(0.0001f);
            }
            else
            {
                //Debug.Log("not pressed");
                yield return new WaitForSeconds(0.001f);
            }
        }
        isRunning = false;
    }
    public void EndDialogue()
    {
        //Debug.Log("End of Conversation");
        if (SceneManager.GetActiveScene().name == "EndCutscene")
        {
            GameObject menuBtn = this.transform.GetChild(0).gameObject;
            GameObject quitBtn = this.transform.GetChild(1).gameObject;
            menuBtn.SetActive(true);
            quitBtn.SetActive(true);
            EventSystem.current.SetSelectedGameObject(menuBtn);
        }
        else
        {
            Managers.ScenesManager.ChangeScene(nextSceneName);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnMenu()
    {
        Managers.ScenesManager.ChangeScene("Menu");
    }
}

