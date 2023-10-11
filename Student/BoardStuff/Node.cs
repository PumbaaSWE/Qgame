namespace QuoridorAI.BoardStuff
{
    public class Node
    {
        public readonly int x;
        public readonly int y;

        public Node parent;
        public bool north;
        public bool south;
        public bool east;
        public bool west;

        public bool visited;
        public int gScore;
        public int fScore;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}