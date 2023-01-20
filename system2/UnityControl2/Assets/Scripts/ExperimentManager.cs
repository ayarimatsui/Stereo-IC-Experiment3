using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{
    [Header("Repeat Times")]
    [Space(20)]
    public int RepeatTime = 15;

    [Header("File Name")]
    [Space(20)]
    public string DirectoryName;

    [Header("Stimulation Settings")]
    [Space(20)]
    //public StimulationCurrents Current;
    public int Frequency = 10;
    public float Duration = 2;
    public WaveForms WaveForm = WaveForms.direct_positive;
    [Range(0, 10)]
    public int Duty = 0;
    [Range(0, 15)]
    public int TransitionDuration = 1;
    public TransitionForms TransitionForm = TransitionForms.constant;


    //public enum StimulationCurrents
    //{
    //    _2_4mA = 2480,
    //    _2_1mA = 2180,
    //    _1_8mA = 1960,
    //    _1_5mA = 1660,
    //    _1_2mA = 1380,
    //    _0_9mA = 1050
    //}

    public enum StimulationCurrents
    {
        LeftCurrent = 1800,
        RightCurrent = 2172
    }

    //public enum StimulationCurrents
    //{
    //    LeftCurrent = 1980,
    //    RightCurrent = 2460
    //}

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

    private enum StimulationType
    {
        LeftGNS,
        RightGNS,
        NoStimuli
    }

    private enum OlfactoryStimulationSide
    {
        Left,
        Right
    }

    public enum Answers
    {
        Left,
        Right,
        NoScent
    }

    public int Trials { get; private set; }
    public int TrialID { get; private set; }
    public bool IsStimulating { get; private set; }
    public bool NeedRetry { get; private set; }
    private int LeftShamCurrent;
    private int RightShamCurrent;
    private GameObject LeftElectricalStimulatorSerialHandlerObject;
    private ElectricalStimulatorSerialHandler leftElectricalStimulatorSerialHandler;
    private GameObject RightElectricalStimulatorSerialHandlerObject;
    private ElectricalStimulatorSerialHandler rightElectricalStimulatorSerialHandler;
    private GameObject ElectricalSwitchingSerialHandlerObject;
    private ElectricalSwitchingSerialHandler electricalSwitchingSerialHandler;
    private GameObject OlfactoryDisplaySerialHandlerObject;
    private OlfactoryDisplaySerialHandler olfactoryDisplaySerialHandler;
    private GameObject RespirationSensorSerialHandlerObject;
    private RespirationSensorSerialHandler respirationSensorSerialHandler;
    private string Respiration;
    private DateTime InhalationDateTime;
    private DateTime ExhalationDateTime;
    private List<List<string>> ExperimentConditionList;
    private bool currentTrial;
    private double currentTrialStartTime;

    public int PracticeTrialID { get; private set; }
    private bool IsPractice;

    private const int LeftScentSlot = 4;
    private const int RightScentSlot = 2;
    private const int CoffeeSlot = 3;
    private const int LeftAirSlot = 5;
    private const int RightAirSlot = 1;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        LeftElectricalStimulatorSerialHandlerObject = GameObject.Find("LeftElectricalStimulatorSerialHandler");
        leftElectricalStimulatorSerialHandler = LeftElectricalStimulatorSerialHandlerObject.GetComponent<ElectricalStimulatorSerialHandler>();
        RightElectricalStimulatorSerialHandlerObject = GameObject.Find("RightElectricalStimulatorSerialHandler");
        rightElectricalStimulatorSerialHandler = RightElectricalStimulatorSerialHandlerObject.GetComponent<ElectricalStimulatorSerialHandler>();
        ElectricalSwitchingSerialHandlerObject = GameObject.Find("ElectricalSwitchingSerialHandler");
        electricalSwitchingSerialHandler = ElectricalSwitchingSerialHandlerObject.GetComponent<ElectricalSwitchingSerialHandler>();
        OlfactoryDisplaySerialHandlerObject = GameObject.Find("OlfactoryDisplaySerialHandler");
        olfactoryDisplaySerialHandler = OlfactoryDisplaySerialHandlerObject.GetComponent<OlfactoryDisplaySerialHandler>();
        RespirationSensorSerialHandlerObject = GameObject.Find("RespirationSensorSerialHandler");
        respirationSensorSerialHandler = RespirationSensorSerialHandlerObject.GetComponent<RespirationSensorSerialHandler>();
        respirationSensorSerialHandler.RespirationSignalEventHandlerObj += new EventHandler<RespirationSensorSerialHandler.RespirationSignalEventArgs>(OnRespirationSignalReceived);
        respirationSensorSerialHandler.RespirationDistanceEventHandlerObj += new EventHandler<RespirationSensorSerialHandler.RespirationDistanceEventArgs>(OnRespirationDistanceReceived);
        currentTrial = false;
    }

    void SetShamCurrents()
    {
        string originalPath = @"C:\Users\cyber\Desktop\matsui\Experiment2System\ElectricalStimulationTest2\adjustedCurrentValue.csv";

        using (StreamReader sr = new StreamReader(originalPath, Encoding.UTF8))
        {
            int line = 0;
            while (!sr.EndOfStream)
            {
                var data = sr.ReadLine();
                if (line == 1)
                {
                    string[] data_str = data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    LeftShamCurrent = Int32.Parse(data_str[0]);
                    RightShamCurrent = Int32.Parse(data_str[1]);

                    StreamWriter sw = new StreamWriter(DirectoryName + "/adjustedShamValue.csv", true, Encoding.GetEncoding("Shift_JIS"));
                    string[] s1 = { "LeftShamCurrent", "RightShamCurrent" };
                    string s2 = string.Join(",", s1);
                    sw.WriteLine(s2);
                    string[] s3 = { LeftShamCurrent.ToString(), RightShamCurrent.ToString() };
                    string s4 = string.Join(",", s3);
                    sw.WriteLine(s4);
                    sw.Close();
                    break;
                }
                line += 1;
            }
            sr.Close();
        }
    }

    public void StartButtonClicked()
    {
        Trials = Enum.GetNames(typeof(StimulationType)).Length * 2 * RepeatTime;
        PracticeTrialID = 1;
        IsPractice = true;
        IsStimulating = false;
        NeedRetry = false;
        MakeOrderCSV();
        MakeResultCSV(DirectoryName);
        SetShamCurrents();
        SceneManager.LoadScene("PracticeExplanationScene");
    }

    public void StartTrial()
    {
        currentTrial = true;
    }

    public void OnRespirationSignalReceived(object sender, RespirationSensorSerialHandler.RespirationSignalEventArgs e)
    {
        string signal = e.Signal;
        if (signal == "i" || signal == "e")
        {
            Respiration = signal;
            if (Respiration == "i")
            {
                InhalationDateTime = DateTime.Now;
            }
            else
            {
                ExhalationDateTime = DateTime.Now;
            }
        }
    }

    public void OnRespirationDistanceReceived(object sender, RespirationSensorSerialHandler.RespirationDistanceEventArgs e)
    {
        if (!currentTrial) return;
        if (IsPractice) return;

        double x_time = e.XTime;
        double distance = e.Distance;

        if (!Directory.Exists(DirectoryName + "/RespirationData/"))
        {
            Directory.CreateDirectory(DirectoryName + "/RespirationData/");
        }

        string fileName = DirectoryName + "/RespirationData/Trial" + TrialID.ToString() + ".csv";
        StreamWriter sw;
        if (!File.Exists(fileName))
        {
            sw = new StreamWriter(fileName, true, Encoding.GetEncoding("Shift_JIS"));
            sw.WriteLine("time,distance");
            currentTrialStartTime = x_time;
        }
        else
        {
            sw = new StreamWriter(fileName, true, Encoding.GetEncoding("Shift_JIS"));
        }
        
        string[] line = { (x_time - currentTrialStartTime).ToString(), distance.ToString() };
        string s = string.Join(",", line);
        sw.WriteLine(s);
        sw.Close();
    }

    public bool CheckRespiration(DateTime instructionTime, bool IsInhalation)
    {
        if (IsInhalation)
        {
            TimeSpan inhalationTimeSpan = InhalationDateTime - instructionTime;
            return Math.Abs(inhalationTimeSpan.TotalMilliseconds) < 500; 
        }
        else
        {
            TimeSpan exhalationTimeSpan = ExhalationDateTime - instructionTime;
            return Math.Abs(exhalationTimeSpan.TotalMilliseconds) < 500;
        }
    }

    public IEnumerator Stimulate()
    {
        yield return new WaitWhile(() => Respiration != "i");
        if (NeedRetry) yield break;
        Debug.Log("Started Stimulation Coroutine");
        IsStimulating = true;
        yield return StartCoroutine("StimulateCoroutine");
        IsStimulating = false;
        currentTrial = false;
        GoToEvaluation();
    }

    public void SetStimulationRetry()
    {
        NeedRetry = true;
    }

    public void SetStimulationRetryOff()
    {
        NeedRetry = false;
    }

    private IEnumerator StimulateCoroutine()
    {
        StimulationType stimulationType;
        OlfactoryStimulationSide olfactionSide;


        if (IsPractice)
        {

            if (PracticeTrialID == 1)
            {
                stimulationType = StimulationType.LeftGNS;
                olfactionSide = OlfactoryStimulationSide.Left;
            }
            else
            {
                stimulationType = StimulationType.RightGNS;
                olfactionSide = OlfactoryStimulationSide.Right;
            }
        }
        else
        {
            stimulationType = (StimulationType)Enum.Parse(typeof(StimulationType), ExperimentConditionList[TrialID - 1][0]);
            olfactionSide = (OlfactoryStimulationSide)Enum.Parse(typeof(OlfactoryStimulationSide), ExperimentConditionList[TrialID - 1][2]);
        }

        if (olfactionSide == OlfactoryStimulationSide.Left)
        {
            olfactoryDisplaySerialHandler.SendDoubleAirPumpOn(LeftScentSlot, RightAirSlot, 255, 1500);
        }
        else
        {
            olfactoryDisplaySerialHandler.SendDoubleAirPumpOn(RightScentSlot, LeftAirSlot, 255, 1500);
        }

        if (stimulationType == StimulationType.LeftGNS)
        {
            electricalSwitchingSerialHandler.SendTurnOnLeftGNS();
            yield return new WaitForSeconds(0.01f);
            electricalSwitchingSerialHandler.SendTurnOnRightSham();
        }
        else if (stimulationType == StimulationType.RightGNS)
        {
            electricalSwitchingSerialHandler.SendTurnOnRightGNS();
            yield return new WaitForSeconds(0.01f);
            electricalSwitchingSerialHandler.SendTurnOnLeftSham();
        }
        else
        {
            electricalSwitchingSerialHandler.SendTurnOff();
        }

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


        int LeftCurrent = 0;
        int RightCurrent = 0;
        if (stimulationType == StimulationType.LeftGNS)
        {
            LeftCurrent = (int)StimulationCurrents.LeftCurrent;
            RightCurrent = RightShamCurrent;
        }
        else if (stimulationType == StimulationType.RightGNS)
        {
            LeftCurrent = LeftShamCurrent;
            RightCurrent = (int)StimulationCurrents.RightCurrent;
        }

        if (stimulationType != StimulationType.NoStimuli)
        {
            SendElectricalStimulation(leftElectricalStimulatorSerialHandler, LeftCurrent, Frequency, waveForm, Duty, TransitionDuration, (int)TransitionForm);
            SendElectricalStimulation(rightElectricalStimulatorSerialHandler, RightCurrent, Frequency, waveForm, Duty, TransitionDuration, (int)TransitionForm);
        }

        yield return new WaitForSeconds(Duration);


        if (stimulationType != StimulationType.NoStimuli)
        {
            if (WaveForm == WaveForms.trapezoidal_negative)
            {
                SendElectricalStimulation(leftElectricalStimulatorSerialHandler, LeftCurrent, Frequency, (int)WaveForms.direct_negative, Duty, TransitionDuration, (int)TransitionForms.lineardecay);
                SendElectricalStimulation(rightElectricalStimulatorSerialHandler, RightCurrent, Frequency, (int)WaveForms.direct_negative, Duty, TransitionDuration, (int)TransitionForms.lineardecay);
                yield return new WaitForSeconds(1.0f * TransitionDuration / Frequency);
            }
            else if (WaveForm == WaveForms.trapezoidal_positive)
            {
                SendElectricalStimulation(leftElectricalStimulatorSerialHandler, LeftCurrent, Frequency, (int)WaveForms.direct_positive, Duty, 5, (int)TransitionForms.lineardecay);
                SendElectricalStimulation(rightElectricalStimulatorSerialHandler, RightCurrent, Frequency, (int)WaveForms.direct_positive, Duty, 5, (int)TransitionForms.lineardecay);
                yield return new WaitForSeconds(1.0f * 5 / Frequency);
            }
        }

        StopElectricalStimulation(leftElectricalStimulatorSerialHandler);
        StopElectricalStimulation(rightElectricalStimulatorSerialHandler);
        electricalSwitchingSerialHandler.SendTurnOff();
        olfactoryDisplaySerialHandler.SendAirPumpOff();
    }

    private void SendElectricalStimulation(ElectricalStimulatorSerialHandler electricalStimulatorSerialHandler, int current, int frequency, int wave_form, int duty, int transition_duration, int transition_form)
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

        electricalStimulatorSerialHandler.SendParameters(buffer);
        Task.Delay(1);
    }

    public void StopElectricalStimulation(ElectricalStimulatorSerialHandler electricalStimulatorSerialHandler)
    {
        SendElectricalStimulation(electricalStimulatorSerialHandler, 0, 0, 0, 0, 0, 0);
    }

    public void CompleteTrial()
    {
        if (IsPractice)
        {
            PracticeTrialID += 1;
            if (PracticeTrialID > 2)
            {
                TrialID = 1;
            }
            SceneManager.LoadScene("IntervalScene");
        }
        else
        {
            TrialID += 1;
            if (TrialID == (Trials / 2) + 1)
            {
                Debug.Log("Go To Interval");
                SceneManager.LoadScene("GoToIntervalScene");
            }
            else if (TrialID <= Trials)
            {
                SceneManager.LoadScene("IntervalScene");
            }
            else
            {
                SceneManager.LoadScene("EndScene");
            }
        }
    }

    public void StartInterval(int intervalDuration)
    {
        StartCoroutine("IntervalCoroutine", intervalDuration);
    }

    private IEnumerator IntervalCoroutine(int intervalDuration) // TODO 要変更 
    {
        olfactoryDisplaySerialHandler.SendSingleAirPumpOn(CoffeeSlot, 255, true, 4000, 4);
        yield return new WaitForSeconds(4.0f);
        olfactoryDisplaySerialHandler.SendSingleAirPumpOn(LeftAirSlot, 255, false, (intervalDuration * 1000 - 4000) / 2, 0);
        yield return new WaitForSeconds((float)(intervalDuration - 4.0f) / 2.0f);
        olfactoryDisplaySerialHandler.SendSingleAirPumpOn(RightAirSlot, 255, false, (intervalDuration * 1000 - 4000) / 2, 0);
        yield return new WaitForSeconds((float)(intervalDuration - 4.0f) / 2.0f);
        olfactoryDisplaySerialHandler.SendAirPumpOff();
    }

    public void GoToEvaluation()
    {
        SceneManager.LoadScene("EvaluationScene");
    }

    public void GoToNextTrial()
    {
        if (IsPractice && PracticeTrialID > 2)
        {
            IsPractice = false;
            SceneManager.LoadScene("TrialExplanationScene");
        }
        else
        {
            SceneManager.LoadScene("TrialScene");
        }
    }

    public void GoToInterval()
    {
        SceneManager.LoadScene("BackToTrialScene");
    }

    public void BackToTrial()
    {
        SceneManager.LoadScene("TrialScene");
    }

    // ファイル操作関連
    private void MakeOrderCSV()
    {
        StreamWriter sw;
        List<List<string>> OrderList = new List<List<string>>();

        foreach (StimulationType stimulationType in Enum.GetValues(typeof(StimulationType)))
        {
            string stimulationTypeName = Enum.GetName(typeof(StimulationType), stimulationType);

            for (int i = 0; i < RepeatTime; i++)
            {
                string GNSSide = "None";
                if (stimulationType == StimulationType.LeftGNS)
                {
                    GNSSide = "Left";
                }
                else if (stimulationType == StimulationType.RightGNS)
                {
                    GNSSide = "Right";
                }
                var orderLine = new List<string>() { stimulationTypeName, GNSSide, OlfactoryStimulationSide.Left.ToString() }; // stimulationType, electricalStimulationSide, olfactoryStimulationSide
                OrderList.Add(orderLine);
                orderLine = new List<string>() { stimulationTypeName, GNSSide, OlfactoryStimulationSide.Right.ToString() };
                OrderList.Add(orderLine);
            }
        }

        var rnd = new System.Random();
        var OrderListShuffled = OrderList.OrderBy(_ => rnd.Next()).ToList();
        ExperimentConditionList = OrderListShuffled;

        sw = new StreamWriter("ExperimentOrder.csv", false, Encoding.GetEncoding("Shift_JIS"));
        string[] s1 = { "trialID", "StimulationType", "GNSSide", "OlfactoryStimulationSide" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        for (int i = 0; i < Trials; i++)
        {
            string[] lineList = { (i + 1).ToString(), ExperimentConditionList[i][0], ExperimentConditionList[i][1], ExperimentConditionList[i][2] };
            string line = string.Join(",", lineList);
            sw.WriteLine(line);
        }
        sw.Close();
        Debug.Log("Experiment Order Saved");
    }

    private void MakeResultCSV(string directoryName)
    {
        if (directoryName == "")
        {
            return;
        }

        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        StreamWriter sw;

        string fileName = directoryName + "/result.csv";
        
        sw = new StreamWriter(fileName, false, Encoding.GetEncoding("Shift_JIS"));
        string[] s1 = { "trialID", "StimulationType", "GNSSide", "OlfactoryStimulationSide", "Consistency", "Answer", "Correct" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        sw.Close();
        Debug.Log("Result CSV Created");
    }

    public void SaveResult(Answers answer)
    {
        if (IsPractice)
        {
            return;
        }
        if (DirectoryName == "")
        {
            return;
        }

        StreamWriter sw;
        string fileName = DirectoryName + "/result.csv";

        sw = new StreamWriter(fileName, true, Encoding.GetEncoding("Shift_JIS"));

        bool consistency = (ExperimentConditionList[TrialID - 1][1] == ExperimentConditionList[TrialID - 1][2]);
        bool correct = (ExperimentConditionList[TrialID - 1][2] == answer.ToString());
        string[] s1 = { TrialID.ToString(), ExperimentConditionList[TrialID - 1][0], ExperimentConditionList[TrialID - 1][1], ExperimentConditionList[TrialID - 1][2], consistency.ToString(), answer.ToString(), correct.ToString() };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        sw.Close();
    }
}
