using Microsoft.Xna.Framework;

namespace QuoridorAI.BoardStuff
{
    public enum MoveType { Horizontal, Vertical, Move }
    public readonly struct Move
    {
        public readonly int x;
        public readonly int y;
        //public Point point;
        public readonly MoveType type;
        public Move(int x, int y, MoveType type)
        {
            this.type = type;
            this.x = x;
            this.y = y;
        }

        public Move(Point point, MoveType type) : this(point.X, point.Y, type) { }

        public static bool operator ==(Move lhs, Move rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Move lhs, Move rhs)
        {
            return !lhs.Equals(rhs);
        }
        public override bool Equals(object obj)
        {
            if (obj is Move move)
            {
                return Equals(move);
            }
            return false;
        }
        public bool Equals(Move other)
        {
            if (x == other.x && y == other.y)
            {
                return type == other.type;
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ") " + type;
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}