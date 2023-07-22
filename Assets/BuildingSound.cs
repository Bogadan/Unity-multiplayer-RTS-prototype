using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSound : MonoBehaviour
{
    [SerializeField] AudioSource buildingAudio;
    // Start is called before the first frame update
    
    public void PlayBuildingSound()
    {
        buildingAudio.Play();
    }
}
