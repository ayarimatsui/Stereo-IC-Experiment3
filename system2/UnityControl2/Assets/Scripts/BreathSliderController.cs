using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreathSliderController : MonoBehaviour
{
    public Slider breathSlider;
    public Image handleImage;
    public Text BreathText;

    private const float period = 4.0f;
    private float currentPeriodTime;

    // Start is called before the first frame update
    void Start()
    {
        breathSlider.value = 0.0f;
        handleImage.color = new Color32(0, 0, 255, 150);
        currentPeriodTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        currentPeriodTime += Time.deltaTime;
        if (currentPeriodTime > period)
        {
            currentPeriodTime -= period;
        }
        float x = 2.0f * Mathf.PI * currentPeriodTime / period;

        breathSlider.value = Mathf.Sin(x);

        int r;
        int b;
        if (x <= 0.5f * Mathf.PI || x >= 1.5f * Mathf.PI)
        {
            r = (int)(255.0f * Mathf.Abs(Mathf.Sin(x)));
            b = 255;
            BreathText.text = "INHALE\n吸って";
            BreathText.color = handleImage.color = new Color32(0, 0, 255, 150);
        }
        else
        {
            r = 255;
            b = (int)(255.0f * Mathf.Abs(Mathf.Sin(x)));
            BreathText.text = "EXHALE\n吐いて";
            BreathText.color = handleImage.color = new Color32(255, 0, 0, 150);
        }
        handleImage.color = new Color32((byte)r, 0, (byte)b, 150);

        RectTransform textTransform = BreathText.GetComponent<RectTransform>();
        textTransform.localScale = new Vector3(1.0f + 0.2f * Mathf.Sin(x), 1.0f + 0.2f * Mathf.Sin(x), 1.0f);
    }

    void OnDestroy()
    {
        breathSlider.value = 0.0f;
        handleImage.color = new Color32(255, 255, 255, 150);
    }
}
