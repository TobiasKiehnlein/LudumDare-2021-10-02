using System;
using UnityEngine;
using UnityEngine.UI;

public class GravityManager :MonoBehaviour
{
 


    private Orientation _current = Orientation.Down;
    public Slider debugslider;
 

    public Orientation GETCurrentOrientation()
    {
        return _current;
    }

    private void Update()
    {
        _current = debugslider.value switch
        {
            0 => Orientation.Up,
            1 => Orientation.Left,
            2 => Orientation.Down,
            3 => Orientation.Right,
            _ => Orientation.Down
        };
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
    Up,
    Left,
    Down,
    Right
}