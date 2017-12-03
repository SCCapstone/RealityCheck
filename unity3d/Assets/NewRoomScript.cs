using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewRoomScript : MonoBehaviour {

    public GameObject player;

    public GameObject MenuPanel;
    public GameObject PropertiesPanel;

    public GameObject VirtualKeyboardCanvas;
    public GameObject VirtualKeyboardLayout;
    public InputField keyboardInputField;

    public GameObject SearchResultsPanel;
    public InputField SearchResultsInputField;
    public Button SearchButtonPrefab;

    public Shader Standard;
    public Shader BumpedDiffuse;
    public Shader BumpedSpecular;

    private bool RTriggerHeld;
    private bool LTriggerHeld;

    private double LDownTime;
    private double RDownTime;
    private double triggerTime = 10.0; // 10 milliseconds
    private bool RTriggerDown;
    private bool LTriggerDown;

    private string menuHoverButton;

    private GameObject rayCastEndSphere;
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
    private bool waitingForDownload = false;

    // Use this for initialization
    void Start () {
        MenuPanel.SetActive(false);
        VirtualKeyboardCanvas.SetActive(false);
        SearchResultsPanel.SetActive(false);
        keyboardSource = "";
        currentSearchPage = 1;
        LTriggerDown = false;
        RTriggerDown = false;

        SearchService.Instance.Flush();

        ModelLoaderService.Instance.BumpedSpecular = BumpedSpecular;
        ModelLoaderService.Instance.BumpedDiffuse = BumpedDiffuse;
        ModelLoaderService.Instance.Standard = Standard;

        updateNumberOfSearchPages();
    }

    // Update is called once per frame
    void Update()
    {
        if (rayCastEndSphere == null)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
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
        }
        
        if (waitingForDownload && ModelLoaderService.Instance.loadStatus == "Done")
        {
            waitingForDownload = false;
            StartCoroutine(firstTimePlaceLastSearchedModel());
        }
        
        RTriggerDown = getRightTriggerDown();
        LTriggerDown = getLeftTriggerDown();
        bool keyButtonOverride = false;

        if (Input.GetButton("AButton") || Input.GetAxisRaw("RHandTrigger") > 0.2f)
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

        if (Input.GetButtonDown("AButton") || RTriggerDown)
        {
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
        }
        else if (keyButtonOverride && VirtualKeyboardCanvas.activeSelf && hoveredKey == "Back")
        {
            if (NowMilliseconds() - keyLastStepTime > keyStepTime)
            {
                keyLastStepTime = NowMilliseconds();
                activateKeyboard();
            }
        }
        else if (Input.GetButtonDown("XButton")) //Also Start button for Vive
        {
            MenuPanel.SetActive(true);
            PropertiesPanel.SetActive(false);
            SearchResultsPanel.SetActive(false);
            VirtualKeyboardCanvas.SetActive(false);
        }
        else if (Input.GetButtonDown("BButton"))
        {
            MenuPanel.SetActive(false);
            PropertiesPanel.SetActive(false);
            SearchResultsPanel.SetActive(false);
            VirtualKeyboardCanvas.SetActive(false);
        }
    }

    private bool getRightTriggerDown()
    {
        float pressure = Input.GetAxisRaw("RHandTrigger");
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
        float pressure = Input.GetAxisRaw("LHandTrigger");
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
                    if (Input.GetButtonDown("AButton") || RTriggerDown)
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
        hoveredKey = "";
        for (int i = 0; i < VirtualKeyboardLayout.transform.childCount; i++)
        {
            GameObject keyBox = VirtualKeyboardLayout.transform.GetChild(i).gameObject;
            Button button = keyBox.GetComponent<Button>();
            ColorBlock cb = button.colors;
            if (CheckBoxCollision(keyBox.GetComponent<BoxCollider>(), rayCastEndSphere.transform.position))
            {
                hoveredKey = keyBox.transform.GetChild(0).gameObject.GetComponent<Text>().text;
                
                if (Input.GetButtonDown("AButton") || RTriggerDown)
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
                    if (Input.GetButtonDown("AButton") || RTriggerDown)
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
                    if (Input.GetButtonDown("AButton") || RTriggerDown)
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
                                if (Input.GetButtonDown("AButton") || RTriggerDown)
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
    }

    private void activateKeyboard()
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
            //TODO
        }
        else
        {
            keyboardInputField.text += hoveredKey.ToLower();
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
        GameObject localPlayer = player.transform.Find("OVRCameraRig").Find("TrackingSpace").Find("CenterEyeAnchor").gameObject;
        Vector3 playerRot = localPlayer.transform.localRotation.eulerAngles;
        Vector3 playerPos = localPlayer.transform.localPosition;

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
                ModelLoaderService.Instance.LoadModel(nm);
                waitingForDownload = true;
            });
        }
    }

    private IEnumerator firstTimePlaceLastSearchedModel()
    {
        int index = ModelLoaderService.Instance.sceneModels.Count - 1;
        if (index < 0)
        {
            index = 0;
        }
        GameObject lastLoadedModel = ModelLoaderService.Instance.sceneModels[index];
        if (lastLoadedModel != null)
        {
            Quaternion currentRotation = lastLoadedModel.transform.rotation;
            lastLoadedModel.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Bounds bounds = new Bounds(lastLoadedModel.transform.position, Vector3.zero);
            foreach (Renderer renderer in lastLoadedModel.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            float roomHeight = 1.0f;
            float heightDiff = (bounds.max.y - bounds.min.y) - roomHeight;

            float newScale = 1.0f;
            if (heightDiff > 0)
            {
                newScale = roomHeight / (bounds.max.y - bounds.min.y);
                lastLoadedModel.transform.localScale = new Vector3(newScale, newScale, newScale);
            }

            float diff = bounds.min.y * newScale;
            
            lastLoadedModel.transform.position = new Vector3(0, Mathf.Abs(diff) + 1.0f, 0);

            lastLoadedModel.layer = 8;

            lastLoadedModel.AddComponent<userAsset>();
            
            lastLoadedModel.GetComponent<Rigidbody>().mass = 1000;
            lastLoadedModel.GetComponent<Rigidbody>().velocity = Vector3.zero;
            lastLoadedModel.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            yield return new WaitForSeconds(0.1f);

            lastLoadedModel.GetComponent<Rigidbody>().isKinematic = false;
            lastLoadedModel.name = downloadModelName;
            
            SearchService.Instance.Flush();
        }
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

}
