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
    string[] CurveType;
    string[] propertyName;
    float[][] keyframeValues;
    Keyframe[][] keyframedata;

    string path;
    // Start is called before the first frame update
    void Start()
    {
        Animation_Copy = new AnimationClip();
        HeightSlider.onValueChanged.AddListener(delegate { SliderListener(); });
        path = Application.persistentDataPath;
        //Debug.Log(path);
        if (Application.isEditor)
        {
            LoadAnimationDataFromEditor();
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
        Debug.Log("Input "+InputAnimation.isLooping + ", " + InputAnimation.wrapMode);
        Animation_Copy.wrapMode = WrapMode.Loop;
        Debug.Log("Copy " + Animation_Copy.isLooping+", "+ Animation_Copy.wrapMode);
        for (int i = 0; i < keyframedata.Length; i++)
        {
            AnimationCurve newCurve = new AnimationCurve(keyframedata[i]);
            Animation_Copy.SetCurve(RelativePath[i], typeof(Transform), propertyName[i], newCurve);
        }
        Animator anim = gameObject.GetComponent<Animator>();
        AnimatorOverrideController animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;
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
        RelativePath = new string[curveBindings.Length];
        CurveType = new string[curveBindings.Length];
        propertyName = new string[curveBindings.Length];
        for (int i = 0; i < curves.Length; i++)
        {
            //Store curve data
            RelativePath[i] = curveBindings[i].path;
            CurveType[i] = curveBindings[i].type.GetType().AssemblyQualifiedName;
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

    void SaveAnimationToFile(string path)
    {
        for (int i = 0; i < RelativePath.Length; i++)
        {
            //The animation data file is the file to store animation data like keyframes;
            string animationdatafilepath = path + "/" + RelativePath[i] + "/" + propertyName[i] + ".txt";
            if (File.Exists(animationdatafilepath))
            {
                File.Delete(animationdatafilepath);
                Debug.Log("Already have a file with same name, overwrite");
            }
            using (StreamWriter sw = File.CreateText(animationdatafilepath))
            {
                sw.WriteLine(RelativePath[i]);//Write the relative path to the transform
                sw.WriteLine(propertyName[i]);//Write the property name of the transform
                sw.WriteLine(keyframedata.Length);//Write how many keyframes are there in the curve (so when loading the program knows how many to expect)
                for (int c = 0; c < keyframedata[i].Length; c++)
                {
                    sw.WriteLine(keyframedata[i][c].time + ","
                        + keyframedata[i][c].value + ","
                        + keyframedata[i][c].inTangent + ","
                        + keyframedata[i][c].outTangent + ","
                        + keyframedata[i][c].weightedMode + ","
                        + keyframedata[i][c].inWeight + ","
                        + keyframedata[i][c].outWeight);
                }
            }
            Debug.Log("File has been created at " + animationdatafilepath);
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
