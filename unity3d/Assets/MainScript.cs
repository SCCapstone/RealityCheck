using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class MainScript : MonoBehaviour
{
	public GameObject panel;
	public GameObject resultsPanel;

	public GameObject debugText;
	public GameObject inputField;

	public GameObject ListItemPrefab;

	public Text searchDebugText;

	public GameObject fpsController;

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
	}

    // Update is called once per frame
    void Update()
    {
        ModelLoaderService.Instance.Update();

        // press tab to show menu
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            var state = fpsController.GetComponent<RigidbodyFirstPersonController>().enabled;
            fpsController.GetComponent<RigidbodyFirstPersonController>().enabled = !state;

            panel.SetActive(state);
            resultsPanel.SetActive(state);
        }
    }

    // Click Handler for search results
    // Loads model based on button index
    void ResultClickHandle(int index) {
		debugText.GetComponent<Text>().text = "Loading... " + searchResults.Hits[index].Asset.Name + "\n" + Application.temporaryCachePath;

		SearchService.Instance.DownloadModel(searchResults.Hits[index], nm => {
			ModelLoaderService.Instance.LoadModel(nm);
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

    public void GoToNewBoxRoom()
    {

    }
}
