using System;
using System.Collections.Generic;
using System.Linq;
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
            Click,
            Idle
        }
        [SerializeField] private Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);

        [SerializeField]
        private Tile model;

        private SpriteRenderer render;
        private Animator animator;
        
        private Vector3 _targetPosition = Vector3.zero;
        private TileAction _requestedAction = TileAction.Idle;
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
           onActionCompleted?.Invoke(TileAction.Click, this);
        }

        #endregion

        #region Public
        public void Init(int row, int column, Tile.TileType type, PowerUp.Type powerUpType = PowerUp.Type.None)
        {
            model = Tile.Create(type, row, column, powerUpType);
        }

        public Tile.TileType GetTileType()
        {
            return model.type;
        }
        
        public PowerUp.Type GetPowerUpTile()
        {
            return model.powerUpType;
        }

        public int GetPositionXInBoard()
        {
            return model.idRow;
        }

        public int GetPositionYInBoard()
        {
            return model.idColumn;
        }
        
        public bool IsPowerUpTile()
        {
            return model.IsPowerUpTile;
        }

        public void UpdatePositionInBoard(int x, int y)
        {
            model.idRow = x;
            model.idColumn = y;
        }
        
        public void SetMoveAction(TileAction action, Vector3 nextPosition)
        {
            _requestedAction = action;
            _targetPosition = nextPosition;
        }

        public void SetAction(TileAction action)
        {
            _requestedAction = action;
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
        
        public List<MatchCandidate> FindAllMatchesInPathFromPosition(Vector2[] path, Vector3? position = null)
        {
            var startPosition = position ?? transform.position;
            return path
                .Select(dir => FindMatchInDirectionFromPosition(startPosition, dir))
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
            onActionCompleted?.Invoke(_requestedAction, this);
            Destroy(gameObject);
        }
        private void TryToPerformAction()
        {
            if (_requestedAction == TileAction.Idle || _targetPosition == Vector3.zero) return;
            if (transform.position != _targetPosition)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, _targetPosition, 5f * Time.fixedDeltaTime);
            }
            else
            {
                var lastAction = _requestedAction;
                _requestedAction = TileAction.Idle;
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

        private MatchCandidate FindMatchInDirectionFromPosition(Vector3 position, Vector2 castDir)
        {
            Tile.TileType tileType = GetTileType();
            
            MatchCandidate matchCandidate = MatchCandidate.Create(castDir);
            
            RaycastHit2D hit = Physics2D.Raycast(position, castDir);
            while (hit.collider != null &&
                   hit.collider.gameObject != gameObject && 
                   (tileType == Tile.TileType.PowerUp ||
                    hit.collider.gameObject.GetComponent<TileController>().GetTileType() == tileType ||
                    hit.collider.gameObject.GetComponent<TileController>().IsPowerUpTile())
                   )
            {
                //take the first item found as default type if power up
                if (tileType == Tile.TileType.PowerUp)
                    tileType = hit.collider.gameObject.GetComponent<TileController>().GetTileType();
                
                matchCandidate.candidates.Add(hit.collider.gameObject);
                hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
            }
            
            return matchCandidate;
        }
        #endregion
    }
}