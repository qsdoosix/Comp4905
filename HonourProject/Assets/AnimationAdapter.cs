using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimationAdapter : MonoBehaviour
{
    List<AnimationData> AnimationDatas = new List<AnimationData>();
    public string filepath;

    private static AnimationAdapter _instance;
    public static AnimationAdapter Instance
    {
        get
        {
            // lazy initialization/instantiation
            if (_instance) return _instance;

            _instance = FindObjectOfType<AnimationAdapter>();

            if (_instance) return _instance;

            _instance = new GameObject("AnimationAdapter").AddComponent<AnimationAdapter>();

            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Debug.LogWarning("Multiple Instances of AnimationAdapter! Will ignore this one!", this);
            return;
        }
        _instance = this;
        filepath = Application.persistentDataPath;
        DontDestroyOnLoad(gameObject);
    }

    public void Start(){
        if (File.Exists(Path.Combine(filepath, "FileList.txt")))
        {
            //Load from the file
        }
        else
        {
            //Do nothing because the there is no file.
        }
    }

    public int AddData(AnimationClip input,string Aname)
    {
        /* Input: The animation clip need to add into the manager
         * Return: The index of that animation clip in the manager
         * Error: Return negative number
         */
        
        //Avoid dumplicated clip
        //Also can be used when loading the clip from file instead of Unity Editor
        int t = SearchClipName(Aname);
        if (t >= 0)
        {
            return t;
        }

        if (Application.isEditor)
        {
            AnimationData data = new AnimationData(input,Aname);
            AnimationDatas.Add(data);
            return AnimationDatas.Count - 1;
        }
        else
        {
            Debug.LogError("Can't add data outside Editor");
            return -1;
        }
    }

    public int SearchClipName(string Name)
    {
        // Search for clip index by name
        // Return -1 if not found
        int result = -1;
        for(int i =0;i< AnimationDatas.Count; i++)
        {
            if (AnimationDatas[i].GetName().CompareTo(Name) == 0)
            {
                result = i;
            }
        }
        return result;
    }

    public int GetAnimationCurveIndex(int clipIndex, string curvename, string relativepath)
    {
        //Search the property name at relative path in given Animation data
        //Returns -1 if not found.
        return AnimationDatas[clipIndex].GetCurveIDbyName(curvename,relativepath);
    }

    public void BindAdaptionFunction(int ClipID,int curveIndex, int keyFrameIndex, Func<Keyframe,GameObject, Keyframe> func)
    {
        AnimationDatas[ClipID].SetAnimationAdaptionFunction(curveIndex, keyFrameIndex, func);
    }

    public AnimationClip UpdateAdaptionFunction(int ClipID,GameObject obj)
    {
        AnimationDatas[ClipID].UpdateAnimationAdaption(obj);
        return AnimationDatas[ClipID].OverrideAnimation(obj);
    }

    public AnimationClip UpdateAdaptionFunction(int ClipID,float Timemodifier, GameObject obj)
    {
        AnimationDatas[ClipID].UpdateAnimationAdaption(obj);
        AnimationDatas[ClipID].RescaleAnimationTime(Timemodifier);
        return AnimationDatas[ClipID].OverrideAnimation(obj);
    }

    public void SaveData()
    {
        using (StreamWriter sw = File.CreateText(Path.Combine(filepath, "FileList.txt")))
        {
            foreach (AnimationData data in AnimationDatas)
            {
                data.SaveToFile(filepath);
                sw.WriteLine(data.GetName() + ".dat");
            }
        }
    }
    public string GetAnimationData(int ClipID)
    {
        return AnimationDatas[ClipID].GetAnimationCurveData();
    }
 }