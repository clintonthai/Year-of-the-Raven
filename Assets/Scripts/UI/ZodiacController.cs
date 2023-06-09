using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZodiacController : MonoBehaviour
{
    [SerializeField] private GameObject dialogueCamera;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public ZodiacDialogueSO zodiacDialogue;
    private int zodiacIndex;
    private int dialogueIndex;
    private bool isTalking;
    private bool isRunning;
    private Animator anim;

    private bool input;
    private bool oldInput;
    private bool currentTalk;
    public int textSpeed;

    // Start is called before the first frame update
    void Start()
    {
        dialogueIndex = -1;
        anim = this.gameObject.GetComponentInChildren<Animator>();
        dialogueCamera.SetActive(false);
        dialogueCanvas.SetActive(false);
        isTalking = false;
        currentTalk = false;
        dialogueText.text = "";
        for(int i = 0; i < zodiacDialogue.zodiacs.Length; i++)
        {
            if (this.gameObject.name == zodiacDialogue.zodiacs[i].zodiacName)
            {
                zodiacIndex = i;
                Debug.Log(this.gameObject.name + " is in index " + zodiacIndex);
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        oldInput = input;
        input = Input.GetAxisRaw("Action1") > 0;

        if (isTalking)
        {
            if (input && !oldInput)
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().actionIndicator.Hide();
                anim.SetBool("isTalking", true);
                dialogueCamera.SetActive(true);
                dialogueCanvas.SetActive(true);
                Debug.Log("Talk with " + this.gameObject.name);
                if (!zodiacDialogue.zodiacs[zodiacIndex].haveTalkedTo)
                {
                    if (!isRunning)
                    {
                        if (!currentTalk)
                        {
                            dialogueText.text = "";
                            string xboxDialogue = zodiacDialogue.zodiacs[zodiacIndex].xboxText;
                            Debug.Log("xbox -> " + xboxDialogue);
                            string dialogue = zodiacDialogue.zodiacs[zodiacIndex].firstDialogue;

      
                            //xbox build
                            #if UNITY_WSA
                                //if the xbox dialogue is not empty and we are in UWP build, use xbox dialogue
                                if(xboxDialogue != "")
                                {
                                    dialogue = xboxDialogue;
                                }
                            #endif
                            zodiacDialogue.zodiacs[zodiacIndex].haveTalkedTo = true;
                            StartCoroutine(TypeDialogue(dialogue, 0.01f));
                            currentTalk = true;
                        }
                        else
                        {
                            EndTalk();
                        }

                    }
                }
                else
                {
                    if (!isRunning)
                    {
                        if (!currentTalk)
                        {
                            if (dialogueIndex < zodiacDialogue.zodiacs[zodiacIndex].randomDialogues.Length - 1)
                            {
                                dialogueIndex++;
                            }
                            else
                            {
                                dialogueIndex = 0;
                            }
                            dialogueText.text = "";
                            string dialogue = zodiacDialogue.zodiacs[zodiacIndex].randomDialogues[dialogueIndex];
                            string xboxDialogue = zodiacDialogue.zodiacs[zodiacIndex].xboxText;

                            //xbox build
                            #if UNITY_WSA
                            //if the xbox dialogue is not empty and we are in UWP build, use xbox dialogue
                            if (xboxDialogue != "")
                            {
                                dialogue = xboxDialogue;
                            }
                            #endif
                            StartCoroutine(TypeDialogue(dialogue, 0.01f));
                            currentTalk = true;
                        }
                        else
                        {
                            EndTalk();
                        }
                    }
                }
                Debug.Log("Current Talk: " + currentTalk);
            }
        }
    }

    private IEnumerator TypeDialogue(string dialogue, float speed)
    {
        isRunning = true;
        dialogueText.text = dialogue;
        dialogueText.maxVisibleCharacters = 0;

        bool input = Input.GetAxisRaw("Action1") > 0;
        bool oldInput = input;

        for (float t = 0; dialogueText.maxVisibleCharacters < dialogue.Length; t += Time.deltaTime)
        {
            dialogueText.maxVisibleCharacters = (int)(t * textSpeed);

            if (input && !oldInput)
            {
                // consume input
                oldInput = input;
                dialogueText.maxVisibleCharacters = dialogue.Length;
            }

            yield return null;
        }
        isRunning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            string TalkButton = "Space";
            #if UNITY_WSA
                //xbox version of control
                TalkButton = "A";
            #endif
            isTalking = true;
            other.GetComponent<PlayerMovement>().actionIndicator.Show(TalkButton, "Talk");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isTalking = false;
            EndTalk();
            other.GetComponent<PlayerMovement>().actionIndicator.Hide();
        }
    }

    private void EndTalk()
    {
        anim.SetBool("isTalking", false);
        Debug.Log("Exited " + this.gameObject.name + " space.");
        dialogueCamera.SetActive(false);
        dialogueCanvas.SetActive(false);
        dialogueText.text = "";
        currentTalk = false;
    }
}
