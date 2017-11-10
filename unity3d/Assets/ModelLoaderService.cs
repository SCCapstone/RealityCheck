using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;


public sealed class ModelLoaderService: Singleton<ModelLoaderService> {

    private List<OBJThread> loaders;
	private List<GameObject> sceneModels;
	private List<Bounds> boundsList;

    protected ModelLoaderService() {
        loaders = new List<OBJThread>();
		sceneModels = new List<GameObject>();
		boundsList = new List<Bounds>();
    }

    public void LoadModel(NetModel nm)
    {
        loaders.Add(new OBJThread());
		loaders[loaders.Count - 1].LoadLocal(nm.obj);
    }

    public void Update()
    {
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
        }
    }

    private void processModel(OBJThread loader)
	{
		List<List<Mesh>> totalMeshes = loader.getModelData();
		Dictionary<string, Material> totalMaterials = loader.getTotalMaterials();
		List<string> objectNames = loader.getObjectNames();

		string objPath = loader.getPath();
		string modelName = objPath.Substring(objPath.LastIndexOf("/") + 1, objPath.LastIndexOf(".obj") - objPath.LastIndexOf("/") - 1);

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
					mat = new Material(Shader.Find("Standard"));
				}

				subMesh.GetComponent<Renderer>().materials = new Material[1] { mat };

				matCount++;
			}
		}

		model = MergeMeshes(model, true);

		//============================================================

		Quaternion currentRotation = model.transform.rotation;
		model.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
		foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
		{
			bounds.Encapsulate(renderer.bounds);
		}

		float diff = bounds.min.y;

		float newZPos = 0;

		if (boundsList.Count > 0)
		{
			newZPos = boundsList[boundsList.Count - 1].max.z +
			sceneModels[boundsList.Count - 1].transform.position.z +
			(bounds.size.z / 2.0f);
		}

		Vector3 pos = new Vector3(
			              model.transform.position.x, 
			              model.transform.position.y, 
			              model.transform.position.z);

		model.transform.position = pos + new Vector3(0, -diff, newZPos);

		boundsList.Add(bounds);
		sceneModels.Add(model);
		loaders.Remove(loader);
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
					renderer.sharedMaterial = new Material(Shader.Find("Standard"));
				}

				combinedObjects.Add(combinedObject);

				//combinedObject.AddComponent<BoxCollider>(); // Add Box Collider for physics
			}
		}

		GameObject resultGO = null;
		if (combinedObjects.Count > 1)
		{
			resultGO = new GameObject("CombinedMeshes_" + parentOfObjectsToCombine.name);
			foreach (var combinedObject in combinedObjects) combinedObject.transform.parent = resultGO.transform;
		}
		else
		{
			resultGO = combinedObjects[0];
		}

		parentOfObjectsToCombine.SetActive(false);
		parentOfObjectsToCombine.transform.position = originalPosition;
		resultGO.transform.position = originalPosition;
		//resultGO.AddComponent<Rigidbody>(); // Add gravity rules for physics

		Destroy(parentOfObjectsToCombine);
		return resultGO;
	}
}