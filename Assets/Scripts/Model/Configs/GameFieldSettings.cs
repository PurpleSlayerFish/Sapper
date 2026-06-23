using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Model.Configs
{
    [CreateAssetMenu(fileName = "GameFieldSettings", menuName = "Configs/GameFieldSettings")]
    public sealed class GameFieldSettings : ScriptableObject
    {
        public Vector2 OriginWorld;
        public float CellSize = 1f;

        [Range(2,50)] public int Columns = 9;
        [Range(2,50)] public int Rows    = 9;
        [Min(1)] public int MineCount = 10;

        public int MaxMineCount => (Columns * Rows) / 2;

        public AssetReferenceSprite SpriteHidden;
        public AssetReferenceSprite SpriteFlagged;
        public AssetReferenceSprite SpriteMine;
        public AssetReferenceSprite SpriteRevealed;
        public AssetReferenceSprite[] SpriteNumbers = new AssetReferenceSprite[8];

#if UNITY_EDITOR
        private void OnValidate()
        {
            MineCount = Mathf.Clamp(MineCount, 1, MaxMineCount);
        }
#endif
    }
}