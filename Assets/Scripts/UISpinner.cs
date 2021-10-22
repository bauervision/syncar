using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpinner : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        var spinAmount = 50 * Time.deltaTime;
        transform.Rotate(0, 0, -spinAmount);
    }
}
