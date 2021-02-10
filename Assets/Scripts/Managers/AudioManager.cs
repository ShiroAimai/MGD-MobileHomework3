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

	// Use this for initialization
	void Start () {
		instance = GetComponent<AudioManager>();
	}

	public void PlayAudio(Clip audioClip) {
		audioClips.FirstOrDefault(clip => clip.clip == audioClip).source.Play();
	}
}
