using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketplaceConductor : MonoBehaviour
{
    [Tooltip("Player to use as target")]
    public Transform Player;
    [Tooltip("Conductor object used to time level")]
    public Conductor Conductor;
    [Tooltip("Gargoyle prefabs to spawn")]
    public Gargoyle Gargoyle;
    [Tooltip("Grunt prefabs to spawn")]
    public MeleeEnemy Grunt;
    [Tooltip("Perches for Gargoyles to Perch on")]
    public List<Transform> GargoylePerches;
    [Tooltip("Spawn points for grunts enemies")]
    public List<Transform> GruntSpawns;
    [Tooltip("Number of grunts to spawn per round")]
    public List<int> GruntCount;
    [Tooltip("Beat offset of first round. Use negative value to start conductor using script.")]
    public int BeatOffset;
    [Tooltip("Beats until gargoyles start moving")]
    public int GargoyleBeginBeat;
    [Tooltip("Audio that plays during the MarketPlace Encounter")]
    public AudioSource[] VoiceLines;
    [Tooltip("Entrace to MarketPlace Encounter")]
    public GameObject Entrance;
    [Tooltip("Exit to MarketPlace Encounter")]
    public GameObject Exit;

    int FirstBeat;

    float[] mInitialAngles;
    float mAccumulatedAngle = 0f;
    Gargoyle[] mGargoyles;

    int mNumGargoyles;
    int mNumGrunts;

    int mNumGargoylesKilled = 0;
    int mNumGargoylesSpawned = 0;
    int mNumGruntsKilled = 0;
    int mNumGruntsSpawned = 0;
    int mRound = 0;
    int mVoiceLineIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        //if (BeatOffset < 0)
        //{
        //    FirstBeat = int.MaxValue;
        //}
        //FirstBeat = BeatOffset;

        mNumGargoyles = GargoylePerches.Count;
        mGargoyles = new Gargoyle[mNumGargoyles];
        mInitialAngles = new float[mNumGargoyles];
        // Get angles around center of gargoyle perches
        for (int i = 0; i < mNumGargoyles; i++)
        {
            Vector2 toPerch = new Vector2(GargoylePerches[i].position.x - transform.position.x, GargoylePerches[i].position.z - transform.position.z);
            float angle = Mathf.Deg2Rad * Vector2.Angle(toPerch, new Vector2(0f, 1f));
            mInitialAngles[i] = toPerch.x > 0 ? angle : -angle;
        }
    }

    void Update()
    {
        if (mRound % 2 == 0)
        {
            GargoyleRound();
        }
        else
        {
            GruntRound();
        }

        if (mRound == 5)
        {
            Exit?.SetActive(false);
            Entrance?.SetActive(false);
        }
    }

    void GargoyleRound()
    {
        int crrtBeat = Conductor.GetBeat() - FirstBeat;

        // Return early if round has not begun
        if (crrtBeat < 0)
        {
            return;
        }
        // Spawn enemies on first few beats
        else if (mNumGargoylesSpawned != mNumGargoyles && crrtBeat == mNumGargoylesSpawned)
        {
            // Play VoicesLine When Gargoyles Spawn (But Only Once)
            if (mVoiceLineIndex == 0 || mVoiceLineIndex == 2 || mVoiceLineIndex == 4)
            {
                VoiceLines[mVoiceLineIndex++].Play();
            }

            Gargoyle gargoyle = mGargoyles[crrtBeat] = Instantiate(Gargoyle, GargoylePerches[crrtBeat].position, Quaternion.identity);
            gargoyle.Conductor = Conductor;
            gargoyle.Target = Player;
            gargoyle.FireBeat = crrtBeat;
            gargoyle.HealthController.OnDeath += IncrementGargoyleKillCount;
            mNumGargoylesSpawned += 1;
        }
        // Orchestrate gargoyle dance once ready
        else if (crrtBeat >= GargoyleBeginBeat)
        {
            int danceBeat = crrtBeat - GargoyleBeginBeat;
            int danceBar = danceBeat / 4;
            int barBeat = danceBeat % 4;
            // Gargoyles swoop down for 2 bars every 6 bars. We assume that it takes 2 bars for all swoops to end.
            if ((danceBar / 2) % 3 == 1)
            {
                if (danceBar % 2 == 0 && barBeat < mNumGargoyles && mGargoyles[barBeat] != null)
                {
                    Gargoyle gargoyle = mGargoyles[barBeat];
                    gargoyle.StartSwoop(transform.position);
                    gargoyle.Destination = Vector3.zero;
                }
            }
            // They dance the rest of the time.
            else
            {
                mAccumulatedAngle += Time.deltaTime;
                int numSwoops = (danceBar + 4) / 6;
                float angleSwing = 0.2f * Mathf.Sin(mAccumulatedAngle * 2f);
                for (int i = 0; i < mNumGargoylesSpawned; i++)
                {
                    float targetAngle = mInitialAngles[i] + numSwoops * Mathf.PI + 0.2f * mAccumulatedAngle + angleSwing;
                    Gargoyle gargoyle = mGargoyles[i];
                    gargoyle.Destination = transform.position + new Vector3(Mathf.Sin(targetAngle) * 20f, 5f, Mathf.Cos(targetAngle) * 20f);
                }
            }
        }
    }

    void GruntRound()
    {
        int crrtBeat = Conductor.GetBeat() - FirstBeat;
        // Return early if round has not begun
        if (crrtBeat < 0)
        {
            return;
        }
        // Spawn grunts periodically if round has begun
        if (mNumGruntsSpawned != mNumGrunts && crrtBeat == mNumGruntsSpawned)
        {
            // Play VoicesLine When Grunts Spawn (But Only Once)
            if (mVoiceLineIndex == 1 || mVoiceLineIndex == 3 || mVoiceLineIndex == 5)
            {
                VoiceLines[mVoiceLineIndex++].Play();
            }
            MeleeEnemy enemy = Instantiate(Grunt, GruntSpawns[mNumGruntsSpawned % GruntSpawns.Count].position, Quaternion.identity);
            enemy.Target = Player;
            enemy.HealthController.OnDeath += IncrementGruntKillCount;
            mNumGruntsSpawned += 1;
        }
    }

    /// <summary> Increase enemies killed by one. Update game state appropriately. </summary>
    void IncrementGargoyleKillCount()
    {
        mNumGargoylesKilled += 1;
        // Ready next round if all gargoyles are dead.
        if (mNumGargoylesKilled == mNumGargoyles)
        {
            FirstBeat = ((Conductor.GetBeat() / 4) + 2) * 4;
            mNumGrunts = GruntCount[mRound/2];
            mRound += 1;

            mNumGargoylesSpawned = mNumGargoylesKilled = 0;
        }
    }

    /// <summary> Increase enemies killed by one. Update game state appropriately. </summary>
    void IncrementGruntKillCount()
    {
        mNumGruntsKilled += 1;
        // Ready next round if all grunts are dead.
        if (mNumGruntsKilled == mNumGrunts)
        {
            if (mRound/2 == GruntCount.Count - 1)
            {
                Destroy(gameObject);
            }
            FirstBeat = ((Conductor.GetBeat() / 4) + 2) * 4;
            mRound += 1;

            mNumGruntsSpawned = mNumGruntsKilled = 0;
        }
    }

    /// <summary> If BeatOffset is set to some negative value, conductor will wait for this function to be called with a future beat to start. </summary>
    public void StartConductor(int startBeat)
    {
        FirstBeat = startBeat;
    }
}
