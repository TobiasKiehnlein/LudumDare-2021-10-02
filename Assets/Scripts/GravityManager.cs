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
        switch (newOrientation)
        {
            case Orientation.Down:
                Physics2D.gravity = new Vector2(0, -9.8f);
                break;
            case Orientation.Up:
                Physics2D.gravity = new Vector2(0, 9.8f);
                break;
            case Orientation.Left:
                Physics2D.gravity = new Vector2(-9.8f, 0);
                break;
            case Orientation.Right:
                Physics2D.gravity = new Vector2(9.8f, 0);
                break;
            default:
                Physics2D.gravity = new Vector2(0, -9.8f);
                break;
        }

        _current = newOrientation;
    }
}

public enum Orientation
{
    Up,Left,Down,Right
    
    
};