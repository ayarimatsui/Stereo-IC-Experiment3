using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class TestManager : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GameObject LeftAdjustSliderObj;
    [SerializeField] private GameObject RightAdjustSliderObj;

    [Header("Electrical Stimulation")]
    [Space(20)]
    public int Frequency;
    public WaveForms WaveForm;
    public int Duty;
    public int TransitionDuration;
    public TransitionForms TransitionForm;
    public int Duration = 2;


    //public enum StimulationCurrents
    //{
    //    LeftCurrent = 1980,
    //    RightCurrent = 2460
    //}

    public enum StimulationCurrents
    {
        LeftCurrent = 1800,
        RightCurrent = 2172
    }

    public enum WaveForms
    {
        square_bipole,
        square_positive,
        square_negative,
        direct_positive,
        direct_negative,
        sin,
        trapezoidal_positive,
        trapezoidal_negative
    }

    public enum TransitionForms
    {
        constant,
        linear,
        smooth,
        lineardecay,
        smoothdecay
    }

    private GameObject LeftSeeduinoSerialHandlerObject;
    private SeeduinoSerialHandler leftSeeduinoSerialHandler;
    private GameObject RightSeeduinoSerialHandlerObject;
    private SeeduinoSerialHandler rightSeeduinoSerialHandler;
    private GameObject ElectricalSwitchingSerialHandlerObject;
    private ElectricalSwitchingSerialHandler electricalSwitchingSerialHandler;

    // canvas用
    private Text ExplanationText;
    private GameObject CheckIntranasalChemosensation;
    private GameObject AdjustTactileSensation;
    private Slider rightAdjustSlider;
    private Slider leftAdjustSlider;

    // 電流値
    private int LeftCurrent;
    private int RightCurrent;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        LeftSeeduinoSerialHandlerObject = GameObject.Find("LeftSeeduinoSerialHandler");
        leftSeeduinoSerialHandler = LeftSeeduinoSerialHandlerObject.GetComponent<SeeduinoSerialHandler>();
        RightSeeduinoSerialHandlerObject = GameObject.Find("RightSeeduinoSerialHandler");
        rightSeeduinoSerialHandler = RightSeeduinoSerialHandlerObject.GetComponent<SeeduinoSerialHandler>();
        ElectricalSwitchingSerialHandlerObject = GameObject.Find("ElectricalSwitchingSerialHandler");
        electricalSwitchingSerialHandler = ElectricalSwitchingSerialHandlerObject.GetComponent<ElectricalSwitchingSerialHandler>();

        SetCanvas();
    }

    void SetCanvas()
    {
        GameObject ExplanationTextObj = GameObject.Find("ExplanationText");
        ExplanationText = ExplanationTextObj.GetComponent<Text>();

        CheckIntranasalChemosensation = GameObject.Find("CheckIntranasalChemosensation");
        AdjustTactileSensation = GameObject.Find("AdjustTactileSensation");

        //GameObject RightAdjustSliderObj = AdjustTactileSensation.transform.Find("RightAdjustSlider").gameObject;
        rightAdjustSlider = RightAdjustSliderObj.GetComponent<Slider>();
        rightAdjustSlider.maxValue = (int)StimulationCurrents.RightCurrent;
        rightAdjustSlider.minValue = 0;
        rightAdjustSlider.value = (int)StimulationCurrents.RightCurrent;
        rightAdjustSlider.wholeNumbers = true;
        //GameObject LeftAdjustSliderObj = AdjustTactileSensation.transform.Find("LeftAdjustSlider").gameObject;
        leftAdjustSlider = LeftAdjustSliderObj.GetComponent<Slider>();
        leftAdjustSlider.maxValue = (int)StimulationCurrents.LeftCurrent;
        leftAdjustSlider.minValue = 0;
        leftAdjustSlider.value = (int)StimulationCurrents.LeftCurrent;
        leftAdjustSlider.wholeNumbers = true;

        ExplanationText.text = "鼻腔内化学感覚の確認";

        CheckIntranasalChemosensation.SetActive(true);
        AdjustTactileSensation.SetActive(false);
    }

    public void LeftGNSClicked()
    {
        LeftCurrent = (int)StimulationCurrents.LeftCurrent;
        RightCurrent = 0;
        StartCoroutine(Stimulate(true));
    }

    public void RightGNSClicked()
    {
        RightCurrent = (int)StimulationCurrents.RightCurrent;
        LeftCurrent = 0;
        StartCoroutine(Stimulate(false));
    }

    public void TurnModeButtonClicked()
    {
        ExplanationText.text = "触覚強度の調整";
        CheckIntranasalChemosensation.SetActive(false);
        AdjustTactileSensation.SetActive(true);
    }

    public void LeftCheckButtonClicked()
    {
        LeftCurrent = (int)StimulationCurrents.LeftCurrent;
        RightCurrent = (int)rightAdjustSlider.value;
        StartCoroutine(Stimulate(true));
    }

    public void RightCheckButtonClicked()
    {
        RightCurrent = (int)StimulationCurrents.RightCurrent;
        LeftCurrent = (int)leftAdjustSlider.value;
        StartCoroutine(Stimulate(false));
    }

    public void OKButtonClicked()
    {
        StreamWriter sw;
        sw = new StreamWriter("adjustedCurrentValue.csv", false, Encoding.GetEncoding("Shift_JIS"));
        string[] s1 = { "LeftAdjustedValue", "RightAdjustedValue" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        int leftValue = (int)leftAdjustSlider.value;
        int rightValue = (int)rightAdjustSlider.value;
        string[] s3 = { leftValue.ToString(), rightValue.ToString() };
        string s4 = string.Join(",", s3);
        sw.WriteLine(s4);
        sw.Close();

        GameObject OKButton = eventSystem.currentSelectedGameObject;
        GameObject ButtonTextObj = OKButton.transform.Find("Text").gameObject;
        Text ButtonText = ButtonTextObj.GetComponent<Text>();
        ButtonText.text = "完了";
    }

    public IEnumerator Stimulate(bool leftGNS)
    {
        int waveForm;
        if (WaveForm == WaveForms.trapezoidal_negative)
        {
            waveForm = (int)WaveForms.direct_negative;
        }
        else if (WaveForm == WaveForms.trapezoidal_positive)
        {
            waveForm = (int)WaveForms.direct_positive;
        }
        else
        {
            waveForm = (int)WaveForm;
        }

        if (leftGNS)
        {
            electricalSwitchingSerialHandler.SendTurnOnLeftGNS();
            yield return new WaitForSeconds(0.01f);
            electricalSwitchingSerialHandler.SendTurnOnRightSham();
        }
        else
        {
            electricalSwitchingSerialHandler.SendTurnOnRightGNS();
            yield return new WaitForSeconds(0.01f);
            electricalSwitchingSerialHandler.SendTurnOnLeftSham();
        }

        SendLeftElectricalStimulation(LeftCurrent, Frequency, waveForm, Duty, TransitionDuration, (int)TransitionForm);
        SendRightElectricalStimulation(RightCurrent, Frequency, waveForm, Duty, TransitionDuration, (int)TransitionForm);
        Debug.Log("STRAT");
        yield return new WaitForSeconds(Duration);
        Debug.Log("STOP");
        if (WaveForm == WaveForms.trapezoidal_negative)
        {
            SendLeftElectricalStimulation(LeftCurrent, Frequency, (int)WaveForms.direct_negative, Duty, TransitionDuration, (int)TransitionForms.lineardecay);
            SendRightElectricalStimulation(RightCurrent, Frequency, (int)WaveForms.direct_negative, Duty, TransitionDuration, (int)TransitionForms.lineardecay);
            yield return new WaitForSeconds(1.0f * TransitionDuration / Frequency);
            StopElectricalStimulation();
        }
        else if (WaveForm == WaveForms.trapezoidal_positive)
        {
            SendLeftElectricalStimulation(LeftCurrent, Frequency, (int)WaveForms.direct_positive, Duty, 5, (int)TransitionForms.lineardecay);
            SendRightElectricalStimulation(RightCurrent, Frequency, (int)WaveForms.direct_positive, Duty, 5, (int)TransitionForms.lineardecay);
            yield return new WaitForSeconds(1.0f * 5 / Frequency);
            StopElectricalStimulation();
        }
        else
        {
            StopElectricalStimulation();
        }
        electricalSwitchingSerialHandler.SendTurnOff();
    }

    private void SendLeftElectricalStimulation(int current, int frequency, int wave_form, int duty, int transition_duration, int transition_form)
    {
        char channel = '0';

        byte[] buffer = new byte[8];
        int[] Send1 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send2 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send3 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send4 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send5 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int i;

        // 最初の3bitがチャンネル
        if (channel == '1') { Send1[7] = 0; Send1[6] = 0; Send1[5] = 1; }
        else if (channel == '2') { Send1[7] = 0; Send1[6] = 1; Send1[5] = 0; }
        else if (channel == '3') { Send1[7] = 0; Send1[6] = 1; Send1[5] = 1; }
        else if (channel == '0') { Send1[7] = 0; Send1[6] = 0; Send1[5] = 0; }
        else if (channel == '4') { Send1[7] = 1; Send1[6] = 0; Send1[5] = 0; }
        else if (channel == '5') { Send1[7] = 1; Send1[6] = 0; Send1[5] = 1; }
        else if (channel == '6') { Send1[7] = 1; Send1[6] = 1; Send1[5] = 0; }

        // 電流値(正)を送る;1byte目の残り5bitと2byte目の7bit
        for (i = 0; i <= 6; i++)
        {
            Send2[i] = current % 2;
            current = current / 2;
        }
        for (i = 0; i <= 4; i++)
        {
            Send1[i] = current % 2;
            current = current / 2;
        }

        // 周波数を送る;2byte目の残り1bitと3byte目と4byte目の1bit
        Send4[0] = frequency % 2;
        frequency = frequency / 2;
        for (i = 0; i <= 7; i++)
        {
            Send3[i] = frequency % 2;
            frequency = frequency / 2;
        }
        Send2[7] = frequency % 2;
        frequency = frequency / 2;

        // 波形情報を送る; 4byte目の残り3bit
        for (i = 4; i <= 6; i++)
        {
            Send4[i] = wave_form % 2;
            wave_form = wave_form / 2;
        }

        // duty比を送る; 4byte目の残り4bit
        for (i = 0; i <= 3; i++)
        {
            Send4[i] = duty % 2;
            duty = duty / 2;
        }

        // 立ち上げ時間を送る; 5byte目の4bit
        for (i = 4; i <= 7; i++)
        {
            Send5[i] = transition_duration % 2;
            transition_duration = transition_duration / 2;
        }

        // 立ち上げ手法を送る; 5byte目の残り3bit
        for (i = 1; i <= 3; i++)
        {
            Send5[i] = transition_form % 2;
            transition_form = transition_form / 2;
        }

        // 転送データの作成
        buffer[0] = Convert.ToByte(71);
        buffer[1] = Convert.ToByte(Send1[7] * 128 + Send1[6] * 64 + Send1[5] * 32 + Send1[4] * 16 + Send1[3] * 8 + Send1[2] * 4 + Send1[1] * 2 + Send1[0]);
        buffer[2] = Convert.ToByte(Send2[7] * 128 + Send2[6] * 64 + Send2[5] * 32 + Send2[4] * 16 + Send2[3] * 8 + Send2[2] * 4 + Send2[1] * 2 + Send2[0]);
        buffer[3] = Convert.ToByte(Send3[7] * 128 + Send3[6] * 64 + Send3[5] * 32 + Send3[4] * 16 + Send3[3] * 8 + Send3[2] * 4 + Send3[1] * 2 + Send3[0]);
        buffer[4] = Convert.ToByte(Send4[7] * 128 + Send4[6] * 64 + Send4[5] * 32 + Send4[4] * 16 + Send4[3] * 8 + Send4[2] * 4 + Send4[1] * 2 + Send4[0]);
        buffer[5] = Convert.ToByte(Send5[7] * 128 + Send5[6] * 64 + Send5[5] * 32 + Send5[4] * 16 + Send5[3] * 8 + Send5[2] * 4 + Send5[1] * 2 + Send5[0]);

        leftSeeduinoSerialHandler.SendParameters(buffer);
        Task.Delay(1);
    }

    private void SendRightElectricalStimulation(int current, int frequency, int wave_form, int duty, int transition_duration, int transition_form)
    {
        char channel = '0';

        byte[] buffer = new byte[8];
        int[] Send1 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send2 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send3 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send4 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] Send5 = { 0, 0, 0, 0, 0, 0, 0, 0 };
        int i;

        // 最初の3bitがチャンネル
        if (channel == '1') { Send1[7] = 0; Send1[6] = 0; Send1[5] = 1; }
        else if (channel == '2') { Send1[7] = 0; Send1[6] = 1; Send1[5] = 0; }
        else if (channel == '3') { Send1[7] = 0; Send1[6] = 1; Send1[5] = 1; }
        else if (channel == '0') { Send1[7] = 0; Send1[6] = 0; Send1[5] = 0; }
        else if (channel == '4') { Send1[7] = 1; Send1[6] = 0; Send1[5] = 0; }
        else if (channel == '5') { Send1[7] = 1; Send1[6] = 0; Send1[5] = 1; }
        else if (channel == '6') { Send1[7] = 1; Send1[6] = 1; Send1[5] = 0; }

        // 電流値(正)を送る;1byte目の残り5bitと2byte目の7bit
        for (i = 0; i <= 6; i++)
        {
            Send2[i] = current % 2;
            current = current / 2;
        }
        for (i = 0; i <= 4; i++)
        {
            Send1[i] = current % 2;
            current = current / 2;
        }

        // 周波数を送る;2byte目の残り1bitと3byte目と4byte目の1bit
        Send4[0] = frequency % 2;
        frequency = frequency / 2;
        for (i = 0; i <= 7; i++)
        {
            Send3[i] = frequency % 2;
            frequency = frequency / 2;
        }
        Send2[7] = frequency % 2;
        frequency = frequency / 2;

        // 波形情報を送る; 4byte目の残り3bit
        for (i = 4; i <= 6; i++)
        {
            Send4[i] = wave_form % 2;
            wave_form = wave_form / 2;
        }

        // duty比を送る; 4byte目の残り4bit
        for (i = 0; i <= 3; i++)
        {
            Send4[i] = duty % 2;
            duty = duty / 2;
        }

        // 立ち上げ時間を送る; 5byte目の4bit
        for (i = 4; i <= 7; i++)
        {
            Send5[i] = transition_duration % 2;
            transition_duration = transition_duration / 2;
        }

        // 立ち上げ手法を送る; 5byte目の残り3bit
        for (i = 1; i <= 3; i++)
        {
            Send5[i] = transition_form % 2;
            transition_form = transition_form / 2;
        }

        // 転送データの作成
        buffer[0] = Convert.ToByte(71);
        buffer[1] = Convert.ToByte(Send1[7] * 128 + Send1[6] * 64 + Send1[5] * 32 + Send1[4] * 16 + Send1[3] * 8 + Send1[2] * 4 + Send1[1] * 2 + Send1[0]);
        buffer[2] = Convert.ToByte(Send2[7] * 128 + Send2[6] * 64 + Send2[5] * 32 + Send2[4] * 16 + Send2[3] * 8 + Send2[2] * 4 + Send2[1] * 2 + Send2[0]);
        buffer[3] = Convert.ToByte(Send3[7] * 128 + Send3[6] * 64 + Send3[5] * 32 + Send3[4] * 16 + Send3[3] * 8 + Send3[2] * 4 + Send3[1] * 2 + Send3[0]);
        buffer[4] = Convert.ToByte(Send4[7] * 128 + Send4[6] * 64 + Send4[5] * 32 + Send4[4] * 16 + Send4[3] * 8 + Send4[2] * 4 + Send4[1] * 2 + Send4[0]);
        buffer[5] = Convert.ToByte(Send5[7] * 128 + Send5[6] * 64 + Send5[5] * 32 + Send5[4] * 16 + Send5[3] * 8 + Send5[2] * 4 + Send5[1] * 2 + Send5[0]);

        rightSeeduinoSerialHandler.SendParameters(buffer);
        Task.Delay(1);
    }

    private void StopElectricalStimulation()
    {
        SendLeftElectricalStimulation(0, 0, 0, 0, 0, 0);
        SendRightElectricalStimulation(0, 0, 0, 0, 0, 0);
    }
}
