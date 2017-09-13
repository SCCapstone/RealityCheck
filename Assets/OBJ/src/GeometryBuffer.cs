using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeometryBuffer
{
	private List<ObjectData> objects;
	public List<Vector3> vertices;
	public List<Vector2> uvs;
	public List<Vector3> normals;
	public int unnamedGroupIndex = 1;
	// naming index for unnamed group. like "Unnamed-1"
	
	private ObjectData current;

	private class ObjectData
	{
		public string name;
		public List<GroupData> groups;
		public List<FaceIndices> allFaces;
		public int normalCount;

		public ObjectData()
		{
			groups = new List<GroupData>();
			allFaces = new List<FaceIndices>();
			normalCount = 0;
		}
	}

	private GroupData curgr;

	private class GroupData
	{
		public string name;
		public string materialName;
		public List<FaceIndices> faces;

		public GroupData()
		{
			faces = new List<FaceIndices>();
		}

		public bool isEmpty { get { return faces.Count == 0; } }
	}

	public GeometryBuffer()
	{
		objects = new List<ObjectData>();
		ObjectData d = new ObjectData();
		d.name = "default";
		objects.Add(d);
		current = d;
		
		GroupData g = new GroupData();
		g.name = "default";
		d.groups.Add(g);
		curgr = g;
		
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
	}

	public void PushObject(string name)
	{
		//Debug.Log("Adding new object " + name + ". Current is empty: " + isEmpty);
		if (isEmpty) objects.Remove(current);
		
		ObjectData n = new ObjectData();
		n.name = name;
		objects.Add(n);
		
		GroupData g = new GroupData();
		g.name = "default";
		n.groups.Add(g);
		
		curgr = g;
		current = n;
	}

	public void PushGroup(string name)
	{
		if (curgr.isEmpty) current.groups.Remove(curgr);
		GroupData g = new GroupData();
		if (name == null)
		{
			name = "Unnamed-" + unnamedGroupIndex;
			unnamedGroupIndex++;
		}
		g.name = name;
		current.groups.Add(g);
		curgr = g;
	}

	public void PushMaterialName(string name)
	{
		//Debug.Log("Pushing new material " + name + " with curgr.empty=" + curgr.isEmpty);
		if (!curgr.isEmpty) PushGroup(name);
		if (curgr.name == "default") curgr.name = name;
		curgr.materialName = name;
	}

	public void PushVertex(Vector3 v)
	{
		vertices.Add(v);
	}

	public void PushUV(Vector2 v)
	{
		uvs.Add(v);
	}

	public void PushNormal(Vector3 v)
	{
		normals.Add(v);
	}

	public void PushFace(FaceIndices f)
	{
		curgr.faces.Add(f);
		current.allFaces.Add(f);
		if (f.vn >= 0)
		{
			current.normalCount++;
		}
	}

	public void Trace()
	{
		//Debug.Log("OBJ has " + objects.Count + " object(s)");
		//Debug.Log("OBJ has " + vertices.Count + " vertice(s)");
		//Debug.Log("OBJ has " + uvs.Count + " uv(s)");
		//Debug.Log("OBJ has " + normals.Count + " normal(s)");
		foreach (ObjectData od in objects)
		{
			//Debug.Log(od.name + " has " + od.groups.Count + " group(s)");
			foreach (GroupData gd in od.groups)
			{
				//Debug.Log(od.name + "/" + gd.name + " has " + gd.faces.Count + " faces(s)");
			}
		}
		
	}

	public int numObjects { get { return objects.Count; } }

	public bool isEmpty { get { return vertices.Count == 0; } }

	public bool hasUVs { get { return uvs.Count > 0; } }

	public bool hasNormals { get { return normals.Count > 0; } }

	public static int MAX_VERTICES_LIMIT_FOR_A_MESH = 64999;

	public void PopulateMeshes(GameObject[] gs, Dictionary<string, Material> mats)
	{
		if (gs.Length != numObjects) return; // Should not happen unless obj file is corrupt...
		//Debug.Log("PopulateMeshes GameObjects count:" + gs.Length);
		for (int i = 0; i < gs.Length; i++)
		{
			ObjectData od = objects[i];
			bool objectHasNormals = (hasNormals && od.normalCount > 0);
			
			if (od.name != "default") gs[i].name = od.name;
			//Debug.Log("PopulateMeshes object name:" + od.name);

			if (od.groups.Count > 1)
			{
				List<Mesh> meshes = new List<Mesh>(); 

				List<List<Vector3>> tvertsList = new List<List<Vector3>>();
				List<List<Vector2>> tuvsList = new List<List<Vector2>>();
				List<List<Vector3>> tnormsList = new List<List<Vector3>>();
				List<List<int>> indexesList = new List<List<int>>();

				for (int q = 0; q < od.groups.Count; q++)
				{
					meshes.Add(new Mesh());
					tvertsList.Add(new List<Vector3>());
					tuvsList.Add(new List<Vector2>());
					tnormsList.Add(new List<Vector3>());
					indexesList.Add(new List<int>());
					Dictionary<Vector3, int> remapTable = new Dictionary<Vector3, int>();

					foreach (FaceIndices fi in od.groups[q].faces)
					{
						if (tvertsList[q].Count > 60000 && indexesList[q].Count % 3 == 0)
						{
							Mesh tempMesh = new Mesh();

							Debug.Log("Number of vertices: " + tvertsList[q].Count);
							tempMesh.vertices = tvertsList[q].ToArray();
							if (hasUVs) tempMesh.uv = tuvsList[q].ToArray();
							if (objectHasNormals) tempMesh.normals = tnormsList[q].ToArray();

							Material mat = null;

							string matName = (od.groups[q].materialName != null) ? od.groups[q].materialName : "default"; // MAYBE: "default" may not enough.
							if (mats.ContainsKey(matName))
							{
								mat = mats[matName];
								//Debug.Log("PopulateMeshes mat:" + matName + " set.");
							}
							else
							{
								Debug.LogWarning("PopulateMeshes mat:" + matName + " not found.");
							}

							tempMesh.subMeshCount = 1;
							tempMesh.SetTriangles(indexesList[q].ToArray(), 0);

							GameObject newObj = new GameObject();
							newObj.name = od.groups[q].name;
							newObj.AddComponent<MeshFilter>();
							newObj.AddComponent<MeshRenderer>();
							newObj.GetComponent<MeshFilter>().mesh = tempMesh;
							newObj.GetComponent<Renderer>().materials = new Material[1] { mat };
							newObj.transform.parent = gs[i].transform;

							meshes[q] = new Mesh();
							tvertsList[q] = new List<Vector3>();
							tuvsList[q] = new List<Vector2>();
							tnormsList[q] = new List<Vector3>();
							indexesList[q] = new List<int>();
							remapTable = new Dictionary<Vector3, int>();
						}

						Vector3 vertex = vertices[fi.vi];

						if (remapTable.ContainsKey(vertex))
						{
							indexesList[q].Add(remapTable[vertex]);
						}
						else
						{
							tvertsList[q].Add(vertex);
							indexesList[q].Add(tvertsList[q].Count - 1);

							remapTable.Add(vertex, tvertsList[q].Count - 1);

							if (hasUVs) tuvsList[q].Add(uvs[fi.vu]);
							if (hasNormals && fi.vn >= 0) tnormsList[q].Add(normals[fi.vn]);
						}
					}

					Debug.Log("Number of vertices: " + tvertsList[q].Count);
					meshes[q].vertices = tvertsList[q].ToArray();
					if (hasUVs) meshes[q].uv = tuvsList[q].ToArray();
					if (objectHasNormals) meshes[q].normals = tnormsList[q].ToArray();
				}

				int gl = od.groups.Count;
				Material[] materials = new Material[gl];

				//=========================================================

				for (int j = 0; j < gl; j++)
				{
					string matName = (od.groups[j].materialName != null) ? od.groups[j].materialName : "default"; // MAYBE: "default" may not enough.
					if (mats.ContainsKey(matName))
					{
						materials[j] = mats[matName];
						//Debug.Log("PopulateMeshes mat:" + matName + " set.");
					}
					else
					{
						Debug.LogWarning("PopulateMeshes mat:" + matName + " not found.");
					}
				
					meshes[j].subMeshCount = 1;
					meshes[j].SetTriangles(indexesList[j].ToArray(), 0);

					GameObject newObj = new GameObject();
					newObj.name = od.groups[j].name;
					newObj.AddComponent<MeshFilter>();
					newObj.AddComponent<MeshRenderer>();
					newObj.GetComponent<MeshFilter>().mesh = meshes[j];
					newObj.GetComponent<Renderer>().materials = new Material[1] { materials[j] };
					newObj.transform.parent = gs[i].transform;
				}

				if (!objectHasNormals)
				{
					for (int t = 0; t < od.groups.Count; t++)
					{
						meshes[t].RecalculateNormals();
					}
				}
			}
			else
			{
				Mesh m = new Mesh();

				List<Vector3> tvertsList = new List<Vector3>();
				List<Vector2> tuvsList = new List<Vector2>();
				List<Vector3> tnormsList = new List<Vector3>();
				List<int> indexesList = new List<int>();

				Dictionary<Vector3, int> remapTable = new Dictionary<Vector3, int>();

				foreach (FaceIndices fi in od.groups[0].faces)
				{
					if (tvertsList.Count > 60000 && indexesList.Count % 3 == 0)
					{
						Mesh tempMesh = new Mesh();

						Debug.Log("Number of vertices: " + tvertsList.Count);
						tempMesh.vertices = tvertsList.ToArray();
						if (hasUVs) tempMesh.uv = tuvsList.ToArray();
						if (objectHasNormals) tempMesh.normals = tnormsList.ToArray();

						Material mat = null;

						string tempMatName = (od.groups[0].materialName != null) ? od.groups[0].materialName : "default"; // MAYBE: "default" may not enough.
						if (mats.ContainsKey(tempMatName))
						{
							mat = mats[tempMatName];
							//Debug.Log("PopulateMeshes mat:" + matName + " set.");
						}
						else
						{
							Debug.LogWarning("PopulateMeshes mat:" + tempMatName + " not found.");
						}

						tempMesh.subMeshCount = 1;
						tempMesh.SetTriangles(indexesList.ToArray(), 0);

						GameObject tempObj = new GameObject();
						tempObj.name = od.groups[0].name;
						tempObj.AddComponent<MeshFilter>();
						tempObj.AddComponent<MeshRenderer>();
						tempObj.GetComponent<MeshFilter>().mesh = tempMesh;
						tempObj.GetComponent<Renderer>().materials = new Material[1] { mat };
						tempObj.transform.parent = gs[i].transform;

						m = new Mesh();
						tvertsList = new List<Vector3>();
						tuvsList = new List<Vector2>();
						tnormsList = new List<Vector3>();
						indexesList = new List<int>();
						remapTable = new Dictionary<Vector3, int>();
					}

					Vector3 vertex = vertices[fi.vi];

					if (remapTable.ContainsKey(vertex))
					{
						indexesList.Add(remapTable[vertex]);
					}
					else
					{
						tvertsList.Add(vertex);
						indexesList.Add(tvertsList.Count - 1);

						remapTable.Add(vertex, tvertsList.Count - 1);

						if (hasUVs) tuvsList.Add(uvs[fi.vu]);
						if (hasNormals && fi.vn >= 0) tnormsList.Add(normals[fi.vn]);
					}
				}

				Debug.Log("Number of vertices: " + tvertsList.Count);
				m.vertices = tvertsList.ToArray();
				if (hasUVs) m.uv = tuvsList.ToArray();
				if (objectHasNormals) m.normals = tnormsList.ToArray();

				//Debug.Log("PopulateMeshes only one group: " + od.groups[0].name);
				GroupData gd = od.groups[0];

				string matName = (gd.materialName != null) ? gd.materialName : "default"; // MAYBE: "default" may not enough.
				if (mats.ContainsKey(matName))
				{
					//Debug.Log("PopulateMeshes mat:" + matName + " set.");
				}
				else
				{
					Debug.LogWarning("PopulateMeshes mat:" + matName + " not found.");
				}

				m.subMeshCount = 1;
				m.SetTriangles(indexesList.ToArray(), 0);

				if (!objectHasNormals)
				{
					m.RecalculateNormals();
				}

				GameObject newObj = new GameObject();
				newObj.name = od.groups[0].name;
				newObj.AddComponent<MeshFilter>();
				newObj.AddComponent<MeshRenderer>();
				newObj.GetComponent<MeshFilter>().mesh = m;
				newObj.GetComponent<Renderer>().materials = new Material[1] { mats[matName] };
				newObj.transform.parent = gs[i].transform;
			}

			gs[i].transform.localScale = new Vector3(1, 1, -1);
			gs[i].transform.rotation = Quaternion.Euler(0, 180, 0);
		}
	}
}



























