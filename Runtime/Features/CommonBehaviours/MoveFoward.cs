using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 direction = Vector3.forward;
    public bool automation = true;
    private float time = 0.0f;
    public float timer = 0.5f;

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
        if(automation)
        {
            time += Time.deltaTime;
            if(time > timer)
            {
                time = 0;
                Pooling.I.PushToPool(this.gameObject);
            }    
        }    
    }
}
