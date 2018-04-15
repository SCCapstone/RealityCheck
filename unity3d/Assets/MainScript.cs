using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class MainScript : MonoBehaviour
{
    public Shader lineShader;

	public GameObject panel;
	public GameObject resultsPanel;

	public GameObject debugText;
	public GameObject inputField;

	public GameObject ListItemPrefab;

	public Text searchDebugText;

	//public GameObject fpsController;

    private GameObject pointerHand;

    public GameObject rightHand;
    public GameObject leftHand;

    public GameObject DoorKnobBox;
    public GameObject DoorKnobDemo;
    public GameObject DoorKnobOpen;

    public GameObject KnobGlowBox;
    public GameObject KnobGlowDemo;
    public GameObject KnobGlowOpen;

    public GameObject Save1NameText;
    public GameObject Save2NameText;
    public GameObject Save3NameText;

    public GameObject Save1Painting;
    public GameObject Save2Painting;
    public GameObject Save3Painting;

    public GameObject Save1Image;
    public GameObject Save2Image;
    public GameObject Save3Image;

    private GameObject rayCastEndSphere;
    private LineRenderer lineRenderer;

	private List<OBJThread> loaders;
	private List<GameObject> sceneModels;
	private List<Bounds> boundsList;

	private List<GameObject> resultsButtons;
	private Search.SearchResult searchResults;

    private bool needLoadFromStart = true;

    private string saveSlot1Room = "";
    private string saveSlot2Room = "";
    private string saveSlot3Room = "";
    
    private int slotCount = 0;

    private bool RTriggerHeld;
    private bool LTriggerHeld;

    private double LDownTime;
    private double RDownTime;
    private double triggerTime = 5.0;
    private bool RTriggerDown;
    private bool LTriggerDown;
    
    // Use this for initialization
    void Start()
	{
        LTriggerDown = false;
        RTriggerDown = false;

        resultsButtons = new List<GameObject>();

		SearchService.Instance.Flush();
		SearchService.Instance.debugText = searchDebugText;

		panel.SetActive(false);
		resultsPanel.SetActive(false);

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(lineShader);
        lineRenderer.widthMultiplier = 0.01f;
        lineRenderer.positionCount = 2;

        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;

        rayCastEndSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rayCastEndSphere.GetComponent<MeshRenderer>().material.color = Color.blue;
        rayCastEndSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        rayCastEndSphere.GetComponent<SphereCollider>().enabled = false;
        rayCastEndSphere.name = "rayCastEndSphere";
        
        pointerHand = rightHand;
        /*
        Debug.Log("loading glock");
        SearchService.Instance.Search("glock", res => {
            Debug.Log("found glock " + res.Hits[0]);
			searchResults = res;

			SearchService.Instance.DownloadModel(searchResults.Hits[0], nm => {
			    Debug.Log(nm.file);
			});
		});*/
    }

    // Update is called once per frame
    void Update()
    {
        ModelLoaderService.Instance.Update();

        // press tab to show menu
        //if (Input.GetKeyUp(KeyCode.Tab))
        //{
        //var state = fpsController.GetComponent<RigidbodyFirstPersonController>().enabled;
        //fpsController.GetComponent<RigidbodyFirstPersonController>().enabled = !state;

        //    panel.SetActive(state);
        //    resultsPanel.SetActive(state);
        //}

        var layerMask = 1 << 2;
        layerMask = ~layerMask;

        RaycastHit hit;

        Vector3 rotation = pointerHand.transform.localEulerAngles;
        rotation.x += 15;

        Vector3 forwardVector = Quaternion.Euler(rotation) * Vector3.forward;
        
        if (Physics.Raycast(pointerHand.transform.position, forwardVector, out hit, Mathf.Infinity, layerMask))
        {
            float size = Mathf.Clamp(hit.distance * 0.01f, 0.01f, 1f);
            rayCastEndSphere.transform.localScale = new Vector3(size, size, size);
            rayCastEndSphere.transform.position = hit.point;
            lineRenderer.SetPosition(0, pointerHand.transform.position);
            lineRenderer.SetPosition(1, hit.point);
        }
        
        if (SceneManager.GetActiveScene().name == "scene")
        {
            if (null == rayCastEndSphere)
            {
                rayCastEndSphere = GameObject.Find("rayCastEndSphere");
            }

            RTriggerDown = getRightTriggerDown();
            LTriggerDown = getLeftTriggerDown();

            checkDoorKnobs();
            checkPaintings();

            if (needLoadFromStart)
            {
                needLoadFromStart = false;
                loadSaves();
            }
        }
    }

    void checkDoorKnobs()
    {
        SphereCollider sphereBox = DoorKnobBox.GetComponent<SphereCollider>();
        SphereCollider sphereDemo = DoorKnobDemo.GetComponent<SphereCollider>();
        SphereCollider sphereOpen = DoorKnobOpen.GetComponent<SphereCollider>();

        Vector3 leftPos = leftHand.transform.position;
        Vector3 rightPos = rightHand.transform.position;

        Vector3 rayPos;
        if (null == rayCastEndSphere)
        {
            rayPos = new Vector3(-1000, -1000, -1000);
        }
        else
        {
            rayPos = rayCastEndSphere.transform.position;
        }

        if (sphereBox.bounds.Contains(leftPos) || sphereBox.bounds.Contains(rightPos)
            || sphereBox.bounds.Contains(rayPos))
        {
            KnobGlowBox.SetActive(true);
            if (RTriggerDown && slotCount < 3)
            {
                SaveLoadService.Instance.Slot = slotCount;
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
            if (RTriggerDown && slotCount < 3)
            {
                SaveLoadService.Instance.Slot = slotCount;
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
            if (RTriggerDown && slotCount < 3)
            {
                SaveLoadService.Instance.Slot = slotCount;
                SceneManager.LoadScene("newOutsideRoom", LoadSceneMode.Single);
            }
        }
        else
        {
            KnobGlowOpen.SetActive(false);
        }
    }

    void checkPaintings()
    {
        BoxCollider boxSave1 = Save1Painting.GetComponent<BoxCollider>();
        BoxCollider boxSave2 = Save2Painting.GetComponent<BoxCollider>();
        BoxCollider boxSave3 = Save3Painting.GetComponent<BoxCollider>();

        Vector3 rayPos;
        if (null == rayCastEndSphere)
        {
            rayPos = new Vector3(-1000, -1000, -1000);
        }
        else
        {
            rayPos = rayCastEndSphere.transform.position;
        }

        int loadSlot = -1;
        string roomName = "";

        if (boxSave1.bounds.Contains(rayPos))
        {
            if (RTriggerDown)
            {
                loadSlot = 0;
                roomName = saveSlot1Room;
            }
        }
        else if (boxSave2.bounds.Contains(rayPos))
        {
            if (RTriggerDown)
            {
                loadSlot = 1;
                roomName = saveSlot2Room;
            }
        }
        else if (boxSave3.bounds.Contains(rayPos))
        {
            if (RTriggerDown)
            {
                loadSlot = 2;
                roomName = saveSlot3Room;
            }
        }

        if (loadSlot >= 0 && loadSlot <= 3 && roomName != "")
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

    void loadSaves()
    {
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
        catch (Exception ex) {  }

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
            Save1NameText.GetComponent<TextMesh>().text = inputName;
            Save1NameText.name = fullName;

            slotCount++;
            saveSlot1Room = fullName;

            var screenshotFn = Application.persistentDataPath + Path.DirectorySeparatorChar + "save" + 0 + ".png";
            if (File.Exists(screenshotFn))
            {
                byte[] fileData = File.ReadAllBytes(screenshotFn);

                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(fileData);
                tex.Apply();
                Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Save1Image.GetComponent<Image>().sprite = screenshotSprite;
            }
        }

        if (state2 != null)
        {
            string fullName = state2.roomName;
            string inputName = fullName;
            int lastSpaceIndex = inputName.LastIndexOf(" ");
            inputName = inputName.Substring(0, lastSpaceIndex);
            Save2NameText.GetComponent<TextMesh>().text = inputName;
            Save2NameText.name = fullName;

            slotCount++;
            saveSlot2Room = fullName;

            var screenshotFn = Application.persistentDataPath + Path.DirectorySeparatorChar + "save" + 1 + ".png";
            if (File.Exists(screenshotFn))
            {
                byte[] fileData = File.ReadAllBytes(screenshotFn);

                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(fileData);
                tex.Apply();
                Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Save2Image.GetComponent<Image>().sprite = screenshotSprite;
            }
        }

        if (state3 != null)
        {
            string fullName = state3.roomName;
            string inputName = fullName;
            int lastSpaceIndex = inputName.LastIndexOf(" ");
            inputName = inputName.Substring(0, lastSpaceIndex);
            Save3NameText.GetComponent<TextMesh>().text = inputName;
            Save3NameText.name = fullName;

            slotCount++;
            saveSlot3Room = fullName;

            var screenshotFn = Application.persistentDataPath + Path.DirectorySeparatorChar + "save" + 2 + ".png";
            if (File.Exists(screenshotFn))
            {
                byte[] fileData = File.ReadAllBytes(screenshotFn);

                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(fileData);
                tex.Apply();
                Sprite screenshotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Save3Image.GetComponent<Image>().sprite = screenshotSprite;
            }
        }
    }

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
