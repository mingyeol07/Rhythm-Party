// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Adventurer : Character
{
    private void Awake()
    {
        skills = new Skill[4];

        skills[0] = new ElectricStorm(this);
        skills[1] = new ElectricStorm(this);
        skills[2] = new ElectricStorm(this);
        skills[3] = new ElectricStorm(this);
    }
}
