// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Adventurer : Character
{
    protected override void Start()
    {
        base.Start();

        base.skills = new Skill[4];

        base.skills[0] = new ElectricStorm(this);
        base.skills[1] = new ElectricStorm(this);
        base.skills[2] = new ElectricStorm(this);
        base.skills[3] = new ElectricStorm(this);
    }
}
