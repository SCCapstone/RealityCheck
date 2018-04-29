using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Linq;

public class NewRoomScript : MonoBehaviour {

    public GameObject player;

    public GameObject MenuPanel;
    public GameObject PropertiesPanel;
    public GameObject TutorialPanel;

    public GameObject LocalPlayer;

    public GameObject VirtualKeyboardCanvas;
    public GameObject VirtualKeyboardLayout;
    public InputField keyboardInputField;

    public GameObject SearchResultsPanel;
    public InputField SearchResultsInputField;
    public Button SearchButtonPrefab;

    public Button MicButton;
    public AudioSource SpeechToTextAudioSource;

    public GameObject SavePanel;
    public InputField SaveInputField;
    public GameObject SaveInstructions;
    public GameObject SaveSlot1Button;
    public GameObject SaveSlot2Button;
    public GameObject SaveSlot3Button;
    public GameObject SaveSlot1ButtonText;
    public GameObject SaveSlot2ButtonText;
    public GameObject SaveSlot3ButtonText;

    public Shader Standard;
    public Shader BumpedDiffuse;
    public Shader BumpedSpecular;

    public GameObject musicVolumeSliderArea;
    public GameObject musicVolumeSlider;
    public GameObject musicVolumeText;
    public GameObject maxVolumePos;
    public GameObject minVolumePos;
    public AudioSource music;

    public GameObject[] videoObjects;
    public VideoPlayer[] videos;

    private bool videoIsPlaying = false;

    private bool RTriggerHeld;
    private bool LTriggerHeld;

    private double LDownTime;
    private double RDownTime;
    private double triggerTime = 5.0; // 5 milliseconds
    private bool RTriggerDown;
    private bool LTriggerDown;

    private string menuHoverButton;
    private string tutorialHoverButton;
    private string saveHoverButton;

    private GameObject rayCastEndSphere;
    private LineRenderer rayCastLineRenderer;
    private string keyboardSource;
    private string hoveredKey;
    private double keyHoldTime = 500.0; // 500 milliseconds
    private double keyStepTime = 200.0; // 200 milliseconds
    private double keyLastStepTime;
    private double keyDownStartTime;

    private Color normalButton = new Color(0.3f, 0.3f, 0.3f);
    private Color highlightButton = new Color(0.6f, 0.6f, 0.6f);
    private Color selectButton = new Color(0.6f, 0.6f, 10f);

    private Color normalInput = new Color(1.0f, 1.0f, 1.0f);
    private Color highlightInput = new Color(0.8f, 0.8f, 0.8f);
    private Color selectInput = new Color(0.6f, 0.6f, 10f);

    private string searchButtonHover;
    private int currentSearchPage;
    private int numberOfSearchPages;
    private string downloadModelName;

    private Search.SearchResult searchResults;

    private bool isLoadingInNewModel = false;
    private List<GameObject> lastNewLoadedModels;
    public GameObject loadingCircle;
    private List<UserAssetState> modelStates;

    private RecordingService recordingService;
    private bool voiceRecordingInProgress;

    private List<GameObject> userAssets = new List<GameObject>();

    private GameState loadedInGameState;

    private string roomSaveName;
    private int roomSaveSlot;

    private int loadIndex = 0;
    private bool allModelsLoadedFromStart = false;
    private bool needLoadFromStart = true;
    private bool isLoadingFromStart = true;

    /// <summary>
    /// This class is the powerhouse of the application.
    /// Everything in each of the design rooms excepted the 
    /// properties panels is managed through this class.
    /// 
    /// Main menu, searching, keyboard input, music volume, tutorial videos, 
    /// saving, and exiting to the start room. 
    /// </summary>
    
    //initalizes everything at start
    void Start ()
    {
        modelStates = new List<UserAssetState>();
        lastNewLoadedModels = new List<GameObject>();
        loadingCircle.SetActive(false);
        MenuPanel.SetActive(false);
        TutorialPanel.SetActive(false);
        SavePanel.SetActive(false);
        SaveInstructions.SetActive(false);
        SaveSlot1Button.SetActive(false);
        SaveSlot2Button.SetActive(false);
        SaveSlot3Button.SetActive(false);
        VirtualKeyboardCanvas.SetActive(false);
        SearchResultsPanel.SetActive(false);

        keyboardSource = "";
        currentSearchPage = 1;
        LTriggerDown = false;
        RTriggerDown = false;
        voiceRecordingInProgress = false;

        SearchService.Instance.Flush();

        ModelLoaderService.Instance.BumpedSpecular = BumpedSpecular;
        ModelLoaderService.Instance.BumpedDiffuse = BumpedDiffuse;
        ModelLoaderService.Instance.Standard = Standard;

        recordingService = SpeechToTextAudioSource.GetComponent<RecordingService>();

        if (MainRoomSettings.musicVolume == -1)
        {
            MainRoomSettings.musicVolume = music.volume * 100.0f;
        }

        updateSliderFromPercentage(MainRoomSettings.musicVolume);
        music.volume = MainRoomSettings.musicVolume * 0.01f;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Gather the raycast end sphere to check where the pointer terminates.
        // This will be used later on to update UI and change the color of
        // the sphere.
        if (rayCastEndSphere == null)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
        }

        // Gather the line render of the raycast. This will be used later
        // to change the color of the line.
        if (rayCastLineRenderer == null)
        {
            rayCastLineRenderer = GameObject.Find("_GameMaster").GetComponent<LineRenderer>();
        }
        
        // Update the color of the bottons and find which 
        // button is hovered over in the active panel.
        if (VirtualKeyboardCanvas.activeSelf)
        {
            findHoverKey();
        }
        else if (SearchResultsPanel.activeSelf)
        {
            findSearchHoverButton();
        }
        else if (MenuPanel.activeSelf)
        {
            findMenuHoverButton();
            updateMusicSlider();
        }
        else if (TutorialPanel.activeSelf)
        {
            findTutorialHoverButton();
        }
        else if (SavePanel.activeSelf)
        {
            findSaveHoverButton();
        }

        // Update trigger input
        RTriggerDown = getRightTriggerDown();
        LTriggerDown = getLeftTriggerDown();
        bool keyButtonOverride = false;

        // This is used to check if a key is held down.
        // This is used to delete multiple characters while the trigger is held
        // while using the keyboard
        if (Input.GetAxisRaw("RightTrigger") > 0.2f || Input.GetAxisRaw("LeftTrigger") > 0.2f)
        {
            if (NowMilliseconds() - keyDownStartTime > keyHoldTime)
            {
                keyButtonOverride = true;
            }
        }
        else
        {
            keyDownStartTime = 0;
        }

        // If the player is loading in a new model, sync the position
        // of the model with where the raycast is pointing
        if (isLoadingInNewModel)
        {
            syncNewModelPosition();
        }
        
        // If either of the triggers are pressed, activate the respective
        // button or event based on where the raycaster is pointing
        if (RTriggerDown || LTriggerDown)
        {
            if (isLoadingInNewModel)
            {
                if (lastNewLoadedModels.Count != 0)
                {
                    // Now that the model has been loaded in and the player 
                    // has decided where to place it, add all the required
                    // scripts and a rigidbody to it.
                    StartCoroutine(placeNewLoadedModel());
                }
            }
            else if (VirtualKeyboardCanvas.activeSelf && hoveredKey != "")
            {
                keyDownStartTime = NowMilliseconds();
                activateKeyboard();
            }
            else if (SearchResultsPanel.activeSelf && searchButtonHover != "")
            {
                activateSeachResults();
            }
            else if (MenuPanel.activeSelf && menuHoverButton != "")
            {
                activateMenu();
            }
            else if (TutorialPanel.activeSelf && tutorialHoverButton != "")
            {
                activateTutorial();
            }
            else if (SavePanel.activeSelf && saveHoverButton != "")
            {
                activateSavePanel();
            }
        }
        // If the back key is held on the keyboard, remove a 
        // character every 0.2 seconds
        else if (keyButtonOverride && VirtualKeyboardCanvas.activeSelf && hoveredKey == "Back")
        {
            if (NowMilliseconds() - keyLastStepTime > keyStepTime)
            {
                keyLastStepTime = NowMilliseconds();
                activateKeyboard();
            }
        }
        // If either of the top menu buttons are pressed, cancel 
        // loading a model close all menus or open the menu panel
        // depending on the current event
        else if (Input.GetButtonDown("YButton") || Input.GetButtonDown("BButton")) //Also Start button for Vive
        {
            if (isLoadingInNewModel)
            {
                cancelNewLoadedModel();
            }
            if (MenuPanel.activeSelf)
            {
                MenuPanel.SetActive(false);
                PropertiesPanel.SetActive(false);
                SearchResultsPanel.SetActive(false);
                VirtualKeyboardCanvas.SetActive(false);
                SavePanel.SetActive(false);
                TutorialPanel.SetActive(false);
                for (int i = 0; i < videoObjects.Length; i++)
                {
                    videoObjects[i].SetActive(false);
                    videos[i].Stop();
                    videoIsPlaying = false;
                }
            }
            else
            {
                MenuPanel.SetActive(true);
                PropertiesPanel.SetActive(false);
                SearchResultsPanel.SetActive(false);
                VirtualKeyboardCanvas.SetActive(false);
                SavePanel.SetActive(false);
                TutorialPanel.SetActive(false);
                for (int i = 0; i < videoObjects.Length; i++)
                {
                    videoObjects[i].SetActive(false);
                    videos[i].Stop();
                    videoIsPlaying = false;
                }
            }
        }
        // If a tutorial video was selected, only show the
        // panel required to show that video until the video
        // is finished.
        else if (videoIsPlaying)
        {
            for (int i = 0; i < videos.Length; i++)
            {
                if (!videos[i].isPlaying)
                {
                    videoObjects[i].SetActive(false);
                }
            }
        }

        // Once all neccessary scripts and assests are loaded in, load in
        // the selected save slot if the save slot exists.
        if (!allModelsLoadedFromStart)
        {
            if (needLoadFromStart)
            {
                needLoadFromStart = false;
                loadLevelOnStart();
            }
            else if (!isLoadingFromStart)
            {
                // Once all models from load are loaded in
                // Add in their rigidbodies.
                // This delay enforces models to stay in place and
                // not be effected by physics. That way stacked models
                // do not fall over while models below then
                // are waiting to be loaded in. 
                allModelsLoadedFromStart = true;
                StartCoroutine(addRigidBodiesToStartUpModels());
            }
        }
    }

    //gets rather or not the right trigger is down
    private bool getRightTriggerDown()
    {
        float pressure = Input.GetAxisRaw("RightTrigger");
        bool down = pressure > 0.2f;
        if (down)
        {
            if (RTriggerHeld)
            {
                if (NowMilliseconds() - RDownTime < triggerTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                RDownTime = NowMilliseconds();
                RTriggerHeld = true;
                return true;
            }
        }
        else
        {
            RTriggerHeld = false;
            return false;
        }
    }

    //gets rather or not the left triger is down
    private bool getLeftTriggerDown()
    {
        float pressure = Input.GetAxisRaw("LeftTrigger");
        bool down = pressure > 0.2f;
        if (down)
        {
            if (LTriggerHeld)
            {
                if (NowMilliseconds() - LDownTime < triggerTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LDownTime = NowMilliseconds();
                LTriggerHeld = true;
                return true;
            }
        }
        else
        {
            LTriggerHeld = false;
            return false;
        }
    }

    //current milli seconds that have passes
    private double NowMilliseconds()
    {
        return (System.DateTime.UtcNow -
                new System.DateTime(1970, 1, 1, 0, 0, 0,
                    System.DateTimeKind.Utc)).TotalMilliseconds;
    }

    // find if the raycaster is accurately colliding with the box collider
    private bool CheckBoxCollision(BoxCollider collider, Vector3 point)
    {
        Vector3 posToCheck = point;
        Vector3 offset = collider.bounds.center - posToCheck;
        posToCheck = point + offset * 0.25f;
        offset = collider.bounds.center - posToCheck;
        Ray inputRay = new Ray(posToCheck, offset.normalized);
        RaycastHit rHit;

        return !collider.Raycast(inputRay, out rHit, offset.magnitude * 1.1f);
    }

    // Find what button is hovered over in the main menu
    // Update the colors on the buttons
    private void findMenuHoverButton()
    {
        menuHoverButton = "";
        for (int i = 0; i < MenuPanel.transform.childCount; i++)
        {
            Transform transform = MenuPanel.transform.GetChild(i);
            if (transform.gameObject.name.Contains("Button"))
            {
                Button currentButton = transform.gameObject.GetComponent<Button>();
                ColorBlock cb = currentButton.colors;
                if (CheckBoxCollision(transform.gameObject.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
                {
                    menuHoverButton = transform.GetChild(0).gameObject.GetComponent<Text>().text;
                    if (RTriggerDown || LTriggerDown)
                    {
                        cb.normalColor = selectButton;
                    }
                    else
                    {
                        cb.normalColor = highlightButton;
                    }
                }
                else
                {
                    cb.normalColor = normalButton;
                }
                currentButton.colors = cb;
            }
        }
    }

    // Find what button is hovered over in the tutorial menu
    // Update the colors on the buttons
    private void findTutorialHoverButton()
    {
        tutorialHoverButton = "";
        for (int i = 0; i < TutorialPanel.transform.childCount; i++)
        {
            Transform transform = TutorialPanel.transform.GetChild(i);
            if (transform.gameObject.name.Contains("Button"))
            {
                Button currentButton = transform.gameObject.GetComponent<Button>();
                ColorBlock cb = currentButton.colors;
                if (CheckBoxCollision(transform.gameObject.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
                {
                    tutorialHoverButton = transform.GetChild(0).gameObject.GetComponent<Text>().text;
                    if (RTriggerDown || LTriggerDown)
                    {
                        cb.normalColor = selectButton;
                    }
                    else
                    {
                        cb.normalColor = highlightButton;
                    }
                }
                else
                {
                    cb.normalColor = normalButton;
                }
                currentButton.colors = cb;
            }
        }
    }

    // Find what button is hovered over on the keyboard
    // Update the colors on the buttons
    private void findHoverKey()
    {
        if (voiceRecordingInProgress)
        {
            ColorBlock cb = MicButton.colors;
            float volume = recordingService.GetMicVolume();
            volume = Mathf.Clamp(volume * 50.0f, 0.25f, 1.0f);
            Color newColor = new Color(1.0f, 0.0f, 0.0f, volume);
            cb.normalColor = newColor;
            MicButton.colors = cb;
        }

        hoveredKey = "";
        for (int i = 0; i < VirtualKeyboardLayout.transform.childCount; i++)
        {
            GameObject keyBox = VirtualKeyboardLayout.transform.GetChild(i).gameObject;
            Button button = keyBox.GetComponent<Button>();
            ColorBlock cb = button.colors;
            if (CheckBoxCollision(keyBox.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
            {
                hoveredKey = keyBox.transform.GetChild(0).gameObject.GetComponent<Text>().text;

                //if (hoveredKey == "Mic" && voiceRecordingInProgress)
                //{
                 //   continue;
                //}

                if (RTriggerDown || LTriggerDown)
                {
                    cb.normalColor = selectButton;
                }
                else
                {
                    cb.normalColor = highlightButton;
                }
            }
            else
            {
                cb.normalColor = normalButton;
            }
            button.colors = cb;
        }
    }

    // Find what button or input is hovered over in the search menu
    // Update the colors on the buttons
    private void findSearchHoverButton()
    {
        //Go through each of the children of the search results panel
        //First check if the child is a button or a page holder
        //If its a button, set the searchHoverButton to equal its text
        //Else go through the active page's children

        searchButtonHover = "";
        bool MainButtonsHoveredOver = false;
        for (int i = 0; i < SearchResultsPanel.transform.childCount; i++)
        {
            Transform transform = SearchResultsPanel.transform.GetChild(i);
            if (transform.gameObject == SearchResultsInputField.gameObject)
            {
                ColorBlock cb = SearchResultsInputField.colors;
                if (CheckBoxCollision(transform.gameObject.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
                {
                    searchButtonHover = "New";
                    MainButtonsHoveredOver = true;
                    if (RTriggerDown || LTriggerDown)
                    {
                        cb.normalColor = selectInput;
                    }
                    else
                    {
                        cb.normalColor = highlightInput;
                    }
                }
                else
                {
                    cb.normalColor = normalInput;
                }
                SearchResultsInputField.colors = cb;
            }
            else if (transform.gameObject.name.Contains("Button"))
            {
                Button currentButton = transform.gameObject.GetComponent<Button>();
                ColorBlock cb = currentButton.colors;
                if (CheckBoxCollision(transform.gameObject.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
                {
                    searchButtonHover = transform.GetChild(0).gameObject.GetComponent<Text>().text;
                    MainButtonsHoveredOver = true;
                    if (RTriggerDown || LTriggerDown)
                    {
                        cb.normalColor = selectButton;
                    }
                    else
                    {
                        cb.normalColor = highlightButton;
                    }
                }
                else
                {
                    cb.normalColor = normalButton;
                }
                currentButton.colors = cb;
            }
            else if (transform.gameObject.name.Contains("Page"))
            {
                if (transform.gameObject.activeSelf)
                {
                    for (int j = 0; j < transform.childCount; j++)
                    {
                        Transform searchResult = transform.GetChild(j);
                        if (searchResult.gameObject.name.Contains("Button"))
                        {
                            Button currentButton = searchResult.gameObject.GetComponent<Button>();
                            ColorBlock cb = currentButton.colors;
                            BoxCollider boxCollider = searchResult.gameObject.GetComponent<BoxCollider>();
                            if (boxCollider != null && CheckBoxCollision(boxCollider, rayCastEndSphere.transform.position) && !MainButtonsHoveredOver)
                            {
                                MainButtonsHoveredOver = true;
                                searchButtonHover = searchResult.GetChild(0).gameObject.GetComponent<Text>().text;
                                if (RTriggerDown || LTriggerDown)
                                {
                                    cb.normalColor = selectButton;
                                }
                                else
                                {
                                    cb.normalColor = highlightButton;
                                }
                            }
                            else
                            {
                                cb.normalColor = normalButton;
                            }
                            currentButton.colors = cb;
                        }
                    }
                }
            }
        }
    }

    // Find what button or input is hovered over in the save menu
    // Update the colors on the buttons
    private void findSaveHoverButton()
    {
        SaveSlot1Button.SetActive(SaveInstructions.activeSelf);
        SaveSlot2Button.SetActive(SaveInstructions.activeSelf);
        SaveSlot3Button.SetActive(SaveInstructions.activeSelf);

        saveHoverButton = "";
        for (int i = 0; i < SavePanel.transform.childCount; i++)
        {
            Transform transform = SavePanel.transform.GetChild(i);
            if (transform.gameObject == SaveInputField.gameObject)
            {
                ColorBlock cb = SaveInputField.colors;
                if (CheckBoxCollision(transform.gameObject.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
                {
                    saveHoverButton = "SaveNameChange";
                    if (RTriggerDown || LTriggerDown)
                    {
                        cb.normalColor = selectInput;
                    }
                    else
                    {
                        cb.normalColor = highlightInput;
                    }
                }
                else
                {
                    cb.normalColor = normalInput;
                }
                SaveInputField.colors = cb;
            }
            else if (transform.gameObject.activeSelf && transform.gameObject.name.Contains("Button"))
            {
                Button currentButton = transform.gameObject.GetComponent<Button>();
                ColorBlock cb = currentButton.colors;
                if (CheckBoxCollision(transform.gameObject.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
                {
                    saveHoverButton = transform.GetChild(0).gameObject.GetComponent<Text>().text;
                    if (RTriggerDown || LTriggerDown)
                    {
                        cb.normalColor = selectButton;
                    }
                    else
                    {
                        cb.normalColor = highlightButton;
                    }
                }
                else
                {
                    cb.normalColor = normalButton;
                }
                currentButton.colors = cb;
            }
        }
    }

    // If a button was selected in the main menu,
    // activate its respective event.
    private void activateMenu()
    {
        if (menuHoverButton == "Exit To Lobby")
        {
            SceneManager.LoadScene("scene", LoadSceneMode.Single);
        }
        else if (menuHoverButton == "Add Model")
        {
            MenuPanel.SetActive(false);
            SearchResultsPanel.SetActive(true);
        }
        else if (menuHoverButton == "Save")
        {
            MenuPanel.SetActive(false);
            SavePanel.SetActive(true);
        }
        else if (menuHoverButton == "Tutorials")
        {
            MenuPanel.SetActive(false);
            TutorialPanel.SetActive(true);
        }
        else if (menuHoverButton == "Close")
        {
            MenuPanel.SetActive(false);
        }
    }

    // If a button was selected in the tutorial menu,
    // activate its respective event.
    private void activateTutorial()
    {
        if (tutorialHoverButton == "Teleport")
        {
            TutorialPanel.SetActive(false);
            videoObjects[0].SetActive(true);
            videos[0].Play();
            videoIsPlaying = true;
        }
        else if (tutorialHoverButton == "Grab")
        {
            TutorialPanel.SetActive(false);
            videoObjects[1].SetActive(true);
            videos[1].Play();
            videoIsPlaying = true;
        }
        else if (tutorialHoverButton == "Search")
        {
            TutorialPanel.SetActive(false);
            videoObjects[2].SetActive(true);
            videos[2].Play();
            videoIsPlaying = true;
        }
        else if (tutorialHoverButton == "Spawn")
        {
            TutorialPanel.SetActive(false);
            videoObjects[3].SetActive(true);
            videos[3].Play();
            videoIsPlaying = true;
        }
        else if (tutorialHoverButton == "Properties pt. 1")
        {
            TutorialPanel.SetActive(false);
            videoObjects[4].SetActive(true);
            videos[4].Play();
            videoIsPlaying = true;
        }
        else if (tutorialHoverButton == "Properties pt. 2")
        {
            TutorialPanel.SetActive(false);
            videoObjects[5].SetActive(true);
            videos[5].Play();
            videoIsPlaying = true;
        }
        else if (tutorialHoverButton == "Close")
        {
            TutorialPanel.SetActive(false);
        }
    }

    // If a button was selected on the keyboard,
    // activate its respective event.
    private void activateKeyboard()
    {
        if (voiceRecordingInProgress)
        {
            if (hoveredKey == "Mic")
            {
                toggleVoiceRecording();
            }
        }
        else
        {
            if (hoveredKey == "Space")
            {
                keyboardInputField.text += " ";
            }
            else if (hoveredKey == "Back")
            {
                if (keyboardInputField.text.Length > 0)
                {
                    keyboardInputField.text = keyboardInputField.text.Substring(0, keyboardInputField.text.Length - 1);
                }
            }
            else if (hoveredKey == "Clear")
            {
                keyboardInputField.text = "";
            }
            else if (hoveredKey == "Done")
            {
                // Find with panel called upon the keyboard
                // and return the final string result
                if (keyboardSource == "NewSearch")
                {
                    updateSearchInput();
                }
                else if (keyboardSource == "SaveNameChange")
                {
                    updateSaveInput();
                }
            }
            else if (hoveredKey == "Mic")
            {
                toggleVoiceRecording();
            }
            else
            {
                keyboardInputField.text += hoveredKey.ToLower();

                // Cut off any characters after the 20th character. This
                // enforces a limit of 20 characters for a save name.
                if (keyboardSource == "SaveNameChange" && keyboardInputField.text.Length > 20)
                {
                    keyboardInputField.text = keyboardInputField.text.Substring(0, 20);
                }
            }
        }
    }

    // If a button or input was selected in the search menu,
    // activate its respective event.
    private void activateSeachResults()
    {
        if (searchButtonHover == "New")
        {
            updateKeyboardPosition();

            keyboardSource = "NewSearch";
            keyboardInputField.text = SearchResultsInputField.text;
            SearchResultsPanel.SetActive(false);
            VirtualKeyboardCanvas.SetActive(true);
        }
        else if (searchButtonHover == "Search")
        {
            keyboardInputField.text = SearchResultsInputField.text;
            currentSearchPage = 1;
            PerformSearch();
        }
        else if (searchButtonHover == "Last Page")
        {
            if (currentSearchPage > 1)
            {
                currentSearchPage--;
                PerformSearch();
            }
        }
        else if (searchButtonHover == "Next Page")
        {
            if (currentSearchPage <= numberOfSearchPages)
            {
                currentSearchPage++;
                PerformSearch();
            }
        }
        else if (searchButtonHover == "Close")
        {
            SearchResultsPanel.SetActive(false);
        }
        else
        {
            if (searchResults!= null && searchResults.Count > 0)
            {
                int index;
                if (int.TryParse(searchButtonHover, out index))
                {
                    downloadModelAtIndex(index);
                }
            }
        }
    }

    // If a button or input was selected in the save menu,
    // activate its respective event.
    private void activateSavePanel()
    {
        if (saveHoverButton == "SaveNameChange")
        {
            updateKeyboardPosition();

            keyboardSource = "SaveNameChange";
            keyboardInputField.text = SaveInputField.text;
            SavePanel.SetActive(false);
            VirtualKeyboardCanvas.SetActive(true);
        }
        else if (saveHoverButton == "Save Room")
        {
            // If the room loaded in with the save slot index as 3, then
            // all 3 slots contain a save slot and the current room
            // may only be saved by overriding a save slot.
            // Display the slot selection instructions is this is the case.
            // Else save the current state of the scene to the respective save slot.
            if (roomSaveSlot == 3)
            {
                SaveInstructions.SetActive(true);
            }
            else if (!SaveInstructions.activeSelf)
            {
                SavePanel.SetActive(false);
                roomSaveName = SaveInputField.text;
                SaveLoadService.Instance.Save(roomSaveSlot,
                    roomSaveName + " " + SceneManager.GetActiveScene().name, userAssets);
            }
        }
        // If slot 1 (aka index 0) was selected. Override the current state of the scene
        // to slot index 0.
        else if (saveHoverButton.Contains("Slot 1:"))
        {
            SaveInstructions.SetActive(false);
            roomSaveSlot = 0;
            SaveLoadService.Instance.Slot = 0;
            roomSaveName = SaveInputField.text;
            SaveSlot1ButtonText.GetComponent<Text>().text = "Slot 1: " + roomSaveName;
            SaveLoadService.Instance.Save(roomSaveSlot, roomSaveName + " " + SceneManager.GetActiveScene().name, userAssets);
        }
        // If slot 2 (aka index 1) was selected. Override the current state of the scene
        // to slot index 1.
        else if (saveHoverButton.Contains("Slot 2:"))
        {
            SaveInstructions.SetActive(false);
            roomSaveSlot = 1;
            SaveLoadService.Instance.Slot = 1;
            roomSaveName = SaveInputField.text;
            SaveSlot2ButtonText.GetComponent<Text>().text = "Slot 2: " + roomSaveName;
            SaveLoadService.Instance.Save(roomSaveSlot,
                    roomSaveName + " " + SceneManager.GetActiveScene().name, userAssets);
        }
        // If slot 3 (aka index 2) was selected. Override the current state of the scene
        // to slot index 2.
        else if (saveHoverButton.Contains("Slot 3:"))
        {
            SaveInstructions.SetActive(false);
            roomSaveSlot = 2;
            SaveLoadService.Instance.Slot = 2;
            roomSaveName = SaveInputField.text;
            SaveSlot3ButtonText.GetComponent<Text>().text = "Slot 3: " + roomSaveName;
            SaveLoadService.Instance.Save(roomSaveSlot,
                    roomSaveName + " " + SceneManager.GetActiveScene().name, userAssets);
        }
        else if (saveHoverButton == "Close")
        {
            SavePanel.SetActive(false);
        }
    }

    // When the keyboard is called upon, this method must be
    // called before as to update the position of the keyboard UI.
    // This forces the keyboard to be placed in front of where
    // the player is facing a faces to keyboard to point towards
    // the player.
    private void updateKeyboardPosition()
    {
        Vector3 playerRot = LocalPlayer.transform.localRotation.eulerAngles;
        Vector3 playerPos = LocalPlayer.transform.localPosition;

        Vector3 defaultLocalKeyboardRot = new Vector3(30.0f, 0.0f, 0.0f); // In degrees
        Vector3 defaultLocalKeyboardPos = new Vector3(-0.327f, 1.0f, -0.02f);

        Vector3 newRot = new Vector3(0.0f, playerRot.y, 0.0f) + defaultLocalKeyboardRot;
        VirtualKeyboardCanvas.transform.localRotation = Quaternion.Euler(newRot.x, newRot.y, newRot.z);

        float radius = 1.0f;
        float angle = Mathf.Deg2Rad * newRot.y;
        float x = radius * Mathf.Sin(angle);
        float z = radius * Mathf.Cos(angle);

        Vector3 newPos = new Vector3(playerPos.x, 1.0f, playerPos.z) + new Vector3(x, 0.0f, z);
        VirtualKeyboardCanvas.transform.localPosition = newPos;
    }

    // Gather the text from the keyboard and assign it
    // to the text box for the search input
    // and close the keyboard.
    private void updateSearchInput()
    {
        SearchResultsInputField.text = keyboardInputField.text;
        keyboardInputField.text = "";
        SearchResultsPanel.SetActive(true);
        VirtualKeyboardCanvas.SetActive(false);
    }

    // Gather the text from the keyboard and assign it
    // to the text box for the save name input
    // and close the keyboard.
    private void updateSaveInput()
    {
        SaveInputField.text = keyboardInputField.text;
        keyboardInputField.text = "";
        SavePanel.SetActive(true);
        VirtualKeyboardCanvas.SetActive(false);
    }

    // Clear all UI related to any old search results.
    // Gather the input string query from the search menu,
    // perform the search, find how many pages are needed to display
    // the search results. The max number of search results per page
    // is 4. Then update the search results UI to represent each of
    // of the search results as well as delegate the button events
    // for each of the search results.
    private void PerformSearch()
    {
        clearResultsUI();

        // Run search algorithm
        // And update search results UI

        string query = SearchResultsInputField.text;

        if (!string.IsNullOrEmpty(query))
        {
            SearchService.Instance.Flush();

            var req = new Search.SearchRequest{
                Query = query,
                PageNumber = currentSearchPage,
                ResultPerPage = 4
            };

            SearchService.Instance.Search(req, res => {
                searchResults = res;
                updateResultsUI();
                numberOfSearchPages = (int)Math.Floor(((double)res.Count) / 4.0f);
            });
        }
    }

    // Clear any and all pages and their data from the search results menu
    private void clearResultsUI()
    {
        List<GameObject> objectsToDelete = new List<GameObject>();

        for (int i = 0; i < SearchResultsPanel.transform.childCount; i++)
        {
            Transform transform = SearchResultsPanel.transform.GetChild(i);
            if (transform.gameObject.name.Contains("Page"))
            {
                objectsToDelete.Add(transform.gameObject);
            }
        }

        for (int i = 0; i < objectsToDelete.Count; i++)
        {
            Destroy(objectsToDelete[i]);
        }
    }

    // For each page, place it on the search results page
    // with the correct transformation. Then, for each search
    // results on each page, instaniate the SearchButtonPrefab prefab
    // for each button and assign the correct transformation to it.
    // Finally change the text in the button to match the name
    // of the model mathcing the search result.
    private void updateResultsUI()
    {
        int modelIndex = 0;

        GameObject page = Instantiate(new GameObject());
        page.name = "Page 1";
        page.transform.SetParent(SearchResultsPanel.transform);
        page.transform.localPosition = new Vector3(0.55f, -4.0f, 0.0f);
        page.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        page.transform.localScale = new Vector3(200.0f, 200.0f, 1.0f);

        for (int j = 1; j <= 4; j++)
        {
            if (modelIndex < searchResults.Count)
            {
                Button item = Instantiate(SearchButtonPrefab);
                item.transform.SetParent(page.transform);

                float xPos = j % 2 == 0 ? 0.12f : -0.12f;
                float yPos = j > 2 ? -0.12f : 0.09f;

                item.gameObject.layer = 0;
                item.transform.localPosition = new Vector3(xPos, yPos, 0.0f);
                item.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                item.transform.localScale = new Vector3(0.005f, 0.005f, 1.0f);

                // Assign the search index name in the first child of the button.
                // This child is hidden and is used to match the button
                // to the search result index later on.
                item.transform.GetChild(0).gameObject.GetComponent<Text>().text = (modelIndex) + "";
                item.transform.GetChild(1).gameObject.GetComponent<Text>().text = searchResults.Hits[modelIndex].Asset.Name + "";

                modelIndex++;
            }
        }
    }

    // Given the search index, download the data for the model from the database.
    // Then change the color of the laser pointer to green to show the player
    // that they are in model placement mode.
    private void downloadModelAtIndex(int searchIndex)
    {
        // Only le the user download a model if one is not already trying to load.
        if (!isLoadingInNewModel && searchIndex < searchResults.Count && searchIndex >= 0)
        {
            isLoadingInNewModel = true;
            downloadModelName = searchResults.Hits[searchIndex].Asset.Filename;
            SearchService.Instance.DownloadModel(searchResults.Hits[searchIndex], nm =>
            {
                SearchResultsPanel.SetActive(false);
                isLoadingInNewModel = true;
                loadingCircle.SetActive(true);
                rayCastEndSphere.GetComponent<MeshRenderer>().material.color = Color.green;
                rayCastLineRenderer.startColor = Color.green;
                rayCastLineRenderer.endColor = Color.green;

                // Once the data is downloaded, parse the information into a useable Unity model,
                // Then call the callback to set up the bounding box for it.
                ModelLoaderService.Instance.LoadModel(nm, modelDoneLoadingCallback);
            });
        }
    }

    // This is the callback for when a model is done being parsed
    // from the model loader service.
    public void modelDoneLoadingCallback(GameObject lastLoadedModel)
    {
        finishUpModelLoad(lastLoadedModel);
    }

    // This method scales the the loaded in game object
    // to an arbitrary uniform height, and forces the game object to
    // be placed where the bottom of the model is on the floor.
    // If also adds a fully encapsulated bounding box to the game object.
    // Finally, if this method was called from the result of a model
    // being loaded from a save file, this model is
    // restored with the transform it was saved with.
    // If the model came back from the ModelLoaderService as null,
    // then the loading event is canceled.
    public void finishUpModelLoad(GameObject lastLoadedModel)
    {
        if (lastLoadedModel != null)
        {
            lastLoadedModel.name = downloadModelName;

            Quaternion currentRotation = lastLoadedModel.transform.rotation;
            lastLoadedModel.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Bounds bounds = new Bounds(lastLoadedModel.transform.position, Vector3.zero);
            foreach (Renderer renderer in lastLoadedModel.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            lastLoadedModel.AddComponent<BoxCollider>(); // Add Box Collider for physics

            BoxCollider collider = lastLoadedModel.GetComponent<BoxCollider>();
            collider.center = bounds.center - lastLoadedModel.transform.position;
            collider.size = bounds.size;

            float roomHeight = 1.0f;
            float heightDiff = (bounds.max.y - bounds.min.y) - roomHeight;

            float newScale = 1.0f;
            if (heightDiff > 0)
            {
                newScale = roomHeight / (bounds.max.y - bounds.min.y);
                lastLoadedModel.transform.localScale = new Vector3(newScale, newScale, newScale);
            }

            float diff = bounds.min.y * newScale;
            
            lastLoadedModel.GetComponent<BoxCollider>().enabled = false;

            GameObject parentObject = new GameObject();
            parentObject.name = downloadModelName;//" parent";
            lastLoadedModel.transform.parent = parentObject.transform;
            parentObject.transform.position = new Vector3(0, Mathf.Abs(diff), 0);
            
            setGameObjectLayer(parentObject, 2);
            
            if (modelStates.Count != 0)
            {
                UserAssetState state = modelStates[modelStates.Count - 1];
                parentObject.transform.position = new Vector3(state.pos.x, state.pos.y, state.pos.z);
                parentObject.transform.rotation = new Quaternion(state.rot.x, state.rot.y, state.rot.z, state.rot.w);
                parentObject.transform.localScale = new Vector3(state.scale.x, state.scale.y, state.scale.z);
                SearchService.Instance.Flush();
            }

            lastNewLoadedModels.Add(parentObject);

            if (loadedInGameState != null)
            {
                if (modelStates.Count >= loadedInGameState.assets.Count)
                {
                    Debug.Log("isLoadingFromStart set to false");
                    isLoadingFromStart = false;
                }
            }
        }
        else
        {
            cancelNewLoadedModel();
        }
    }
    
    // When a mode is loaded in, set its layer to Ignore Raycast
    // until the model is placed. This way the raycaster does not
    // collide with it and affect the placement event.
    private void setGameObjectLayer(GameObject model, int layer)
    {
        model.layer = layer;
        int childCount = model.transform.childCount;
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                setGameObjectLayer(model.transform.GetChild(i).gameObject, layer);
            }
        }
    }

    // For whatever reason the newly loaded model was canceled
    // reset the laser pointer to blue, and destroy any data related to the
    // newly loaded in model.
    private void cancelNewLoadedModel()
    {
        isLoadingInNewModel = false;
        loadingCircle.SetActive(false);
        rayCastEndSphere.GetComponent<MeshRenderer>().material.color = Color.blue;
        rayCastLineRenderer.startColor = Color.blue;
        rayCastLineRenderer.endColor = Color.blue;

        if (lastNewLoadedModels.Count != 0)
        {
            Destroy(lastNewLoadedModels[0]);
        }

        lastNewLoadedModels.Clear();
        //SearchService.Instance.Flush();
    }

    // When the player where to place the model after
    // loading it from the database, sync the location
    // of the model with where the user is pointing
    // the laser pointer. Also update the location
    // of the loading circle game object.
    private void syncNewModelPosition()
    {
        Vector3 rayCastPos = rayCastEndSphere.transform.position;
        Vector3 newPos = new Vector3(rayCastPos.x, 0, rayCastPos.z);

        if (lastNewLoadedModels.Count != 0)
        {
            lastNewLoadedModels[0].transform.position = newPos + 
                    new Vector3(0, lastNewLoadedModels[0].transform.position.y, 0);
        }
        
        if (loadingCircle.activeSelf)
        {
            loadingCircle.transform.position = newPos;
        }
    }

    // After a model is loaded from the database, and the
    // player has decided where they would like to place the
    // model, this method is called to add the required scripts
    // and a rigidbody. These scripts include SteamVR's 
    // VelocityEstimator, Interactable, and Throwable scripts.
    // Also add the userAsset script to be used later for saving
    // and altering the transform of the object through the properties
    // panel. This method returns an IEnumerator because there must
    // be a pause for adding the rigidbody or else the model
    // could potentially be moved or spin before the model is placed.
    // Finally set its layer back to default to allow raycasting.
    private IEnumerator placeNewLoadedModel()
    {
        if (lastNewLoadedModels.Count != 0)
        {
            isLoadingInNewModel = false;
            loadingCircle.SetActive(false);
            rayCastEndSphere.GetComponent<MeshRenderer>().material.color = Color.blue;
            rayCastLineRenderer.startColor = Color.blue;
            rayCastLineRenderer.endColor = Color.blue;

            GameObject lastNewLoadedModel = lastNewLoadedModels[0];
            
            lastNewLoadedModel.AddComponent<Valve.VR.InteractionSystem.VelocityEstimator>();
            lastNewLoadedModel.AddComponent<Valve.VR.InteractionSystem.Interactable>();
            lastNewLoadedModel.AddComponent<Valve.VR.InteractionSystem.Throwable>();

            Valve.VR.InteractionSystem.Throwable throwable = lastNewLoadedModel.GetComponent<Valve.VR.InteractionSystem.Throwable>();
            throwable.onPickUp = new UnityEngine.Events.UnityEvent();
            throwable.onDetachFromHand = new UnityEngine.Events.UnityEvent();

            lastNewLoadedModel.AddComponent<userAsset>(); 
            
            lastNewLoadedModel.transform.GetChild(0).gameObject.GetComponent<BoxCollider>().enabled = true;

            if (lastNewLoadedModel.GetComponent<Rigidbody>() == null)
            {
                lastNewLoadedModel.AddComponent<Rigidbody>(); // Add gravity rules for physics
            }

            Rigidbody rigidBody = lastNewLoadedModel.GetComponent<Rigidbody>();

            rigidBody.isKinematic = true;

            rigidBody.mass = 1000;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            yield return new WaitForSeconds(0.1f);

            rigidBody.isKinematic = false;

            setGameObjectLayer(lastNewLoadedModel, 0);
            
            userAssets.Add(lastNewLoadedModel);
            lastNewLoadedModels.Clear();
            
            SearchService.Instance.Flush();
        }
    }

    // Once all models from load are loaded in
    // Add in their rigidbodies.
    // This delay enforces models to stay in place and
    // not be effected by physics. That way stacked models
    // do not fall over while models below then
    // are waiting to be loaded in.
    // This method returns an IEnumerator because there must
    // be a pause for adding the rigidbody or else the model
    // could potentially be moved or spin before the model is placed.
    // Finally set its layer back to default to allow raycasting.
    private IEnumerator addRigidBodiesToStartUpModels()
    {
        for (int i = 0; i < lastNewLoadedModels.Count; i++)
        {
            GameObject lastNewLoadedModel = lastNewLoadedModels[i];

            lastNewLoadedModel.AddComponent<Valve.VR.InteractionSystem.VelocityEstimator>();
            lastNewLoadedModel.AddComponent<Valve.VR.InteractionSystem.Interactable>();
            lastNewLoadedModel.AddComponent<Valve.VR.InteractionSystem.Throwable>();
            
            Valve.VR.InteractionSystem.Throwable throwable = lastNewLoadedModel.GetComponent<Valve.VR.InteractionSystem.Throwable>();
            throwable.onPickUp = new UnityEngine.Events.UnityEvent();
            throwable.onDetachFromHand = new UnityEngine.Events.UnityEvent();

            lastNewLoadedModel.AddComponent<userAsset>();
            
            lastNewLoadedModel.transform.GetChild(0).gameObject.GetComponent<BoxCollider>().enabled = true;

            if (lastNewLoadedModel.GetComponent<Rigidbody>() == null)
            {
                lastNewLoadedModel.AddComponent<Rigidbody>(); // Add gravity rules for physics
            }
            
            Rigidbody rigidBody = lastNewLoadedModel.GetComponent<Rigidbody>();

            rigidBody.isKinematic = true;

            rigidBody.mass = 1000;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < lastNewLoadedModels.Count; i++)
        {
            GameObject lastNewLoadedModel = lastNewLoadedModels[i];
            Rigidbody rigidBody = lastNewLoadedModel.GetComponent<Rigidbody>();

            rigidBody.isKinematic = false;

            setGameObjectLayer(lastNewLoadedModel, 0);
            
            if (modelStates.Count != 0)
            {
                UserAssetState state = modelStates[0];

                lastNewLoadedModel.GetComponent<userAsset>().Gravity = state.gravity;
                lastNewLoadedModel.GetComponent<userAsset>().Physics();

                modelStates.Remove(state);
            }

            userAssets.Add(lastNewLoadedModel);
        }
        
        Debug.Log("allModelsLoadedFromStart set to true");

        allModelsLoadedFromStart = true;
        modelStates.Clear();
        lastNewLoadedModels.Clear();
        loadedInGameState = null;
    }
    
    // If a voice recording is active, hault it, and
    // transcribe the recording. The transcribe function
    // converts the voice recording into text and
    // updates the keyboard as seen in the callback
    // function in the else statement.
    // If the recording services times out after
    // 5 seconds, the callback is called after transcribe.
    private void toggleVoiceRecording()
    {
        if (recordingService.IsRecording())
        {
            voiceRecordingInProgress = false;
            ColorBlock cb = MicButton.colors;
            cb.normalColor = normalButton;
            MicButton.colors = cb;
            recordingService.Transcribe();
        }
        else
        {
            voiceRecordingInProgress = true;

            ColorBlock cb = MicButton.colors;
            cb.normalColor = Color.red;
            MicButton.colors = cb;

            recordingService.Record(text =>
            {
                voiceRecordingInProgress = false;
                if (keyboardInputField.text.LastIndexOf(" ") == keyboardInputField.text.Length - 1)
                {
                    keyboardInputField.text += text;
                }
                else
                {
                    keyboardInputField.text += " " + text;
                }

                // Force the total character count to be 20. This forces the 
                // save slot name to be less than or equal to 20 characters.
                if (keyboardSource == "SaveNameChange" && keyboardInputField.text.Length > 20)
                {
                    keyboardInputField.text = keyboardInputField.text.Substring(0, 20);
                }
            });
        }
    }
    
    // If the player is poiting at the music slider and
    // holding either of the triggers, updated the slider
    // and the music volume based on the raycaster.
    private void updateMusicSlider()
    {
        if (Input.GetAxisRaw("RightTrigger") > 0.2f || Input.GetAxisRaw("LeftTrigger") > 0.2f)
        {
            BoxCollider areaCollider = musicVolumeSliderArea.GetComponent<BoxCollider>();
            if (CheckBoxCollision(areaCollider, rayCastEndSphere.transform.position))
            {
                BoxCollider sliderCollider = musicVolumeSlider.GetComponent<BoxCollider>();
                float width = areaCollider.bounds.size.magnitude;

                Vector3 minPos = minVolumePos.transform.position;
                Vector3 maxPos = maxVolumePos.transform.position;

                float distanceBetweenMinAndMax = (maxPos - minPos).magnitude + sliderCollider.bounds.size.magnitude;
                float distanceToMin = (rayCastEndSphere.transform.position - minPos).magnitude;
                float distanceToMax = (rayCastEndSphere.transform.position - maxPos).magnitude;

                if (distanceToMax < distanceBetweenMinAndMax && distanceToMin < distanceBetweenMinAndMax)
                {
                    float percentage = Mathf.Clamp(((distanceToMin / width) * 100.0f - 5.0f),
                        0.0f, 100.0f);

                    music.volume = percentage * 0.01f;
                    updateSliderFromPercentage(percentage);
                }
            }
        }
    }

    // Updates the location of the position of the slider based on the
    // passed in percentage value.
    // Takes in a value from 0 to 100
    private void updateSliderFromPercentage(float percentage)
    {
        // Enfore the value to be between 0 and 100
        percentage = Mathf.Clamp(percentage, 0.0f, 100.0f);
        MainRoomSettings.musicVolume = percentage;

        musicVolumeText.GetComponent<Text>().text = ((int)Mathf.Round(percentage)).ToString() + "%";
        
        Vector3 minPos = minVolumePos.transform.localPosition;
        Vector3 maxPos = maxVolumePos.transform.localPosition;

        float percentDecimal = percentage * 0.01f;
        Vector3 newSliderPosition = 
            (minPos * (1.0f - percentDecimal)) +
            (maxPos * percentDecimal);

        musicVolumeSlider.transform.localPosition = newSliderPosition;
    }

    // If the user loaded in a new room even though 3 save slots
    // have been taken up, don't load any models from any save slot.
    // Load in the names of the save slots to be used later
    // to help the user decide which save slot to override.
    // Then update the save panel based on these names
    //
    // Else if the user loaded in one of the 3 three slots,
    // load in all data from the save slot except the image
    // related to it.
    // Each GameState contains a list of models that were saved
    // in the design as well as their transforms.
    private void loadLevelOnStart()
    {
        roomSaveSlot = SaveLoadService.Instance.Slot;
        if (roomSaveSlot == 3)
        {
            allModelsLoadedFromStart = true;

            GameState state1 = null;
            GameState state2 = null;
            GameState state3 = null;

            try
            {
                state1 = SaveLoadService.Instance.Load(0);
            }
            catch (Exception ex) { }

            try
            {
                state2 = SaveLoadService.Instance.Load(1);
            }
            catch (Exception ex) { }

            try
            {
                state3 = SaveLoadService.Instance.Load(2);
            }
            catch (Exception ex) { }

            if (state1 != null)
            {
                string fullName = state1.roomName;
                string inputName = fullName;
                int lastSpaceIndex = inputName.LastIndexOf(" ");
                inputName = inputName.Substring(0, lastSpaceIndex);
                SaveSlot1ButtonText.GetComponent<Text>().text = "Slot 1: " + inputName;
            }

            if (state2 != null)
            {
                string fullName = state2.roomName;
                string inputName = fullName;
                int lastSpaceIndex = inputName.LastIndexOf(" ");
                inputName = inputName.Substring(0, lastSpaceIndex);
                SaveSlot2ButtonText.GetComponent<Text>().text = "Slot 2: " + inputName;
            }

            if (state3 != null)
            {
                string fullName = state3.roomName;
                string inputName = fullName;
                int lastSpaceIndex = inputName.LastIndexOf(" ");
                inputName = inputName.Substring(0, lastSpaceIndex);
                SaveSlot3ButtonText.GetComponent<Text>().text = "Slot 3: " + inputName;
            }
        }
        else
        {
            GameState state = null;
            try
            {
                state = SaveLoadService.Instance.Load(roomSaveSlot);
            }
            catch (Exception ex)
            {
                Debug.Log("Failed to load at slot " + roomSaveSlot + " ex:" + ex);
            }

            Debug.Log(SceneManager.GetActiveScene().name);
            if (state != null && state.roomName.Contains(SceneManager.GetActiveScene().name))
            {
                loadedInGameState = state;
                loadIndex = 0;

                string inputName = state.roomName;
                int lastSpaceIndex = inputName.LastIndexOf(" ");
                inputName = inputName.Substring(0, lastSpaceIndex);

                roomSaveName = inputName;
                SaveInputField.text = inputName;

                onStartLoadAtIndex(state.assets);
            }
            else
            {
                allModelsLoadedFromStart = true;
            }
        }
    }

    // Recursively load in one model at a time to ensure
    // maximum correctness for each model. This method takes in a
    // list of UserAssetStates and loads in the lest model in line.
    // Then creates a callback for the model to be loaded in.
    private void onStartLoadAtIndex(List<UserAssetState> states)
    {
        if (loadIndex >= states.Count())
        {
            return;
        }

        Debug.Log("loading at index " + loadIndex);
        UserAssetState state = states[loadIndex];
        modelStates.Add(state);

        SearchService.Instance.DownloadModel(state, nm =>
        {
            downloadModelName = state.uuid;
            ModelLoaderService.Instance.LoadModel(nm, modelDoneLoadingCallback);

            loadIndex++;
            onStartLoadAtIndex(states);
        });
    }
}
