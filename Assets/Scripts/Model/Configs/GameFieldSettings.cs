using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Model.Configs
{
    [CreateAssetMenu(fileName = "GameFieldSettings", menuName = "Configs/GameFieldSettings")]
    public sealed class GameFieldSettings : ScriptableObject
    {
        [SerializeField] private Vector2 _originWorld;
        [Min(1), Tooltip("Not more, then half of a field"), SerializeField] private int _mineCount = 10;
        [Range(2, 50)] public int Columns = 9;
        [Range(2, 50)] public int Rows = 9;
        public float CellSize = 1f;

        public int MineCount => Mathf.Min(_mineCount, (Columns * Rows) / 2);
        public float ColumnOffset => ((float) Columns) / 2 * CellSize;
        public float RowOffset => ((float) Rows) / 2 * CellSize;
        public Vector2 FieldStartPosition => new(_originWorld.x - ColumnOffset, _originWorld.y - RowOffset);

        public AssetReferenceSprite SpriteHidden;
        public AssetReferenceSprite SpriteFlagged;
        public AssetReferenceSprite SpriteMine;
        public AssetReferenceSprite SpriteRevealed;
        public AssetReferenceSprite[] SpriteNumbers = new AssetReferenceSprite[8];
    }
}