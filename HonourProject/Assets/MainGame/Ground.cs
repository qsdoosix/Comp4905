using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ground : MonoBehaviour
{
    public Slider SlopeSlider;
    Vector3 Zaxis = new Vector3(0, 0, 1);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.rotation= Quaternion.Euler(0, 0, SlopeSlider.value*30);
    }
}
