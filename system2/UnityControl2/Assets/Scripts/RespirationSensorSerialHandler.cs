using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class RespirationSensorSerialHandler : MonoBehaviour
{
    public class RespirationSignalEventArgs : EventArgs
    {
        public RespirationSignalEventArgs(string signal)
        {
            Signal = signal;
        }

        public string Signal { get; set; }
    }

    public class RespirationDistanceEventArgs : EventArgs
    {
        public RespirationDistanceEventArgs(double x_time, double distance)
        {
            XTime = x_time;
            Distance = distance;
        }

        public double XTime { get; set; }
        public double Distance { get; set; }
    }

    private SerialDataReceivedEventHandler RespirationSensorDataReceivedEventHandler;
    public event EventHandler<RespirationSignalEventArgs> RespirationSignalEventHandlerObj;
    public event EventHandler<RespirationDistanceEventArgs> RespirationDistanceEventHandlerObj;


    public string portName = "COM9";  //変更する必要あり
    public int baudRate = 9600;  // ボーレート(Arduinoに記述したものに合わせる)

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;

    private string message_;
    private bool isNewMessageReceived_ = false;

    private DateTime StartTime;

    void Awake()
    {
        Open();
        DontDestroyOnLoad(this.gameObject);
        StartTime = DateTime.Now;
    }


    void OnDestroy()
    {
        Close();
    }

    private void Open()
    {
        serialPort_ = new SerialPort();
        serialPort_.PortName = portName;
        serialPort_.BaudRate = baudRate;
        serialPort_.Parity = Parity.None;
        serialPort_.DataBits = 8;
        serialPort_.StopBits = StopBits.One;
        serialPort_.Open();

        isRunning_ = true;

        thread_ = new Thread(Read);
        thread_.Start();
    }

    private void Close()
    {
        isNewMessageReceived_ = false;
        isRunning_ = false;

        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }

    protected virtual void SendRespirationSignal(RespirationSignalEventArgs e)
    {
        if (RespirationSignalEventHandlerObj != null)
        {
            RespirationSignalEventHandlerObj(this, e);
        }
    }

    protected virtual void SendRespirationDistanceData(RespirationDistanceEventArgs e)
    {
        if (RespirationDistanceEventHandlerObj != null)
        {
            RespirationDistanceEventHandlerObj(this, e);
        }
    }

    private void Read()
    {
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                int read_size = serialPort_.BytesToRead;
                byte[] data = new byte[read_size];

                for (int i = 0; i < read_size; i++)
                {
                    data[i] = (byte)serialPort_.ReadByte();
                }

                if (read_size == 6)
                {
                    bool respiration_signal_detected = BitConverter.ToBoolean(data, 0);
                    bool inhaling = BitConverter.ToBoolean(data, 1);
                    double distance = (double)BitConverter.ToSingle(data, 2);
                    DateTime currentTime = DateTime.Now;
                    TimeSpan timeDiff = currentTime - StartTime;
                    double time = timeDiff.TotalMilliseconds;

                    if (respiration_signal_detected)
                    {
                        if (inhaling)
                        {
                            SendRespirationSignal(new RespirationSignalEventArgs("i"));
                        }
                        else
                        {
                            SendRespirationSignal(new RespirationSignalEventArgs("e"));
                        }
                    }

                    SendRespirationDistanceData(new RespirationDistanceEventArgs(time, distance));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
}