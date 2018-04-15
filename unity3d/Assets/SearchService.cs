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
using System.Linq;

public sealed class SearchService: Singleton<SearchService> {

    //private static string HOST = "http://45.55.197.39:8001/api/v1/";
    // http://165.227.191.119:8080/e5dff6ec-cf94-47b5-87ed-d26c37a0f9e9
    private static string HOST = "http://165.227.191.119:8080/api/v1/";
    private static string SEARCH_URI = HOST + "search";
    private static string DOWNLOAD_URI = "https://storage.googleapis.com/realitycheck/";
    
    private FastZip fastZip = new FastZip();
    public Text debugText;

    protected SearchService() {}

    // Delete App Cache Folder
    public void Flush() {
        var path = Application.temporaryCachePath;
        var di = new DirectoryInfo(path);
    
        foreach (var file in di.GetFiles()) file.Delete();
        foreach (var dir in di.GetDirectories()) dir.Delete(true);
    }

    // Send search request
    public void Search(string query, Action<Search.SearchResult> callBack)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-protobuf" }
            };

            var req = new Search.SearchRequest
            {
                Query = query,
                ResultPerPage = 4,
                PageNumber = 1
            };

            WWW api = new WWW(SEARCH_URI, req.ToByteArray(), headers);

            StartCoroutine(SearchRequest(api, callBack));
        }
        catch (UnityException ex)
        {
            debugText.text = ex.Message;
            Debug.Log(ex.Message);
        }
    }

    public void Search(Search.SearchRequest req, Action<Search.SearchResult> callBack)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-protobuf" }
            };

            WWW api = new WWW(SEARCH_URI, req.ToByteArray(), headers);

            StartCoroutine(SearchRequest(api, callBack));
        }
        catch (UnityException ex)
        {
            debugText.text = ex.Message;
            Debug.Log(ex.Message);
        }
    }

    // Download model from server
    public void DownloadModel(Search.Hit hit, Action<NetModel> callBack)
    {
        DownloadModel(hit.Asset.Filename, hit.Asset.Uuid, callBack);
    }

    public void DownloadModel(UserAssetState ua, Action<NetModel> callBack)
    {
        DownloadModel(ua.uuid, ua.uuid, callBack);
    }

    public void DownloadModel(string filename, string uuid, Action<NetModel> callBack)
    {
        try
        {

            var fp = DOWNLOAD_URI + filename;
            StartCoroutine(DownloadRequest(new WWW(fp), fp, filename, uuid, callBack));
        }
        catch (UnityException ex)
        {
            debugText.text = ex.Message;
            Debug.Log(ex.Message);
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
    private IEnumerator DownloadRequest(WWW www, string path, string file_uuid, string uuid, Action<NetModel> callBack) {
        yield return www;

        try {

            var nm = new NetModel();

            // string uuid = hit.Asset.Uuid;
            string filePath = Application.temporaryCachePath + Path.DirectorySeparatorChar + uuid + ".zip";
            string extractPath = Application.temporaryCachePath + Path.DirectorySeparatorChar;
            
            debugText.text = "Downloading " + path + " as " + filePath;
            Debug.Log("Downloading " + path + " as " + filePath);

            File.WriteAllBytes(filePath, www.bytes);
            
            debugText.text = "Extracting " + filePath + " as " + extractPath;
            Debug.Log("Extracting " + filePath + " as " + extractPath);

            ZipConstants.DefaultCodePage = 0;
            
            fastZip.ExtractZip(filePath, extractPath, null);

            var daeFiles = Directory.GetFiles(extractPath, "*.dae", SearchOption.AllDirectories).ToList();
            var blendFiles = Directory.GetFiles(extractPath, "*.blend", SearchOption.AllDirectories).ToList();
            var fbxFiles = Directory.GetFiles(extractPath, "*.fbx", SearchOption.AllDirectories).ToList();
            var objFiles = Directory.GetFiles(extractPath, "*.obj", SearchOption.AllDirectories).ToList();

            string assetFile = null;
            if (objFiles.Any()) assetFile = objFiles[0];
            if (fbxFiles.Any()) assetFile = fbxFiles[0];
            if (daeFiles.Any()) assetFile = daeFiles[0];
            if (blendFiles.Any()) assetFile = blendFiles[0];
            
            
            Debug.Log("Found ... " + assetFile);


            nm.file = assetFile.Replace("\\", "/");

            Debug.Log("DL: " + nm.file);

            nm.file_uuid = file_uuid; // hit.Asset.Filename;
            nm.obj = extractPath + Path.DirectorySeparatorChar + "0.obj";
            nm.mtl = extractPath + Path.DirectorySeparatorChar + "0.mtl";

            nm.uuid = uuid;
            Debug.Log("IDS : " + nm.uuid + " " + nm.file_uuid);

            callBack(nm);
        } catch (UnityException ex) {
            debugText.text = ex.Message;
            Debug.Log(ex.Message);
	    }
    }
}