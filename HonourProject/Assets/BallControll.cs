using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class BallControll : MonoBehaviour
{
    public Text output;
    public Slider HeightSlider;
    public AnimationClip InputAnimation;
    AnimationClip Animation_Copy;
    //Data needed to create a curve and apply to animation clip
    string[] RelativePath;
    string[] propertyName;
    float[][] keyframeValues;
    Keyframe[][] keyframedata;
    List<CurveInfo> ListOfCurves;
    //Control for Animation override
    Animator anim;
    AnimatorOverrideController animOverride;

    
    [Serializable]
    public sealed class CurveInfo
    {
        public string relativePath;
        public string propertyName;
        public List<KeyFrameInfo> Key;

        public CurveInfo() { }

        public CurveInfo(string rp,string pn, Keyframe[] keys)
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

        public Keyframe toKeyframe()
        {
            Keyframe k = new Keyframe(Time, Value, InTangent, OutTangent, InWeight, OutWeight);
            k.weightedMode = WeightedMode;
            return k;
        }
    }

    string path;
    // Start is called before the first frame update
    void Start()
    {
        ListOfCurves = new List<CurveInfo>();
        anim = gameObject.GetComponent<Animator>();
        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;
        HeightSlider.onValueChanged.AddListener(delegate { SliderListener(); });
        path = Application.persistentDataPath;
        //Debug.Log(path);
        if (Application.isEditor)
        {
            if (true)
            {
                //LoadAnimationFromFile(path);
                LoadAnimationFromSerializedFile(path);
            }
            else
            {
                LoadAnimationDataFromEditor();
                //SaveAnimationToFile(path);
                SaveAnimationToSerializedFile(path);
            }
        }
    }

    void SliderListener()
    {
        keyframedata[0][0].value = keyframeValues[0][0] + HeightSlider.value;
        keyframedata[0][2].value = keyframeValues[0][2] + HeightSlider.value;
        CopyAnimationClip();
    }

    void CopyAnimationClip()
    {
        Animation_Copy = new AnimationClip();
        for (int i = 0; i < keyframedata.Length; i++)
        {
            AnimationCurve newCurve = new AnimationCurve(keyframedata[i]);
            Animation_Copy.SetCurve(RelativePath[i], typeof(Transform), propertyName[i], newCurve);
        }
        animOverride["BounceAnimation"] = Animation_Copy;
    }

    void LoadAnimationDataFromEditor()
    {
        //Get the curve binding in the animation.
        UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(InputAnimation);
        //Get the binded curve in the curve binding.
        AnimationCurve[] curves = new AnimationCurve[curveBindings.Length];
        //Create and store the data needed to create a curve
        keyframedata = new Keyframe[curves.Length][];
        keyframeValues = new float[curves.Length][];
        RelativePath = new string[curves.Length];
        propertyName = new string[curves.Length];
        for (int i = 0; i < curves.Length; i++)
        {
            //Store curve data
            RelativePath[i] = curveBindings[i].path;
            propertyName[i] = curveBindings[i].propertyName;
            curves[i] = UnityEditor.AnimationUtility.GetEditorCurve(InputAnimation, curveBindings[i]);

            //Get key frame in each curve
            Keyframe[] keyframes = curves[i].keys;
            //Create an array of keyframes to store the game keyframe data
            keyframedata[i] = new Keyframe[curves[i].keys.Length];
            keyframeValues[i] = new float[curves[i].keys.Length];
            for (int a = 0; a < keyframes.Length; a++)
            {
                //Copy the keyframe data to array for editing test
                keyframedata[i][a] = keyframes[a];
                keyframeValues[i][a] = keyframes[a].value;
            }
        }
    }

    void DisplayAnimationCurveData()
    {
        string outputtext = "";
        for (int i = 0; i < propertyName.Length; i++)
        {
            //Output text
            outputtext += "Keyframe " + propertyName[i] + " in curve " + i + " : ";
            string s = "";
            for (int a = 0; a < keyframeValues[i].Length; a++)
            {
                //Output the value of keyframe for testing
                s += keyframeValues[i][a] + ", ";
            }
            outputtext += s + "\n";
        }
        output.text = outputtext;
    }

    void LoadAnimationFromFile(string path)
    {
        int counter = 0;
        foreach (string filename in Directory.EnumerateFiles(path, "*.txt"))
        {
            counter++;//Count how many files needs to be read
        }
        Debug.Log(counter+" files will be loaded");
        //Create and store the data needed to create a curve
        keyframedata = new Keyframe[counter][];
        keyframeValues = new float[counter][];
        RelativePath = new string[counter];
        propertyName = new string[counter];

        counter = 0;

        foreach (string filename in Directory.EnumerateFiles(path, "*.txt"))
        {
            Debug.Log("Loading from : " + filename);
            string contents = File.ReadAllText(filename);
            string[] lines = contents.Split('\n');
            RelativePath[counter] = lines[0];
            propertyName[counter] = lines[1];
            keyframedata[counter] = new Keyframe[int.Parse(lines[2])];
            keyframeValues[counter] = new float[int.Parse(lines[2])];
            for (int i = 3; i < lines.Length; i++)
            {
                string[] filekeyframeData = lines[i].Split(',');
                Debug.Log("Line "+i+" : "+lines[i]);
                Debug.Log("Split length" + filekeyframeData.Length);
                if (filekeyframeData.Length != 7)
                {
                    continue;//Avoid empty line been read into the data and cause crash;
                }
                float time = float.Parse(filekeyframeData[0], System.Globalization.NumberStyles.Number,
                      System.Globalization.CultureInfo.InvariantCulture);
                float value = float.Parse(filekeyframeData[1], System.Globalization.NumberStyles.Number,
                      System.Globalization.CultureInfo.InvariantCulture);
                float inTangent = float.Parse(filekeyframeData[2], System.Globalization.NumberStyles.Number,
                      System.Globalization.CultureInfo.InvariantCulture);
                float outTangent = float.Parse(filekeyframeData[3], System.Globalization.NumberStyles.Number,
                      System.Globalization.CultureInfo.InvariantCulture);
                float inWeight = float.Parse(filekeyframeData[5], System.Globalization.NumberStyles.Number,
                      System.Globalization.CultureInfo.InvariantCulture);
                float outWeight = float.Parse(filekeyframeData[6], System.Globalization.NumberStyles.Number,
                      System.Globalization.CultureInfo.InvariantCulture);
                keyframedata[counter][i - 3] = new Keyframe(time,//time
                                                value,//value
                                                inTangent,//in tangent
                                                outTangent,//out tangent
                                                inWeight,//in weight
                                                outWeight);//out weight
                keyframedata[counter][i - 3].weightedMode = PraseWeightmode(filekeyframeData[4]);
                keyframeValues[counter][i - 3] = keyframedata[counter][i - 3].value;
            }
            counter++;
        }
    }

    WeightedMode PraseWeightmode(string input)
    {
        if (input.Equals("None", System.StringComparison.InvariantCultureIgnoreCase))
        {
            return WeightedMode.None;
        }
        else if (input.Equals("In", System.StringComparison.InvariantCultureIgnoreCase))
        {
            return WeightedMode.In;
        }
        else if (input.Equals("Out", System.StringComparison.InvariantCultureIgnoreCase))
        {
            return WeightedMode.Out;
        }
        else
        {
            return WeightedMode.Both;
        }
    }

    void SaveAnimationToFile(string path)
    {
        for (int i = 0; i < RelativePath.Length; i++)
        {
            //The animation data file is the file to store animation data like keyframes;
            string animationdatafilepath = path + "/" + RelativePath[i] + "/" + propertyName[i] + ".txt";
            if (File.Exists(animationdatafilepath))
            {
                //If the file already exists, delete and create new one.
                File.Delete(animationdatafilepath);
                Debug.Log("Already have a file with same name, overwrite");
            }
            using (StreamWriter sw = File.CreateText(animationdatafilepath))
            {
                sw.WriteLine(RelativePath[i]);//Write the relative path of the transform
                propertyName[i] = propertyName[i].Replace("m_", "");
                sw.WriteLine(propertyName[i]);//Write the property name of the transform
                sw.WriteLine(keyframedata[i].Length);//Write how many keyframes are there in the curve (so when loading the program knows how many to expect)
                for (int c = 0; c < keyframedata[i].Length; c++)
                {
                    sw.WriteLine(keyframedata[i][c].time.ToString("F7") + ","//Keep 7 decimals, that is almost the maximum accurate for float
                        + keyframedata[i][c].value.ToString("F7") + ","
                        + keyframedata[i][c].inTangent.ToString("F7") + ","
                        + keyframedata[i][c].outTangent.ToString("F7") + ","
                        + keyframedata[i][c].weightedMode + ","
                        + keyframedata[i][c].inWeight.ToString("F7") + ","
                        + keyframedata[i][c].outWeight.ToString("F7"));
                }
            }
            Debug.Log("File has been created at " + animationdatafilepath);
        }
    }

    void SaveAnimationToSerializedFile(string path)
    {        // create a new file e.g. AnimationCurves.dat in the StreamingAssets folder
        for(int i = 0; i < RelativePath.Length; i++)
        {
            ListOfCurves.Add(new CurveInfo(RelativePath[i], propertyName[i],keyframedata[i]));
        }
        FileStream fileStream = new FileStream(Path.Combine(path, "AnimationData.dat"), FileMode.Create);

        // Construct a BinaryFormatter and use it to serialize the data to the stream.
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(fileStream, ListOfCurves);
        }
        catch (SerializationException e)
        {
            Debug.LogErrorFormat(this, "Serialization error with {0}", e.Message);
        }
        finally
        {
            fileStream.Close();
        }
    }

    void LoadAnimationFromSerializedFile(string path)
    {
        string filePath = Path.Combine(path, "AnimationData.dat");

        if (!File.Exists(filePath))
        {
            Debug.LogErrorFormat(this, "Could not find file : {0}", filePath);
            return;
        }

        FileStream fileStream = new FileStream(filePath, FileMode.Open);

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the hashtable from the file and 
            // assign the reference to the local variable.
            ListOfCurves = (List<CurveInfo>)formatter.Deserialize(fileStream);
        }
        catch (SerializationException e)
        {
            Debug.LogErrorFormat(this, "Failed to deserialize. Reason: {0}", e.Message);
        }
        finally
        {
            fileStream.Close();
        }

        //Copy data from List to curve data array (and initialize the array).
        RelativePath = new string[ListOfCurves.Count];
        propertyName = new string[ListOfCurves.Count];
        keyframeValues = new float[ListOfCurves.Count][];
        keyframedata = new Keyframe[ListOfCurves.Count][];

        for(int i = 0;i< ListOfCurves.Count; i++)
        {
            RelativePath[i] = ListOfCurves[i].relativePath;
            propertyName[i] = ListOfCurves[i].propertyName;
            keyframeValues[i] = new float[ListOfCurves[i].Key.Count];
            keyframedata[i] = new Keyframe[ListOfCurves[i].Key.Count];
            for(int a = 0;a< ListOfCurves[i].Key.Count; a++)
            {
                keyframeValues[i][a] = ListOfCurves[i].Key[a].Value;
                keyframedata[i][a] = ListOfCurves[i].Key[a].toKeyframe();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            DisplayAnimationCurveData();
        }
    }
}