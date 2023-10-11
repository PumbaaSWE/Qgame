using QuoridorAI.BoardStuff;

namespace QuoridorAI.SearchStuff
{
    public class Evaluator
    {
        private readonly Board board;
        public int PathWeight = 3;
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

            int eval = (blackPathLen - whitePathLen) * PathWeight + (whiteWalls - blackWalls) * WallWeight;

            return eval * turn;
        }
    }
}