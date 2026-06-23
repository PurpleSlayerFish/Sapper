namespace Model.Entities
{
    public enum CellState { Hidden, Revealed, Flagged }

    public struct Cell
    {
        public CellState State;
        public bool IsMine;
        public int AdjacentMines;
    }
}