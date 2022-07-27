using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public int numberToSpawn = 5;
    public Vector3 spawnPointCenter = Vector3.zero;
    public float positionFactor = .25f;
    public float scaleFactor = .25f;

   public void OnReadyToSpawn()
    {
        for(int i = 0; i < numberToSpawn; i++)
        {
            GameObject cube = PhotonNetwork.Instantiate(cubePrefab.name, spawnPointCenter, Quaternion.identity);

            cube.transform.SetParent(transform.parent);

            cube.transform.position *= Random.Range(1 - positionFactor, 1 + positionFactor);
            cube.transform.localScale *= Random.Range(1 - scaleFactor, 1 + scaleFactor);
        }
    }
}
