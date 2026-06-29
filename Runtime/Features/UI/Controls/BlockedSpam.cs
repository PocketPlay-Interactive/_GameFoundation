using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BlockedSpam : MonoBehaviour
{
    private Button button;
    public float time = 0.5f;
    private float timer = 0.0f;
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            if(gameObject.activeInHierarchy)
            {
                button.enabled = false;
            }
        });
    }

    private void Update()
    {
        if(button.enabled == false)
        {
            timer += Time.deltaTime;
            if(timer > time)
            {
                timer = 0;
                button.enabled = true;
            }
        }
    }
}
