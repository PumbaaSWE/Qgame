using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace QuoridorAI.BoardStuff
{
    public class Board
    {
        public readonly int N = 9;

        public readonly int W = 8;


        private readonly List<Point> whitePath = new();
        private readonly List<Point> blackPath = new();

        public readonly Node[,] grid;
        public readonly byte[,] walls;
        public readonly Player white = new();
        public readonly Player black = new();
        public bool WhiteToMove = true;

        public Player Player => WhiteToMove ? white : black;
        public Player Opponent => WhiteToMove ? black : white;

        public Board()
        {
            walls = new byte[W, W];
            grid = new Node[N, N];
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    grid[x, y] = new(x, y)
                    {
                        north = ValidateCoord(x, y + 1),
                        east = ValidateCoord(x + 1, y),
                        south = ValidateCoord(x, y - 1),
                        west = ValidateCoord(x - 1, y)
                    };
                }
            }
            white.SetPlayer(new Point(4, 0), 10, 8);
            black.SetPlayer(new Point(4, 8), 10, 0);

            ComputePath(whitePath, white);
            ComputePath(blackPath, black);
            white.SetPath(whitePath.ToArray());
            black.SetPath(blackPath.ToArray());
        }

        public void Reset()
        {
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    Node n = grid[x, y];
                    n.north = ValidateCoord(x, y + 1);
                    n.east = ValidateCoord(x + 1, y);
                    n.south = ValidateCoord(x, y - 1);
                    n.west = ValidateCoord(x - 1, y);
                }
            }
            for (int y = 0; y < W; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    walls[x, y] = 0;
                }
            }
        }

        public void SetBoard(bool[,] verticalWalls, bool[,] horizontalWalls, bool whiteToMove)
        {
            Reset();
            WhiteToMove = whiteToMove;
            for (int y = 0; y < W; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    if (verticalWalls[x, y]) AddWall(x, y, false);
                    if (horizontalWalls[x, y]) AddWall(x, y, true);
                }
            }
            ComputePath(whitePath, white);
            ComputePath(blackPath, black);
            white.SetPath(whitePath.ToArray());
            black.SetPath(blackPath.ToArray());

        }

        public bool DoMove(Move move)
        {

            if (move.type == MoveType.Move)
            {
                if (ValidatePlayerMove(Player, move.x, move.y))
                {
                    Player.PushHistory();
                    Player.pos = new Point(move.x, move.y);
                    //compute new path? yes... maybe no if the path is the same  !!
                    ComputePath(whitePath, Player);
                    Player.SetPath(whitePath.ToArray());

                    WhiteToMove = !WhiteToMove;
                    return true;
                }
                return false;
            }
            //if (Player.walls <= 0) return false;
            if (ValidateWall(move.x, move.y, move.type == MoveType.Horizontal))
            {

                AddWall(move.x, move.y, move.type == MoveType.Horizontal); // accually add the wall

                bool whiteHasPath = ComputePath(whitePath, white);
                bool blackHasPath = ComputePath(blackPath, black);

                if (whiteHasPath && blackHasPath)
                {
                    Player.walls--; //player should loose a wall

                    WhiteToMove = !WhiteToMove;

                    white.PushHistory();
                    white.SetPath(whitePath.ToArray());
                    black.PushHistory();
                    black.SetPath(blackPath.ToArray());
                    return true;
                }
                else
                {
                    RemoveWall(move.x, move.y, move.type == MoveType.Horizontal);
                }

            }

            return false;
        }

        private bool ValidateWall(int x, int y, bool horizontal)
        {
            if (x < 0 || x >= W) return false;
            if (y < 0 || y >= W) return false;

            //if (MatchWall(x, y, 3)) return false; // to avoid cross walls

            if (horizontal)
            {
                if (MatchWall(x - 1, y, 1)) return false;
                if (MatchWall(x, y, 1)) return false; // to avoid cross wall we can move this out an set 3 as mask
                if (MatchWall(x + 1, y, 1)) return false;
            }
            else
            {
                if (MatchWall(x, y + 1, 2)) return false;
                if (MatchWall(x, y, 2)) return false;
                if (MatchWall(x, y - 1, 2)) return false;
            }

            return true;
        }

        private bool MatchWall(int x, int y, byte mask)
        {
            if (x < 0 || x >= W) return false;
            if (y < 0 || y >= W) return false;
            return (walls[x, y] & mask) > 0;

        }

        private bool ValidatePlayerMove(Player player, int x, int y)
        {
            if (!ValidateCoord(x, y)) return false;//coords not valid
            int dx = player.pos.X - x;
            int dy = player.pos.Y - y;
            int manDist = Math.Abs(dx) + Math.Abs(dy);
            if (manDist != 1) return false; // we have moved to far or short
            //now we need to check da walls! for neighbors
            //so is there a wall between player.x/y och x/y? Depending on what direction we are gouing check neigbors
            bool clearPath = true;
            if (dx == 1) clearPath = grid[x, y].east;
            else if (dx == -1) clearPath = grid[x, y].west;
            else if (dy == 1) clearPath = grid[x, y].north;
            else if (dy == -1) clearPath = grid[x, y].south;
            return clearPath; // we can move!
        }

        public void UndoMove(Move move)
        {
            WhiteToMove = !WhiteToMove;
            if (move.type == MoveType.Move)
            {
                Player.PopHistory(); //pop history
            }
            else
            {
                RemoveWall(move.x, move.y, move.type == MoveType.Horizontal);
                Player.walls++; // get back the wall

                //pop all histories as walls my screwed both paths
                white.PopHistory();
                black.PopHistory();
            }
        }

        //does no validity checks, just cuts neighbors
        public void AddWall(int x, int y, bool horizontal)
        {
            if (horizontal)
            {
                grid[x, y].north = false;
                grid[x, y + 1].south = false;
                grid[x + 1, y].north = false;
                grid[x + 1, y + 1].south = false;
                walls[x, y] |= 1;
            }
            else
            {
                grid[x, y].east = false;
                grid[x + 1, y].west = false;
                grid[x, y + 1].east = false;
                grid[x + 1, y + 1].west = false;
                walls[x, y] |= 2;
            }

        }

        //does no validity checks, just adds neighbors
        public void RemoveWall(int x, int y, bool horizontal)
        {
            if (horizontal)
            {
                grid[x, y].north = true;
                grid[x, y + 1].south = true;
                grid[x + 1, y].north = true;
                grid[x + 1, y + 1].south = true;
                walls[x, y] &= 0b11111110; //0xFE ?
            }
            else
            {
                grid[x, y].east = true;
                grid[x + 1, y].west = true;
                grid[x, y + 1].east = true;
                grid[x + 1, y + 1].west = true;
                walls[x, y] &= 0b11111101; //0xFD ?
            }
        }

        public bool ValidateCoord(int x, int y)
        {
            if (x < 0 || x >= N) return false;
            if (y < 0 || y >= N) return false;
            return true;
        }


        private bool ComputePath(List<Point> path, Player player)
        {
            bool clearToMove = true;
            Point p = player.pos;
            int t = player.targetRank;
            Node n = AStar(p.X, p.Y, t);
            if (n != null)
            {
                path.Clear();
                while (n.parent != null)
                {
                    path.Add(new Point(n.x, n.y));
                    n = n.parent;
                }
            }
            else
            {
                clearToMove = false; // no clear path!!
            }
            return clearToMove;
        }


        private readonly Stack<Node> visited = new();
        private readonly Queue<Node> queue = new Queue<Node>();
        private Node BFS(int x, int y, int targetRow)
        {
            //foreach (Node n in grid) //reset tha grid D:
            //{
            //    n.visited = false;
            //}
            while (visited.Count > 0)
            {
                visited.Pop().visited = false;
            }

            queue.Clear();
            Node start = grid[x, y];
            start.parent = null;
            start.visited = true;
            visited.Push(start);
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();
                if (node.y == targetRow)
                {
                    return node;
                }
                if (node.east) BFSExploreNode(node.x + 1, node.y, node, queue, visited);
                if (node.north) BFSExploreNode(node.x, node.y + 1, node, queue, visited);
                if (node.west) BFSExploreNode(node.x - 1, node.y, node, queue, visited);
                if (node.south) BFSExploreNode(node.x, node.y - 1, node, queue, visited);
            }
            return null;
        }

        private void BFSExploreNode(int x, int y, Node parent, Queue<Node> queue, Stack<Node> visited)
        {
            Node node = grid[x, y];
            if (!node.visited)
            {
                node.visited = true;
                visited.Push(node);
                node.parent = parent;
                queue.Enqueue(node);
            }

        }


        private readonly PriorityQueue<Node, int> pqueue = new();
        private Node AStar(int x, int y, int targetRow)
        {
            pqueue.Clear();

            foreach (Node n in grid) //reset tha grid D:
            {
                n.gScore = int.MaxValue;
                n.fScore = int.MaxValue;
            }

            Node start = grid[x, y];
            start.parent = null;
            start.gScore = 0;
            start.fScore = 0;
            pqueue.Enqueue(start, start.gScore);
            while (pqueue.Count > 0)
            {
                Node node = pqueue.Dequeue();
                if (node.y == targetRow)
                {
                    return node;
                }
                if (node.east) ExploreNode(node.x + 1, node.y, node, pqueue, targetRow);
                if (node.north) ExploreNode(node.x, node.y + 1, node, pqueue, targetRow);
                if (node.west) ExploreNode(node.x - 1, node.y, node, pqueue, targetRow);
                if (node.south) ExploreNode(node.x, node.y - 1, node, pqueue, targetRow);
            }
            return null;
        }

        private void ExploreNode(int x, int y, Node current, PriorityQueue<Node, int> queue, int targetRow)
        {
            Node neighbor = grid[x, y];
            int tentativeScore = current.gScore + 1;
            if (tentativeScore < neighbor.gScore)
            {
                neighbor.parent = current;
                neighbor.gScore = tentativeScore;
                neighbor.fScore = tentativeScore + Math.Abs(neighbor.y - targetRow);
                queue.Enqueue(neighbor, neighbor.fScore);
            }

        }


        public bool IsTerminal()
        {
            return white.pos.Y == white.targetRank || black.pos.Y == black.targetRank;
        }
    }
}
