using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enums;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = System.Random;

namespace Utils
{
    public readonly struct Point
    {
        public readonly int X, Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public static class Extensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            var e = source.ToArray();
            for (var i = e.Length - 1; i >= 0; i--)
            {
                var swapIndex = rng.Next(i + 1);
                yield return e[swapIndex];
                e[swapIndex] = e[i];
            }
        }

        public static int ToInt(this bool source)
        {
            return source ? 1 : 0;
        }

        public static CellState OppositeWall(this CellState orig)
        {
            return (CellState) (((int) orig >> 2) | ((int) orig << 2)) & CellState.Initial;
        }

        public static bool HasFlag(this CellState cs, CellState flag)
        {
            return ((int) cs & (int) flag) != 0;
        }
    }

    [Flags]
    public enum CellState
    {
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
        Visited = 128,
        Initial = Top | Right | Bottom | Left
    }

    public struct RemoveWallAction
    {
        public Point Neighbour;
        public CellState Wall;
    }

    public class Maze
    {
        private readonly CellState[,] _cells;
        private readonly int _height;
        private readonly Random _rng;
        private readonly int _width;

        public Maze(int radius)
        {
            _width = radius * 2 - 1;
            _height = radius * 2 - 1;
            _cells = new CellState[radius * 2 - 1, radius * 2 - 1];
            for (var x = 0; x < radius * 2 - 1; x++)
            for (var y = 0; y < radius * 2 - 1; y++)
            {
                var distanceToCenter = Vector2.Distance(new Vector2 {x = x, y = y},
                    new Vector2 {x = radius - 1, y = radius - 1});
                _cells[x, y] = distanceToCenter < radius ? CellState.Initial : CellState.Visited;
            }

            _rng = new Random();
            VisitCell(_rng.Next(radius), _rng.Next(radius));
        }

        private CellState this[int x, int y]
        {
            get => _cells[x, y];
            set => _cells[x, y] = value;
        }

        private IEnumerable<RemoveWallAction> GetNeighbours(Point p)
        {
            if (p.X > 0) yield return new RemoveWallAction {Neighbour = new Point(p.X - 1, p.Y), Wall = CellState.Left};
            if (p.Y > 0) yield return new RemoveWallAction {Neighbour = new Point(p.X, p.Y - 1), Wall = CellState.Top};
            if (p.X < _width - 1)
                yield return new RemoveWallAction {Neighbour = new Point(p.X + 1, p.Y), Wall = CellState.Right};
            if (p.Y < _height - 1)
                yield return new RemoveWallAction {Neighbour = new Point(p.X, p.Y + 1), Wall = CellState.Bottom};
        }

        private void VisitCell(int x, int y)
        {
            this[x, y] |= CellState.Visited;
            foreach (var p in GetNeighbours(new Point(x, y)).Shuffle(_rng)
                .Where(z => !this[z.Neighbour.X, z.Neighbour.Y].HasFlag(CellState.Visited)))
            {
                this[x, y] -= p.Wall;
                this[p.Neighbour.X, p.Neighbour.Y] -= p.Wall.OppositeWall();
                VisitCell(p.Neighbour.X, p.Neighbour.Y);
            }
        }

        public void Display()
        {
            var firstLine = string.Empty;
            for (var y = 0; y < _height; y++)
            {
                var sbTop = new StringBuilder();
                var sbMid = new StringBuilder();
                for (var x = 0; x < _width; x++)
                {
                    sbTop.Append(this[x, y].HasFlag(CellState.Top) ? "+--" : "+  ");
                    sbMid.Append(this[x, y].HasFlag(CellState.Left) ? "|  " : "   ");
                }

                if (firstLine == string.Empty)
                    firstLine = sbTop.ToString();
                Debug.WriteLine(sbTop + "+");
                Debug.WriteLine(sbMid + "|");
                Debug.WriteLine(sbMid + "|");
            }

            Debug.WriteLine(firstLine);
        }

        public (TileType, Rot) GetTileInfo(int x, int y)
        {
            var isTop = this[x, y].HasFlag(CellState.Top);
            var isRight = this[x, y].HasFlag(CellState.Right);
            var isLeft = this[x, y].HasFlag(CellState.Left);
            var isBottom = this[x, y].HasFlag(CellState.Bottom);
            var exitCount = isTop.ToInt() + isRight.ToInt() + isLeft.ToInt() + isBottom.ToInt();
            Rot rot;
            switch (exitCount)
            {
                case 0:
                    return (TileType.CrossJunction, Rot.Top); //todo random orientation
                case 1:
                {
                    var rotation = Rot.Top;
                    if (isTop)
                        rotation = Rot.Bottom;
                    else if (isLeft)
                        rotation = Rot.Right;
                    else if (isRight) rotation = Rot.Left;

                    return (TileType.TJunction, rotation);
                }
                case 2:
                    var tileType = TileType.Angle;
                    rot = Rot.Bottom;
                    if (isLeft && isRight || isTop && isBottom)
                    {
                        tileType = TileType.Straight;
                        if (isLeft && isRight)
                            rot = Rot.Left; //todo random left right
                        else
                            rot = Rot.Top; //Todo random top bottom
                    }
                    else
                    {
                        if (isLeft)
                            rot = isTop ? Rot.Right : Rot.Top;
                        else if (isRight) rot = isTop ? Rot.Bottom : Rot.Left;
                    }

                    return (tileType, rot);
                case 3:
                    if (!isLeft)
                        rot = Rot.Bottom;
                    else if (!isTop)
                        rot = Rot.Left;
                    else if (!isRight)
                        rot = Rot.Top;
                    else
                        rot = Rot.Right;

                    return (TileType.DeadEnd, rot);
                default:
                    return (TileType.Straight, Rot.Top);
            }
        }
    }
}