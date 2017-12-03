using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

public class OBJThread
{
    public Shader Standard;
    public Shader BumpedDiffuse;
    public Shader BumpedSpecular;

    public string objPath;

	/* OBJ file tags */
	private const string O = "o";
	private const string G = "g";
	private const string V = "v";
	private const string VT = "vt";
	private const string VN = "vn";
	private const string F = "f";
	private const string MTL = "mtllib";
	private const string UML = "usemtl";

	/* MTL file tags */
	private const string NML = "newmtl";
	private const string NS = "Ns";
	// Shininess
	private const string KA = "Ka";
	// Ambient component (not supported)
	private const string KD = "Kd";
	// Diffuse component
	private const string KS = "Ks";
	// Specular component
	private const string D = "d";
	// Transparency (not supported)
	private const string TR = "Tr";
	// Same as 'd'
	private const string ILLUM = "illum";
	// Illumination model. 1 - diffuse, 2 - specular
	private const string MAP_KA = "map_Ka";
	// Ambient texture
	private const string MAP_KD = "map_Kd";
	// Diffuse texture
	private const string MAP_KS = "map_Ks";
	// Specular texture
	private const string MAP_KE = "map_Ke";
	// Emissive texture
	private const string MAP_BUMP = "map_bump";
	// Bump map texture
	private const string BUMP = "bump";
	// Bump map texture

	private Dictionary<char, int> digits;

	private string basepath;
	private string mtllib;
	private GeometryBufferThread buffer;

	private float loadProgressPercentage = 0;

	private List<List<Mesh>> totalMeshes;
	private Dictionary<string, Material> materials;

	private loadOBJFileJob loadOBJFileJob;

	private bool loadDone = false;
	private bool buildDone = false;

	public bool AllJobsDone
	{
		get
		{
			return loadDone && buildDone;
		}
	}

	void Start()
	{
		buffer = new GeometryBufferThread();
		//StartCoroutine(Load(objPath));
		//StartCoroutine(LoadLocal(objPath));
	}

	public void Update()
	{
		if (loadOBJFileJob != null)
		{
			if (loadOBJFileJob.Update())
			{
				// Alternative to the OnFinished callback
				//loadOBJFileJob = null;
			}

			if (loadOBJFileJob.IsDone)
			{
				loadOBJFileJob = null;
				loadDone = true;
				loadMats();
				Build();
				buildDone = true;
			}
		}
	}

	public void LoadLocal(string path)
	{
		objPath = path.Replace("\\", "/");

		digits = new Dictionary<char, int>();
		digits.Add('0', 0);
		digits.Add('1', 1);
		digits.Add('2', 2);
		digits.Add('3', 3);
		digits.Add('4', 4);
		digits.Add('5', 5);
		digits.Add('6', 6);
		digits.Add('7', 7);
		digits.Add('8', 8);
		digits.Add('9', 9);

		buffer = new GeometryBufferThread();

		loadOBJFileJob = new loadOBJFileJob();
		loadOBJFileJob.setPath(objPath);
		loadOBJFileJob.setThis(this);
		loadOBJFileJob.Start();
    }

	private Texture2D GetTextureLoaderLocal(MaterialData m, string texpath)
	{
		string ext = Path.GetExtension(texpath).ToLower();
		if (ext != ".png" && ext != ".jpg")
		{
			return new Texture2D(2, 2);
		}

		Texture2D tex = null;
		byte[] fileData;

		if (File.Exists(texpath))
		{
			fileData = File.ReadAllBytes(texpath);
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}

		return tex;
	}

	private void loadMats()
	{
		if (hasMaterials)
		{
			basepath = (objPath.IndexOf("/") == -1) ? "" : objPath.Substring(0, objPath.LastIndexOf("/") + 1);

			if (File.Exists(basepath + mtllib))
			{
				string mtText = new StreamReader(basepath + mtllib).ReadToEnd();
				//Debug.Log("base path = " + basepath);
				//Debug.Log("MTL path = " + (basepath + mtllib));
				SetMaterialData(mtText);

				foreach (MaterialData m in materialData)
				{
					if (m.diffuseTexPath != null)
					{
						m.diffuseTex = GetTextureLoaderLocal(m, basepath + m.diffuseTexPath);
					}
					if (m.bumpTexPath != null)
					{
						m.bumpTex = GetTextureLoaderLocal(m, basepath + m.bumpTexPath);
					}
				}
			}
			else
			{
				mtllib = null;
			}
		}

		materials = new Dictionary<string, Material>();

		if (hasMaterials)
		{
			foreach (MaterialData md in materialData)
			{
				if (materials.ContainsKey(md.name))
				{
					Debug.LogWarning("duplicate material found: " + md.name + ". ignored repeated occurences");
					continue;
				}
				materials.Add(md.name, GetMaterial(md));
			}
		}
		else
		{
			materials.Add("default", new Material(Standard));
		}
	}

	//=========================================================================

	private void GetFaceIndicesByOneFaceLine(FaceIndices[] faces, string[] p, bool isFaceIndexPlus)
	{
		if (isFaceIndexPlus)
		{
			for (int j = 1; j < p.Length; j++)
			{
				string[] c = p[j].Trim().Split(new char[] {'/'});
				FaceIndices fi = new FaceIndices();
				// vertex
				int vi = ci(c[0]);
				fi.vi = vi - 1;
				// uv
				if (c.Length > 1 && c[1] != "")
				{
					int vu = ci(c[1]);
					fi.vu = vu - 1;
				}

				faces[j - 1] = fi;
			}
		}
		else
		{
			// for minus index
			int vertexCount = buffer.vertices.Count;
			int uvCount = buffer.uvs.Count;
			for (int j = 1; j < p.Length; j++)
			{
				string[] c = p[j].Trim().Split(new char[] {'/'});
				FaceIndices fi = new FaceIndices();
				// vertex
				int vi = ci(c[0]);
				fi.vi = vertexCount + vi;
				// uv
				if (c.Length > 1 && c[1] != "")
				{
					int vu = ci(c[1]);
					fi.vu = uvCount + vu;
				}

				faces[j - 1] = fi;
			}
		}
	}

	public void SetGeometryData(string data)
	{
		string[] lines = data.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
		bool isFirstInGroup = true;
		bool isFaceIndexPlus = true;

		double totalStartTime = NowMilliseconds();

		for (int i = 0; i < lines.Length; i++)
		{
			string l = lines[i].Trim();

			if (l.Length < 3 || l.Substring(0, 2).Equals(VN))
			{
				continue;
			}

			string[] p = l.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

			if (p.Length == 0)
			{
				continue;
			}

			switch (p[0])
			{
			case O:
				buffer.PushObject(p[1].Trim());
				isFirstInGroup = true;
				break;
			case G:
				string groupName = null;
				if (p.Length >= 2)
				{
					groupName = p[1].Trim();
				}
				isFirstInGroup = true;
				buffer.PushGroup(groupName);
				break;
			case V:
				buffer.PushVertex(new Vector3(cf(p[1]), cf(p[2]), cf(p[3])));
				break;
			case VT:
				buffer.PushUV(new Vector2(cf(p[1]), cf(p[2])));
				break;
			case F:
				if (l.Contains("\\"))
				{
					string newLine = l.Substring(0, l.IndexOf('\\'));
					do
					{
						i++;
						string nextLine = lines[i].Trim();
						if (nextLine.Contains("\\"))
						{
							newLine += nextLine.Substring(0, nextLine.IndexOf('\\'));
						}
						else
						{
							newLine += nextLine;
						}
					}
					while(lines[i].Contains("\\"));

					l = newLine;
					p = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					if (p.Length == 0)
					{
						continue;
					}
				}

				FaceIndices[] faces = new FaceIndices[p.Length - 1];
				if (isFirstInGroup)
				{
					isFirstInGroup = false;
					string[] c = p[1].Trim().Split(new char[] {'/'});
					isFaceIndexPlus = (ci(c[0]) >= 0);
				}
				GetFaceIndicesByOneFaceLine(faces, p, isFaceIndexPlus);
				if (p.Length == 4) //triangle
				{
					buffer.PushFace(faces[0]);
					buffer.PushFace(faces[1]);
					buffer.PushFace(faces[2]);
				}
				else if (p.Length == 5) //quad
				{
					buffer.PushFace(faces[0]);
					buffer.PushFace(faces[1]);
					buffer.PushFace(faces[3]);

					buffer.PushFace(faces[3]);
					buffer.PushFace(faces[1]);
					buffer.PushFace(faces[2]);
				}
				else //polygon
				{
					for (int j = 1; j < faces.Length - 1; j++)
					{
						buffer.PushFace(faces[0]);
						buffer.PushFace(faces[j]);
						buffer.PushFace(faces[j + 1]);
					}

					//Debug.LogWarning("face vertex count :" + (p.Length - 1) + " larger than 4:");
				}
				break;
			case MTL:
				//mtllib = l.Substring(p[0].Length + 1).Trim();
                mtllib = "0.mtl";
                break;
			case UML:
				buffer.PushMaterialName(p[1].Trim());
				break;
			}

			loadProgressPercentage = ((float)i / (float)lines.Length) * 75.0f;
		}

		double endTime = NowMilliseconds();

		double totalTime = endTime - totalStartTime;

		Debug.Log("Parse Time: " + totalTime);

		// buffer.Trace();
	}

	private double NowMilliseconds()
	{
		return (System.DateTime.UtcNow - 
		        new System.DateTime(1970, 1, 1, 0, 0, 0, 
		            System.DateTimeKind.Utc)).TotalMilliseconds;
	}

	private float cf(string v)
	{
		float outValue;
		if (float.TryParse(v, out outValue))
		{
			return outValue;
		}
		else
		{
			Debug.LogWarning("Could not parse float");
			return 0;
		}
	}

	private int ci(string v)
	{
		int outValue;
		if (int.TryParse(v, out outValue))
		{
			return outValue;
		}
		else
		{
			Debug.LogWarning("Could not parse int");
			return 0;
		}
	}

	private bool hasMaterials
	{
		get
		{
			return mtllib != null;
		}
	}

	/* ############## MATERIALS */
	private List<MaterialData> materialData;

	private class MaterialData
	{
		public string name;
		public Color ambient;
		public Color diffuse;
		public Color specular;
		public float shininess;
		public float alpha;
		public int illumType;
		public string diffuseTexPath;
		public string bumpTexPath;
		public Texture2D diffuseTex;
		public Texture2D bumpTex;
	}

	private void SetMaterialData(string data)
	{
		string[] lines = data.Split("\n".ToCharArray());

		materialData = new List<MaterialData>();
		MaterialData current = new MaterialData();
		//Regex regexWhitespaces = new Regex(@"\s+");

		for (int i = 0; i < lines.Length; i++)
		{
			string l = lines[i].Trim();

			if (l.IndexOf("#") != -1) l = l.Substring(0, l.IndexOf("#"));

			string[] p = l.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			if (p.Length == 0)
			{
				continue;
			}
			//string[] p = regexWhitespaces.Split(l); // (slow)

			if (p[0].Trim() == "") continue;

			switch (p[0])
			{
			case NML:
				current = new MaterialData();
				current.name = p[1].Trim();
				materialData.Add(current);
				break;
			case KA:
				current.ambient = gc(p);
				break;
			case KD:
				current.diffuse = gc(p);
				break;
			case KS:
				current.specular = gc(p);
				break;
			case NS:
				current.shininess = cf(p[1]) / 1000;
				break;
			case D:
				break;
			case TR:
				current.alpha = cf(p[1]);
				break;
			case MAP_KD:
				current.diffuseTexPath = p[p.Length - 1].Trim();
				break;
			case MAP_BUMP:
			case BUMP:
				BumpParameter(current, p);
				break;
			case ILLUM:
				current.illumType = ci(p[1]);
				break;
			default:
				//Debug.Log("this line was not processed :" + l);
				break;
			}
		}	
	}

	private Material GetMaterial(MaterialData md)
	{
		Material m;

		if (md.illumType == 2)
		{
            if (md.bumpTex != null)
            {
                m = new Material(BumpedSpecular);
            }
            else
            {
                m = new Material(Standard);
            }
            
			m.SetColor("_SpecColor", md.specular);
			m.SetFloat("_Shininess", md.shininess);
		}
		else
		{
            if (md.bumpTex != null)
            {
                m = new Material(BumpedDiffuse);
            }
            else
            {
                m = new Material(Standard);
            }
        }

		if (md.diffuseTex != null)
		{
			m.SetTexture("_MainTex", md.diffuseTex);
		}
		else
		{
			m.SetColor("_Color", md.diffuse);
		}
		if (md.bumpTex != null) m.SetTexture("_BumpMap", md.bumpTex);

		m.name = md.name;

		return m;
	}

	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,
		Transparent
	}

	public void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
	{
		switch (blendMode)
		{
		case BlendMode.Opaque:
			standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
			standardShaderMaterial.SetInt("_ZWrite", 1);
			standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
			standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
			standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			standardShaderMaterial.renderQueue = -1;
			break;
		case BlendMode.Cutout:
			standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
			standardShaderMaterial.SetInt("_ZWrite", 1);
			standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
			standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
			standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			standardShaderMaterial.renderQueue = 2450;
			break;
		case BlendMode.Fade:
			standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			standardShaderMaterial.SetInt("_ZWrite", 0);
			standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
			standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
			standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			standardShaderMaterial.renderQueue = 3000;
			break;
		case BlendMode.Transparent:
			standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			standardShaderMaterial.SetInt("_ZWrite", 0);
			standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
			standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
			standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			standardShaderMaterial.renderQueue = 3000;
			break;
		}

	}

	private class BumpParamDef
	{
		public string optionName;
		public string valueType;
		public int valueNumMin;
		public int valueNumMax;

		public BumpParamDef(string name, string type, int numMin, int numMax)
		{
			this.optionName = name;
			this.valueType = type;
			this.valueNumMin = numMin;
			this.valueNumMax = numMax;
		}
	}

	private void BumpParameter(MaterialData m, string[] p)
	{
		Regex regexNumber = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");

		var bumpParams = new Dictionary<String, BumpParamDef>();
		bumpParams.Add("bm", new BumpParamDef("bm", "string", 1, 1));
		bumpParams.Add("clamp", new BumpParamDef("clamp", "string", 1, 1));
		bumpParams.Add("blendu", new BumpParamDef("blendu", "string", 1, 1));
		bumpParams.Add("blendv", new BumpParamDef("blendv", "string", 1, 1));
		bumpParams.Add("imfchan", new BumpParamDef("imfchan", "string", 1, 1));
		bumpParams.Add("mm", new BumpParamDef("mm", "string", 1, 1));
		bumpParams.Add("o", new BumpParamDef("o", "number", 1, 3));
		bumpParams.Add("s", new BumpParamDef("s", "number", 1, 3));
		bumpParams.Add("t", new BumpParamDef("t", "number", 1, 3));
		bumpParams.Add("texres", new BumpParamDef("texres", "string", 1, 1));
		int pos = 1;
		string filename = null;
		while (pos < p.Length)
		{
			if (!p[pos].StartsWith("-"))
			{
				filename = p[pos];
				pos++;
				continue;
			}
			// option processing
			string optionName = p[pos].Substring(1);
			pos++;
			if (!bumpParams.ContainsKey(optionName))
			{
				continue;
			}
			BumpParamDef def = bumpParams[optionName];
			ArrayList args = new ArrayList();
			int i = 0;
			bool isOptionNotEnough = false;
			for (; i < def.valueNumMin; i++, pos++)
			{
				if (pos >= p.Length)
				{
					isOptionNotEnough = true;
					break;
				}
				if (def.valueType == "number")
				{
					Match match = regexNumber.Match(p[pos]);
					if (!match.Success)
					{
						isOptionNotEnough = true;
						break;
					}
				}
				args.Add(p[pos]);
			}
			if (isOptionNotEnough)
			{
				//Debug.Log("bump variable value not enough for option:" + optionName + " of material:" + m.name);
				continue;
			}
			for (; i < def.valueNumMax && pos < p.Length; i++, pos++)
			{
				if (def.valueType == "number")
				{
					Match match = regexNumber.Match(p[pos]);
					if (!match.Success)
					{
						break;
					}
				}
				args.Add(p[pos]);
			}
			// TODO: some processing of options
			//Debug.Log("found option: " + optionName + " of material: " + m.name + " args: " + String.Concat(args.ToArray()));
		}
		if (filename != null)
		{
			m.bumpTexPath = filename;
		}
	}

	private Color gc(string[] p)
	{
		return new Color(cf(p[1]), cf(p[2]), cf(p[3]));
	}

	public List<string> getObjectNames()
	{
		return buffer.getObjectNames();
	}

	public Dictionary<string, Material> getTotalMaterials()
	{
		//return buffer.getTotalMaterials();
		return materials;
	}

	public List<List<Mesh>> getModelData()
	{
		return totalMeshes;
	}

	public string getPath()
	{
		return objPath;
	}

	public float getLoadPercentage()
	{
		return loadProgressPercentage + buffer.getBuildProgress();
	}

	public void Prepare()
	{
		if (!buffer.PopulateMeshes(buffer.numObjects, materials))
		{
			Debug.Log("Number of objects not equal");	
		}
	}

	public void Build()
	{
		totalMeshes = buffer.finishMeshes(buffer.numObjects);
	}
}
