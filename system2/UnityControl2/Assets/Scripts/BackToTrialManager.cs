using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToTrialManager : MonoBehaviour
{
    private GameObject ExperimentManagerObject;
    private ExperimentManager experimentManager;

    void Start()
    {
        ExperimentManagerObject = GameObject.Find("ExperimentManager");
        experimentManager = ExperimentManagerObject.GetComponent<ExperimentManager>();
    }

    public void ContinueButtonClicked()
    {
        experimentManager.BackToTrial();
    }
}
