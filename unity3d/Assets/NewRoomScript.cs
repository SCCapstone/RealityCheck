using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewRoomScript : MonoBehaviour {

    public GameObject player;
    public GameObject PlayerText;

    public GameObject PropertiesPanel;

    public GameObject VirtualKeyboardCanvas;
    public GameObject VirtualKeyboardLayout;
    public InputField keyboardInputField;

    public GameObject SearchResultsPanel;
    public InputField SearchResultsInputField;
    public Button SearchButtonPrefab;
    public Toggle maintainScaleToggle;
    public Toggle physicsToggle;

    private string pauseText;

    private GameObject rayCastEndSphere;
    private string keyboardSource;
    private string hoveredKey;

    private Color normalButton = new Color(0.3f, 0.3f, 0.3f);
    private Color highlightButton = new Color(0.6f, 0.6f, 0.6f);
    private Color selectButton = new Color(0.6f, 0.6f, 10f);

    private Color normalInput = new Color(1.0f, 1.0f, 1.0f);
    private Color highlightInput = new Color(0.8f, 0.8f, 0.8f);
    private Color selectInput = new Color(0.6f, 0.6f, 10f);

    private string searchButtonHover;
    private int currentSearchPage;
    private int numberOfSearchPages;

    private Search.SearchResult searchResults;
    private bool waitingFoDownload = false;

    // Use this for initialization
    void Start () {
        pauseText = "Pause Menu\n\nPress \"A\" to exit to main menu.";
        VirtualKeyboardCanvas.SetActive(false);
        SearchResultsPanel.SetActive(false);
        keyboardSource = "";
        currentSearchPage = 1;

        SearchService.Instance.Flush();

        updateNumberOfSearchPages();
    }

    // Update is called once per frame
    void Update()
    {
        if(rayCastEndSphere == null)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
        }

        if (VirtualKeyboardCanvas.activeSelf)
        {
            findHoverfKey();
        }
        else if (SearchResultsPanel.activeSelf)
        {
            findSearchHoverButton();
        }
        
        if (waitingFoDownload && ModelLoaderService.Instance.loadStatus == "Done")
        {
            waitingFoDownload = false;
            firstTimePlaceLastSearchedModel();
        }

        if (Input.GetButtonDown("Start"))
        {
            if (PlayerText.GetComponent<Text>().text == pauseText)
            {
                PlayerText.GetComponent<Text>().text = "";
            }
            else
            {
                PropertiesPanel.SetActive(false);
                SearchResultsPanel.SetActive(false);
                VirtualKeyboardCanvas.SetActive(false);
                PlayerText.GetComponent<Text>().text = pauseText;
            }
        }
        
        if (Input.GetButtonDown("AButton"))
        {
            if (PlayerText.GetComponent<Text>().text == pauseText)
            {
                SceneManager.LoadScene("scene", LoadSceneMode.Single);
            }

            PlayerText.GetComponent<Text>().text = "";

            if (VirtualKeyboardCanvas.activeSelf && hoveredKey != "")
            {
                activateKeyboard();
            }
            else if (SearchResultsPanel.activeSelf && searchButtonHover != "")
            {
                activateSeachResults();
            }
        }
        else if (Input.GetButtonDown("XButton"))
        {
            PlayerText.GetComponent<Text>().text = "";
            PropertiesPanel.SetActive(false);
            SearchResultsPanel.SetActive(true);
        }
        else if (Input.GetButtonDown("BButton"))
        {
            PlayerText.GetComponent<Text>().text = "";
            SearchResultsPanel.SetActive(false);
            VirtualKeyboardCanvas.SetActive(false);
        }
    }

    private void findHoverfKey()
    {
        hoveredKey = "";
        for (int i = 0; i < VirtualKeyboardLayout.transform.childCount; i++)
        {
            GameObject keyBox = VirtualKeyboardLayout.transform.GetChild(i).gameObject;
            Button button = keyBox.GetComponent<Button>();
            if (keyBox.GetComponent<BoxCollider>().bounds.Contains(rayCastEndSphere.transform.position))
            {
                ColorBlock cb = button.colors;

                if (Input.GetButtonDown("AButton"))
                {
                    cb.normalColor = selectButton;
                }
                else
                {
                    cb.normalColor = highlightButton;
                }

                button.colors = cb;
                hoveredKey = keyBox.transform.GetChild(0).gameObject.GetComponent<Text>().text;
            }
            else
            {
                ColorBlock cb = button.colors;
                cb.normalColor = normalButton;
                button.colors = cb;
            }
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
            if (i == 0)
            {
                ColorBlock cb = SearchResultsInputField.colors;
                if (transform.gameObject.GetComponent<BoxCollider>().bounds.Contains(rayCastEndSphere.transform.position))
                {
                    searchButtonHover = "New";
                    MainButtonsHoveredOver = true;
                    if (Input.GetButtonDown("AButton"))
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
                if (transform.gameObject.GetComponent<BoxCollider>().bounds.Contains(rayCastEndSphere.transform.position))
                {
                    searchButtonHover = transform.GetChild(0).gameObject.GetComponent<Text>().text;
                    MainButtonsHoveredOver = true;
                    if (Input.GetButtonDown("AButton"))
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
                            if (boxCollider != null && boxCollider.bounds.Contains(rayCastEndSphere.transform.position) && !MainButtonsHoveredOver)
                            {
                                searchButtonHover = searchResult.GetChild(0).gameObject.GetComponent<Text>().text;
                                if (Input.GetButtonDown("AButton"))
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
            waitingFoDownload = true;
            SearchService.Instance.DownloadModel(searchResults.Hits[searchIndex], nm =>
            {
                ModelLoaderService.Instance.LoadModel(nm);
                SearchService.Instance.Flush();
            });
        }
    }

    private void firstTimePlaceLastSearchedModel()
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

            float roomHeight = 4.5f;
            float heightDiff = (bounds.max.y - bounds.min.y) - roomHeight;

            float newScale = 1.0f;
            if (heightDiff > 0)
            {
                newScale = roomHeight / (bounds.max.y - bounds.min.y);
                lastLoadedModel.transform.localScale = new Vector3(newScale, newScale, newScale);
            }

            float diff = bounds.min.y * newScale;
            
            //lastLoadedModel.transform.position = new Vector3(0, -diff, 0);
            lastLoadedModel.transform.position = new Vector3(-3, 0, -4);

            lastLoadedModel.AddComponent<userAsset>();
            lastLoadedModel.GetComponent<userAsset>().MaintainScale = maintainScaleToggle;
            lastLoadedModel.GetComponent<userAsset>().ObeyGravity = physicsToggle;
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
            page.transform.localPosition = new Vector3(0.55f, 0.0f, 0.0f);
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
                    item.transform.GetChild(1).gameObject.GetComponent<Text>().text = searchResults.Hits[modelIndex] + "";

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
