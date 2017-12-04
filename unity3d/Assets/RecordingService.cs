using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Text;
using SimpleJSON;

public class RecordingService : Singleton<RecordingService> {

    private static int TIMEOUT_SECONDS = 5;

    private AudioSource audioSource;

    List<float> tempRecording = new List<float>();
    private float[] prevRecording;
    private bool isRecording = false;
    private int counter = 0;

    private Action<String> textAction;

    // Use this for initialization
    void Start ()
    {
        audioSource = GetComponent<AudioSource>();
        //set up recording to last a max of 1 seconds and loop over and over
        audioSource.clip = Microphone.Start(null, true, 1, 44100);
        audioSource.Play();
        //resize our temporary vector every second
        Invoke("ResizeRecording", 1);
    }

    void ResizeRecording()
    {
        if (isRecording)
        {
            //add the next second of recorded audio to temp vector
            int length = 44100;
            float[] clipData = new float[length];
            audioSource.clip.GetData(clipData, 0);
            tempRecording.AddRange(clipData);
            Invoke("ResizeRecording", 1);
        }
    }

    public void Record(Action<String> textAction)
    {
        Assert.IsFalse(isRecording);

        isRecording = true;

        //stop audio playback and start new recording...
        audioSource.Stop();
        tempRecording.Clear();
        Microphone.End(null);
        audioSource.clip = Microphone.Start(null, true, 1, 44100);
        Invoke("ResizeRecording", 1);

        
        this.textAction = textAction;
        StartCoroutine("Timeout");
    }

    IEnumerator Timeout()
    {
        yield return new WaitForSeconds(TIMEOUT_SECONDS);

        Debug.Log("Checking Timeout Status");

        if (isRecording)
        {
            Debug.Log("Invoking timeout");
            Transcribe();
        }
        else
        {
            Debug.Log("Timeout already occurred");
        }
    }

    public void Stop()
    {
        Assert.IsFalse(isRecording);
        isRecording = false;
        counter++;

        //stop recording, get length, create a new array of samples
        int length = Microphone.GetPosition(null);

        Microphone.End(null);
        float[] clipData = new float[length];
        audioSource.clip.GetData(clipData, 0);

        //create a larger vector that will have enough space to hold our temporary
        //recording, and the last section of the current recording
        float[] fullClip = new float[clipData.Length + tempRecording.Count];
        for (int i = 0; i < fullClip.Length; i++)
        {
            //write data all recorded data to fullCLip vector
            if (i < tempRecording.Count)
                fullClip[i] = tempRecording[i];
            else
                fullClip[i] = clipData[i - tempRecording.Count];
        }

        prevRecording = fullClip;

        audioSource.clip = AudioClip.Create("recorded samples", fullClip.Length, 1, 44100, false);
        audioSource.clip.SetData(fullClip, 0);
        audioSource.loop = true;
    }

    public void Transcribe()
    {
        if (isRecording) Stop();

        var filename = "clip" + counter + ".wav";

        audioSource.Stop();
        int length = prevRecording.Length;
        Debug.Log("Length: " + length);
        audioSource.clip = AudioClip.Create("recorded samples", length, 1, 44100, false);
        audioSource.clip.SetData(prevRecording, 0);

        Debug.Log("Saving " + filename);
        WavBuilder.Save(filename, audioSource.clip);

        var filepath = Path.Combine(Application.persistentDataPath, filename);

        audioSource.loop = false;
        audioSource.Play();

        var headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/x-protobuf");
        var b64 = Encoding.ASCII.GetBytes(ConvertBase64(filepath));

        var api = new WWW("http://45.55.197.39:8001/api/v1/transcribe", b64, headers);
        StartCoroutine(TranscribeRequest(api));
    }

    private String ConvertBase64(String filename)
    {
        Byte[] bytes = File.ReadAllBytes(filename);
        return Convert.ToBase64String(bytes);
    }

    IEnumerator TranscribeRequest(WWW www)
    {
        yield return www;

        try
        {
            var j = JSON.Parse(www.text);
            textAction(j["results"][0]["alternatives"]["transcript"]);
        } catch(UnityException ex)
        {
            Debug.LogError(ex);
        }
    }

    public bool IsRecording()
    {
        return isRecording;
    }


}
