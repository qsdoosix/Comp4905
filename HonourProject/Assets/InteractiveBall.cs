using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveBall : MonoBehaviour
{
    public Text textfield;
    public Slider TimeSlider,HardnessSlider;
    public AnimationClip InputAnimation;
    AnimationAdapter Adapter;
    int AnimationID;
    Animator anim;
    AnimatorOverrideController animOverride;
    float MousePoint=0.0f;
    // Start is called before the first frame update
    void Start()
    {
        //Create/get animation adapter
        Adapter = AnimationAdapter.Instance;
        //Add animation into animation adapter
        AnimationID = Adapter.AddData(InputAnimation, InputAnimation.name);
        anim = gameObject.GetComponent<Animator>();
        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        Adapter.BindAdaptionFunction(AnimationID, 0, 0, SetJumpHeight);
        Adapter.BindAdaptionFunction(AnimationID, 0, 1, SetLowPoint);
        Adapter.BindAdaptionFunction(AnimationID, 0, 2, SetJumpHeight);
        Adapter.BindAdaptionFunction(AnimationID, 1, 2, SetCrashWidth);
        Adapter.BindAdaptionFunction(AnimationID, 2, 2, SetCrashHeight);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("mouseClicked");
            MousePoint= Camera.main.ScreenToWorldPoint(Input.mousePosition).y + 4;
            anim.runtimeAnimatorController = animOverride;
            animOverride["interactiveBouncing"] = Adapter.UpdateAdaptionFunction(AnimationID, ScaleTimeOnHeight(), gameObject);
            textfield.text = Adapter.GetAnimationData(AnimationID);
        }
    }

    float ScaleTimeOnHeight()
    {
        float timescale = 1.0f;
        timescale *= (MousePoint / 5);//The height affect the time;
        timescale *= (TimeSlider.value + 1);//The time slider modifies the time scale
        return timescale;
    }

    Keyframe SetJumpHeight(Keyframe k, GameObject obj)
    {
        float highpoint = obj.GetComponent<InteractiveBall>().MousePoint;
        k.value = highpoint;
        if (k.value < 0)
        {
            k.value = 0;
        }
        return k;
    }

    Keyframe SetLowPoint(Keyframe k, GameObject obj)
    {
        InteractiveBall ball = obj.GetComponent<InteractiveBall>();
        float highpoint = ball.MousePoint;
        k.value = highpoint / -70*((4+HardnessSlider.value)/5);//The 
        k.inTangent = -highpoint;
        k.inWeight = highpoint / (22 * ball.ScaleTimeOnHeight());
        k.outTangent = highpoint;
        k.outWeight = highpoint / (22 * ball.ScaleTimeOnHeight());
        return k;
    }

    Keyframe SetCrashWidth(Keyframe k, GameObject obj)
    {
        //The expanding on the x axis for ball when it hits the ground
        //Determined with the height(kinetic energy) and softness slider
        float highpoint = obj.GetComponent<InteractiveBall>().MousePoint;
        k.value = k.value * (1 + highpoint / 25 * (1 - HardnessSlider.value));
        return k;
    }
    Keyframe SetCrashHeight(Keyframe k, GameObject obj)
    {
        //The contraction on the Y axis for ball when it hits the ground
        //Determined with the height(kinetic energy) and softness slider
        float highpoint = obj.GetComponent<InteractiveBall>().MousePoint;
        k.value = k.value * (1 - highpoint / 40 * (1 - HardnessSlider.value));
        return k;
    }
}
