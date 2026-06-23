using Model.Configs;
using Model.Signals;
using System;
using System.Collections.Generic;
using Model.Entities;
using Services;
using UnityEngine;
using Zenject;

namespace Model.Processors
{
    public sealed class GameFieldProcessor : IInitializable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private GameFieldModel _model;
        [Inject] private GameFieldSettings _settings;

        private bool _isFirstMove;
        private int _revealedCount;
        private int _totalSafeCells;

        public void Initialize()
        {
            _isFirstMove = true;
            _revealedCount = 0;
            _totalSafeCells = _model.Columns * _model.Rows - _settings.MineCount;

            _signalBus.Subscribe<OnCellPointerSignal>(HandleCellPointer);
        }

        private void HandleCellPointer(OnCellPointerSignal signal)
        {
            if (signal.Phase != PointerPhase.Up)
                return;

            switch (signal.Button)
            {
                case 0: HandleReveal(signal.Column, signal.Row); break;
                case 1: HandleFlag(signal.Column, signal.Row); break;
            }
        }

        private void HandleReveal(int col, int row)
        {
            ref var cell = ref _model.GetCell(col, row);

            if (cell.State != CellState.Hidden)
                return;

            // Первый ход — генерируем поле так, чтобы первая клетка была безопасной
            if (_isFirstMove)
            {
                _isFirstMove = false;
                PlaceMines(col, row);
                CalculateAdjacentMines();
            }

            if (cell.IsMine)
            {
                RevealCell(col, row);
                FireGameOver(isWin: false);
                return;
            }

            // Рекурсивное открытие через стек — без рекурсии, без GC в рантайме
            FloodReveal(col, row);

            if (_revealedCount >= _totalSafeCells)
                FireGameOver(isWin: true);
        }

        private void HandleFlag(int col, int row)
        {
            ref var cell = ref _model.GetCell(col, row);

            if (cell.State == CellState.Revealed)
                return;

            cell.State = cell.State == CellState.Flagged
                ? CellState.Hidden
                : CellState.Flagged;

            FireCellChanged(col, row, ref cell);
        }

        // Стек переиспользуем — аллоцируем один раз
        private readonly Stack<(int col, int row)> _floodStack = new Stack<(int, int)>(64);

        private void FloodReveal(int startCol, int startRow)
        {
            _floodStack.Clear();
            _floodStack.Push((startCol, startRow));

            while (_floodStack.Count > 0)
            {
                var (col, row) = _floodStack.Pop();

                ref var cell = ref _model.GetCell(col, row);

                if (cell.State == CellState.Revealed || cell.State == CellState.Flagged)
                    continue;

                RevealCell(col, row);

                // Если клетка пустая — раскрываем соседей
                if (cell.AdjacentMines == 0)
                {
                    for (var dc = -1; dc <= 1; dc++)
                    for (var dr = -1; dr <= 1; dr++)
                    {
                        if (dc == 0 && dr == 0) continue;

                        var nc = col + dc;
                        var nr = row + dr;

                        if (nc < 0 || nc >= _model.Columns || nr < 0 || nr >= _model.Rows)
                            continue;

                        if (_model.GetCell(nc, nr).State != CellState.Hidden)
                            continue;

                        _floodStack.Push((nc, nr));
                    }
                }
            }
        }

        private void RevealCell(int col, int row)
        {
            ref var cell = ref _model.GetCell(col, row);
            cell.State = CellState.Revealed;

            if (!cell.IsMine)
                _revealedCount++;

            FireCellChanged(col, row, ref cell);
        }

        // Размещаем мины случайно, исключая стартовую клетку и её соседей
        private void PlaceMines(int safeCol, int safeRow)
        {
            var placed = 0;
            var rng = new System.Random();

            while (placed < _settings.MineCount)
            {
                var col = rng.Next(_model.Columns);
                var row = rng.Next(_model.Rows);

                // Исключаем зону 3x3 вокруг первого клика
                if (Mathf.Abs(col - safeCol) <= 1 && Mathf.Abs(row - safeRow) <= 1)
                    continue;

                ref var cell = ref _model.GetCell(col, row);

                if (cell.IsMine)
                    continue;

                cell.IsMine = true;
                placed++;
            }
        }

        private void CalculateAdjacentMines()
        {
            for (var col = 0; col < _model.Columns; col++)
            for (var row = 0; row < _model.Rows; row++)
            {
                ref var cell = ref _model.GetCell(col, row);

                if (cell.IsMine)
                    continue;

                var count = 0;

                for (var dc = -1; dc <= 1; dc++)
                for (var dr = -1; dr <= 1; dr++)
                {
                    if (dc == 0 && dr == 0) continue;

                    var nc = col + dc;
                    var nr = row + dr;

                    if (nc < 0 || nc >= _model.Columns || nr < 0 || nr >= _model.Rows)
                        continue;

                    if (_model.GetCell(nc, nr).IsMine)
                        count++;
                }

                cell.AdjacentMines = count;
            }
        }

        private void FireCellChanged(int col, int row, ref Cell cell)
        {
            _signalBus.Fire(new OnCellStateChangedSignal
            {
                Col = col,
                Row = row,
                State = cell.State,
                AdjacentMines = cell.AdjacentMines,
                IsMine = cell.IsMine
            });
        }

        private void FireGameOver(bool isWin)
        {
            // При проигрыше раскрываем все мины
            if (!isWin)
                RevealAllMines();

            _signalBus.Fire(new OnGameOverSignal {IsWin = isWin});
        }

        private void RevealAllMines()
        {
            for (var col = 0; col < _model.Columns; col++)
            for (var row = 0; row < _model.Rows; row++)
            {
                ref var cell = ref _model.GetCell(col, row);

                if (cell.IsMine && cell.State != CellState.Revealed)
                    RevealCell(col, row);
            }
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<OnCellPointerSignal>(HandleCellPointer);
            _floodStack.Clear();
        }
    }
}