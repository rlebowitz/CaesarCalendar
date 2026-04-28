using System.Diagnostics;

namespace CaesarCalendar.Web
{
    public class Puzzle
    {
        public static int Width { get; } = 7;
        public static int Height { get; } = 8;

        private static readonly Board emptyBoard = new(
        [
            0b00000011,
            0b00000011,
            0b00000001,
            0b00000001,
            0b00000001,
            0b00000001,
            0b00000001,
            0b11110001
        ], Width, Height);

        private static readonly (Piece, int)[] pieces =
        {
            (new Piece([0b10000000,0b11100000]),1),
            (new Piece([0b11110000]),1),
            (new Piece([0b10000000,0b11100000,0b10000000]),1),
            (new Piece([0b11000000,0b10000000,0b11000000]),1),
            (new Piece([0b00110000,0b11100000]),1),
            (new Piece([0b10000000,0b11110000]),1),
            (new Piece([0b10000000,0b10000000,0b11100000]),1),
            (new Piece([0b11000000,0b01000000,0b01100000]),1),
            (new Piece([0b11100000,0b11000000]),1),
            (new Piece([0b01100000,0b11000000]),1)
        };

        private readonly Dictionary<int, int> Months = new()
        {
            {0,5},
            {1,4},
            {2,3},
            {3,2},
            {4,1},
            {5,0},
            {6,11},
            {7,10},
            {8,9},
            {9,8},
            {10,7},
            {11,6}
        };

        private readonly Dictionary<int, int> Weekdays = new()
        {
            {0,3},
            {1,4},
            {2,5},
            {3,6},
            {4,0},
            {5,1},
            {6,2}
        };

        private readonly List<(Piece, int, int)>[] rotatedPiecesList;
        public Puzzle()
        {
            rotatedPiecesList = new List<(Piece, int, int)>[pieces.Length];
            int totalPiecesPopCount = 0;
            for (int i = 0; i < pieces.Length; i++)
            {
                (var piece, var count) = pieces[i];
                totalPiecesPopCount += count * piece.popCount;

                var rotatedPieces = new List<(Piece, int, int)>();
                rotatedPiecesList[i] = rotatedPieces;
                for (int d = 0; d < 4; d++)
                {
                    for (int m = 0; m < 2; m++)
                    {
                        var r_piece = (m & 1) == 0 ? piece : piece.Mirror();
                        if (rotatedPieces.All(t => !t.Item1.Equals(r_piece)))
                        {
                            rotatedPieces.Add((r_piece, d, m));
                            Debug.WriteLine($"{i} {d} {m}: {r_piece.offset}");
                        }
                    }
                    if (d < 3)
                        piece = piece.Rotate90();
                }
            }
            var boardOpenCount = emptyBoard.OpenCount;
            if (totalPiecesPopCount + 3 != boardOpenCount)
            {
                throw new Exception($"Squares covered by available pieces {totalPiecesPopCount} is inconsistent with empty board open squares-3 {boardOpenCount - 3}");
            }
        }



        public (Piece, int, int)[][] Solve(int month, int day, int weekday)
        {
            Board board = emptyBoard.Clone();
            int daym1 = day - 1;
            // convert to Hebrew layout
            month = Months[month];
            board.Set(month % 6, month / 6);
            // convert to Hebrew layout
            weekday = Weekdays[weekday];
            if (weekday < 4)
                board.Set(weekday + 3, 6);
            else
                board.Set(weekday, 7);
            board.Set(daym1 % 7, 2 + (daym1) / 7);
            LinkedList<(Piece, int, int)[]> solutions = new LinkedList<(Piece, int, int)[]>();
            Solve(board,
                Enumerable.Range(0, pieces.Length).ToArray(), 0,
                pieces.Select(i => (i.Item2)).ToArray(),
                board.Next(4, board.height - 1),
                solutions);
            return solutions.ToArray();
        }
        private bool Solve(Board board, int[] pieceIndices, int index, int[] pieceCounts, (int, int)? pos, LinkedList<(Piece, int, int)[]> solutions)
        {
            if (pos == null)
            {
                return false;
            }
            int n = pieceIndices.Length;
            if (index == n)
            {
                Debug.WriteLine("Found Solution: " + board);
                solutions.AddLast(board.Pieces.ToArray());
                return true;
            }

            bool b = false;
            int pieceIndex = pieceIndices[index];
            for (int i = index; i < n; i++)
            {
                int pieceI = pieceIndices[i];
                int pieceCount = pieceCounts[pieceI];
                pieceCounts[pieceI]--;
                int newIndex = index;
                if (pieceCount <= 1)
                {
                    newIndex = index + 1;
                    if (i > index)
                    {
                        pieceIndices[i] = pieceIndex;
                        pieceIndices[index] = pieceI;
                    }
                }
                foreach ((var p, int d, int m) in rotatedPiecesList[pieceI])
                {
                    (int x, int y) = pos.Value;
                    int px = x - p.offset;
                    int py = y - p.height + 1;
                    if (board.Fits(p, px, py))
                    {
                        board.Push(p, px, py);
                        b |= Solve(board, pieceIndices, newIndex, pieceCounts, board.Next(x, y), solutions);
                        board.Pop();
                    }
                }
                pieceCounts[pieceI]++;
                if (pieceCount <= 1)
                {
                    if (i > index)
                    {
                        pieceIndices[index] = pieceIndex;
                        pieceIndices[i] = pieceI;
                    }
                }
            }
            return b;
        }
    }
}
