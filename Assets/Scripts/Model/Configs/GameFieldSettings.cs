using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Model.Configs
{
    [CreateAssetMenu(fileName = "GameFieldSettings", menuName = "Configs/GameFieldSettings")]
    public sealed class GameFieldSettings : ScriptableObject
    {
        public Vector2 OriginWorld;
        public float CellSize = 1f;

        [Min(2)] public int Columns = 9;
        [Min(2)] public int Rows    = 9;

        public AssetReferenceGameObject CellViewPrefab;

        public AssetReferenceSprite SpriteHidden;
        public AssetReferenceSprite SpriteFlagged;
        public AssetReferenceSprite SpriteMine;
        public AssetReferenceSprite SpriteRevealed;
        // Индекс 0 = 1 мина, ..., 7 = 8 мин
        public AssetReferenceSprite[] SpriteNumbers = new AssetReferenceSprite[8];
    }
}