using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Models;
using UnityEngine;

namespace Controllers
{
    internal static class AdjacentDirections
    {
        public readonly static Vector2 Up = Vector2.up;
        public readonly static Vector2 Down = Vector2.down;
        public readonly static Vector2 Left = Vector2.left;
        public readonly static Vector2 Right = Vector2.right;

        public static Vector2 GetOpposite(this Vector2 dir)
        {
            //opposite direction of a vector is obtained by
            //-(1/magnitude)*vector
            return (-(1 / dir.magnitude)) * dir;
        }
        internal static class Bundle
        {
            public static readonly Vector2[] Horizontal = new Vector2[] {Left, Right};
            public static readonly Vector2[] Vertical = new Vector2[] {Up, Down};
            public static readonly Vector2[] All = new Vector2[] {Up, Down, Left, Right};
        }
    }

    public class TileController : MonoBehaviour
    {
        [SerializeField] private Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);

        private Tile model;

        private SpriteRenderer render;
        private Animator animator;

        private bool matchFound = false;

        public Vector3 swapPosition = Vector3.zero;
        public event Action<TileController> onSwapped;
        #region Lifecycle
        void Awake()
        {
            render = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            TryToUpdateSwapPosition();
        }
        private void OnMouseDown()
        {
            BoardManager.instance.ProcessTileClick(this);
        }
        #endregion

        #region Public

        public void Init(int row, int column, Tile.TileType type)
        {
            model = new Tile {idRow = row, idColumn = column, type = type};
        }

        public Tile.TileType GetTileType()
        {
            return model.type;
        }

        public int GetPositionXInBoard()
        {
            return model.idRow;
        }

        public int GetPositionYInBoard()
        {
            return model.idColumn;
        }

        public void UpdatePositionInBoard(int x, int y)
        {
            model.idRow = x;
            model.idColumn = y;
        }
        
        public void Select()
        {
            render.color = selectedColor;
        }

        public void Deselect()
        {
            render.color = Color.white;
        }
        
        public bool IsTileAdjacentTo(TileController otherTile)
        {
            return GetAllAdjacentTiles().Contains(otherTile.gameObject);
        }

        public bool CanMatchAnyInPosition(Vector3 position)
        {
            bool horizontalMatches = AdjacentDirections.Bundle.Horizontal
                .Select(dir => FindMatchInDirectionFromPosition(position, dir).matches.Count)
                .Sum() >= Match.MatchMinLength;
            bool verticalMatches = AdjacentDirections.Bundle.Vertical
                .Select(dir => FindMatchInDirectionFromPosition(position, dir).matches.Count)
                .Sum() >= Match.MatchMinLength;
            return horizontalMatches || verticalMatches;
        }

        public List<GameObject> FindAllMatches()
        {
            return AdjacentDirections.Bundle.All
                .SelectMany(dir => FindMatchInDirectionFromPosition(transform.position, dir).matches)
                .ToList();
        }
        #endregion

        #region Private
        private void TryToUpdateSwapPosition()
        {
            if (swapPosition == Vector3.zero) return;
            if (transform.position != swapPosition)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, swapPosition, 10f * Time.fixedDeltaTime);
            }
            else
            {
                swapPosition = Vector3.zero;
                onSwapped?.Invoke(this);
            }
        }
        
        private List<GameObject> GetAllAdjacentTiles()
        {
            return AdjacentDirections.Bundle.All.Select(GetAdjacentTileInDir).ToList();
        }

        private GameObject GetAdjacentTileInDir(Vector2 castDir)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
            return hit.collider != null ? hit.collider.gameObject : null;
        }

        private Match FindMatchInDirectionFromPosition(Vector3 position, Vector2 castDir)
        {
            List<GameObject> matchingTiles = new List<GameObject>();
            RaycastHit2D hit = Physics2D.Raycast(position, castDir);
            while (hit.collider != null && 
                   hit.collider.gameObject != gameObject && 
                   hit.collider.gameObject.GetComponent<TileController>().GetTileType() == GetTileType()
                   )
            {
                matchingTiles.Add(hit.collider.gameObject);
                hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
            }

            return new Match() {dir = castDir, matches = matchingTiles};
        }
        #endregion
    }
}