using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud_MoveH : MonoBehaviour
{
    public float speed = 1;
    public int maxCount = 100;
    int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        count += 1;

        if (count >= maxCount)
        {
            count = 0;
            speed *= -1;
        }

        this.transform.Translate(speed / 50, 0, 0);
    }
}
