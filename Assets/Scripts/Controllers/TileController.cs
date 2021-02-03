using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Models;
using UnityEngine;
using Utils;

namespace Controllers
{
    public static class TileAnimation
    {
        public static string Explode => "Explode";
        public static string BlockSwap => "BlockSwap";
        
    }
    public class TileController : MonoBehaviour
    {
        public enum TileAction
        {
            Swap,
            Shift,
            Explode,
            Idle
        }
        [SerializeField] private Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);

        [SerializeField]
        private Tile model;

        private SpriteRenderer render;
        private Animator animator;
        
        private Vector3 _targetPosition = Vector3.zero;
        private TileAction _action = TileAction.Idle;
        public event Action<TileAction, TileController> onActionCompleted;
        
        #region Lifecycle
        void Awake()
        {
            render = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            TryToPerformAction();
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
        
        public void SetMoveAction(TileAction action, Vector3 nextPosition)
        {
            _action = action;
            _targetPosition = nextPosition;
        }

        public void SetAction(TileAction action)
        {
            _action = action;
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

        public List<GameObject> FindAllMatchesInPath(Vector2[] path)
        {
            return path
                .SelectMany(dir => FindMatchInDirectionFromPosition(transform.position, dir).matches)
                .ToList();
        }

        public void Play(string animation)
        {
            animator.SetTrigger(animation);
        }
        
        #endregion

        #region Private

        private void OnExplode() //used in animation Explode to destroy gameobject on animation end
        {
            onActionCompleted?.Invoke(_action, this);
            Destroy(gameObject);
        }
        private void TryToPerformAction()
        {
            if (_action == TileAction.Idle || _targetPosition == Vector3.zero) return;
            if (transform.position != _targetPosition)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, _targetPosition, 5f * Time.fixedDeltaTime);
            }
            else
            {
                var lastAction = _action;
                _action = TileAction.Idle;
                _targetPosition = Vector3.zero;
                onActionCompleted?.Invoke(lastAction, this);
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