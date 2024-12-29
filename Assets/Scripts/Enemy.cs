// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Enemy : Character
{
    protected override void Start()
    {
        base.Start();

        base.skills = new Skill[4];

        base.skills[0] = new Axe(this);
        base.skills[1] = new Axe(this);
        base.skills[2] = new Axe(this);
        base.skills[3] = new Axe(this);
    }
}
