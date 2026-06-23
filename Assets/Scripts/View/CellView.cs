using UnityEngine;

namespace View
{
    public sealed class CellView : BaseGameObjectView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public int Col { get; private set; }
        public int Row { get; private set; }

        public void Setup(int col, int row, Vector3 worldPosition)
        {
            Col = col;
            Row = row;
            transform.position = worldPosition;
        }

        public void SetSprite(Sprite sprite)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = sprite;
        }
    }
}