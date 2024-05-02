using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{

    RectTransform crosshair;
    float crosshairDefultSize = 20;
    float crosshairSize = 20;
    float speed = 0.1f; // crosshair Áõ°¡ µÇ´Â ½Ã°£ 

    public PlayerController player;

    void Start()
    {
        crosshair = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (player.isShot) // ÃÑÀ» ½úÀ»¶§
        {
            crosshairSize = Mathf.Lerp(crosshairSize, -player.maxCross , speed * Time.deltaTime);
        }
        else // ¾È ½ò ¶§
        {
            crosshairSize = Mathf.Lerp(crosshairSize, crosshairDefultSize, speed * 100.0f * Time.deltaTime);
        }

        crosshair.sizeDelta = new Vector2(crosshairSize, crosshairSize);
    }
}
