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
        p_perks = new HashSet<IPerk>();
        r_relationships = new Dictionary<IUnit, Relationship>();
        m_navAgent = GetComponent<NavMeshAgent>();
        m_anim = GetComponent<Animator>();
        m_taskQueue = new Queue<IJob>();
    }

    void Start()
    {
        StartCoroutine(GameTick());
        m_anim.SetInteger("moving", 0);
        m_anim.SetInteger("battle", 1);
    }

    //Used to identify and recognize the unit
    public string m_name { get; set; }
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
    public IJob CurrentJob { get; set; }
    public Queue<IJob> m_taskQueue { get; set; } 
    Bed m_bed { get; set; }
    public Bed m_Bed { get { return m_bed; } }
    public State m_state { get; set; }
    public delegate void m_EndPathAction(IUnit unit);
    m_EndPathAction m_endPathAction;
    private float m_updateTick = 1.0f;

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

    public void JobFinished()
    {
        StopCoroutine(GameTick());
        var job = m_taskQueue.Dequeue();
        job.OnFinished(this);
        if (m_taskQueue.Count > 0)
        {
            m_state = IUnit.State.IDLE;
            CurrentJob = null;
            //CurrentJob.OnStart(this);
        }
        else
        {
            var newJob = job.GetAdjacentJob();
            if (newJob == null)
            {
                m_state = IUnit.State.IDLE;
                CurrentJob = null;
            }
            else if(newJob.CanAssignUnit())
            {
                newJob.AssignUnit(this);
                CurrentJob = null;
            }
        }
        StartCoroutine(GameTick());
    }

    public bool HasAJob()
    {
        if (m_taskQueue.Count == 1)
            return !m_taskQueue.Peek().GetType().Equals(typeof(JobMoveTo));
        else
            return m_taskQueue.Count > 0;
    }

    public void ClearJobs()
    {
        StopCoroutine(GameTick());
        while (m_taskQueue.Count > 0)
        {
            var task = m_taskQueue.Dequeue();
            if(!task.Personnal)
                JobManager.JobWasCleared(task);
        }
        m_state = IUnit.State.IDLE;
        StartCoroutine(GameTick());
    }

    public void LookForAJob()
    {
        JobManager.FindJobForUnit(this);
    }

    public void SignalJobProgress()
    {
        JobManager.UpdateJobProgress(this);                      
    }

    public virtual void PlayActionAnimation()
    {
        m_anim.SetInteger("battle", 1);
        m_anim.SetInteger("moving", 6);
    }

    public void GoToSleep()
    {
        // Go to bed location
        if (m_bed == null)
            FindABed();
        JobSleep sleep;
        if (m_bed == null)
        {
            var tile = Tile.TileAtPosition(transform.position);
            Debug.Log("Sleeping on the floor");
            sleep = new JobSleep(tile.Coord.x, tile.Coord.z, tile.Level, null);
            sleep.AssignUnit(this);
        }
        else
        {
            sleep = new JobSleep(m_bed.location.x, m_bed.location.z, m_bed.Level, m_bed);
            sleep.AssignUnit(this);
        }
    }

    public void WakeUp()
    {
        m_state = State.IDLE;
        m_anim.SetInteger("battle", 1);
        m_anim.SetInteger("moving", 1);
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
    public void FallAsleep()
    {
        //If his plan was sleeping, do this here
        m_state = State.SLEEP;
        if(m_bed)
            m_bed.Use(this);
        m_anim.SetInteger("battle", 1);
        m_anim.SetInteger("moving", 12);
    }

    public static void StartTheJob(IUnit unit)
    {
        
    }

    private IEnumerator GameTick()
    {
        while(true)
        {
            yield return new WaitForSeconds(m_updateTick);
            if (m_taskQueue.Count > 0)
            {
                if (CurrentJob == null)
                {
                    CurrentJob = m_taskQueue.Peek();
                    CurrentJob.OnStart(this);
                }
                else
                {
                    m_taskQueue.Peek().OnUpdate(this);
                }
            }
            else
            {
                LookForAJob();
            }
        }        
    }

    public enum State
    {
        IDLE,
        MOVING,
        WORKING,
        FINISHED,
        SLEEP,
        ERROR
    }


    #endregion

    

    void Update()
    {      
        
    }
}
