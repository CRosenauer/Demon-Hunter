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
    [Space]
    
    [SerializeField] float m_moveSpeed;
    [SerializeField] float m_timeBetweenStates;
    
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
        int i = 0;
        foreach (GameObject spawnPoint in m_skeletonSpawnLocations)
        {
            Vector3 position = spawnPoint.transform.position;
            position.x = position.x + Random.Range(-m_skeletonSpawnLocationRange, m_skeletonSpawnLocationRange);

            StartCoroutine(SpawnCoroutine(m_skeletonObject, position, i++ * m_skeletonSpawnDelayStep));
        }
    }

    IEnumerator SpawnCoroutine(GameObject go, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(Random.Range(0f, delay));
        Instantiate(go, position, Quaternion.identity, transform.parent);
    }

    void OnSpawnSkeletons()
    {
        OnExitSpawnSkeletons();
        OnEnterMoveToLocation();
        m_state = GrandCultistState.moveToLocation;
    }

    void OnExitSpawnSkeletons()
    {

    }

    void OnEnterSpawnProjectiles()
    {
        foreach (GameObject location in m_projectileSpawnLocations)
        {
            StartCoroutine(SpawnProjectilesCoroutine(location.transform.position));
        }
    }

    IEnumerator SpawnProjectilesCoroutine(Vector2 waitPosition)
    {
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
    }

    void OnSpawnProjectiles()
    {
        OnExitSpawnProjectiles();
        OnEnterMoveToLocation();
        m_state = GrandCultistState.moveToLocation;
    }

    void OnExitSpawnProjectiles()
    {

    }

    void OnEnterVulnerable()
    {
        m_vulerableHitCount = 0;
        Move(Vector2.down * m_moveSpeed);
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
    }

    void OnExitVulnerable()
    {

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
        if(m_stateTimerCoroutine != null)
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
        Move(Vector2.down * m_moveSpeed);
    }

    void OnDead()
    {

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
        Vector2 thisPosition = transform.position;
        Vector2 targetPosition = m_hoverPoints[m_moveLocationIndex].transform.position;

        Vector2 displacement = targetPosition - thisPosition;
        Vector2 direction = displacement.normalized;

        Vector2 movement = direction * m_moveSpeed;

        Vector2 postMoveDistplacement = targetPosition - (thisPosition + movement * Time.fixedDeltaTime);

        // moved past the target
        if(Vector2.Dot(postMoveDistplacement, displacement) <= 0f)
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

    Coroutine m_stateTimerCoroutine;

    GrandCultistState m_nextState;

    int m_vulerableHitCount;
    int m_moveLocationIndex;

    [SerializeField] GrandCultistState m_state;
}
