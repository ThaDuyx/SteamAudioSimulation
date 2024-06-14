using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public delegate void TimerEndedAction();
    public static event TimerEndedAction OnTimerEnded;
    
    public bool IsActive { get; private set; }
    public float CurrentDuration { get { return _duration; }}
    public float InRoomDuration { get { return _inRoomDuration; }}
    public float TotalDuration { get { return _totalDuration; }}
    public float DurationContiner { get { return _durationContainer; }}

    private bool shouldResetInRoomDuration = false, shouldResetTotalDuration = false;
    private float _duration, _inRoomDuration, _totalDuration, _durationContainer;

    public void SetTo(float duration, int amountOfRenders)
    {
        this._duration = duration;
        IsActive = true;
        
        if (!shouldResetInRoomDuration)
        {
            _inRoomDuration = Calculator.CalculateRenderDuration(duration);
            shouldResetInRoomDuration = true;
        }

        if (!shouldResetTotalDuration)
        {
            _totalDuration = Calculator.CalculateRenderProgress(duration, amountOfRenders);
            _durationContainer = _totalDuration;
            shouldResetTotalDuration = true;
        }

        StartCoroutine(Countdown());
    }

    public void Stop()
    {
        IsActive = false;
        shouldResetInRoomDuration = false;
    }

    public void ResetProgress()
    {
        shouldResetTotalDuration = false;
    }

    private IEnumerator Countdown()
    {
        while (_duration > 0)
        {
            yield return new WaitForSeconds(1.0f);
            _duration--;
            _inRoomDuration--;
            _totalDuration--;
        }
        
        IsActive = false;

        OnTimerEnded.Invoke(); // callback
    }
}
