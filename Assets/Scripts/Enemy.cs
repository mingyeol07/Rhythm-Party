// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Enemy : Character
{
    private void Awake()
    {
        skills = new Skill[4];

        skills[0] = new Axe(this);
        skills[1] = new Axe(this);
        skills[2] = new Axe(this);
        skills[3] = new Axe(this);
    }
}
