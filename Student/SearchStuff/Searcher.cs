using QuoridorAI.BoardStuff;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QuoridorAI.SearchStuff
{
    public class Searcher
    {
        private readonly Board board;
        private readonly Evaluator evaluator;
        private readonly MoveGenerator moveGenerator;


        private const int posInf = 99999;
        private const int negInf = -posInf;

        public Move bestMove;
        private int searchDepth = 3;
        public int Depth { get { return searchDepth + 1; } set { searchDepth = value - 1; } } //-1 because the initial call adds one
        int moveCount = 0;

        public Searcher(Board board, Evaluator evaluator, MoveGenerator moveGenerator)
        {
            this.board = board;
            this.evaluator = evaluator;
            this.moveGenerator = moveGenerator;
        }

        public Move BeginSearch()
        {

            List<Move> moves = moveGenerator.GetMoves(); //hopefully sorted in best to worst move
            int bestEval = negInf;
            bestMove = new Move(-1, -1, MoveType.Move);
            moveCount = 0;
            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                if (board.DoMove(move))
                {
                    int eval = -AlphaBeta(searchDepth, bestEval, -bestEval); //this is the maximizing par whitch means we have to swap signs
                    //Debug.WriteLine(move + ", eval=" + eval);
                    moveCount++;
                    board.UndoMove(move);
                    if (eval > bestEval)
                    {
                        bestMove = move;
                        bestEval = eval;
                    }
                }
            }
            Debug.WriteLine("Moves searched " + moveCount);
            Debug.WriteLine("Best Move " + bestMove + " with eval = " + bestEval);
            return bestMove;
        }


        //THe negamax version based on the fact that max(a,b) == -min(-a,-b) , also switch alpha and beta for correct cut-offs (hi <=> lo)
        private int AlphaBeta(int depth, int alpha, int beta)
        {
            if (depth <= 0 || board.IsTerminal())
            {
                return evaluator.Evaluate();
            }

            List<Move> moves = moveGenerator.GetMoves(); //hopefully sorted in best to worst move

            int eval = negInf;
            foreach (Move move in moves)
            {
                if (board.DoMove(move))
                {
                    eval = Math.Max(eval, -AlphaBeta(depth - 1, -beta, -Math.Max(eval, alpha)));
                    board.UndoMove(move);
                    moveCount++;
                    if (eval >= beta) return eval;
                }
            }
            return eval;

        }
    }
}
