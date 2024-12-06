// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Enemy : Character
{
    private List<Skill> preparedSkills = new List<Skill>();
    public List<Skill> PreparedSkills => preparedSkills;
}
