using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandCultistComponent : EnemyComponent
{
    [SerializeField] List<GameObject> m_skeletonSpawnLocations;
    [SerializeField] GameObject m_skeletonObject;
    [SerializeField] float m_skeletonSpawnLocationRange;
    [SerializeField] float m_skeletonSpawnDelayStep;
    [Space]

    [SerializeField] List<GameObject> m_projectileSpawnLocations;
    [SerializeField] GameObject m_projectileObject;
    [SerializeField] float m_projectileSpawnDelay;
    [Space]

    [SerializeField] List<GameObject> m_hoverPoints;
    [SerializeField] GameObject m_vulnerablePoint;
    [Space]
    
    [SerializeField] float m_moveSpeed;
    [SerializeField] float m_timeBetweenStates;
    [Space]

    [SerializeField] GameObject m_exitCutscene;
    [Space]
    [SerializeField] GameObject m_landSFX;
    
    enum GrandCultistState
    {
        waitForActivation,
        spawnSkeletons,
        spawnProjectiles,
        vulnerable,
        moveToLocation,
        dead
    }

    new void Start()
    {
        base.Start();

        m_state = GrandCultistState.waitForActivation;
        m_nextState = GrandCultistState.spawnProjectiles;
        OnEnterWaitForActivation();

        for(int i = 0; i < m_skeletonSpawnLocations.Count; ++i)
        {
            m_spawnSkeletonCoroutine.Add(null);
        }

        for (int i = 0; i < m_projectileSpawnLocations.Count; ++i)
        {
            m_spawnProjectileCoroutine.Add(null);
        }
    }

    void FixedUpdate()
    {
        QueryOnGround();
        QueryDirectionToPlayer();

        switch (m_state)
        {
            case GrandCultistState.waitForActivation:
                OnWaitForActivation();
                break;
            case GrandCultistState.spawnSkeletons:
                OnSpawnSkeletons();
                break;
            case GrandCultistState.spawnProjectiles:
                OnSpawnProjectiles();
                break;
            case GrandCultistState.vulnerable:
                OnVulnerable();
                break;
            case GrandCultistState.dead:
                OnDead();
                break;
            case GrandCultistState.moveToLocation:
                OnMoveToLocation();
                break;
        }
    }

    void Activate()
    {
        OnExitWaitForActivation();
        m_state = GrandCultistState.moveToLocation;
        OnEnterMoveToLocation();
    }

    void OnEnterWaitForActivation()
    {

    }

    void OnWaitForActivation()
    {

    }

    void OnExitWaitForActivation()
    {

    }

    void OnEnterSpawnSkeletons()
    {
        Animator.SetTrigger("Summon");

        for(int i = 0; i < m_skeletonSpawnLocations.Count; ++i)
        {
            GameObject spawnPoint = m_skeletonSpawnLocations[i];

            Vector3 position = spawnPoint.transform.position;
            position.x = position.x + Random.Range(-m_skeletonSpawnLocationRange, m_skeletonSpawnLocationRange);

            m_spawnSkeletonCoroutine[i] = StartCoroutine(SpawnCoroutine(m_skeletonObject, position, i * m_skeletonSpawnDelayStep, i));
        }
    }

    IEnumerator SpawnCoroutine(GameObject go, Vector3 position, float delay, int index)
    {
        yield return new WaitForSeconds(0.33f); // delay for animation
        yield return new WaitForSeconds(Random.Range(0f, delay));
        Instantiate(go, position, Quaternion.identity, transform.parent);

        m_spawnSkeletonCoroutine[index] = null;
    }

    void OnSpawnSkeletons()
    {
        OnExitSpawnSkeletons();
        OnEnterMoveToLocation();
        m_state = GrandCultistState.moveToLocation;
    }

    void OnExitSpawnSkeletons()
    {
        Animator.ResetTrigger("Summon");
    }

    void OnEnterSpawnProjectiles()
    {
        Animator.SetTrigger("Summon");

        for(int i = 0; i < m_projectileSpawnLocations.Count; ++i)
        {
            GameObject location = m_projectileSpawnLocations[i];
            m_spawnProjectileCoroutine[i] = StartCoroutine(SpawnProjectilesCoroutine(location.transform.position, i));
        }
    }

    IEnumerator SpawnProjectilesCoroutine(Vector2 waitPosition, int index)
    {
        yield return new WaitForSeconds(0.33f); // delay for animation
        GameObject energyBall = Instantiate(m_projectileObject, transform.position, Quaternion.identity, transform.parent);
        Vector2 direction = waitPosition - (Vector2) energyBall.transform.position;
        direction.Normalize();

        energyBall.SendMessage("OnSetDirection", direction);

        while(true)
        {
            Vector2 ballPosition = energyBall.transform.position;
            Vector2 distanceToPosition = waitPosition - ballPosition;

            if(Vector2.Dot(direction, distanceToPosition) < 0f)
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        energyBall.SendMessage("OnSetDirection", Vector2.zero);
        yield return new WaitForSeconds(m_projectileSpawnDelay);

        Vector2 playerPosition = m_player.transform.position;
        Vector2 directionToPlayer = playerPosition - (Vector2) energyBall.transform.position;
        directionToPlayer.Normalize();

        energyBall.SendMessage("OnSetDirection", directionToPlayer);

        m_spawnProjectileCoroutine[index] = null;
    }

    void OnSpawnProjectiles()
    {
        OnExitSpawnProjectiles();
        OnEnterMoveToLocation();
        m_state = GrandCultistState.moveToLocation;
    }

    void OnExitSpawnProjectiles()
    {
        Animator.ResetTrigger("Summon");
    }

    void OnEnterVulnerable()
    {
        Animator.SetTrigger("Weak");
        Animator.ResetTrigger("Unweak");
        m_vulerableHitCount = 0;
        if(m_stateTimerCoroutine != null)
        {
            StopCoroutine(m_stateTimerCoroutine);
            m_stateTimerCoroutine = StartCoroutine(VulerableStateTimer());
        }
    }

    void OnVulnerable()
    {
        if(m_vulerableHitCount >= 4)
        {
            if(m_stateTimerCoroutine != null)
            {
                StopCoroutine(m_stateTimerCoroutine);
            }

            ExitVulerableState();
        }

        MoveToTarget(m_vulnerablePoint.transform.position);
    }

    void OnExitVulnerable()
    {
        Animator.SetTrigger("Unweak");
        Animator.ResetTrigger("Weak");
    }

    void ExitVulerableState()
    {
        OnExitVulnerable();
        m_state = GrandCultistState.moveToLocation;
        OnEnterMoveToLocation();
    }

    void OnHit(int damage)
    {
        m_vulerableHitCount++;
    }

    IEnumerator VulerableStateTimer()
    {
        yield return new WaitForSeconds(m_timeBetweenStates);
        ExitVulerableState();
    }

    void OnDeath()
    {
        if (m_stateTimerCoroutine != null)
        {
            StopCoroutine(m_stateTimerCoroutine);
        }
        
        switch(m_state)
        {
            case GrandCultistState.spawnSkeletons:
                OnExitSpawnSkeletons();
                break;
            case GrandCultistState.spawnProjectiles:
                OnExitSpawnProjectiles();
                break;
            case GrandCultistState.vulnerable:
                OnExitVulnerable();
                break;
            default:
                break;
        }

        OnEnterDead();
        m_state = GrandCultistState.dead;
    }

    void OnDeathAnimationEnd()
    {

    }

    void OnEnterDead()
    {
        ApplyScore();

        for (int i = 0; i < m_spawnSkeletonCoroutine.Count; ++i)
        {
            if(m_spawnSkeletonCoroutine[i] == null)
            {
                continue;
            }

            StopCoroutine(m_spawnSkeletonCoroutine[i]);
            m_spawnSkeletonCoroutine[i] = null;
        }

        for (int i = 0; i < m_spawnProjectileCoroutine.Count; ++i)
        {
            if (m_spawnProjectileCoroutine[i] == null)
            {
                continue;
            }

            StopCoroutine(m_spawnProjectileCoroutine[i]);
            m_spawnProjectileCoroutine[i] = null;
        }

        PersistentHitboxComponent persistentHitboxComponent = GetComponent<PersistentHitboxComponent>();
        persistentHitboxComponent.SetActive(false, true);

        Rigidbody2D rbody = GetComponent<Rigidbody2D>();
        rbody.gravityScale = 1f;
        Move(9f * Vector2.up);

        Animator.SetTrigger("Death");

        GameObject systems = GameObject.Find("Systems");
        AudioSource musicSource = systems.GetComponent<AudioSource>();
        musicSource.Stop();
    }

    void OnDead()
    {
        if (IsOnGround())
        {
            Animator.SetTrigger("DeathBounce");

            if(m_landSFX)
            {
                AudioSource landSFXSource = m_landSFX.GetComponent<AudioSource>();
                landSFXSource.Play();
                m_landSFX = null;
            }

            m_state = GrandCultistState.waitForActivation;
        }
    }

    void OnExitDead()
    {

    }

    IEnumerator ExitMoveTimerCoroutine()
    {
        yield return new WaitForSeconds(m_timeBetweenStates);
        
        switch(m_state)
        {
            case GrandCultistState.spawnSkeletons:
                OnExitSpawnSkeletons();
                break;
            case GrandCultistState.spawnProjectiles:
                OnExitSpawnProjectiles();
                break;
            case GrandCultistState.vulnerable:
                OnExitVulnerable();
                break;
            case GrandCultistState.dead:
                m_nextState = GrandCultistState.dead;
                break;
            default:
                break;
        }

        m_state = m_nextState;

        switch (m_nextState)
        {
            case GrandCultistState.spawnSkeletons:
                OnEnterSpawnSkeletons();
                m_nextState = GrandCultistState.vulnerable;
                break;
            case GrandCultistState.spawnProjectiles:
                OnEnterSpawnProjectiles();
                m_nextState = GrandCultistState.spawnSkeletons;
                break;
            case GrandCultistState.vulnerable:
                OnEnterVulnerable();
                m_nextState = GrandCultistState.spawnProjectiles;
                break;
            default:
                break;
        }
    }

    void OnEnterMoveToLocation()
    {
        if(m_stateTimerCoroutine != null)
        {
            StopCoroutine(m_stateTimerCoroutine);
        }
        
        m_stateTimerCoroutine = StartCoroutine(ExitMoveTimerCoroutine());
    }

    void OnMoveToLocation()
    {
        Vector2 targetPosition = m_hoverPoints[m_moveLocationIndex].transform.position;
        MoveToTarget(targetPosition);
    }

    void MoveToTarget(Vector2 targetPosition)
    {
        Vector2 thisPosition = transform.position;

        Vector2 displacement = targetPosition - thisPosition;
        Vector2 direction = displacement.normalized;

        Vector2 movement = direction * m_moveSpeed;

        Vector2 postMoveDistplacement = targetPosition - (thisPosition + movement * Time.fixedDeltaTime);

        // moved past the target
        if (Vector2.Dot(postMoveDistplacement, displacement) <= 0f)
        {
            transform.position = targetPosition;
            m_moveLocationIndex = (m_moveLocationIndex + 1) % m_hoverPoints.Count;
            Move(Vector2.zero);
        }
        else
        {
            Move(movement);
        }
    }

    void OnExitMoveToLocation()
    {

    }

    public void ExitCutscene()
    {
        m_exitCutscene.SendMessage("PlayCutscene");
    }

    List<Coroutine> m_spawnSkeletonCoroutine = new();
    List<Coroutine> m_spawnProjectileCoroutine = new();

    Coroutine m_stateTimerCoroutine;

    GrandCultistState m_nextState;

    int m_vulerableHitCount;
    int m_moveLocationIndex;

    [SerializeField] GrandCultistState m_state;
}
