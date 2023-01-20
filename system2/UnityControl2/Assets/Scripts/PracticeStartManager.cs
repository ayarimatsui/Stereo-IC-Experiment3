using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeStartManager : MonoBehaviour
{
    private GameObject ExperimentManagerObject;
    private ExperimentManager experimentManager;
    private float StartTime;

    void Start()
    {
        ExperimentManagerObject = GameObject.Find("ExperimentManager");
        experimentManager = ExperimentManagerObject.GetComponent<ExperimentManager>();
        StartTime = Time.time;
    }

    void Update()
    {
        if (Time.time - StartTime > 3.0f)
        {
            experimentManager.GoToNextTrial();
        }
    }
}
