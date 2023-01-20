using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;

public class TrialManager : MonoBehaviour
{
    public GameObject StimulationText;

    private GameObject ExperimentManagerObject;
    private ExperimentManager experimentManager;
    private float timeToStimulate;
    private Text countText;
    private Coroutine _stimulateCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        ExperimentManagerObject = GameObject.Find("ExperimentManager");
        experimentManager = ExperimentManagerObject.GetComponent<ExperimentManager>();

        timeToStimulate = 3.0f;

        experimentManager.StartTrial();
        _stimulateCoroutine = StartCoroutine("CountToStimulate");
    }

    void Update()
    {
        timeToStimulate -= Time.deltaTime;

        if (timeToStimulate > 0 && timeToStimulate <= 3.0f)
        {
            countText = StimulationText.GetComponent<Text>();
            countText.text = String.Format("Stimulation starts in {0}\n{0}秒後に刺激を開始します", (int)(timeToStimulate) + 1);
        }
        else if (timeToStimulate > 3.0f)
        {
            countText = StimulationText.GetComponent<Text>();
            countText.text = "Failed to detect respiration 呼吸検出に失敗";
        }
        else if (experimentManager.IsStimulating)
        {
            countText = StimulationText.GetComponent<Text>();
            countText.text = "Stimulating\n刺激中";
        }
        else if (timeToStimulate > -0.50f && !experimentManager.IsStimulating)
        {
            countText = StimulationText.GetComponent<Text>();
            countText.text = "Stimulation starts in 0\n0秒後に刺激を開始します";
        }
        // 0.5秒経っても刺激が開始されない場合は呼吸検出に失敗しているのでやり直す
        else
        {
            StopCoroutine(_stimulateCoroutine);
            experimentManager.SetStimulationRetry();
            timeToStimulate = 3.5f;
            _stimulateCoroutine = StartCoroutine("CountToStimulate");
            countText = StimulationText.GetComponent<Text>();
            countText.text = "Failed to detect respiration 呼吸検出に失敗";
        }
    }

    public IEnumerator CountToStimulate()
    {
        yield return new WaitForSeconds(timeToStimulate - 0.50f);

        Debug.Log("Start Stimulate");
        experimentManager.SetStimulationRetryOff();
        experimentManager.StartCoroutine("Stimulate");
    }
}
