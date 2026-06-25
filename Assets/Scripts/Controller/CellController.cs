using Common;
using Model;
using Model.Entities;
using UnityEngine;
using View;

namespace Controller
{
    public struct CellData
    {
        public CellSprites Sprites;
    }

    public sealed class CellController : BaseControllerWithViewAndData<CellView, CellData>
    {
        public override void OnAfterInit()
        {
            ApplyState(CellState.Hidden, 0, false, false);
        }

        public void ApplyState(CellState state, int adjacentMines, bool isMine, bool isCurrent)
        {
            View.SetSprite(ResolveSprite(state, adjacentMines, isMine, isCurrent));
        }

        private Sprite ResolveSprite(CellState state, int adjacentMines, bool isMine, bool isCurrent) => state switch
        {
            CellState.Hidden => Data.Sprites.Hidden,
            CellState.Flagged => Data.Sprites.Flagged,
            CellState.Revealed when isMine && isCurrent => Data.Sprites.CurrentMine,
            CellState.Revealed when isMine => Data.Sprites.Mine,
            CellState.Revealed when adjacentMines > 0 => Data.Sprites.Numbers[adjacentMines - 1],
            CellState.Revealed => Data.Sprites.Revealed,
            _ => Data.Sprites.Hidden
        };
    }

    public sealed class CellSprites
    {
        public Sprite Hidden;
        public Sprite Flagged;
        public Sprite Mine;
        public Sprite CurrentMine;
        public Sprite Revealed;
        public Sprite[] Numbers; // индекс 0 = 1 мина, ..., 7 = 8 мин
    }
}