using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallControl_New : MonoBehaviour
{
    public Text Output;
    public Slider HeightSlider;
    public AnimationClip InputAnimation;
    AnimationAdapter Adapter;
    int BallAnimationID;

    Animation anim;

    void Start()
    {
        //Create/get animation adapter
        Adapter = AnimationAdapter.Instance;
        //Add animation into animation adapter
        BallAnimationID = Adapter.AddData(InputAnimation, InputAnimation.name);
        //Bind adaption function to the adapter
        Debug.Log("Ball: " + gameObject.name + " has Animation ID " + BallAnimationID);
        Adapter.BindAdaptionFunction(BallAnimationID, 0, 0, BounceHeightAdaption);
        Adapter.BindAdaptionFunction(BallAnimationID, 0, 2, BounceHeightAdaption);
        Adapter.BindAdaptionFunction(BallAnimationID, 3, 0, RotationAdaption);
        Adapter.BindAdaptionFunction(BallAnimationID, 3, 1, RotationAdaption);
        HeightSlider.onValueChanged.AddListener(delegate { SliderHandle(); });
        anim = gameObject.GetComponent<Animation>();
        anim.AddClip(InputAnimation, "BounceAnimation");
        anim.Play("BounceAnimation");
        Adapter.SaveData();
    }

    void SliderHandle()
    {
        anim.RemoveClip("BounceAnimation");
        anim.AddClip(Adapter.UpdateAdaptionFunction(BallAnimationID, gameObject, 1 + (HeightSlider.value * 0.1f)), "BounceAnimation");
        anim.Play("BounceAnimation");
        anim.wrapMode = WrapMode.Loop;
    }

    Keyframe BounceHeightAdaption(Keyframe key,GameObject obj)
    {
        Slider HeightSlider = obj.GetComponent<BallControl_New>().HeightSlider;
        key.value += HeightSlider.value;
        return key;
    }

    Keyframe RotationAdaption(Keyframe key, GameObject obj)
    {
        Slider HeightSlider = obj.GetComponent<BallControl_New>().HeightSlider;
        int modifier = Mathf.RoundToInt(HeightSlider.value * 2)-1;
        key.value = key.value * modifier;
        key.inTangent = key.inTangent * modifier;
        key.outTangent = key.outTangent * modifier;
        return key;
    }
}