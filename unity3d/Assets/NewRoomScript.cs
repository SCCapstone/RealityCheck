using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

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
    private double triggerTime = 5.0; // 10 milliseconds
    private bool RTriggerDown;
    private bool LTriggerDown;

    private string menuHoverButton;
    private string tutorialHoverButton;

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

    private RecordingService recordingService;
    private bool voiceRecordingInProgress;

    // Use this for initialization
    void Start () {
        lastNewLoadedModels = new List<GameObject>();
        loadingCircle.SetActive(false);
        MenuPanel.SetActive(false);
        TutorialPanel.SetActive(false);
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

        updateNumberOfSearchPages();

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
        if (rayCastEndSphere == null)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
        }

        if (rayCastLineRenderer == null)
        {
            rayCastLineRenderer = GameObject.Find("_GameMaster").GetComponent<LineRenderer>();
        }

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

        RTriggerDown = getRightTriggerDown();
        LTriggerDown = getLeftTriggerDown();
        bool keyButtonOverride = false;

        if (Input.GetAxisRaw("RightTrigger") > 0.2f)
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

        if (isLoadingInNewModel)
        {
            syncNewModelPosition();
        }

        if (RTriggerDown)
        {
            if (isLoadingInNewModel)
            {
                if (lastNewLoadedModels.Count != 0)
                {
                    StartCoroutine(placeNewLoadedModel());
                }
            }
            if (VirtualKeyboardCanvas.activeSelf && hoveredKey != "")
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
            else if (TutorialPanel.activeSelf && menuHoverButton != "")
            {
                activateTutorial();
            }
        }
        else if (keyButtonOverride && VirtualKeyboardCanvas.activeSelf && hoveredKey == "Back")
        {
            if (NowMilliseconds() - keyLastStepTime > keyStepTime)
            {
                keyLastStepTime = NowMilliseconds();
                activateKeyboard();
            }
        }
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
                TutorialPanel.SetActive(false);
                for (int i = 0; i < videoObjects.Length; i++)
                {
                    videoObjects[i].SetActive(false);
                    videos[i].Stop();
                    videoIsPlaying = false;
                }
            }
        }
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
    }

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

    private double NowMilliseconds()
    {
        return (System.DateTime.UtcNow -
                new System.DateTime(1970, 1, 1, 0, 0, 0,
                    System.DateTimeKind.Utc)).TotalMilliseconds;
    }

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
                    if (RTriggerDown)
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
                    if (RTriggerDown)
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

                if (RTriggerDown)
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

    private void findSearchHoverButton()
    {
        //Go through eacho of the children of the search results panel
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
                    if (RTriggerDown)
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
                    if (RTriggerDown)
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
                                searchButtonHover = searchResult.GetChild(0).gameObject.GetComponent<Text>().text;
                                if (RTriggerDown)
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
        else if (menuHoverButton == "Close")
        {
            MenuPanel.SetActive(false);
        }
        else if (menuHoverButton == "Tutorials")
        {
            MenuPanel.SetActive(false);
            TutorialPanel.SetActive(true);
        }
    }

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

    }

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
                if (keyboardSource == "NewSearch")
                {
                    updateSearchInput();
                }
            }
            else if (hoveredKey == "Mic")
            {
                toggleVoiceRecording();
            }
            else
            {
                keyboardInputField.text += hoveredKey.ToLower();
            }
        }
    }

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
            PerformSearch();
        }
        else if (searchButtonHover == "Last Page")
        {
            if (currentSearchPage > 1)
            {
                SearchResultsPanel.transform.Find("Page " + currentSearchPage).gameObject.SetActive(false);
                currentSearchPage--;
                SearchResultsPanel.transform.Find("Page " + currentSearchPage).gameObject.SetActive(true);
            }
        }
        else if (searchButtonHover == "Next Page")
        {
            if (currentSearchPage < numberOfSearchPages)
            {
                SearchResultsPanel.transform.Find("Page " + currentSearchPage).gameObject.SetActive(false);
                currentSearchPage++;
                SearchResultsPanel.transform.Find("Page " + currentSearchPage).gameObject.SetActive(true);
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

    private void updateSearchInput()
    {
        SearchResultsInputField.text = keyboardInputField.text;
        keyboardInputField.text = "";
        SearchResultsPanel.SetActive(true);
        VirtualKeyboardCanvas.SetActive(false);
    }

    private void PerformSearch()
    {
        currentSearchPage = 1;
        clearResultsUI();

        // Run search algorithm
        // And update search results UI

        string query = SearchResultsInputField.text;

        if (!string.IsNullOrEmpty(query))
        {
            SearchService.Instance.Flush();
            SearchService.Instance.Search(query, res =>
            {
                searchResults = res;
                updateResultsUI();
            });
        }
    }

    private void downloadModelAtIndex(int searchIndex)
    {
        if (searchIndex < searchResults.Count && searchIndex >= 0)
        {
            downloadModelName = searchResults.Hits[searchIndex].Asset.Name;
            SearchService.Instance.DownloadModel(searchResults.Hits[searchIndex], nm =>
            {
                SearchResultsPanel.SetActive(false);
                isLoadingInNewModel = true;
                loadingCircle.SetActive(true);
                rayCastEndSphere.GetComponent<MeshRenderer>().material.color = Color.green;
                rayCastLineRenderer.startColor = Color.green;
                rayCastLineRenderer.endColor = Color.green;

                ModelLoaderService.Instance.LoadModel(nm, modelDoneLoadingCallback);
            });
        }
    }

    public void modelDoneLoadingCallback(GameObject lastLoadedModel)
    {
        finishUpModelLoad(lastLoadedModel);
    }

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

            lastLoadedModel.transform.position = new Vector3(0, Mathf.Abs(diff), 0);
            lastLoadedModel.GetComponent<BoxCollider>().enabled = false;

            GameObject parentObject = new GameObject();
            parentObject.name = downloadModelName + " parent";
            lastLoadedModel.transform.parent = parentObject.transform;
            setGameObjectLayer(parentObject, 2);

            lastNewLoadedModels.Add(parentObject);
        }
        else
        {
            cancelNewLoadedModel();
        }
    }

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
            //parentObject.AddComponent<Valve.VR.InteractionSystem.InteractableHoverEvents>();
            lastNewLoadedModel.AddComponent<Valve.VR.InteractionSystem.Throwable>();

            Valve.VR.InteractionSystem.Throwable throwable = lastNewLoadedModel.GetComponent<Valve.VR.InteractionSystem.Throwable>();
            throwable.onPickUp = new UnityEngine.Events.UnityEvent();
            throwable.onDetachFromHand = new UnityEngine.Events.UnityEvent();

            lastNewLoadedModel.AddComponent<userAsset>();

            //parentObject.AddComponent<Rigidbody>(); // Add gravity rules for physics

            lastNewLoadedModel.transform.GetChild(0).gameObject.GetComponent<BoxCollider>().enabled = true;
            lastNewLoadedModel.AddComponent<Rigidbody>(); // Add gravity rules for physics
            Rigidbody rigidBody = lastNewLoadedModel.GetComponent<Rigidbody>();

            rigidBody.isKinematic = true;

            rigidBody.mass = 1000;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            yield return new WaitForSeconds(0.1f);

            rigidBody.isKinematic = false;

            setGameObjectLayer(lastNewLoadedModel, 0);

            lastNewLoadedModels.Clear();

            SearchService.Instance.Flush();
        }
    }

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
    
    private void clearResultsUI()
    {
        updateNumberOfSearchPages();
        for (int i = 1; i <= numberOfSearchPages; i++)
        {
            Destroy(SearchResultsPanel.transform.Find("Page " + i).gameObject);
        }
    }

    private void updateResultsUI()
    {
        updateNumberOfSearchPages();
        for (int i = 1; i <= numberOfSearchPages; i++)
        {
            Destroy(SearchResultsPanel.transform.Find("Page " + i).gameObject);
        }

        int numberOfPagesNeeded = (int)Mathf.Ceil((float)searchResults.Count / 4.0f);

        int modelIndex = 0;
        for (int i = 1; i <= numberOfPagesNeeded; i++)
        {
            GameObject page = Instantiate(new GameObject());
            page.name = "Page " + i;
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

                    item.transform.GetChild(0).gameObject.GetComponent<Text>().text = (modelIndex) + "";
                    item.transform.GetChild(1).gameObject.GetComponent<Text>().text = searchResults.Hits[modelIndex].Asset.Name + "";

                    modelIndex++;
                }
            }
        }
    }

    private void updateNumberOfSearchPages()
    {
        numberOfSearchPages = 0;
        for (int i = 0; i < SearchResultsPanel.transform.childCount; i++)
        {
            Transform transform = SearchResultsPanel.transform.GetChild(i);
            if (transform.gameObject.name.Contains("Page"))
            {
                numberOfSearchPages++;
            }
        }

        if (numberOfSearchPages > 0)
        {
            //Subrat 2 because of the 2 buttons that say
            // Last Page and Next Page
            numberOfSearchPages -= 2;
        }
    }

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
            });
        }
    }
    
    private void updateMusicSlider()
    {
        if (Input.GetAxisRaw("RightTrigger") > 0.2f)
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
    
    private void updateSliderFromPercentage(float percentage)
    {
        //Takes in a value from 0 to 100
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
}
