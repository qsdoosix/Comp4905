using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class BallControll : MonoBehaviour
{
    public AnimationClip BallAnimation;
    public Text output;
    string path;
    // Start is called before the first frame update
    void Start()
    {
        path = Application.persistentDataPath;
        //Debug.Log(path);
        if (Application.isEditor)
        {
            //string transformPath=UnityEditor.AnimationUtility.CalculateTransformPath(,gameObject.transform);
            //If the code is running in Unity Editor, read the given animation and output it into files (Each file represent one transform curve)
            string outputtext = "";
            //Get the curve binding in the animation.
            UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(BallAnimation);
            //Get the binded curve in the curve binding.
            AnimationCurve[] curves = new AnimationCurve[curveBindings.Length];
            for (int i = 0; i < curves.Length; i++)
            {
                //Get curves for each curve binding
                curves[i] = UnityEditor.AnimationUtility.GetEditorCurve(BallAnimation, curveBindings[i]);
                //Get key frame in each curve
                Keyframe[] keyframes = curves[i].keys;
                /*
                Debug.Log("CurrentCurveBindings path"+i+" : "+curveBindings[i].path);
                Debug.Log("CurrentCurveBindings type" + i + " : " + curveBindings[i].type);
                Debug.Log("CurrentCurveBindings propertyName" + i + " : " + curveBindings[i].propertyName);
                */
                //The animation data file is the file to store animation data like keyframes and some others.
                string animationdatafilepath = path + "/" + curveBindings[i].path + "/" + curveBindings[i].propertyName+".txt";
                if (!File.Exists(animationdatafilepath))
                {
                    using (StreamWriter sw = File.CreateText(animationdatafilepath))
                    {
                        sw.WriteLine(curveBindings[i].path);//Write the relative path to the transform
                        sw.WriteLine(curveBindings[i].propertyName);//Write the property name of the transform
                        sw.WriteLine(keyframes.Length);//Write how many keyframes are there in the curve (so when loading the program knows how many to expect)
                        for(int c = 0; c < keyframes.Length; c++)
                        {
                            sw.WriteLine(keyframes[c].time + "," + keyframes[c].value);
                        }
                    }
                    
                    Debug.Log("File has been created at " + animationdatafilepath);
                }
                else
                {
                    Debug.Log("File at " + animationdatafilepath+" already exists");
                }
                
                string s = "";
                if (keyframes.Length > 0)
                {
                    outputtext += "Keyframes in curve " + i + " : ";
                    for (int a = 0; a < keyframes.Length; a++)
                    {
                        s += keyframes[a].value + ", ";
                    }
                    outputtext += s + "\n";
                }
                else
                {
                }
            }
            output.text = outputtext;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*Alternative solution to create curve at runtime
        AnimationCurve curve = new AnimationCurve();
        BallAnimation.SetCurve();
        */

        /*
         * A might be useful code from Unity forum
             System.IO.FileStream oFileStream = null;
             oFileStream = new System.IO.FileStream("D:\\chats.txt", System.IO.FileMode.Create);
             oFileStream.Write(byteData, 0, byteData.Length);
             oFileStream.Close ();

         */
    }
}
