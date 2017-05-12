using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class HumanMilitia : IUnit
{
    public HumanMilitia() : base("Human Militia", 18.0f)
    {
        a_strenght = new Attribute(3, 100);
        a_agility = new Attribute(3, 100);
        a_toughness = new Attribute(3, 100);

        s_combat = 3.0f;
        s_raiding = 1.0f;
        s_defender = 4.0f;
        s_social = 5.0f;
        s_gatherer = 3.0f;
        s_crafter = 3.0f;

        //Add relevant body parts
        b_parts = new List<BodyPart>();
        BodyPart body = new BodyPart("Body", 0.5f, BodyPart.BodyPartType.BODY);
        body.AddVital();
        b_parts.Add(body);

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
        Weapon sword = new Weapon("Short Sword", 10, 7, 7, false, Weapon.Range.MELEE);
        sword.perkTriggers.Add(PerkCondition.SWORD);
        EquipWeapon(sword);

        Armor bodyA = new Armor("Chain Mail", 100, 25, BodyPart.BodyPartType.BODY);
        Armor glove1 = new Armor("Chain Mail Glove", 100, 25, BodyPart.BodyPartType.ARMS);
        Armor glove2 = new Armor("Chain Mail Glove", 100, 25, BodyPart.BodyPartType.ARMS);
        Armor boot1 = new Armor("Iron Boot", 100, 25, BodyPart.BodyPartType.LEGS);
        Armor boot2 = new Armor("Iron Boot", 100, 25, BodyPart.BodyPartType.LEGS);

        EquipArmor(bodyA);
        EquipArmor(glove1);
        EquipArmor(glove2);
        EquipArmor(boot1);
        EquipArmor(boot2);

        //Order body parts by size
        b_parts = b_parts.OrderByDescending(x => x.size).ToList();
    }
}

