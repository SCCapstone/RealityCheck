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

    private GameObject rayCastEndSphere;
    private LineRenderer lineRenderer;

	private List<OBJThread> loaders;
	private List<GameObject> sceneModels;
	private List<Bounds> boundsList;

	private List<GameObject> resultsButtons;
	private Search.SearchResult searchResults;
    
    // Use this for initialization
    void Start()
	{
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
        
        RaycastHit hit;

        Vector3 rotation = pointerHand.transform.localEulerAngles;
        rotation.x += 15;

        Vector3 forwardVector = Quaternion.Euler(rotation) * Vector3.forward;
        
        if (Physics.Raycast(pointerHand.transform.position, forwardVector, out hit))
        {
            float size = Mathf.Clamp(hit.distance * 0.01f, 0.01f, 1f);
            rayCastEndSphere.transform.localScale = new Vector3(size, size, size);
            rayCastEndSphere.transform.position = hit.point;
            lineRenderer.SetPosition(0, pointerHand.transform.position);
            lineRenderer.SetPosition(1, hit.point);
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
}
