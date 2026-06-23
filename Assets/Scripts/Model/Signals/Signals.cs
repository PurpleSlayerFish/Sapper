using Services;
using UnityEngine;

namespace Model.Signals
{
    public struct OnCellStateChangedSignal
    {
        public int Col;
        public int Row;
        public CellState State;
        public int AdjacentMines;
        public bool IsMine;
    }

    public struct OnGameOverSignal
    {
        public bool IsWin;
    }
    
    public enum PointerPhase { Down, Hold, Up }
    
    public struct OnPointerSignal
    {
        public int Button;
        public PointerPhase Phase;
        public Vector2 ScreenPosition;
    }

    public struct OnCellPointerSignal
    {
        public int Column;
        public int Row;
        public int Button;
        public PointerPhase Phase;
    }
}