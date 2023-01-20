using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntervalManager : MonoBehaviour
{
    public int IntervalDuration = 20;
    public Slider IntervalTimeSlider;

    private GameObject ExperimentManagerObject;
    private ExperimentManager experimentManager;

    private float StartTime;

    // Start is called before the first frame update
    void Start()
    {
        ExperimentManagerObject = GameObject.Find("ExperimentManager");
        experimentManager = ExperimentManagerObject.GetComponent<ExperimentManager>();
        StartTime = Time.time;
        IntervalTimeSlider.value = 0.0f;
        experimentManager.StartInterval(IntervalDuration);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - StartTime < (float)IntervalDuration)
        {
            float t = (Time.time - StartTime) / (float)IntervalDuration;
            IntervalTimeSlider.value = t;
        }
        else
        {
            experimentManager.GoToNextTrial();
        }
    }
}
