using System;
using UnityEngine;
using Unity.Mathematics;

public class AudioControl : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField, Range(0, 1)] private float volume = 0.0f; // 0:mute-1:max

    public ClickGain _meClickelevel; //入力1
    public P2PGain _p2pClickelevel; //入力2
    public AudioGain _meAudiolevel; //入力1
    public P2PGain _p2pAudiolevel; //入力2

    public bool Me = true;
    private float displacement;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;
    }

    void Update()
    {
        if (Me)
        {
            displacement = math.max(_meClickelevel.GetClickLevel(), _meAudiolevel.GetAudioLevel());
        }
        else
        {
            displacement = math.max(_p2pClickelevel.GetP2PLevel(), _p2pAudiolevel.GetP2PLevel());
        }

        audioSource.volume = displacement;
    }
}
