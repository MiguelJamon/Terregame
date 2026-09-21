using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
    [Header("Prefab y referencias")]
    [SerializeField] private GameObject debrisPrefab;

    [Header("Tiempo de activación")]
    [SerializeField] private float startDelayAfterEarthquake = 5f;
    [SerializeField] private float spawnInterval = 2.5f;
    [SerializeField] private bool spawnEnabled = true;
    [SerializeField] private bool onlyDuringEarthquake = true;

    [Header("Rango de aparición")]
    [SerializeField] private float minSpawnX = -8f;
    [SerializeField] private float maxSpawnX = 8f;
    [SerializeField] private float spawnY = 7f;

    [Header("Movimiento y daño")]
    [SerializeField] private float fallSpeed = 1.8f;
    [SerializeField] private int debrisDamage = 20;
    [SerializeField] private float debrisLifetime = 8f;
    [SerializeField] private float damageCooldown = 0.35f;
    [SerializeField] private int maxSimultaneousDebris = 8;

    private readonly List<DebrisPiece> activeDebris = new List<DebrisPiece>();
    private Coroutine spawnRoutine;

    public bool SpawnEnabled
    {
        get => spawnEnabled;
        set => spawnEnabled = value;
    }

    private void Update()
    {
        if (!spawnEnabled)
        {
            return;
        }

        if (!onlyDuringEarthquake)
        {
            if (spawnRoutine == null)
            {
                spawnRoutine = StartCoroutine(SpawnRoutine());
            }

            return;
        }

        if (EarthquakeManager.Instance != null && EarthquakeManager.Instance.IsEarthquakeActive)
        {
            if (spawnRoutine == null)
            {
                spawnRoutine = StartCoroutine(SpawnRoutine());
            }
        }
        else if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(startDelayAfterEarthquake);

        while (spawnEnabled)
        {
            if (EarthquakeManager.Instance == null || !EarthquakeManager.Instance.IsEarthquakeActive)
            {
                if (onlyDuringEarthquake)
                {
                    spawnRoutine = null;
                    yield break;
                }
            }

            if (activeDebris.Count < maxSimultaneousDebris)
            {
                SpawnDebris();
            }

            yield return new WaitForSeconds(Mathf.Max(0.1f, spawnInterval));
        }

        spawnRoutine = null;
    }

    private void SpawnDebris()
    {
        if (debrisPrefab == null)
        {
            return;
        }

        float randomX = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);
        GameObject debrisObject = Instantiate(debrisPrefab, spawnPosition, Quaternion.identity);

        DebrisPiece debrisPiece = debrisObject.GetComponent<DebrisPiece>();
        if (debrisPiece == null)
        {
            debrisPiece = debrisObject.AddComponent<DebrisPiece>();
        }

        debrisPiece.Initialize(this, fallSpeed, debrisDamage, debrisLifetime, damageCooldown);
        activeDebris.Add(debrisPiece);
    }

    public void NotifyDebrisDestroyed(DebrisPiece debrisPiece)
    {
        if (debrisPiece == null)
        {
            return;
        }

        if (activeDebris.Contains(debrisPiece))
        {
            activeDebris.Remove(debrisPiece);
        }
    }
}
