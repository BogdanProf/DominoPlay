using System;

namespace DominoPlay.Models
{
    public class DominoTile
    {
        public int Left { get; set; }
        public int Right { get; set; }

        public DominoTile(int left, int right)
        {
            Left = left;
            Right = right;
        }

        public bool IsDouble => Left == Right;

        public void Flip()
        {
            (Left, Right) = (Right, Left);
        }

        public override string ToString() => $"{Left}:{Right}";
    }
}