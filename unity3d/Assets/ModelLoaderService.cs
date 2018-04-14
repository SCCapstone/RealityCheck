using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using TriLib;
//using UnityEditor.PackageManager;


public sealed class ModelLoaderService: Singleton<ModelLoaderService> {

    public Shader Standard;
    public Shader BumpedDiffuse;
    public Shader BumpedSpecular;

    private List<OBJThread> loaders;
	public List<GameObject> sceneModels;
	private List<Bounds> boundsList;

    Action<GameObject> processFinishedCallBack;

    private AssetLoaderOptions assetLoaderOptions;



    protected ModelLoaderService()
    {
        loaders = new List<OBJThread>();
		sceneModels = new List<GameObject>();
		boundsList = new List<Bounds>();
    }

    public void Start()
    {
        assetLoaderOptions = ScriptableObject.CreateInstance<AssetLoaderOptions>();
        assetLoaderOptions.AutoPlayAnimations = false;
        assetLoaderOptions.DontLoadAnimations = true;
    }


    public void LoadModel(NetModel nm, Action<GameObject> callBack)
    {
        StartCoroutine(loadModel(nm, callBack));
        /*
        processFinishedCallBack = callBack;
        loaders.Add(new OBJThread());
        loaders[loaders.Count - 1].BumpedSpecular = BumpedSpecular;
        loaders[loaders.Count - 1].BumpedDiffuse = BumpedDiffuse;
        loaders[loaders.Count - 1].Standard = Standard;
        loaders[loaders.Count - 1].LoadLocal(nm.obj);*/
    }

    private IEnumerator loadModel(NetModel nm, Action<GameObject> callBack)
    {
        using (var assetLoader = new AssetLoader())
        {
            try
            {
                GameObject assetGameObject = assetLoader.LoadFromFile(nm.file, assetLoaderOptions);
                if (assetGameObject != null)
                {
                    assetGameObject.name = nm.file_uuid; //nm.uuid;
                    Debug.Log("MLS name: " + assetGameObject.name);
                    callBack(assetGameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("NOPE!!! ex " + e);
            }
            
            yield return null;
        }
    }

    public void Update()
    {
        /*
        if (loaders != null) {
            for (int i = 0; i < loaders.Count; i++)
			{
				loaders[i].Update();

				if (loaders[i].AllJobsDone)
				{
					processModel(loaders[i]);
					i--;
				}
			}
        }*/
    }

    private void processModel(OBJThread loader)
	{
        List<List<Mesh>> totalMeshes = loader.getModelData();
		Dictionary<string, Material> totalMaterials = loader.getTotalMaterials();
		List<string> objectNames = loader.getObjectNames();

		string objPath = loader.getPath();
		//string modelName = objPath.Substring(objPath.LastIndexOf("/") + 1, objPath.LastIndexOf(".obj") - objPath.LastIndexOf("/") - 1);

        //Correct for the 0 name
        string modelPath = objPath.Replace("/0.obj", "");
        string modelName = modelPath.Substring(modelPath.LastIndexOf("/") + 1, modelPath.Length - (modelPath.LastIndexOf("/") + 1));

        GameObject model = new GameObject();
		model.name = modelName;
		//model.AddComponent<Rigidbody>();

		int matCount = 0;
         // for each number of buffer objects
		for (int i = 0; i < totalMeshes.Count; i++)
		{
			GameObject bufferObject = new GameObject();
			bufferObject.name = objectNames[i];
			bufferObject.transform.parent = model.transform;
		    
            // for each sub mesh
			for (int j = 0; j < totalMeshes[i].Count; j++)
			{
				totalMeshes[i][j].RecalculateBounds();
				GameObject subMesh = new GameObject();
				subMesh.name = totalMeshes[i][j].name;
				subMesh.transform.parent = bufferObject.transform;
				subMesh.AddComponent<MeshFilter>();
                subMesh.AddComponent<MeshRenderer>();
                subMesh.GetComponent<MeshFilter>().mesh = totalMeshes[i][j];

				Material mat;
				if (!totalMaterials.TryGetValue(totalMeshes[i][j].name, out mat))
				{
					mat = new Material(Standard);
				}

				subMesh.GetComponent<Renderer>().materials = new Material[1] { mat };

				matCount++;
			}
		}

		model = MergeMeshes(model, true);

		//============================================================
                
		sceneModels.Add(model);
		loaders.Remove(loader);

        if (processFinishedCallBack != null)
        {
            processFinishedCallBack(model);
        }
    }

	private GameObject MergeMeshes(GameObject parentOfObjectsToCombine, bool useMaterial = true)
	{
		if (parentOfObjectsToCombine == null) return null;

		Vector3 originalPosition = parentOfObjectsToCombine.transform.position;
		parentOfObjectsToCombine.transform.position = Vector3.zero;

		MeshFilter[] meshFilters = parentOfObjectsToCombine.GetComponentsInChildren<MeshFilter>();
		Dictionary<Material, List<MeshFilter>> materialToMeshFilterList = new Dictionary<Material, List<MeshFilter>>();
		List<GameObject> combinedObjects = new List<GameObject>();

		for (int i = 0; i < meshFilters.Length; i++)
		{
			var materials = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
			if (materials == null) continue;
			if (materials.Length > 1)
			{
				parentOfObjectsToCombine.transform.position = originalPosition;
				Debug.LogWarning("Objects with multiple materials on the same mesh are not supported. Create multiple meshes from this object's sub-meshes in an external 3D tool and assign separate materials to each.");
			}
			var material = materials[0];
			if (materialToMeshFilterList.ContainsKey(material)) materialToMeshFilterList[material].Add(meshFilters[i]);
			else materialToMeshFilterList.Add(material, new List<MeshFilter>() { meshFilters[i] });
		}

        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var entry in materialToMeshFilterList)
		{
			List<MeshFilter> meshesWithSameMaterial = entry.Value;
			string materialName = entry.Key.ToString().Split(' ')[0];
			int i = 0;

			while (i != meshesWithSameMaterial.Count)
			{
				List<CombineInstance> combine = new List<CombineInstance>();
				int vertexCount = 0;
				for (; i < meshesWithSameMaterial.Count;)
				{
					CombineInstance tempCombine = new CombineInstance();
					vertexCount += meshesWithSameMaterial[i].sharedMesh.vertices.Length;
					if (vertexCount > 64999)
					{
						break;
					}

					tempCombine.mesh = meshesWithSameMaterial[i].sharedMesh;
					tempCombine.transform = meshesWithSameMaterial[i].transform.localToWorldMatrix;
					combine.Add(tempCombine);
					i++;
				}

				Mesh combinedMesh = new Mesh();
				combinedMesh.CombineMeshes(combine.ToArray());

				string goName = (materialToMeshFilterList.Count > 1) ? "CombinedMeshes_" + materialName : "CombinedMeshes_" + parentOfObjectsToCombine.name;
				GameObject combinedObject = new GameObject(goName);
				var filter = combinedObject.AddComponent<MeshFilter>();
				filter.sharedMesh = combinedMesh;
				var renderer = combinedObject.AddComponent<MeshRenderer>();

				if (useMaterial)
				{
					renderer.sharedMaterial = entry.Key;
				}
				else
				{
					renderer.sharedMaterial = new Material(Standard);
				}

                bounds.Encapsulate(renderer.bounds);

                combinedObjects.Add(combinedObject);

				//combinedObject.AddComponent<BoxCollider>(); // Add Box Collider for physics
			}
		}
        
        GameObject resultGO = null;
		if (combinedObjects.Count > 1)
		{
			resultGO = new GameObject(parentOfObjectsToCombine.name);
			foreach (var combinedObject in combinedObjects) combinedObject.transform.parent = resultGO.transform;
		}
		else
		{
			resultGO = combinedObjects[0];
		}

		parentOfObjectsToCombine.SetActive(false);
		parentOfObjectsToCombine.transform.position = originalPosition;
		resultGO.transform.position = originalPosition;
        
        Destroy(parentOfObjectsToCombine);
		return resultGO;
	}
}