namespace Model
{
    public sealed class GameFieldModel
    {
        public readonly Cell[,] Cells;
        public readonly int Columns;
        public readonly int Rows;

        public GameFieldModel(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            Cells = new Cell[columns, rows];
        }

        public ref Cell GetCell(int col, int row) => ref Cells[col, row];
    }
}