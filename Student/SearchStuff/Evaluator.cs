using QuoridorAI.BoardStuff;
using System;

namespace QuoridorAI.SearchStuff
{
    public class Evaluator
    {
        private readonly Board board;
        public int PathWeight = 10;
        public int WallWeight = 1;
        public int LooseScore = 9998;
        public Evaluator(Board board)
        {
            this.board = board;
        }

        public int Evaluate()
        {
            int turn = board.WhiteToMove ? 1 : -1;
            if (board.white.pos.Y == board.white.targetRank) return LooseScore * turn; //cannot be infinity as false moves might score equal
            if (board.black.pos.Y == board.black.targetRank) return -LooseScore * turn; //(ie still do a move even if you loose, nothing else might be availible) 

            int whitePathLen = board.white.currentPath.Length;
            int blackPathLen = board.black.currentPath.Length;


            int whiteWalls = board.white.walls;
            int blackWalls = board.black.walls;

            //if(whiteWalls + blackWalls == 0)//both players out of walls
            //{
            //    return Math.Sign(blackPathLen - whitePathLen)*LooseScore*turn; // then the one with shortest path will win
            //}

            int eval = (blackPathLen - whitePathLen) * PathWeight + (whiteWalls - blackWalls) * WallWeight;

            return eval * turn;
        }
    }
}