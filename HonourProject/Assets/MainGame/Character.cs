using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public Slider SlopeSlider;
    public Slider WeightSlider;
    public AnimationClip WalkingAnim;
    public Text Textfield;
    public GameObject BoxSprite;
    float groundslope;
    float carryweight;
    float rotationAngle;
    bool refresh;

    //Constants for animation adaption
    //Use a threshold so the animation won't be updated until the difference is too high to save some computation
    const float adaptionThreshold = 0.1f;

    AnimationAdapter Adapter;
    int WalkingAnimID;
    Animator animator;
    AnimatorOverrideController animOverride;
    float timescale;
    // Start is called before the first frame update
    void Start()
    {
        groundslope = 0.0f;
        carryweight = 0.0f;
        rotationAngle = 0.0f;
        timescale = 1.0f;
        //Create/get animation adapter
        Adapter = AnimationAdapter.Instance;
        //Add animation into animation adapter
        WalkingAnimID = Adapter.AddData(WalkingAnim, WalkingAnim.name);
        animator = gameObject.GetComponent<Animator>();
        animOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        Adapter.BindAdaptionFunction(WalkingAnimID, 3, 0, UpperLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 3, 1, UpperLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 3, 2, UpperLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 4, 0, UpperLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 4, 1, UpperLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 4, 2, UpperLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 5, 0, LowerLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 5, 1, LowerLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 5, 2, LowerLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 5, 3, LowerLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 6, 0, LowerLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 6, 1, LowerLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 6, 2, LowerLegAdaption);
        Adapter.BindAdaptionFunction(WalkingAnimID, 6, 3, LowerLegAdaption);
        groundslope = GetSlope();
        carryweight = GetWeight();
        Adapter.SaveData();
        RefreshAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        Updateslope();
        Updateweight();
        ScaleBackpack();
        if (refresh)
        {
            groundslope = GetSlope();
            carryweight = GetWeight();
            RefreshAnimation();
            refresh = false;
            Textfield.text = Adapter.GetAnimationData(WalkingAnimID);
        }
    }
    //Methods to simulate the process to get infomation from game engine
    float GetSlope()
    {
        return SlopeSlider.value;
    }
    float GetWeight()
    {
        return WeightSlider.value;
    }

    void Updateslope()
    {
        if(Mathf.Abs(groundslope - GetSlope()) >adaptionThreshold)
        {
            groundslope = GetSlope();
            refresh = true;
        }
    }
    void Updateweight()
    {
        if (Mathf.Abs(carryweight - GetWeight()) > adaptionThreshold)
        {
            carryweight = GetWeight();
            RefreshAnimation();
            refresh = true;
        }
    }
    public void RefreshAnimation()
    {
        UpdateSpeed();
        rotationAngle = BodyAdaption();
        animator.runtimeAnimatorController = animOverride;
        animOverride["Walking"] = Adapter.UpdateAdaptionFunction(WalkingAnimID, timescale, gameObject);
    }

    public float BodyAdaption()
    {
        //slight move the game object
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, groundslope * groundslope / 15 - 0.5f, gameObject.transform.position.z);
        //This is rotating the gameobject, not using animation adaption.
        float rotateangle = 0.0f;
        rotateangle = (groundslope + 1)/2;
        rotateangle += carryweight * 1.2f;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, rotateangle * -13);
        return rotateangle;
    }

    public void UpdateSpeed()
    {
        if (groundslope < -0.5)
        {
            timescale = (2 + groundslope - carryweight/10) / 2;
        }
        else if(groundslope>0.5)
        {
            timescale = (4+carryweight)/4;
        }
        else
        {
            timescale = 1;
        }
    }

    public void ScaleBackpack()
    {
        //The method to scale the backpack according to the carryweight slider. Not part of animation
        BoxSprite.transform.localScale = new Vector3(BoxSprite.transform.localScale.x, (0.25f + carryweight) / 2, BoxSprite.transform.localScale.z);
    }

    Keyframe UpperLegAdaption(Keyframe f, GameObject obj)
    {
        //If the value is postive (the forward leg)
        if (f.value > 0)
        {
            if (GetSlope() > 0)
            {
                f.value += groundslope * 48;
            }
            else
            {
                f.value += groundslope * 15;
            }
        }
        else if (f.value < 0) // the leg behind
        {
            if (GetSlope() > 0)
            {
                f.value += groundslope * 10;
            }
            else
            {
                f.value -= groundslope * 5;
            }
        }
        //Shift the rotation to cancel the rotation of the body
        f.value += obj.GetComponent<Character>().rotationAngle * 13;
        return f;
    }

    Keyframe LowerLegAdaption(Keyframe f, GameObject obj)
    {
        if (groundslope > 0)
        {
            if (f.value < -1)
            {
                f.value -= groundslope * 65;
            }
        }
        else
        {
            if (f.value > -1)
            {
                f.value += groundslope * 65;
            }
        }
        return f;
    }
}
