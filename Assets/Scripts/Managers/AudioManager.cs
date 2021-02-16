using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Clip { Select, Swap, Clear};

[Serializable]
public struct AudioClip
{
	public Clip clip;
	public AudioSource source;
}

public class AudioManager : MonoBehaviour {
	public static AudioManager instance;
 
	[SerializeField]
	private List<AudioClip> audioClips;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	public void PlayAudio(Clip audioClip) {
		audioClips.FirstOrDefault(clip => clip.clip == audioClip).source.Play();
	}
}
