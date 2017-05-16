using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Goblin : IUnit
{
    public Goblin() : base("Goblin", 12.0f)
    {
        a_strenght = new Attribute(2, 110);
        a_agility = new Attribute(4, 90);
        a_toughness = new Attribute(2, 110);

        s_combat    = 3.0f;
        s_raiding   = 3.0f;
        s_defender  = 3.0f;
        s_social    = 3.0f;
        s_gatherer  = 3.0f;
        s_crafter   = 3.0f;        

        //Add relevant body parts
        b_parts = new List<BodyPart>();
        BodyPart arm1 = new BodyPart("Right Arm", 0.125f, BodyPart.BodyPartType.ARMS);
        arm1.AddOffence(1, 2, Weapon.Range.MELEE);
        arm1.mayHold.Add(ItemSlot.HAND);
        BodyPart arm2 = new BodyPart("Left Arm", 0.125f, BodyPart.BodyPartType.ARMS);
        arm1.AddOffence(1, 2, Weapon.Range.MELEE);
        arm2.mayHold.Add(ItemSlot.HAND);
        b_parts.Add(arm1);
        b_parts.Add(arm2);

        BodyPart leg1 = new BodyPart("Right Leg", 0.125f, BodyPart.BodyPartType.LEGS);
        BodyPart leg2 = new BodyPart("Left Leg", 0.125f, BodyPart.BodyPartType.LEGS);
        arm1.AddOffence(1, 2, Weapon.Range.MELEE);
        arm1.AddOffence(1, 2, Weapon.Range.MELEE);
        leg1.AddMove();
        leg2.AddMove();
        b_parts.Add(leg1);
        b_parts.Add(leg2);

        BodyPart head = new BodyPart("Head", 0.125f, BodyPart.BodyPartType.HEAD);
        head.AddVital();
        b_parts.Add(head);

        //Add equipment
        Weapon spear = new Weapon("Wood Spear", 10, 5, 10, false, Weapon.Range.MELEE);
        spear.perkTriggers.Add(PerkCondition.SPEAR);
        b_parts[0].heldItem = spear;
    }

    void Update()
    {
        /*
        if (m_navAgent.hasPath && m_state.Equals(State.MOVING))
        {
            m_anim.SetInteger("moving", 2);
            m_anim.SetInteger("battle", 1);
        }
        */
    }
}

