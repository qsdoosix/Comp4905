using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public Slider SlopeSlider;
    public Slider WeightSlider;
    public Slider HeightSlider;
    public AnimationClip WalkingAnim,JumpingAnim;
    public GameObject BoxSprite;
    public KeyCode jump,move;
    float groundslope;
    float carryweight;
    float jumpheight;
    float rotationAngle;
    bool refresh;

    //Constants for animation adaption
    //Use a threshold so the animation won't be updated until the difference is too high to save some computation
    //Not necessary for this simple scene but could be helpful on larger games.
    const float adaptionThreshold = 0.1f;

    AnimationAdapter Adapter;
    int WalkingAnimID, JumpingAnimID;
    Animation animator;
    AnimatorOverrideController animOverride;
    float timescale;
    // Start is called before the first frame update
    void Start()
    {
        rotationAngle = 0.0f;
        timescale = 1.0f;
        //Create/get animation adapter
        Adapter = AnimationAdapter.Instance;
        if (!Application.isEditor)
        {
            Adapter.filepath = Application.persistentDataPath;
            Adapter.ReadFileList();
        }
        //Add animation into animation adapter
        WalkingAnimID = Adapter.AddData(WalkingAnim, WalkingAnim.name);
        JumpingAnimID = Adapter.AddData(JumpingAnim, JumpingAnim.name);
        animator = gameObject.GetComponent<Animation>();
        animator.AddClip(WalkingAnim, "Walking");
        animator.AddClip(JumpingAnim, "Jumping");
        BindWalkingAnimAdapter();
        BindJumpAnimAdapter();
        groundslope = GetSlope();
        carryweight = GetWeight();
        jumpheight = GetHeight();
        Adapter.SaveData();
        RefreshAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateValue();
        ScaleBackpack();
        if (refresh)
        {
            groundslope = GetSlope();
            carryweight = GetWeight();
            jumpheight = GetHeight();
            RefreshAnimation();
            refresh = false;
        }
        if (Input.GetKeyDown(jump))
        {
            animator.RemoveClip("Jumping");
            rotationAngle = BodyAdaption();
            animator.AddClip(Adapter.UpdateAdaptionFunction(JumpingAnimID, gameObject, timescale), "Jumping");
            animator.Play("Jumping");
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            animator.wrapMode = WrapMode.Loop;
        }
        if (Input.GetKeyDown(move))
        {
            //Play walking before calculating the angle, because the angle is fixed to 0 when jumping
            animator.Play("Walking");
            rotationAngle = BodyAdaption();
            animator.RemoveClip("Walking");
            animator.AddClip(Adapter.UpdateAdaptionFunction(WalkingAnimID, gameObject, timescale), "Walking");
            animator.Play("Walking");
            animator.wrapMode = WrapMode.Loop;
        }
    }

    void BindWalkingAnimAdapter()
    {
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
    }

    void BindJumpAnimAdapter()
    {
        Adapter.BindAdaptionFunction(JumpingAnimID, 0, 2, JumpingHeightAdapation);
        Adapter.BindAdaptionFunction(JumpingAnimID, 0, 4, JumpingHeightAdapation);
        Adapter.BindAdaptionFunction(JumpingAnimID, 5, 3, JumpingArmAdapation);
        Adapter.BindAdaptionFunction(JumpingAnimID, 5, 4, JumpingArmAdapation);
        Adapter.BindAdaptionFunction(JumpingAnimID, 6, 3, JumpingArmAdapation);
        Adapter.BindAdaptionFunction(JumpingAnimID, 6, 4, JumpingArmAdapation);
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
    float GetHeight()
    {
        return HeightSlider.value;
    }

    void UpdateValue()
    {
        //Mark for refresh if the difference between the previous value and new value is too large
        if (Mathf.Abs(groundslope - GetSlope()) > adaptionThreshold || 
            Mathf.Abs(jumpheight - GetHeight()) > adaptionThreshold ||
            Mathf.Abs(carryweight - GetWeight()) > adaptionThreshold)
        {
            refresh = true;
        }
    }

    public void RefreshAnimation()
    {
        if (animator.IsPlaying("Walking"))
        {
            UpdateSpeed();
            //Remove old clip
            animator.RemoveClip("Walking");
            rotationAngle = BodyAdaption();
            //Compute and add new clip
            animator.AddClip(Adapter.UpdateAdaptionFunction(WalkingAnimID, gameObject, timescale), "Walking");
            //Play the new clip
            animator.Play("Walking");
            animator.wrapMode = WrapMode.Loop;
        }else if (animator.IsPlaying("Jumping"))
        {
            animator.RemoveClip("Jumping");
            rotationAngle = BodyAdaption();
            animator.AddClip(Adapter.UpdateAdaptionFunction(JumpingAnimID, gameObject), "Jumping");
            animator.Play("Jumping");
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            animator.wrapMode = WrapMode.Loop;
        }
    }

    public float BodyAdaption()
    {
        //slight move the game object
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, groundslope * groundslope / 15 - 0.5f, gameObject.transform.position.z);
        //This is rotating the gameobject, not using animation adaption.
        float rotateangle = 0.0f;
        if (animator.IsPlaying("Walking"))
        {
            rotateangle = (groundslope + 1) / 2;
            rotateangle += carryweight * 1.2f;
        }
        gameObject.transform.rotation = Quaternion.Euler(0, 0, rotateangle * -13);
        return rotateangle;
    }

    public void UpdateSpeed()
    {
        if (groundslope < -0.5)
        {
            timescale = (2 + groundslope - carryweight / 10) / 2;
        }
        else if (groundslope > 0.5)
        {
            timescale = (4 + carryweight) / 4;
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

    Keyframe JumpingArmAdapation(Keyframe f, GameObject obj)
    {
        if (f.value > 0)
        {
            f.value += GetHeight() * 20;
        }
        else if (f.value < 0)
        {
            f.value -= GetHeight() * 20;
        }
        return f;
    }

    Keyframe JumpingHeightAdapation(Keyframe f, GameObject obj)
    {
        if (f.value > 0.5)
        {
            f.value += GetHeight() * 0.15f;
        }
        else if (f.value < 0.5)
        {
            f.value -= GetHeight() * 0.05f;
        }
        return f;
    }
    Keyframe JumpingULegAdaption(Keyframe f, GameObject obj)
    {
        if (f.value > 0)
        {
            f.value += GetHeight() * 10;
        }
        else if (f.value < 0)
        {
            f.value -= GetHeight() * 13;
        }
        return f;
    }
    Keyframe JumpingLLegAdaption(Keyframe f, GameObject obj)
    {
        if (f.value > 0)
        {
            f.value += GetHeight() * 12;
        }
        else if (f.value < 0)
        {
            f.value -= GetHeight() * 15;
        }
        return f;
    }
}
