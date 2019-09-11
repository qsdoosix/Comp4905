using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class BallControll : MonoBehaviour
{
    public AnimationClip InputAnimation;
    public Text output;
    public Slider HeightSlider;
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
        HeightSlider.onValueChanged.AddListener(delegate { SliderListener(); });
        path = Application.persistentDataPath;
        //Debug.Log(path);
        if (Application.isEditor)
        {
            string outputtext = "";
            //If the code is running in Unity Editor, read the given animation and output it into files (Each file represent one transform curve)
            SaveAnimationToFile(path);
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

                RelativePath[i]= curveBindings[i].path;
                CurveType[i] = curveBindings[i].type.GetType().AssemblyQualifiedName;
                propertyName[i] = curveBindings[i].propertyName;
                //Get curves for each curve binding
                curves[i] = UnityEditor.AnimationUtility.GetEditorCurve(InputAnimation, curveBindings[i]);
                //Get key frame in each curve
                Keyframe[] keyframes = curves[i].keys;
                //Create an array of keyframes to store the game keyframe data
                keyframedata[i] = new Keyframe[curves[i].keys.Length];
                keyframeValues[i] = new float[curves[i].keys.Length];

                //Output text
                outputtext += "Keyframe " + curveBindings[i].propertyName + " in curve " + i + " : ";
                string s = "";
                for (int a = 0; a < keyframes.Length; a++)
                {
                    //Output the value of keyframe for testing
                    s += keyframes[a].value + ", ";

                    //Copy the keyframe data to array for editing test
                    keyframedata[i][a] = keyframes[a];
                    keyframeValues[i][a] = keyframes[a].value;
                }
                outputtext += s + "\n";
            }
            output.text = outputtext;
        }
    }

    void SliderListener()
    {
        keyframedata[1][0].value = keyframeValues[1][0] + HeightSlider.value;
        keyframedata[1][2].value = keyframeValues[1][2] + HeightSlider.value;
        AnimationCurve newCurve = new AnimationCurve(keyframedata[1]);
        InputAnimation.SetCurve(RelativePath[1], typeof(Transform), propertyName[1], newCurve);
    }

    void SaveAnimationToFile(string path)
    {
        //Get the curve binding in the animation.
        UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(InputAnimation);
        //Get the binded curve in the curve binding.
        AnimationCurve[] curves = new AnimationCurve[curveBindings.Length];
        for (int i = 0; i < curves.Length; i++)
        {
            //Get curves from each curve binding
            curves[i] = UnityEditor.AnimationUtility.GetEditorCurve(InputAnimation, curveBindings[i]);
            //Get key frame in each curve
            Keyframe[] keyframes = curves[i].keys;
            //The animation data file is the file to store animation data like keyframes;
            string animationdatafilepath = path + "/" + curveBindings[i].path + "/" + curveBindings[i].propertyName + ".txt";
            if (File.Exists(animationdatafilepath))
            {
                File.Delete(animationdatafilepath);
                Debug.Log("Already have a file with same name, deleting");
            }
            using (StreamWriter sw = File.CreateText(animationdatafilepath))
            {
                sw.WriteLine(curveBindings[i].path);//Write the relative path to the transform
                sw.WriteLine(curveBindings[i].type.GetType().AssemblyQualifiedName);//Write the AssemblyQualifiedName to the data
                sw.WriteLine(curveBindings[i].propertyName);//Write the property name of the transform
                sw.WriteLine(keyframes.Length);//Write how many keyframes are there in the curve (so when loading the program knows how many to expect)
                for (int c = 0; c < keyframes.Length; c++)
                {
                    sw.WriteLine(keyframes[c].time + ","
                        + keyframes[c].value + ","
                        + keyframes[c].inTangent + ","
                        + keyframes[c].outTangent + ","
                        + keyframes[c].weightedMode + ","
                        + keyframes[c].inWeight + ","
                        + keyframes[c].outWeight);
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
            string outputtext = "";
            //Get the curve binding in the animation.
            UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(InputAnimation);
            //Get the binded curve in the curve binding.
            AnimationCurve[] curves = new AnimationCurve[curveBindings.Length];
            for (int i = 0; i < curves.Length; i++)
            {
                //Get curves for each curve binding
                curves[i] = UnityEditor.AnimationUtility.GetEditorCurve(InputAnimation, curveBindings[i]);
                //Get key frame in each curve
                Keyframe[] keyframes = curves[i].keys;
                //Output text
                outputtext += "Keyframe " + curveBindings[i].propertyName + " in curve " + i + " : ";
                string s = "";
                for (int a = 0; a < keyframes.Length; a++)
                {
                    //Output the value of keyframe for testing
                    s += keyframes[a].value + ", ";
                }
                outputtext += s + "\n";
            }
            output.text = outputtext;
        }
    }
}
