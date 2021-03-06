﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
	public AudioSource[] sourcePool;
	public bool isWalking;
	public AudioSource walkingSource;
	
	public void PlaySFX(string sfx){
		
		// Runs through every AudioSource in the pool.
		foreach(AudioSource source in sourcePool)
		{
			
			// If this source is not playing sound,
			// break the loop and play the requested sfx.
			if(!source.isPlaying)
			{
				source.PlayOneShot(Resources.Load("SFX/" + sfx) as AudioClip);
				break;
			}
		}
	}
}
