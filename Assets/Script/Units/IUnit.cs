using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Interface that emcompasses the basic layout of any unit described in the game.
/// </summary>
public abstract class IUnit : MonoBehaviour {

    public IUnit(string name, float age)
    {
        this.m_name = name;
        this.m_age = age;
    }

    void Awake()
    {
        this.isAlive = true;
        this.isHarmless = false;
        
        this.m_currentJob = null;
        p_perks = new HashSet<IPerk>();
        r_relationships = new Dictionary<IUnit, Relationship>();
        m_navAgent = GetComponent<NavMeshAgent>();
        m_anim = GetComponent<Animator>();
    }

    //Used to identify and recognize the unit
    public string m_name         { get; set; }
    public float m_age { get; set; }

    //Attributes of the unit - Those change slowly overtime and influence many fields
    public Attribute a_strenght    { get; set; }
    public Attribute a_agility     { get; set; }
    public Attribute a_toughness   { get; set; }
    public bool isAlive            { get; set; }
    public bool isHarmless         { get; set; }

    //Skills of the unit - Those are trained as they are used. Are inflenced by perks
    public float s_combat      { get; set; }
    public float s_raiding     { get; set; }
    public float s_defender    { get; set; }
    public float s_social      { get; set; }
    public float s_gatherer    { get; set; }
    public float s_crafter     { get; set; }

    //Perks the unit has acquired
    public HashSet<IPerk> p_perks { get; set; }

    //Relationships of the unit, and allegiance to squad
    public Dictionary<IUnit, Relationship> r_relationships { get; set; }

    //Bodyparts and other
    public List<BodyPart> b_parts { get; set; }

    //Combat stuff
    public float c_initiative;

    //Management layer
    public NavMeshAgent m_navAgent { get; set; }
    public Animator m_anim { get; set; }
    public IJob m_currentJob { get; set; }
    Bed m_bed { get; set; }
    protected State m_state;
    public delegate void m_EndPathAction(IUnit unit);
    m_EndPathAction m_endPathAction;

    #region Combat

    //Functions to ease equiping
    public void EquipWeapon(Weapon weapon)
    {
        BodyPart target = null;
        //Find first place to put weapon
        foreach (BodyPart x in b_parts)
        {
            if (x.mayHold.Contains(ItemSlot.HAND) && (int)x.wound.gravity < (int)Wound.WoundGravity.BROKEN)
            {
                if (x.heldItem == null)
                {
                    target = x;
                    break;
                }
                else if (target == null)
                {
                    target = x;
                    break;
                }                
            }
        }

        if (target != null)
        {
            target.heldItem = weapon;
            //Debug.Log(weapon.name + " was put on " + target.name);
        }
        else
        {
            Debug.Log("Nowhere to put this weapon");
        }
    }

    public void EquipArmor(Armor armor)
    {
        BodyPart target = null;
        //Find optimal place to put armor
        foreach (BodyPart x in b_parts)
        {
            if(x.type.Equals(armor.type))
            {
                if (x.wornArmor == null)
                    target = x;
                else if (target == null)
                    target = x;
                else
                {
                    if(target.wornArmor != null)
                        if (target.wornArmor.protection > x.wornArmor.protection)
                            target = x;
                }
            }
        }

        if (target != null)
        {
            target.wornArmor = armor;
            //Debug.Log(armor.name + " was put on " + target.name);
        }
        else
        {
            Debug.Log("Nowhere to put this armor");
        }
    }

    public void RollInitiative(IUnit leader)
    {
        float max = a_agility.currValue + s_combat;
        if (leader.isAlive)
            max += (leader.s_combat / 2);
        c_initiative = Random.Range(0, max);
    }

    public bool CheckIfHarmless()
    {
        bool isHarmless = GameMath.ChooseAttackOption(this, null) == null;
        this.isHarmless = isHarmless;
        return isHarmless;
    }

    #endregion

    #region Management
    //Management movement
    public void SetDestination(Vector3 destination, m_EndPathAction endPathAction, bool onlyCenterDestination = false)
    {
        NavMeshPath path = new NavMeshPath();
        
        for (int i = 0; i < 5; i++)
        {
            var nextDest = GetNextSquarePosition(destination, i);
            m_navAgent.CalculatePath(nextDest, path);

            //Check if square is reachable
            if (path.status.Equals(NavMeshPathStatus.PathPartial) || path.status.Equals(NavMeshPathStatus.PathInvalid))
            {
                if(onlyCenterDestination)
                {
                    Debug.Log("Center is not reachable");
                    CancelInvoke("CheckPathStatus");
                    ClearJob();
                    return;
                }

                if (nextDest.Equals(new Vector3(-1, 0, 0)))
                {
                    CancelInvoke("CheckPathStatus");
                    ClearJob();
                    return;
                }
            }
            else
            {
                m_navAgent.SetPath(path);
                InvokeRepeating("CheckPathStatus", 0.2f, 0.2f);
                m_endPathAction = endPathAction;
                return;
            }
        }        
    }

    void DestinationReached()
    {
        //Stop observing path status
        CancelInvoke("CheckPathStatus");
        m_navAgent.ResetPath();
        m_endPathAction(this);        
    }

    public bool HasAJob()
    {
        return m_currentJob != null;
    }

    public void ClearJob()
    {
        if (m_currentJob == null)
        {
            //InvokeRepeating("LookForAJob", 0.5f, 2f);
            return;
        }

        //Signal the manager the job is being cleared
        JobManager.JobWasCleared(m_currentJob);

        m_currentJob = null;
        m_state = State.IDLE;
        m_navAgent.stoppingDistance = 0.1f;
        CancelInvoke("SignalJobProgress");

        InvokeRepeating("LookForAJob", 0.5f, 2f);
    }

    public void LookForAJob()
    {
        Debug.Log(m_state + " ");
        if(m_state.Equals(State.IDLE))
            JobManager.FindJobForUnit(this);
    }

    public void CheckPathStatus()
    {
        // Check if we've reached the destination
        if (!m_navAgent.pathPending)
        {            
            if (m_navAgent.remainingDistance <= m_navAgent.stoppingDistance)
            {
                if (!m_navAgent.hasPath || m_navAgent.velocity.sqrMagnitude == 0f)
                {
                    //m_state = State.IDLE;
                    DestinationReached();
                }
            }
        }
    }

    public void SignalJobProgress()
    {
        if (m_state.Equals(State.WORKING))
        {
            m_anim.SetInteger("moving", 6);
            JobManager.UpdateJobProgress(this);
        }                
    }

    public void GoToSleep()
    {
        ClearJob();
        CancelInvoke();
        //Go to bed location
        if (m_bed == null)
            FindABed();

        if(m_bed != null)
        {
            Debug.Log("Going to sleep at bed location");
            m_state = State.SLEEP;
            SetDestination(new Vector3(m_bed.location.x, 0, m_bed.location.z), FallAsleep, true);
        }
    }

    public void FindABed()
    {
        if(m_bed == null)
        {
            var list = IRoom.RoomList();

            foreach (IRoom x in list)
            {
                if(x.GetType().Equals(typeof(Quarter)))
                {
                    var bed = x.m_objectsContained.Find(y => y.GetType().Equals(typeof(Bed)) && (y.isPublic || y.m_owner == null)) as Bed;

                    if(bed != null)
                    {
                        bed.BeClaimedBy(this);
                        m_bed = bed;
                        return;
                    }
                }
            }
        }
    }

    //Actions to be done once the destination is reached
    public void FallAsleep(IUnit unit)
    {
        //If his plan was sleeping, do this here
        Debug.Log("Bed has been reached!");
        m_state = State.SLEEP;
        m_bed.Use(this);
        m_anim.SetInteger("moving", 12);
        return;
    }

    public static void StartTheJob(IUnit unit)
    {
        //Check if he can start his job
        if (unit.m_currentJob != null)
        {
            if (unit.m_currentJob.IsInRange(unit.transform.position))
            {
                unit.m_currentJob.state = IJob.State.PROGRESS;
                unit.m_state = State.WORKING;
                unit.InvokeRepeating("SignalJobProgress", 0.5f, 0.5f);
            }
            else
            {
                Debug.Log("Job not in range");
                unit.ClearJob();
            }
        }
    }

    protected enum State
    {
        IDLE,
        MOVING,
        WORKING,
        FINISHED,
        SLEEP,
        ERROR
    }


    #endregion

    public Vector3 GetNextSquarePosition(Vector3 baseDest, int iteration)
    {
        switch (iteration)
        {
            case 0:
                return new Vector3(baseDest.x, baseDest.y * -1.25f, baseDest.z);
                break;
            case 1:
                return new Vector3(baseDest.x - 0.75f, baseDest.y * -1.25f, baseDest.z);
                break;
            case 2:
                return new Vector3(baseDest.x, baseDest.y * -1.25f, baseDest.z + 0.75f);
                break;
            case 3:
                return new Vector3(baseDest.x + 0.75f, baseDest.y * -1.25f, baseDest.z);
                break;
            case 4:
                return new Vector3(baseDest.x, baseDest.y * -1.25f, baseDest.z - 0.75f);
                break;
            case 5:
                return new Vector3(-1, 0, 0);
                break;
            default:
                break;
        }
        return new Vector3(-1, 0, 0);
    }

    void Update()
    {      
        
    }


    

}
