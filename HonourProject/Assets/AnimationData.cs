using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimationData
{
    [Serializable]
    public sealed class AnimationInfo
    {
        public AnimationInfo() { }
        public AnimationInfo(string Aname) {
            AnimationName = Aname;
        }
        public string AnimationName;
        public List<CurveInfo> ListOfCurves;
    }
    [Serializable]
    public sealed class CurveInfo
    {
        public string relativePath;
        public string propertyName;
        public List<KeyFrameInfo> Key;

        public CurveInfo() { }

        public CurveInfo(string rp, string pn, Keyframe[] keys)
        {
            relativePath = rp;
            propertyName = pn;
            Key = new List<KeyFrameInfo>();
            foreach (Keyframe k in keys)
            {
                Key.Add(new KeyFrameInfo(k));
            }
        }
    }
    [Serializable]
    public sealed class KeyFrameInfo
    {
        public float Value;
        public float InTangent;
        public float InWeight;
        public float OutTangent;
        public float OutWeight;
        public float Time;
        public WeightedMode WeightedMode;

        // default constructor is sometimes required for (de)serialization
        public KeyFrameInfo() { }

        public KeyFrameInfo(Keyframe keyframe)
        {
            Value = keyframe.value;
            InTangent = keyframe.inTangent;
            InWeight = keyframe.inWeight;
            OutTangent = keyframe.outTangent;
            OutWeight = keyframe.outWeight;
            Time = keyframe.time;
            WeightedMode = keyframe.weightedMode;
        }

        public Keyframe ToKeyframe()
        {
            Keyframe k = new Keyframe(Time, Value, InTangent, OutTangent, InWeight, OutWeight)
            {
                weightedMode = WeightedMode
            };
            return k;
        }
    }
    //The name of this animation, like attack, move, jump, etc.
    //Data encapsulation
    private string[] _RelativePath;
    public string[] RelativePath {
        get
        {
            return _RelativePath;
        }
    }
    private string[] _PropertyName;
    public string[] PropertyName
    {
        get
        {
            return _PropertyName;
        }
    }
    private Keyframe[][] _Keyframedata;
    public Keyframe[][] Keyframedata
    {
        get
        {
            return _Keyframedata;
        }
    }
    private Keyframe[][] _KeyframeValues;
    public Keyframe[][] KeyframeValues
    {
        get
        {
            return _KeyframeValues;
        }
    }

    private Func<Keyframe, Keyframe>[][] AdaptFunctions;
    private AnimationInfo Data;
    public float TimeModifier;


    public AnimationData(AnimationClip data,string Aname)
    {
        Data = new AnimationInfo();
        TimeModifier = 1.0f;
        if (Application.isEditor)
        {
            LoadAnimationFromEditor(data);
            Data = new AnimationInfo(Aname);
        }
        else
        {
            Debug.LogError("Can't create data outside Editor");
        }
    }

    public string GetName()
    {
        return Data.AnimationName;
    }
    public void LoadFromFile(string path)
    {
        path = Path.Combine(path, (Data.AnimationName + ".dat"));
        if (!File.Exists(path))
        {
            Debug.LogErrorFormat("Could not find file : {0}", path);
            return;
        }

        FileStream fileStream = new FileStream(path, FileMode.Open);

        try
        {
            //Create the Binary formatter and load from the serialized file.
            BinaryFormatter formatter = new BinaryFormatter();
            Data = (AnimationInfo)formatter.Deserialize(fileStream);
        }
        catch (SerializationException e)
        {
            Debug.LogErrorFormat("Deserialize failed. Reason: {0}", e.Message);
        }
        finally
        {
            fileStream.Close();
        }

        //Copy data from List to curve data array (and initialize the array).
        _RelativePath = new string[Data.ListOfCurves.Count];
        _PropertyName = new string[Data.ListOfCurves.Count];
        _KeyframeValues = new Keyframe[Data.ListOfCurves.Count][];
        _Keyframedata = new Keyframe[Data.ListOfCurves.Count][];

        for (int i = 0; i < Data.ListOfCurves.Count; i++)
        {
            _RelativePath[i] = Data.ListOfCurves[i].relativePath;
            _PropertyName[i] = Data.ListOfCurves[i].propertyName;
            _KeyframeValues[i] = new Keyframe[Data.ListOfCurves[i].Key.Count];
            _Keyframedata[i] = new Keyframe[Data.ListOfCurves[i].Key.Count];
            for (int a = 0; a < Data.ListOfCurves[i].Key.Count; a++)
            {
                _KeyframeValues[i][a] = Data.ListOfCurves[i].Key[a].ToKeyframe();
                _Keyframedata[i][a] = Data.ListOfCurves[i].Key[a].ToKeyframe();
            }
        }
        InitializeFunctionArray();
    }
    public void SaveToFile(string path)
    {
        path = Path.Combine(path, (Data.AnimationName + ".dat"));
        Data.ListOfCurves.Clear();//Clean the existing data
        for (int i = 0; i < _RelativePath.Length; i++)
        {
            Data.ListOfCurves.Add(new CurveInfo(_RelativePath[i], _PropertyName[i], _Keyframedata[i]));
        }
        FileStream fileStream = new FileStream(path, FileMode.Create);

        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(fileStream, Data);
        }
        catch (SerializationException e)
        {
            Debug.LogErrorFormat("Serialization error with {0}", e.Message);
        }
        finally
        {
            fileStream.Close();
        }
    }

    public AnimationClip OverrideAnimation(GameObject target)
    {
        AnimationClip Animation_Copy;
        Animation_Copy = new AnimationClip();
        AnimatorOverrideController animOverride = new AnimatorOverrideController(target.GetComponent<Animator>().runtimeAnimatorController);
        for (int i = 0; i < Keyframedata.Length; i++)
        {
            AnimationCurve newCurve;
            if (TimeModifier != 1)
            {
                Keyframe[] temp = new Keyframe[Keyframedata[i].Length];
                Array.Copy(Keyframedata[i], temp, Keyframedata[i].Length);
                for (int a = 0; a < temp.Length; a++)
                {
                    temp[i].time *= TimeModifier;
                }
                newCurve = new AnimationCurve(temp);
            }
            else
            {
                newCurve = new AnimationCurve(Keyframedata[i]);
            }
            Animation_Copy.SetCurve(RelativePath[i], typeof(Transform), PropertyName[i], newCurve);
        }
        return Animation_Copy;
    }

    public void RescaleAnimationTime(float Modifier)
    {
        for(int a = 0; a < _Keyframedata.Length; a++)
        {
            for(int b = 0; b < _Keyframedata[a].Length; b++)
            {
                _Keyframedata[a][b].time = _KeyframeValues[a][b].time * Modifier;
            }
        }
    }

    public void RescaleAnimationValue(float Modifier)
    {
        for (int a = 0; a < _Keyframedata.Length; a++)
        {
            for (int b = 0; b < _Keyframedata[a].Length; b++)
            {
                _Keyframedata[a][b].value = _KeyframeValues[a][b].value * Modifier;
            }
        }
    }

    public void LoadAnimationFromEditor(AnimationClip InputAnimation)
    {
        if (!Application.isEditor)
        {
            Debug.LogError("You can't load animation from Editor outside Editor");
            return;
        }
        //Get the curve binding in the animation.
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(InputAnimation);
        //Get the binded curve in the curve binding.
        AnimationCurve[] curves = new AnimationCurve[curveBindings.Length];
        //Create and store the data needed to create a curve
        _Keyframedata = new Keyframe[curves.Length][];
        _KeyframeValues = new Keyframe[curves.Length][];
        _RelativePath = new string[curves.Length];
        _PropertyName = new string[curves.Length];
        for (int i = 0; i < curves.Length; i++)
        {
            //Store curve data
            _RelativePath[i] = curveBindings[i].path;
            _PropertyName[i] = curveBindings[i].propertyName;
            curves[i] = AnimationUtility.GetEditorCurve(InputAnimation, curveBindings[i]);

            //Get key frame in each curve
            Keyframe[] keyframes = curves[i].keys;
            //Create an array of keyframes to store the game keyframe data
            _Keyframedata[i] = new Keyframe[curves[i].keys.Length];
            _KeyframeValues[i] = new Keyframe[curves[i].keys.Length];
            for (int a = 0; a < keyframes.Length; a++)
            {
                //Copy the keyframe data to array for editing test
                _Keyframedata[i][a] = keyframes[a];
            }
            Array.Copy(_Keyframedata[i], _KeyframeValues[i], keyframes.Length);
        }
        InitializeFunctionArray();
    }

    private void InitializeFunctionArray()
    {
        //Will be called after loading the animation
        AdaptFunctions = new Func<Keyframe, Keyframe>[_Keyframedata.Length][];
        for(int i = 0; i < AdaptFunctions.Length; i++)
        {
            AdaptFunctions[i] = new Func<Keyframe, Keyframe>[_Keyframedata[i].Length];
            for(int a = 0;a< AdaptFunctions[i].Length; a++)
            {
                AdaptFunctions[i][a] = null;
            }
        }
    }

    public void UpdateAnimationAdaption()
    {
        for (int a = 0; a < AdaptFunctions.Length; a++)
        {
            for (int b = 0; b < AdaptFunctions[a].Length; b++)
            {
                if(AdaptFunctions[a][b] != null)
                {
                    Debug.Log("Key before:" + _Keyframedata[a][b].value);
                    _Keyframedata[a][b] = AdaptFunctions[a][b](KeyframeValues[a][b]);
                    Debug.Log("Key after:" + _Keyframedata[a][b].value);
                }
            }
        }
        DisplayAnimationCurveData();
    }

    public void SetAnimationAdaptionFunction(int a,int b, Func<Keyframe, Keyframe> func)
    {
        AdaptFunctions[a][b] = func;
    }
    void DisplayAnimationCurveData()
    {
        string outputtext = "";
        for (int i = 0; i < PropertyName.Length; i++)
        {
            //Output text
            outputtext += "Keyframe " + PropertyName[i] + " in curve " + i + " : ";
            string s = "";
            for (int a = 0; a < Keyframedata[i].Length; a++)
            {
                //Output the value of keyframe for testing
                s += Keyframedata[i][a].value + ", ";
            }
            outputtext += s + "\n";
        }
        Text output = GameObject.FindObjectOfType<Text>();
        output.text = outputtext;
    }
}