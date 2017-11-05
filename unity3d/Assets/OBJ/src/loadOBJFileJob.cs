using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class loadOBJFileJob : ThreadedJob 
{
	private OBJThread thisLoader;
	private string pathName;

	public void setPath(string path)
	{
		pathName = path;
	}

	public void setThis(OBJThread loader)
	{
		thisLoader = loader;
	}

	protected override void ThreadFunction()
	{
		string fileText = new StreamReader(pathName).ReadToEnd();
		thisLoader.SetGeometryData(fileText);
		thisLoader.Prepare();
	}

	protected override void OnFinished()
	{
		
	}
}
