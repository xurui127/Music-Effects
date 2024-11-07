using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public enum Practice
    {
        one = 0,
        two = 1,
        three = 2,
        four = 3,
        five = 4,
    }
    AudioSource audioSource;
    [SerializeField] private List<Transform> bars;
    [SerializeField] private GameObject barsObject;
    [SerializeField] private float scaleMultiplier = 20f;
    [SerializeField] private float timer = 0.05f;

    private float[] spectrum = new float[256];

    [SerializeField] private Light sceneLight;

    [SerializeField] private Color beatColor = Color.red;
    [SerializeField] private Color defaultColor = Color.white;

    private int lowFrequencyEnd = 85; 
    private int midFrequencyEnd = 170;

    private float lightIntensity = 1f;

    [SerializeField] private float beatThreshold = 0.5f; 
    [SerializeField] private float beatDecayRate = 2f;


    [SerializeField] private ParticleSystem beatEffect;

    private bool beatTriggered = false;
    private bool beatActive = false;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(SetVolumeP2());
    }

    // Update is called once per frame
    void Update()
    {
        //float[] spectrum = new float[256];
        //audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        //for (int i = 0; i < volums.Count; i++)
        //{
        //    volums[i].transform.localScale = new Vector3(
        //        volums[i].localScale.x, 
        //        spectrum[i] * step,
        //        volums[i].localScale.z);

        //}
        //for (int i = 1; i < spectrum.Length - 1; i++)
        //{
        //    Debug.DrawLine(
        //        new Vector3(i - 1, spectrum[i] + 10, 0),
        //        new Vector3(i, spectrum[i + 1] + 10, 0),
        //        Color.red);
        //    Debug.DrawLine(
        //        new Vector3(i - 1,
        //        Mathf.Log(spectrum[i - 1]) + 10, 2),
        //        new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2),
        //        Color.cyan);
        //    Debug.DrawLine(
        //        new Vector3(Mathf.Log(i - 1),
        //        spectrum[i - 1] - 10, 1),
        //        new Vector3(Mathf.Log(i),
        //        spectrum[i] - 10, 1),
        //        Color.green);
        //    Debug.DrawLine(
        //        new Vector3(Mathf.Log(i - 1),
        //        Mathf.Log(spectrum[i - 1]), 3),
        //        new Vector3(Mathf.Log(i),
        //        Mathf.Log(spectrum[i]), 3),
        //        Color.blue);
        //}
    }

    private IEnumerator SetVolumeP1()
    {
        while (true)
        {

            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

            for (int i = 0; i < bars.Count && i < spectrum.Length; i++)
            {
                bars[i].transform.localScale = new Vector3(
                    bars[i].localScale.x,
                    Mathf.Clamp(spectrum[i] * scaleMultiplier, 0.1f, 10f),
                    bars[i].localScale.z);
            }
         
        }
    }

    private IEnumerator SetVolumeP2()
    {
        while (true)
        {
            while (true)
            {
                audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

                float bass = CalculateAverage(spectrum, 0, lowFrequencyEnd);

                if (bass > beatThreshold && !beatTriggered)
                {
                    TriggerBeatEffect();

                    // 直接将灯光颜色设置为 beatColor 以增强变化效果
                    sceneLight.color = beatColor;
                    lightIntensity = Mathf.Lerp(lightIntensity, 5, bass * 10);

                    // 增加 bars 的缩放
                    for (int i = 0; i < bars.Count && i < spectrum.Length; i++)
                    {
                        float scaleY = Mathf.Clamp(spectrum[i] * scaleMultiplier, 0.1f, 15f);
                        bars[i].localScale = new Vector3(bars[i].localScale.x, scaleY, bars[i].localScale.z);
                    }

                    beatTriggered = true;
                }
                else
                {
                    // 将颜色逐渐过渡回 defaultColor
                    sceneLight.color = Color.Lerp(sceneLight.color, defaultColor, beatDecayRate * Time.deltaTime);
                    lightIntensity = Mathf.Lerp(lightIntensity, 1, beatDecayRate * Time.deltaTime);

                    // 逐步衰减 bars 的缩放
                    for (int i = 0; i < bars.Count && i < spectrum.Length; i++)
                    {
                        float scaleY = Mathf.Clamp(spectrum[i] * scaleMultiplier, 0.1f, 10f);
                        bars[i].localScale = Vector3.Lerp(bars[i].localScale, new Vector3(bars[i].localScale.x, scaleY, bars[i].localScale.z), beatDecayRate * Time.deltaTime);
                    }

                    // 当音量低于一半阈值时，重置节奏触发标记
                    if (bass < beatThreshold * 0.5f)
                    {
                        beatTriggered = false;
                    }
                }

                sceneLight.intensity = lightIntensity;
                yield return new WaitForSeconds(timer);
            }
        }
    }
    private float CalculateAverage(float[] data, int start, int end)
    {
        float sum = 0;
        for (int i = start; i < end; i++)
        {
            sum += data[i];
        }
        return sum / (end - start);
    }
    private void TriggerBeatEffect()
    {
        sceneLight.color = beatColor;
    }
    private void ResetBeatEffect()
    {
        sceneLight.color = defaultColor;
    }
}
