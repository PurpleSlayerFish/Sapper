using UnityEngine;

namespace Model.Configs
{
    [CreateAssetMenu(fileName = "GameFieldSettings", menuName = "Configs/GameFieldSettings")]
    public sealed class GameFieldSettings : ScriptableObject
    {
        [Tooltip("Мировая позиция левого нижнего угла поля")]
        public Vector2 OriginWorld;

        [Tooltip("Размер одной клетки в мировых единицах")]
        public float CellSize = 1f;

        public int Columns = 9;
        public int Rows = 9;
    }
}