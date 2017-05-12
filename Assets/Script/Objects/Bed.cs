using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class Bed : IObject
{
    public override void BeClaimedBy(IUnit user)
    {
        //Can use object
        if (m_owner == null && !isPublic)
        {
            Debug.Log("This object has been claimed by " + user.m_name);
            m_owner = user;            
        }
        else
        {
            Debug.Log("This is object may not be claimed");
        }
    }

    public override void Use(IUnit user)
    {
        //Can use object
        if(m_owner == null || user.Equals(m_owner))
        {
            Debug.Log("This object has been used by " + user.m_name);
        }
        else
        {
            Debug.Log("This is not the owner, cannot use the object");
        }
    }
}

