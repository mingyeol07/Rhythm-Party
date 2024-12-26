// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public enum NoteType
{
    None,
    Short,
    Long,
}

public class Note : MonoBehaviour
{
    public Arrow Dir;
    public NoteType Type;
    public int NoteTime;

    public Note(Arrow dir, NoteType type, int noteTime = 1)
    {
        Dir = dir;
        Type = type;
        NoteTime = noteTime;
    }
}
