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

    public void BindAdaptionFunction(int ClipID,int curveIndex, int keyFrameIndex, Func<Keyframe, Keyframe> func)
    {
        AnimationDatas[ClipID].SetAnimationAdaptionFunction(curveIndex, keyFrameIndex, func);
    }

    public AnimationClip UpdateAdaptionFunction(int ClipID,GameObject obj)
    {
        AnimationDatas[ClipID].UpdateAnimationAdaption();
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
 }