﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyePickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().AddJump();
            other.gameObject.GetComponent<PlayerController>().StopExplode();
            this.gameObject.SetActive(false);
        }
    }

}
