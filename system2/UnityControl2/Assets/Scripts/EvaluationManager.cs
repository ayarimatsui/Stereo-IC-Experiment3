using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvaluationManager : MonoBehaviour
{
    public Text InstructionText;

    private GameObject ExperimentManagerObject;
    private ExperimentManager experimentManager;
    
    // Start is called before the first frame update
    void Start()
    {
        ExperimentManagerObject = GameObject.Find("ExperimentManager");
        experimentManager = ExperimentManagerObject.GetComponent<ExperimentManager>();
    }

    public void LeftButtonClicked()
    {
        experimentManager.SaveResult(ExperimentManager.Answers.Left);
        experimentManager.CompleteTrial();
    }

    public void RightButtonClicked()
    {
        experimentManager.SaveResult(ExperimentManager.Answers.Right);
        experimentManager.CompleteTrial();
    }

    public void NoScecntButtonClicked()
    {
        experimentManager.SaveResult(ExperimentManager.Answers.NoScent);
        experimentManager.CompleteTrial();
    }
}
