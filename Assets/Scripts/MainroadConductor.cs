using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemySpawn
{
    [Tooltip("Beat on which enemy is spawned relative to round start")]
    public int Beat;
    [Tooltip("Position at which enemy is spawned")]
    public Transform SpawnLocation;
    [Tooltip("Enemy to spawn")]
    public Enemy Enemy;
}

public class MainroadConductor : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Beat offset of first round. Use negative value to start conductor using script.")]
    public int BeatOffset;
    [Tooltip("Conductor object used to time actions")]
    public Conductor Conductor;
    [Tooltip("Player to assign to enemies as target")]
    public Transform Player;
    [Tooltip("Beat and location of enemy spawns relative to round start")]
    public List<EnemySpawn> EnemySpawns;
    [Tooltip("Boundaries on encounter entrance")]
    public GameObject Entrance;
    [Tooltip("Boundaries on encounter exit")]
    public GameObject Exit;
    
    [Header("Visuals")]
    [Tooltip("Particle system emitting fog on left side of end of road.")]
    public ParticleSystem LeftFogCurtain;
    [Tooltip("Particle system emitting fog on right side of end of road.")]
    public ParticleSystem RightFogCurtain;
    [Tooltip("Particle system emitting fog on ground.")]
    public ParticleSystem GroundFog;

    int mRoundBeatOffset;
    bool mIsRoundOver = false;
    int mCrrtIndex = 0;
    int mKillCount = 0;
    bool mIsComplete = false;

    /// <summary> Start function. </summary>
    void Start()
    {
        if (BeatOffset < 0)
        {
            mRoundBeatOffset = int.MaxValue;
        }
        mRoundBeatOffset = BeatOffset;
    }

    /// <summary> Update function. </summary>
    void Update()
    {
        // Return early if done or not yet started.
        if (mIsRoundOver)
        {
            // If encounter is complete, part curtains.
            if (mIsComplete)
            {
                LeftFogCurtain.transform.position -= LeftFogCurtain.transform.right * Time.deltaTime * 5f;
                RightFogCurtain.transform.position -= RightFogCurtain.transform.right * Time.deltaTime * 5f;
            }
            return;
        }

        // Spawn enemy at appropriate location if on beat.
        int crrtRoundBeat = Conductor.GetBeat() - mRoundBeatOffset;
        while (mCrrtIndex < EnemySpawns.Count && crrtRoundBeat - EnemySpawns[mCrrtIndex].Beat == 0)
        {
            Enemy enemy = Instantiate(EnemySpawns[mCrrtIndex].Enemy, EnemySpawns[mCrrtIndex].SpawnLocation.position, Quaternion.identity);
            enemy.Target = Player;
            enemy.Conductor = Conductor;
            enemy.HealthController.OnDeath += IncrementKillCount;
            mCrrtIndex++;
        }

        // If spawn beat of next enemy is less than current beat, next enemy belongs to next round.
        // Current round is over.
        if (mCrrtIndex == EnemySpawns.Count || crrtRoundBeat > EnemySpawns[mCrrtIndex].Beat)
        {
            mIsRoundOver = true;
        }
    }

    /// <summary> Increase enemies killed by one. Update game state as necessary. </summary>
    void IncrementKillCount()
    {
        mKillCount += 1;        
        // If all rounds are complete, part curtains and end main road encounter.
        if (mKillCount == EnemySpawns.Count)
        {
            mIsComplete = true;
            LeftFogCurtain?.Stop();
            RightFogCurtain?.Stop();
            GroundFog?.Stop();
            Exit?.SetActive(false);
            Destroy(this.gameObject, 15f);
        }
        // Start next round if all enemies are dead.
        else if (mKillCount == mCrrtIndex && mIsRoundOver)
        {
            int crrtBeat = Conductor.GetBeat();
            int nextBarBeat = (crrtBeat/4 + 1) * 4;
            mRoundBeatOffset = nextBarBeat;
            mIsRoundOver = false;
            Debug.Log("StartingNewRound");
        }
    }

    /// <summary> If BeatOffset is set to some negative value, conductor will wait for this function to be called with a future beat to start. </summary>
    public void StartConductor(int startBeat)
    {
        mRoundBeatOffset = startBeat;
        LeftFogCurtain?.Play();
        RightFogCurtain?.Play();
        GroundFog?.Play();
        Entrance?.SetActive(true);
    }
}
