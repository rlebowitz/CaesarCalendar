using System.Data;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace CaesarCalendar.Web
{
    public class Piece(byte[] bits, int w, int h) : IEquatable<Piece>
    {
        public readonly byte[] bits = bits;
        public readonly int hashCode = bits.Aggregate(0, HashCode.Combine);
        public readonly int width = w, height = h, offset = BitOperations.LeadingZeroCount((uint)(bits.Last()) << 24);
        public readonly int popCount = bits.Aggregate(0, (a, b) => a + BitOperations.PopCount(b));

        public Piece(byte[] bits) :
            this(bits,
                8 - BitOperations.TrailingZeroCount(bits.Aggregate((byte)0b0, (a, b) => (byte)(a | b))),
                bits.Length)
        {
        }
        public override String ToString()
        {
            return String.Join("/", bits.Select(x => Convert.ToString(x, 2).PadLeft(8, '0').Substring(0, width)));
        }
        public Piece Mirror()
        {
            return new Piece([.. bits.Reverse()], width, height);
        }

        static Pen blackPen = new Pen(Color.Black, 3);
        static Brush pieceBrush = new SolidBrush(Color.FromArgb(225, 174, 143));

        public Piece Rotate90()
        {
            int new_width = height;
            int new_height = width;
            var new_bits = new byte[new_height];
            byte new_bit = (byte)(0x80 >> (new_width - 1));
            foreach (var row in bits)
            {
                byte bit = 0x80;
                for (int y = 0; y < new_height; y++)
                {
                    if ((row & bit) != 0)
                        new_bits[y] |= new_bit;
                    bit >>= 1;
                }
                new_bit <<= 1;
            }
            return new Piece(new_bits, new_width, new_height);
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Piece? other)
        {
            return other is not null &&
                    bits.SequenceEqual(other.bits) &&
                    width == other.width &&
                    height == other.height;
        }

        internal void Render(Graphics graphics, int x, int y, int blockWidth, int blockHeight)
        {
            byte prevRow = 0;
            byte row = bits[0];
            for (int py = 0; py < height; py++)
            {
                byte bitMask = 0x80;
                bool prevBit = false;
                byte nextRow = (py < height - 1) ? bits[py + 1] : (byte)0;
                bool bit = (row & bitMask) != 0;
                for (int px = 0; px < width; px++)
                {
                    bool prevRowBit = (prevRow & bitMask) != 0;
                    bool nextRowBit = (nextRow & bitMask) != 0;
                    bitMask >>= 1;
                    bool nextBit = (row & bitMask) != 0;
                    if (bit)
                    {
                        int x1 = x + px * blockWidth;
                        int x2 = x1 + blockWidth;
                        int y1 = y + py * blockHeight;
                        int y2 = y1 + blockHeight;
                        graphics.FillRectangle(pieceBrush, x1, y1, blockWidth, blockHeight);
                        if (!prevBit)
                            graphics.DrawLine(blackPen, x1, y1, x1, y2);
                        if (!nextBit)
                            graphics.DrawLine(blackPen, x2, y1, x2, y2);
                        if (!prevRowBit)
                            graphics.DrawLine(blackPen, x1, y1, x2, y1);
                        if (!nextRowBit)
                            graphics.DrawLine(blackPen, x1, y2, x2, y2);
                    }
                    prevBit = bit;
                    bit = nextBit;
                }
                prevRow = row;
                row = nextRow;
            }
        }

        internal string RenderSvg(int x, int y, int blockWidth, int blockHeight)
        {
            var sb = new StringBuilder(); // group for this piece sb.AppendLine("<g>");
                                          // same colors as original
            const string fill = "#E1AE8F"; // RGB(225,174,143)
            const string stroke = "black";
            const int strokeWidth = 3;

            byte prevRow = 0;
            byte row = bits[0];
            for (int py = 0; py < height; py++)
            {
                byte bitMask = 0x80;
                bool prevBit = false;
                byte nextRow = (py < height - 1) ? bits[py + 1] : (byte)0;
                bool bit = (row & bitMask) != 0;
                for (int px = 0; px < width; px++)
                {
                    bool prevRowBit = (prevRow & bitMask) != 0;
                    bool nextRowBit = (nextRow & bitMask) != 0;
                    bitMask >>= 1;
                    bool nextBit = (row & bitMask) != 0;

                    if (bit)
                    {
                        int x1 = x + px * blockWidth;
                        int y1 = y + py * blockHeight;
                        int x2 = x1 + blockWidth;
                        int y2 = y1 + blockHeight;

                        // filled block
                        sb.AppendLine($"  <rect x=\"{x1}\" y=\"{y1}\" width=\"{blockWidth}\" height=\"{blockHeight}\" fill=\"{fill}\" />");

                        // draw borders only where neighbours are absent
                        if (!prevBit)
                            sb.AppendLine($"  <line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x1}\" y2=\"{y2}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />");
                        if (!nextBit)
                            sb.AppendLine($"  <line x1=\"{x2}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />");
                        if (!prevRowBit)
                            sb.AppendLine($"  <line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y1}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />");
                        if (!nextRowBit)
                            sb.AppendLine($"  <line x1=\"{x1}\" y1=\"{y2}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />");
                    }

                    prevBit = bit;
                    bit = nextBit;
                }

                prevRow = row;
                row = nextRow;
            }

            sb.AppendLine("</g>");
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public static bool operator ==(Piece left, Piece right)
        {
            return EqualityComparer<Piece>.Default.Equals(left, right);
        }

        public static bool operator !=(Piece left, Piece right)
        {
            return !(left == right);
        }
    }
}
