using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    private const int FPS_SAMPLE_COUNT = 20;

    [SerializeField] private TMP_Text _fpsText;

    private readonly int[] _fpsSamples = new int[FPS_SAMPLE_COUNT];
    private int _fpsSampleIndex;

    private void Awake()
    {
        Application.targetFrameRate = -1;
        InvokeRepeating(nameof(UpdateFps), 0, 0.1f);
    }

    private void Update()
    {
        _fpsSamples[_fpsSampleIndex++] = (int) (1.0f / Time.deltaTime);
        if (_fpsSampleIndex >= FPS_SAMPLE_COUNT)
        {
            _fpsSampleIndex = 0;
        }
    }

    private void UpdateFps()
    {
        var sum = 0;
        for (var i = 0; i < FPS_SAMPLE_COUNT; i++)
        {
            sum += _fpsSamples[i];
        }

        _fpsText.text = $"FPS: {sum / FPS_SAMPLE_COUNT}";
    }
}