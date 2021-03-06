﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    private float length, startpos;
    private Camera cam;
    public float parallaxEffect;
    public bool noRepeat;

    // Start is called before the first frame update
    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        cam = Camera.main;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        if (!noRepeat)
        {
            if (temp > startpos + length)
            {
                startpos += length * 2;
            }
            else if (temp < startpos - length)
            {
                startpos -= length * 2;
            }
        }
    }

}