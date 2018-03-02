using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Net;

using UnityEngine;
using UnityEngine.UI;
//using SimpleJSON;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime.Serialization.Formatters.Binary; 
using Google.Protobuf;

public sealed class SearchService: Singleton<SearchService> {

    private static string HOST = "http://45.55.197.39:8001/api/v1/";
    private static string SEARCH_URI = HOST + "search";
    private static string DOWNLOAD_URI = "https://storage.googleapis.com/realitycheck/";
    
    private FastZip fastZip = new FastZip();

    private Dictionary<Search.Hit, NetModel> cachedModels = new Dictionary<Search.Hit, NetModel>();

    public Text debugText;

    protected SearchService() {}

    // Delete App Cache Folder
    public void Flush() {
        cachedModels.Clear();

        string path = Application.temporaryCachePath;
 
        DirectoryInfo di = new DirectoryInfo(path);
    
        foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();
        foreach (System.IO.DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
    }

    // Send search request
    public void Search(string query, Action<Search.SearchResult> callBack) {
        try {
            Dictionary<string,string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/x-protobuf");

            var req = new Search.SearchRequest{
                Query = query
            };
            
            WWW api = new WWW(SEARCH_URI, req.ToByteArray(), headers);

            StartCoroutine(SearchRequest(api, callBack));
        } catch (UnityException ex) {
            debugText.text = ex.Message;
            Debug.Log(ex.Message);
	    }
    }

    // Download model from server
    public void DownloadModel(Search.Hit hit, Action<NetModel> callBack) {

        if (cachedModels.ContainsKey(hit))
        {
            callBack(cachedModels[hit]);
        }
        else
        {
            try
            {
                var fp = DOWNLOAD_URI + hit.Asset.Filename;
                StartCoroutine(DownloadRequest(new WWW(fp), fp, hit, callBack));
            }
            catch (UnityException ex)
            {
                debugText.text = ex.Message;
                Debug.Log(ex.Message);
            }
        }
    }

    // Send search query
    private IEnumerator SearchRequest(WWW www, Action<Search.SearchResult> callBack) {
		yield return www;
		
		string payload = "";

		if (string.IsNullOrEmpty(www.error)) {
            byte[] bytes = Convert.FromBase64String(www.text);

            var res = new Search.SearchResult();
            res.MergeFrom(bytes);
            
            callBack(res);
		} else {
			payload = www.error;
		}
	}

    // Decompress a GZip file
    // Not in use at the moment
    private void DecompressGZip(string path) {
        FileInfo info = new FileInfo(path);

        using (FileStream fs = info.OpenRead()) {
            string currentFileName = info.FullName;
            string newFileName = currentFileName.Remove(currentFileName.Length - info.Extension.Length);

            using (FileStream decompressedFileStream = File.Create(newFileName)) {
                using (GZipStream decompressionStream = new GZipStream(fs, CompressionMode.Decompress)) {
                    StreamExtensions.CopyTo(decompressionStream, decompressedFileStream);
                    Debug.Log("Decompressed: " + info.Name);
                }
            }
        }
    }

    // Download the compressed model files from the server
    // Extract the files
    private IEnumerator DownloadRequest(WWW www, string path, Search.Hit hit, Action<NetModel> callBack) {
        yield return www;

        try {
            string noExt = hit.Asset.Filename.Substring(0, hit.Asset.Filename.Length-4);

            string filePath = Application.temporaryCachePath + Path.DirectorySeparatorChar + hit.Asset.Filename;
            string extractPath = Application.temporaryCachePath + Path.DirectorySeparatorChar + noExt;        
            
            debugText.text = "Downloading " + path + " as " + filePath;
            Debug.Log("Downloading " + path + " as " + filePath);

            File.WriteAllBytes(filePath, www.bytes);
            
            debugText.text = "Extracting " + filePath + " as " + extractPath;
            Debug.Log("Extracting " + filePath + " as " + extractPath);

            ZipConstants.DefaultCodePage = 0;
            
            fastZip.ExtractZip(filePath, extractPath, null);
            
            try {
                File.Move(extractPath + Path.DirectorySeparatorChar + noExt + "\\0.obj", extractPath + Path.DirectorySeparatorChar + "0.obj");
                File.Move(extractPath + Path.DirectorySeparatorChar + noExt + "\\0.mtl", extractPath + Path.DirectorySeparatorChar + "0.mtl");
            }
            catch (UnityException ex)
            {
                debugText.text = ex.Message;
                Debug.LogError(ex.Message);
                Debug.Log(ex.Message);
            }
            
            var nm = new NetModel();
            
            nm.obj = extractPath + Path.DirectorySeparatorChar + "0.obj";
            nm.mtl = extractPath + Path.DirectorySeparatorChar + "0.mtl";
            Debug.Log("DL: " + nm.obj);

            if (!cachedModels.ContainsKey(hit))
            {
                cachedModels.Add(hit, nm);
            }
            
            callBack(nm);
        } catch (UnityException ex) {
            debugText.text = ex.Message;
            Debug.Log(ex.Message);
	    }
    }
}