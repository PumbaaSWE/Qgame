using Microsoft.Xna.Framework;
using QuoridorAI.BoardStuff;
using System.Collections.Generic;

namespace QuoridorAI.SearchStuff
{
    public class MoveGenerator
    {
        private readonly Point n = new Point(0, 1);
        private readonly Point ne = new Point(1, 1);
        private readonly Point e = new Point(1, 0);
        private readonly Point se = new Point(1, -1);
        private readonly Point s = new Point(0, -1);
        private readonly Point sw = new Point(-1, -1);
        private readonly Point w = new Point(-1, 0);
        private readonly Point nw = new Point(-1, 1);


        private readonly Board board;
        private readonly int[,] scoreBoard;
        private readonly MoveComparer moveComparer;


        public MoveGenerator(Board board)
        {
            this.board = board;
            scoreBoard = new int[board.N, board.N];
            moveComparer = new MoveComparer(scoreBoard);
        }

        //this generate all move possible, does not check leagality or even psuodo leagal (as duplicate walls)
        //according to profiling its the pathfinding that is >80% of cpu time so not sure if doing pseudo leagal will help much so I dont at this time...
        public List<Move> GetMoves(bool addPrevTopMove = false, Move prevTopMove = default)
        {
            Player player = board.Player;
            List<Move> moves = new List<Move>(player.walls > 0 ? 133 : 5); //if we can place walls there is maximum 128 wall moves + 4 player moves(+ 1 extra)

            if (addPrevTopMove) moves.Add(prevTopMove);

            //presumably the along path should be searched first
            Player opponent = board.Opponent;
            //if (player.currentPath != null && player.currentPath.Length > 0) //too many checks?
            //{
            //    Point p = player.currentPath[^1];
            //    AddMove(moves, p, MoveType.Move); //prio the move along the path
            //}

            //AddScore(player.pos, 40);
            if (!board.SimplifiedRules)
            {
                GenerateWalkMoves(moves);
            }
            else
            {
                if (player.currentPath != null && player.currentPath.Length > 0) //too many checks?
                {
                    Point p = player.currentPath[^1];
                    AddMove(moves, p, MoveType.Move); //prio the move along the path
                }
                Node node = board.grid[player.pos.X, player.pos.Y];
                if (node.north) AddMove(moves, player.pos + n, MoveType.Move);
                if (node.west) AddMove(moves, player.pos + w, MoveType.Move);
                if (node.east) AddMove(moves, player.pos + e, MoveType.Move);
                if (node.south) AddMove(moves, player.pos + s, MoveType.Move);
                //AddMove(moves, player.pos + n, MoveType.Move);
                //AddMove(moves, player.pos + e, MoveType.Move);
                //AddMove(moves, player.pos + w, MoveType.Move);
                //AddMove(moves, player.pos + s, MoveType.Move);
            }


            if (player.walls > 0)
            {
                //they are in here because if we dont have walls just go for the closest... in theory enemy would block some paths but we cannot see far enough ahead to be sure
                //AddMove(moves, player.pos + n, MoveType.Move);
                //AddMove(moves, player.pos + e, MoveType.Move);
                //AddMove(moves, player.pos + w, MoveType.Move);
                //AddMove(moves, player.pos + s, MoveType.Move);

                // add wall moves
                for (int y = 0; y < board.N; y++)
                {
                    for (int x = 0; x < board.N; x++)
                    {
                        scoreBoard[x, y] = 0;
                    }
                }
                //score should be per move, but adding a score to move struct is overkill?
                for (int i = opponent.currentPath.Length - 1; i >= 0; --i) // this is perhaps suboptimal as intersects might score high while not being "good"
                {
                    Point p = opponent.currentPath[i];
                    AddScore(p, 5);
                    AddScore(p + n, 5);
                    AddScore(p + w, 1);
                    AddScore(p + e, 5);
                    AddScore(p + s, 1);
                    AddScore(p + se, 1);
                    AddScore(p + sw, 1);
                    AddScore(p + ne, 5);
                    AddScore(p + nw, 1);
                }
                for (int i = player.currentPath.Length - 1; i >= 0; --i)
                {
                    Point p = player.currentPath[i];
                    AddScore(p, 40);
                    AddScore(p + n,5);
                    AddScore(p + w,5);
                    AddScore(p + e,5);
                    AddScore(p + s,5);
                    AddScore(p + se);
                    AddScore(p + sw);
                    AddScore(p + ne);
                    AddScore(p + nw);
                }


                for (int y = 0; y < board.W; y++)
                {
                    for (int x = 0; x < board.W; x++)
                    {
                        if (board.ValidateWall(x,y, true))
                            moves.Add(new Move(x, y, MoveType.Horizontal));
                        if (board.ValidateWall(x, y, false))
                            moves.Add(new Move(x, y, MoveType.Vertical));
                    }
                }
                //moves.Sort(moveComparer); //sort lo to hi
                moves.Sort(1, moves.Count-1, moveComparer); //sort lo to hi, but not first element
            }

            return moves;
        }

        public void GenerateWalkMoves(List<Move> moves)
        {
            Player player = board.Player;
            Point opp = board.Opponent.pos;
            Point pos = player.pos;
            Node node = board.grid[pos.X, pos.Y];
            if (node.north)
            {
                Point to = pos + n;
                if (to == opp)
                {
                    Node next = board.grid[to.X, to.Y];
                    if (next.north)
                    {
                        AddMove(moves, to + n, MoveType.Move);
                    }
                    else
                    {
                        if (next.west)
                        {
                            AddMove(moves, to + w, MoveType.Move);
                        }
                        if (next.east)
                        {
                            AddMove(moves, to + e, MoveType.Move);
                        }
                    }
                }
                else
                {
                    AddMove(moves, to, MoveType.Move);
                }
            }
            if (node.south)
            {
                Point to = pos + s;
                if (to == opp)
                {
                    Node next = board.grid[to.X, to.Y];
                    if (next.north)
                    {
                        AddMove(moves, to + s, MoveType.Move);
                    }
                    else
                    {
                        if (next.west)
                        {
                            AddMove(moves, to + w, MoveType.Move);
                        }
                        if (next.east)
                        {
                            AddMove(moves, to + e, MoveType.Move);
                        }
                    }
                }
                else
                {
                    AddMove(moves, to, MoveType.Move);
                }
            }
            if (node.west)
            {
                Point to = pos + w;
                if (to == opp)
                {
                    Node next = board.grid[to.X, to.Y];
                    if (next.west)
                    {
                        AddMove(moves, to + w, MoveType.Move);
                    }
                    else
                    {
                        if (next.north)
                        {
                            AddMove(moves, to + n, MoveType.Move);
                        }
                        if (next.south)
                        {
                            AddMove(moves, to + s, MoveType.Move);
                        }
                    }
                }
                else
                {
                    AddMove(moves, to, MoveType.Move);
                }
            }
            if (node.east)
            {
                Point to = pos + e;
                if (to == opp)
                {
                    Node next = board.grid[to.X, to.Y];
                    if (next.west)
                    {
                        AddMove(moves, to + e, MoveType.Move);
                    }
                    else
                    {
                        if (next.north)
                        {
                            AddMove(moves, to + n, MoveType.Move);
                        }
                        if (next.south)
                        {
                            AddMove(moves, to + s, MoveType.Move);
                        }
                    }
                }
                else
                {
                    AddMove(moves, to, MoveType.Move);
                }
            }
        }



        private void AddScore(Point point, int score = 1)
        {
            int x = point.X;
            int y = point.Y;
            if (x < 0 || x >= board.N) return;
            if (y < 0 || y >= board.N) return;
            scoreBoard[x, y] -= score; //sorted lo to hi, ie low score is good
        }

        private void AddMove(List<Move> moves, Point point, MoveType type)
        {
            if (point.X < 0 || point.X >= board.N) return;
            if (point.Y < 0 || point.Y >= board.N) return;
            Move move = new(point, type);
            if (!moves.Contains(move)) moves.Add(move);
        }
    }


    public class MoveComparer : Comparer<Move>
    {
        readonly int[,] scoreBoard;
        public MoveComparer(int[,] scoreBoard)
        {
            this.scoreBoard = scoreBoard;
        }


        public override int Compare(Move x, Move y)
        {
            return GetScore(x.x, x.y).CompareTo(GetScore(y.x, y.y));
        }

        private int GetScore(int x, int y)
        {
            //if (x < 0 || x > 9) return 9999;
            //if (y < 0 || y > 9) return 9999;

            return scoreBoard[x, y];
        }
    }
}
