using UnityEngine;
using TMPro;
using System;

public class TimerBehavior : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime;
    public Boolean stopped = false;

    void Update() 
    {
        while (!stopped)
        {
            elapsedTime += Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
            timerText.text = time.ToString(@"mm\:ss\:ff");
        }
    }
}