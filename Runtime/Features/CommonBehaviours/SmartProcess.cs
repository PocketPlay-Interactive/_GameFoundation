using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SmartProcess : MonoBehaviour
{
    public float NumberOfTimer = 5;
    public Image process;
    public Text processText;

    [SerializeField]
    private UnityEvent onProcess;

    //[SerializeField]
    //private UnityEvent<Text> onProcessFrame;

    [SerializeField]
    private UnityEvent onProcessComplete;

    private void OnEnable()
    {
        onProcess?.Invoke();
    }

    private void Update()
    {
        if(process.fillAmount > 0)
        {
            process.fillAmount -= Time.deltaTime / NumberOfTimer;
            if(process.fillAmount <= 0)
            {
                onProcessComplete?.Invoke();
            }
            int processNumber = (int)(process.fillAmount * NumberOfTimer);
            if (processNumber >= 3)
                processNumber = 3;
            processText.text = processNumber.ToString();
        }    
    }
}
