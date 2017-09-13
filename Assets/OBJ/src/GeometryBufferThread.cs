using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GeometryBufferThread
{
	private List<ObjectData> objects;

	public List<Vector3> vertices;
	public List<Vector2> uvs;
	public List<Vector3> normals;
	public int unnamedGroupIndex = 1;
	// naming index for unnamed group. like "Unnamed-1"

	//=======================================
	//Used in the PopulateMeshes class

	List<List<string>> totalMaterials;
	List<List<Vector3>>[] tvertsList;
	List<List<Vector2>>[] tuvsList;
	List<List<int>>[] indexesList;

	//=======================================

	private float buildProgressPercentage = 0;

	private ObjectData current;

	public class ObjectData
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

	public class GroupData
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

	public GeometryBufferThread()
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

	public List<string> getObjectNames()
	{
		List<string> returnValue = new List<string>();

		foreach (ObjectData obj in objects)
		{
			returnValue.Add(obj.name);
		}

		return returnValue;
	}

	public float getBuildProgress()
	{
		return buildProgressPercentage;
	}

	private double NowMilliseconds()
	{
		return (System.DateTime.UtcNow - 
		        new System.DateTime(1970, 1, 1, 0, 0, 0, 
		                            System.DateTimeKind.Utc)).TotalMilliseconds;
	}

	public bool PopulateMeshes(int numberOfObject, Dictionary<string, Material> mats)
	{
		if (numberOfObject != numObjects) return false; // Should not happen unless obj file is corrupt...

		double startTime = NowMilliseconds();

		totalMaterials = new List<List<string>>();
		tvertsList = new List<List<Vector3>>[numberOfObject];
		tuvsList = new List<List<Vector2>>[numberOfObject];
		indexesList = new List<List<int>>[numberOfObject];

		for (int i = 0; i < numberOfObject; i++)
		{
			totalMaterials.Add(new List<string>());
			tvertsList[i] = new List<List<Vector3>>();
			tuvsList[i] = new List<List<Vector2>>();
			indexesList[i] = new List<List<int>>();

			int q = 0;
			int numberOfGroups = objects[i].groups.Count;

			foreach (GroupData currentGroup in objects[i].groups)
			{
				tvertsList[i].Add(new List<Vector3>());
				tuvsList[i].Add(new List<Vector2>());
				indexesList[i].Add(new List<int>());
				Dictionary<Vector3, int> remapTable = new Dictionary<Vector3, int>();

				string matName = (currentGroup.materialName != null) ? currentGroup.materialName : "default";

				totalMaterials[i].Add(matName);

				foreach (FaceIndices fi in currentGroup.faces)
				{
					if (tvertsList[i][q].Count > 60000 && indexesList[i][q].Count % 3 == 0)
					{
						totalMaterials[i].Add(matName);
						tvertsList[i].Add(new List<Vector3>());
						tuvsList[i].Add(new List<Vector2>());
						indexesList[i].Add(new List<int>());
						remapTable = new Dictionary<Vector3, int>();

						q++;
					}

					Vector3 vertex = vertices[fi.vi];

					int outValue;

					if (remapTable.TryGetValue(vertex, out outValue))
					{
						indexesList[i][q].Add(outValue);
					}
					else
					{
						tvertsList[i][q].Add(vertex);
						indexesList[i][q].Add(tvertsList[i][q].Count - 1);

						remapTable.Add(vertex, tvertsList[i][q].Count - 1);

						if (hasUVs) tuvsList[i][q].Add(uvs[fi.vu]);
					}
				}

				buildProgressPercentage = 
					(((float)(i + 1) / (float)numberOfObject) * 
				    (float)q / (float)numberOfGroups) 
					* 25.0f;
				q++;
			}
		}

		double endTime = NowMilliseconds();

		Debug.Log("Build Time: " + (endTime - startTime));

		return true;
	}

	public List<List<Mesh>> finishMeshes(int numberOfObject)
	{
		if (numberOfObject != numObjects) return new List<List<Mesh>>(); // Should not happen unless obj file is corrupt...

		List<List<Mesh>> totalMeshes = new List<List<Mesh>>();

		for (int i = 0; i < numberOfObject; i++)
		{
			totalMeshes.Add(new List<Mesh>());

			for (int j = 0; j < tvertsList[i].Count; j++)
			{
				Mesh newMesh = new Mesh();
				//Debug.Log("Number of vertices: " + tvertsList[i][j].Count);

				newMesh.vertices = tvertsList[i][j].ToArray();
				if (hasUVs) newMesh.uv = tuvsList[i][j].ToArray();

				newMesh.subMeshCount = 1;
				newMesh.SetTriangles(indexesList[i][j].ToArray(), 0);
				newMesh.name = totalMaterials[i][j];
				newMesh.RecalculateNormals();
				totalMeshes[i].Add(newMesh);
			}
		}

		return totalMeshes;
	}
}
