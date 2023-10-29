using Microsoft.Xna.Framework;
using QuoridorAI.BoardStuff;
using QuoridorAI.SearchStuff;
using System;
using System.Diagnostics;

class Agent:BaseAgent {
    [STAThread]
    static void Main(string[] args) {
        //for (int i = 0; i < args.Length; i++)
        //{
        //    if (args[i] == "-d")
        //    {
        //        int depth = TryGetInt(args, ++i, "-d");
        //    }
        //    else if (args[i] == "-w")
        //    {
        //        int weight = TryGetInt(args, ++i, "-d");
        //    }
        //}
        Program.Start(new Agent());
    }

    private static int TryGetInt(string[] args, int i , string match)
    {
        if(i >= args.Length)
        {
            throw new ArgumentException("Expected int after argument: " + match);
        }
        if (!int.TryParse(args[i], out int nbr))
        {
            throw new ArgumentException("Expected int after argument: " + match + " but got: " + args[i]);
        }
        return nbr;
    }

    Board board;
    Searcher searcher;
    Evaluator evaluator;
    MoveGenerator moveGenerator;

    public Agent() {
        board = new Board();
        evaluator = new Evaluator(board);
        moveGenerator = new MoveGenerator(board);
        searcher = new Searcher(board, evaluator, moveGenerator);
    }
    public override Drag SökNästaDrag(SpelBräde bräde) { 

        bool whiteToMove = bräde.spelare[0].färg == Färg.Röd;
        board.WhiteToMove = whiteToMove;

        //if (whiteToMove)
        //{
        //    evaluator.WallWeight = 2;
        //    evaluator.PathWeight = 3;
        //}
        //else
        //{
        //    evaluator.WallWeight = 4;
        //    evaluator.PathWeight = 2;
        //}

        board.Player.SetPlayer(bräde.spelare[0].position, bräde.spelare[0].antalVäggar, 8);
        board.Opponent.SetPlayer(bräde.spelare[1].position, bräde.spelare[1].antalVäggar, 0);
        board.SetBoard(bräde.vertikalaLångaVäggar, bräde.horisontellaLångaVäggar, whiteToMove);

        Move move = searcher.BeginSearch();
        Drag drag = new()
        {
            point = new Point(move.x, move.y)
        };
        switch (move.type)
        {
            case MoveType.Horizontal:
                drag.typ = Typ.Horisontell;
                break;
            case MoveType.Vertical:
                drag.typ = Typ.Vertikal;
                break;
            case MoveType.Move:
                drag.typ = Typ.Flytta;
                break;
        }

        return drag;
    }

    public override Drag GörOmDrag(SpelBräde bräde, Drag drag) {
        //Om draget ni försökte göra var felaktigt så kommer ni hit

        System.Diagnostics.Debugger.Break();    //Brytpunkt
        return SökNästaDrag(bräde);
    }
}
//enum Typ { Flytta, Horisontell, Vertikal }
//struct Drag {
//    public Typ typ;
//    public Point point;
//}