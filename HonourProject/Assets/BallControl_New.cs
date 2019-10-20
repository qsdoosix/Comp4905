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

    Animator anim;
    AnimatorOverrideController animOverride;

    void Start()
    {
        //Create/get animation adapter
        Adapter = AnimationAdapter.Instance;
        //Add animation into animation adapter
        BallAnimationID = Adapter.AddData(InputAnimation, InputAnimation.name);
        Debug.Log("Ball:" + gameObject.name + ", AID:" + BallAnimationID);
        //Bind adaption function to the adapter
        Adapter.BindAdaptionFunction(BallAnimationID, 0, 0, BounceHeightAdaption);
        Adapter.BindAdaptionFunction(BallAnimationID, 0, 2, BounceHeightAdaption);
        Adapter.BindAdaptionFunction(BallAnimationID, 3, 0, RotationAdaption);
        Adapter.BindAdaptionFunction(BallAnimationID, 3, 1, RotationAdaption);
        HeightSlider.onValueChanged.AddListener(delegate { SliderHandle(); });
        anim = gameObject.GetComponent<Animator>();
        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        Adapter.SaveData();
    }

    void SliderHandle()
    {
        anim.runtimeAnimatorController = animOverride;
        animOverride["BounceAnimation"] = Adapter.UpdateAdaptionFunction(BallAnimationID,gameObject);
    }

    Keyframe BounceHeightAdaption(Keyframe key,GameObject obj)
    {
        Slider HeightSlider = obj.GetComponent<BallControl_New>().HeightSlider;
        key.value += HeightSlider.value;
        key.time *= 1 + (HeightSlider.value * 0.1f);
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