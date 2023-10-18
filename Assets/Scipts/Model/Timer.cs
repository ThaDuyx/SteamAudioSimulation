using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private bool _isActive = false;
    private float duration;

    public void Begin(float duration)
    {
        this.duration = duration;
        _isActive = true;
        StartCoroutine(Countdown());
    }

    public void Stop()
    {
        _isActive = false;
    }
    private IEnumerator Countdown()
    {
        while (duration > 0)
        {
            yield return new WaitForSeconds(1.0f);
            duration--;
        }
        
        _isActive = false;
    }

    public bool IsActive()
    {
        return _isActive;
    }

    public float GetTimeLeft()
    {
        return duration;
    }
}
