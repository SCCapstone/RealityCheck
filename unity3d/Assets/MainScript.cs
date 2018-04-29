using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;

/// <summary>
/// This class is used to control all the UI in the starting room
/// expect the music setting. This is controlled int eh MainRoomSettings class.
/// This class first loads in all saved design files and generates the neccessary UI
/// required including the image and the name of the design. There are also 3 door knobs
/// the player can interact with that are controlled here.
/// 
/// This script is used in ever scene of the application, because this script
/// controls the blue raycaster used throughout the application.
/// </summary>

public class MainScript : MonoBehaviour
{
    public Shader lineShader;

	public GameObject panel;
	public GameObject resultsPanel;

	public GameObject debugText;
	public GameObject inputField;

	public GameObject ListItemPrefab;

	public Text searchDebugText;
    
    private GameObject pointerHand;

    public GameObject rightHand;
    public GameObject leftHand;

    public GameObject SteamCameraRig;
    public GameObject HandPanels;

    public GameObject DoorKnobBox;
    public GameObject DoorKnobDemo;
    public GameObject DoorKnobOpen;

    public GameObject KnobGlowBox;
    public GameObject KnobGlowDemo;
    public GameObject KnobGlowOpen;

    public GameObject[] SavePanels;
    public Sprite DefaultSaveScreen;

    private GameObject rayCastEndSphere;
    private LineRenderer lineRenderer;

	private List<OBJThread> loaders;
	private List<GameObject> sceneModels;
	private List<Bounds> boundsList;

	private List<GameObject> resultsButtons;
	private Search.SearchResult searchResults;

    private bool needLoadFromStart = true;

    private string[] saveSlotNames = new string[3];
    private int firstEmptySlot = -1;
    private string savingDirectory;

    private bool RTriggerHeld;
    private bool LTriggerHeld;

    private double LDownTime;
    private double RDownTime;
    private double triggerTime = 5.0; // 5 milliseconds
    private bool RTriggerDown;
    private bool LTriggerDown;

    private bool leftIsLeftIndex;

    private Vector3 rayHitPoint;
    
    private Color normalButton = new Color(0.3f, 0.3f, 0.3f);
    private Color highlightButton = new Color(0.6f, 0.6f, 0.6f);
    private Color selectButton = new Color(0.6f, 0.6f, 10f);
    
    void Start()
	{
        LTriggerDown = false;
        RTriggerDown = false;

        resultsButtons = new List<GameObject>();

		SearchService.Instance.Flush();
		SearchService.Instance.debugText = searchDebugText;

		panel.SetActive(false);
		resultsPanel.SetActive(false);

        // Set up the line renderer used for the blue laser
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(lineShader);
        lineRenderer.widthMultiplier = 0.01f;
        lineRenderer.positionCount = 2;

        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;
        
        // Set up the blue sphere used to display where the blue laser terminates
        rayCastEndSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rayCastEndSphere.GetComponent<MeshRenderer>().material.color = Color.blue;
        rayCastEndSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        rayCastEndSphere.GetComponent<SphereCollider>().enabled = false;
        rayCastEndSphere.name = "rayCastEndSphere";
        
        //Assign the pointer hand to the default right hand
        pointerHand = rightHand;

        //savingDirectory = Application.persistentDataPath + Path.DirectorySeparatorChar;
        savingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "RealityCheck" + Path.DirectorySeparatorChar;

        // If this is the starting room, load in the save files neccessary
        // for the loading UI.
        if (SceneManager.GetActiveScene().name == "scene")
        {
            loadSaves();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // Update trigger input
        RTriggerDown = getRightTriggerDown();
        LTriggerDown = getLeftTriggerDown();

        ModelLoaderService.Instance.Update();
        
        // Raycasting requires start point and a direction.
        // Optionaly, Unity allows a distance and a layer mask.
        // This layer mask is used to ignore object with the layer "Ignore raycast" assigned to it.

        var layerMask = 1 << 2;
        layerMask = ~layerMask;

        RaycastHit hit;

        Vector3 rotation = pointerHand.transform.localEulerAngles;
        rotation.x += 15;

        Vector3 forwardVector = Quaternion.Euler(rotation) * Vector3.forward;
        
        if (Physics.Raycast(pointerHand.transform.position, forwardVector, out hit, Mathf.Infinity, layerMask))
        {
            rayHitPoint = hit.point;

            float size = Mathf.Clamp(hit.distance * 0.01f, 0.01f, 1f);
            rayCastEndSphere.transform.localScale = new Vector3(size, size, size);
            rayCastEndSphere.transform.position = hit.point;
            lineRenderer.SetPosition(0, pointerHand.transform.position);
            lineRenderer.SetPosition(1, hit.point);
        }

        // If the lef tor right trigger is pressed, figure out if the pointer hand is still
        // on the correct hand. The pointer hand should be assigned to the controler that
        // pressed down on the trigger
        if (LTriggerDown || RTriggerDown)
        {
            Player player = SteamCameraRig.GetComponent<Player>();
            bool swap = false;

            if (RTriggerDown)
            {
                if (pointerHand != (leftIsLeftIndex ? rightHand : leftHand))
                {
                    swap = true;
                }
            }
            else if (pointerHand != (leftIsLeftIndex ? leftHand : rightHand))
            {
                swap = true;
            }
            
            // If the pointer is on the wrong hand, move it to the correct hand
            // and swap the panels over the opposite hand.
            if (swap)
            {
                GameObject nonpointerHand = null;

                if (pointerHand == leftHand)
                {
                    pointerHand = rightHand;
                    nonpointerHand = leftHand;
                }
                else
                {
                    pointerHand = leftHand;
                    nonpointerHand = rightHand;
                }

                if (HandPanels != null)
                {
                    HandPanels.transform.parent = nonpointerHand.transform;
                    HandPanels.transform.localPosition = new Vector3(0, 0.25f, 0.15f);
                    HandPanels.transform.localRotation = Quaternion.Euler(30, 0, 0);

                    PropertyController prop = nonpointerHand.GetComponent<PropertyController>();

                    System.Type type = prop.GetType();
                    Component copy = pointerHand.AddComponent(type);
                    // Copied fields can be restricted with BindingFlags
                    System.Reflection.FieldInfo[] fields = type.GetFields();
                    foreach (System.Reflection.FieldInfo field in fields)
                    {
                        field.SetValue(copy, field.GetValue(prop));
                    }
                    Destroy(prop);
                }
            }
        }

        if (SceneManager.GetActiveScene().name == "scene")
        {
            // If this is the main room, update the UI based
            // on where the raycaster is pointing

            checkDoorKnobs();
            checkPaintings();
        }

        // Once the controller has been assigned from SteamVR, find out which controller
        // is the left controller. This is required because SteamVR is designed to be ambidextrous.
        if (needLoadFromStart && leftHand.GetComponent<Hand>().controller != null)
        {
            needLoadFromStart = false;
            leftIsLeftIndex = false;

            if (leftHand.GetComponent<Hand>().controller.index == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.First))
            {
                leftIsLeftIndex = true;
            }
        }
    }

    // If the raycaster is pointing at a doorknob, or if a controller is touching a doorknob,
    // load the respective room that the doorknob is assigned to
    void checkDoorKnobs()
    {
        SphereCollider sphereBox = DoorKnobBox.GetComponent<SphereCollider>();
        SphereCollider sphereDemo = DoorKnobDemo.GetComponent<SphereCollider>();
        SphereCollider sphereOpen = DoorKnobOpen.GetComponent<SphereCollider>();

        Vector3 leftPos = leftHand.transform.position;
        Vector3 rightPos = rightHand.transform.position;

        Vector3 rayPos = rayCastEndSphere.transform.position;

        if (sphereBox.bounds.Contains(leftPos) || sphereBox.bounds.Contains(rightPos)
            || sphereBox.bounds.Contains(rayPos))
        {
            KnobGlowBox.SetActive(true);
            if (RTriggerDown || LTriggerDown)
            {
                SaveLoadService.Instance.Slot = firstEmptySlot;
                SceneManager.LoadScene("newBoxRoom", LoadSceneMode.Single);
            }
        }
        else
        {
            KnobGlowBox.SetActive(false);
        }

        if (sphereDemo.bounds.Contains(leftPos) || sphereDemo.bounds.Contains(rightPos)
            || sphereDemo.bounds.Contains(rayPos))
        {
            KnobGlowDemo.SetActive(true);
            if (RTriggerDown || LTriggerDown)
            {
                SaveLoadService.Instance.Slot = firstEmptySlot;
                SceneManager.LoadScene("newDemoRoom", LoadSceneMode.Single);
            }
        }
        else
        {
            KnobGlowDemo.SetActive(false);
        }

        if (sphereOpen.bounds.Contains(leftPos) || sphereOpen.bounds.Contains(rightPos)
            || sphereOpen.bounds.Contains(rayPos))
        {
            KnobGlowOpen.SetActive(true);
            if (RTriggerDown || LTriggerDown)
            {
                SaveLoadService.Instance.Slot = firstEmptySlot;
                SceneManager.LoadScene("newOutsideRoom", LoadSceneMode.Single);
            }
        }
        else
        {
            KnobGlowOpen.SetActive(false);
        }
    }

    // This method updates all of the UI related to the three save slots 
    // in the start room. First it check is if the raycaster is pointing at a TV.
    // If it is, set the load/delete page is set to visible. Then, if the raycaster is poiting at one of buttons,
    // highlight it. If the player presses down on one of the triggers, activate the respective button.
    // If the user presses load, the respective room is loaded in. If the player presses delete,
    // the respective TV changes to a delete confirmation screen. If the player presses yes,
    // the save slot is deleted and the UI is reset to the default settings. If the user
    // pressed no, the display is reverted back to the default load/delete page
    void checkPaintings()
    {
        Vector3 rayPos = rayCastEndSphere.transform.position;

        int loadSlot = -1;
        string roomName = "";

        for (int i = 0; i < saveSlotNames.Length; i++)
        {
            if (saveSlotNames[i] == "")
            {
                continue;
            }

            BoxCollider boxSave = SavePanels[i].GetComponent<BoxCollider>();
            GameObject blackCover = SavePanels[i].transform.GetChild(2).gameObject;

            // For checking if the raycaster is generaly pointing at the TV
            // fix a temperoray vector to be on the same plane as the boxSave collider.
            // This is required because once the load/save page and delete confirmation
            // page is active, their colliders will override the boxSave collider.
            Vector3 rayPosFixed = rayPos;
            rayPosFixed.x = boxSave.bounds.center.x;

            if (boxSave.bounds.Contains(rayPosFixed))
            {
                blackCover.SetActive(true);
                bool buttonPressed = false;

                if (RTriggerDown || LTriggerDown)
                {
                    GameObject deleteConfirm = blackCover.transform.GetChild(2).gameObject;

                    if (!deleteConfirm.activeSelf)
                    {
                        GameObject loadButton = blackCover.transform.GetChild(0).gameObject;
                        GameObject deleteButton = blackCover.transform.GetChild(1).gameObject;

                        BoxCollider loadBox = loadButton.GetComponent<BoxCollider>();
                        BoxCollider deleteBox = deleteButton.GetComponent<BoxCollider>();

                        if (loadBox.bounds.Contains(rayPos))
                        {
                            loadSlot = i;
                            roomName = saveSlotNames[i];
                            buttonPressed = true;
                        }
                        else if (deleteBox.bounds.Contains(rayPos))
                        {
                            deleteConfirm.SetActive(true);
                            buttonPressed = true;
                        }
                    }
                    else
                    {
                        GameObject yesButton = deleteConfirm.transform.GetChild(0).gameObject;
                        GameObject noButton = deleteConfirm.transform.GetChild(1).gameObject;

                        BoxCollider yesBox = yesButton.GetComponent<BoxCollider>();
                        BoxCollider noBox = noButton.GetComponent<BoxCollider>();

                        if (yesBox.bounds.Contains(rayPos))
                        {
                            deleteConfirm.SetActive(false);
                            blackCover.SetActive(false);
                            deleteSaveSlot(i);
                            buttonPressed = true;
                        }
                        else if (noBox.bounds.Contains(rayPos))
                        {
                            deleteConfirm.SetActive(false);
                            buttonPressed = true;
                        }
                    }
                }
                
                updateButtonColors(blackCover.transform);
                if (buttonPressed)
                {
                    break;
                }
            }
            else
            {
                blackCover.SetActive(false);
            }
        }

        // The save slot name includes the name of the scene used
        // to create the design. Reload the corresponding scene, then
        // the GameState save is loading in in the NewRoomScript class later on.
        if (loadSlot != -1 && roomName != "")
        {
            SaveLoadService.Instance.Slot = loadSlot;
            if (roomName.Contains("newBoxRoom"))
            {
                SceneManager.LoadScene("newBoxRoom", LoadSceneMode.Single);
            }
            else if (roomName.Contains("newDemoRoom"))
            {
                SceneManager.LoadScene("newDemoRoom", LoadSceneMode.Single);
            }
            else if (roomName.Contains("newOutsideRoom"))
            {
                SceneManager.LoadScene("newOutsideRoom", LoadSceneMode.Single);
            }
        }
    }
    
    // Recursively go through each element int the parent and update
    // the color of the element if the element is tagged with the
    // Button tag.
    private void updateButtonColors(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform element = parent.GetChild(i);
            updateButtonColorsInChildren(element);
            updateButtonColors(element);
        }
    }

    // If the button is hovered over, the button is colored light grey.
    // If the button is hovered over and one of the triggers is down,
    // the button is colored blue.
    // Else, the button is colored dark grey.
    private void updateButtonColorsInChildren(Transform parent)
    {
        GameObject element = parent.gameObject;
        if (element.tag == "Button")
        {
            Button currentButton = element.GetComponent<Button>();
            ColorBlock cb = currentButton.colors;
            if (currentButton.GetComponent<BoxCollider>().bounds.Contains(rayHitPoint))
            {
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

    // Attempt to load all 3 save slots and find their names and image data.
    // If there is no data for a save slot, set its name in memory to an empty string.
    // If the empty save slot is the first slot in order to be empty, set the 
    // firstEmptySlot variable to its slot index. This will be used later to settup the
    // save slot index for a newly created room.
    void loadSaves()
    {
        GameState[] states = new GameState[saveSlotNames.Length];

        for (int i = 0; i < saveSlotNames.Length; i++)
        {
            states[i] = null;

            try
            {
                states[i] = SaveLoadService.Instance.Load(i);
            }
            catch (Exception ex) { }
        }

        for (int i = 0; i < saveSlotNames.Length; i++)
        {
            if (states[i] != null)
            {
                GameObject saveNameplate = SavePanels[i].transform.GetChild(0).gameObject;
                GameObject saveImage = SavePanels[i].transform.GetChild(1).gameObject;

                // Update the nameplate to represent the name of the design.
                // The design name contains the name if the scene used to create
                // the design. Filter out this name for the nameplate.

                string fullName = states[i].roomName;
                string inputName = fullName;
                int lastSpaceIndex = inputName.LastIndexOf(" ");
                inputName = inputName.Substring(0, lastSpaceIndex);
                saveNameplate.GetComponent<Text>().text = inputName;
                saveSlotNames[i] = fullName;

                // If the image exists for the save slot, load it and
                // assign it to the respective TV screen.

                var screenshotFn = savingDirectory + "save" + i + ".png";
                if (File.Exists(screenshotFn))
                {
                    byte[] fileData = File.ReadAllBytes(screenshotFn);

                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(fileData);
                    tex.Apply();
                    Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    saveImage.GetComponent<Image>().sprite = screenshotSprite;
                }
            }
            else
            {
                // firstEmptySlot is assigned to -1 at start to show that
                // no empty slot has been found yet. Once a save slot is 
                // found to be empty, assign the respective index.

                if (firstEmptySlot == -1)
                {
                    firstEmptySlot = i;
                }
                saveSlotNames[i] = "";
            }
        }

        // If no empty slot was found, assign firstEmptySlot to 3
        // Later this will be used to say that if they would like to
        // save a new design, they will have to override an old save slot.
        if (firstEmptySlot == -1)
        {
            firstEmptySlot = saveSlotNames.Length;
        }
    }

    // Reset the UI used to represent a save slot.
    // Then delete any data related to this slot index.
    void deleteSaveSlot(int slotIndex)
    {
        GameObject saveNameplate = SavePanels[slotIndex].transform.GetChild(0).gameObject;
        GameObject saveImage = SavePanels[slotIndex].transform.GetChild(1).gameObject;
        
        saveNameplate.GetComponent<Text>().text = "Empty Save";
        saveSlotNames[slotIndex] = "";
        saveImage.GetComponent<Image>().sprite = DefaultSaveScreen;
        if (slotIndex < firstEmptySlot)
        {
            firstEmptySlot = slotIndex;
        }

        string screenshotFn = savingDirectory + "save" + slotIndex + ".png";
        string jsonFn = savingDirectory + "save" + slotIndex + ".json";

        if (File.Exists(screenshotFn))
        {
            File.Delete(screenshotFn);
        }

        if (File.Exists(jsonFn))
        {
            File.Delete(jsonFn);
        }
    }

#region Deprecated

    // Click Handler for search results
    // Loads model based on button index
    void ResultClickHandle(int index) {
		debugText.GetComponent<Text>().text = "Loading... " + searchResults.Hits[index].Asset.Name + "\n" + Application.temporaryCachePath;

		SearchService.Instance.DownloadModel(searchResults.Hits[index], nm => {
			ModelLoaderService.Instance.LoadModel(nm, null);
			debugText.GetComponent<Text>().text = "Added " + searchResults.Hits[index].Asset.Name;
			SearchService.Instance.Flush();
		});
	}

	// Update search results UI
	// Display latest results
	// TODO move to its own controller
	public void UpdateResultsUI()
	{
		for (int i=0; i<resultsButtons.Count; i++) {
			resultsButtons[i].transform.SetParent(null);
		}

		resultsButtons.Clear();

		for (int i=0; i<searchResults.Hits.Count; i++) {
			GameObject nr = Instantiate(ListItemPrefab) as GameObject;

			ListItemController controller = nr.GetComponent<ListItemController>();
			Debug.Log(searchResults.Hits[i]);
			controller.Name.text = searchResults.Hits[i].Asset.Name;

			nr.transform.SetParent(resultsPanel.transform);

			int index = i;

			nr.GetComponent<Button>().onClick.AddListener(
				delegate {
					ResultClickHandle(index);
				});

			resultsButtons.Add(nr);
		}
	}

	// Get search query
	// Send query to server
	// Load search results
	public void DoStuff()
	{
		//panel.SetActive(false);

		string query = inputField.GetComponent<InputField>().text;

		if (string.IsNullOrEmpty(query))
			return;

		SearchService.Instance.Search(query, res => {
			searchResults = res;
			UpdateResultsUI();
		});
	}

#endregion

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

}
