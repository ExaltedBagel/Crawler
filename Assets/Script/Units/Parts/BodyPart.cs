using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Defines a body part along with functionnalities, armor and possession.
/// </summary>
public class BodyPart
{
    public BodyPart(string name, float size, BodyPartType type)
    {
        //Basic things
        this.name = name;
        this.size = size;
        this.type = type;

        //Items and armor
        this.heldItem       = null;
        this.naturalAttack  = null;
        this.naturalArmor   = null;
        this.wornArmor      = null;

        //Functions and wounds
        partFunctions   = new HashSet<BodyPartFunction>();
        mayHold         = new HashSet<ItemSlot>();
        wound           = new Wound();
    }

    public void AddOffence(float minDmg, float maxDmg, Weapon.Range range = Weapon.Range.MELEE)
    {
        partFunctions.Add(BodyPartFunction.OFFENSE);
        naturalAttack = new Weapon(name, 0, minDmg, maxDmg, true, range);
        naturalAttack.perkTriggers.Add(PerkCondition.NATURAL);
    }

    public void AddMove()
    {
        partFunctions.Add(BodyPartFunction.MOVE);
    }

    public void AddVital()
    {
        partFunctions.Add(BodyPartFunction.VITAL);
    }

    public void AddNaturalArmor(Armor naturalArmor)
    {
        this.naturalArmor = naturalArmor;
    }

    public float GetTotalArmor()
    {
        float value = 0.0f;
        if (naturalArmor != null)
            value += naturalArmor.protection;
        if (wornArmor != null)
            value += wornArmor.protection;
        return value;
    }

    public void EquipArmor(Armor armor)
    {
        if(armor.type.Equals(this.type) && !wound.gravity.Equals(Wound.WoundGravity.REMOVED))
        {
            wornArmor = armor;
        }
        else
        {
            //Cannot wear armor there
        }
    }

    public float BodyPartAtkValue()
    {
        if (partFunctions.Contains(BodyPartFunction.OFFENSE) && (int)wound.gravity < (int)Wound.WoundGravity.BROKEN)
        {
            if (heldItem == null)
                return naturalAttack.minDmg + naturalAttack.maxDmg;
            else
                return ((Weapon)heldItem).minDmg + ((Weapon)heldItem).maxDmg;
        }
        else
            return 0.0f;
        
    }

    public void GetBodyMinMax(out float min, out float max)
    {
        if(heldItem == null)
        {
            min = naturalAttack.minDmg;
            max = naturalAttack.maxDmg;
        }
        else
        {
            min = ((Weapon)heldItem).minDmg;
            max = ((Weapon)heldItem).maxDmg;
        }
    }

    public string name  { get; }
    public float size { get; }
    public BodyPartType type { get; }
    public HashSet<BodyPartFunction> partFunctions { get; }
    public HashSet<ItemSlot> mayHold { get; }
    public IItem heldItem { get; set; }
    public Weapon naturalAttack { get; set; }

    public Armor naturalArmor { get; set; }
    public Armor wornArmor { get; set; }

    public Wound wound { get; set; }
    
    public enum BodyPartFunction
    {
        OFFENSE,
        MOVE,
        VITAL
    }

    public enum BodyPartType
    {
        ARMS,
        LEGS,
        BODY,
        HEAD,
        ORGAN,
        WING,
        OTHER
    }
}

