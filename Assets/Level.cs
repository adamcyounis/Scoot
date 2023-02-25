using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
    public List<Transform> airSpawnPoints;
    int airSpawnIndex;
    public List<Transform> groundedSpawnPoints;
    int groundSpawnIndex;
    public void SpawnCharacter(Character ch, bool grounded = false) {
        ch.gameObject.SetActive(true);
        if (grounded) {
            Vector2 pos = groundedSpawnPoints[groundSpawnIndex % groundedSpawnPoints.Count].position;
            ch.transform.position = (Vector3)pos + ch.selfAwareness.inverseFootOffset;
            Debug.Log("spawning character at " + ch.transform.position);
            groundSpawnIndex++;
        } else {
            Vector2 pos = airSpawnPoints[airSpawnIndex % airSpawnPoints.Count].position;
            ch.platform.SpawnIn(pos);
            airSpawnIndex++;
        }

    }
}