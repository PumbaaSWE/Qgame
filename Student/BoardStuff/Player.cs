using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace QuoridorAI.BoardStuff
{
    public sealed class Player
    {
        public Point pos;
        public int targetRank = 0;
        public int walls = 10;
        readonly Stack<Point> moveHistory = new();
        readonly Stack<Point[]> pathHistory = new();
        public Point[] currentPath;

        public void SetPlayer(Point pos, int walls, int targetRank)
        {
            this.pos = pos;
            this.walls = walls;
            this.targetRank = targetRank;
            currentPath = null;
            moveHistory.Clear();
            pathHistory.Clear();
        }

        public void PushHistory()
        {
            moveHistory.Push(pos);
            pathHistory.Push(currentPath);
        }

        public void PopHistory()
        {
            pos = moveHistory.Pop();
            currentPath = pathHistory.Pop();
        }

        public void SetPath(Point[] path)
        {
            currentPath = path;
        }

    }
}