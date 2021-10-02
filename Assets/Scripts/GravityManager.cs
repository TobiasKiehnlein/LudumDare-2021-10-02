using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager
{

    private static GravityManager _instance;

    public static GravityManager GeTInstance()
    {
         _instance ??=  new GravityManager();
        return _instance;
    }


    private Orientation _current = Orientation.Down;

    public Orientation GETCurrentOrientation()
    {
        return _current;
    }
    public void ChangeGravity(Orientation newOrientation)
    {
        if (_current == newOrientation) return;
        Physics2D.gravity = newOrientation switch
        {
            Orientation.Down => new Vector2(0, -9.8f),
            Orientation.Up => new Vector2(0, 9.8f),
            Orientation.Left => new Vector2(-9.8f, 0),
            Orientation.Right => new Vector2(9.8f, 0),
            _ => new Vector2(0, -9.8f)
        };

        _current = newOrientation;
    }
}

public enum Orientation
{
    Up,Left,Down,Right
    
    
};