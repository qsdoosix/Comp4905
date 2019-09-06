using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class BallControll : MonoBehaviour
{
    public AnimationClip BallAnimation;
    public Text output;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        EditorCurveBinding[] curveBindings;
        AnimationCurve[] curves;
        curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(BallAnimation);
        curves = new AnimationCurve[curveBindings.Length];
        string outputtext = "";
        for (int i = 0; i < curves.Length; i++)
        {
            curves[i] = AnimationUtility.GetEditorCurve(BallAnimation, curveBindings[i]);
            Keyframe[] keyframes = curves[i].keys;
            string s = "";
            if (keyframes.Length > 0)
            {
                outputtext += "Keyframes in curve " + i + " : ";
                for (int a = 0; a < keyframes.Length; a++)
                {
                    s += keyframes[a].value + ", ";
                }
                outputtext+=s+"\n";
            }
            else
            {
            }
        }
        output.text = outputtext;
    }
}
