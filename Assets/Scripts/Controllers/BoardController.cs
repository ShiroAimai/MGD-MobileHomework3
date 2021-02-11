using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Models;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class BoardPoint
    {
        public int x;
        public int y;

        private BoardPoint() {}

        public static BoardPoint Create(int _x, int _y)
        {
            return new BoardPoint() {x = _x, y = _y};
        }
    }
    
    public class BoardController : MonoBehaviour
    {
        /**
         * Elements
         */
        [Header("Tile config")]
        [SerializeField] private List<TileEntry> availableSpriteTiles = new List<TileEntry>();
        private readonly List<Tile.TileType> _currentAvailableTileTypes = new List<Tile.TileType>();
        [SerializeField] private GameObject tilePrefab;
        private Vector2 _tilesOffset;

        [Header("Board config")] 
        [SerializeField] private int xSize;
        [SerializeField] private int ySize;

        [Header("Power ups config")] 
        [SerializeField][Range(1, 100)] private int powerUpProbability;
        [SerializeField] private PowerUpHandler handler = new PowerUpHandler();
        [SerializeField] private List<PowerUpEntry> availablePowerUpTiles = new List<PowerUpEntry>();
        
        private TileController[,] _tiles;

        private TileController SelectedTile;

        private bool IsBoardBusy = false;
        private bool anyMatchToClear = false;

        /*
         * Routine used to normalize board    
         */
        private Coroutine normalizeRoutine;
        /*
         * Routine used to check if board contains at least one possible match
         */
        private Coroutine checkRoutine;
        
        #region Lifecycle

        void Start()
        {
            GetPreferredPowerUp();
            _tilesOffset = tilePrefab.GetComponentInChildren<SpriteRenderer>().bounds.size;
            CreateBoard();
        }

        private void OnDestroy()
        {
            ClearBoard();
        }

        #endregion

        #region Board Handler
        private void DoBoardCheck()
        {
            if (anyMatchToClear) return;
            
            if(checkRoutine != null)
                StopCoroutine(checkRoutine);
            checkRoutine = StartCoroutine(CheckIfAnyMovesIsPossibleOnBoard());
        }
        
        /**
	    * Creates a Board starting from bottom left
	    * And goes ahead completing row per row
	    * X are the columns, Y are the rows
	    */
        private void CreateBoard()
        {
            _tiles = new TileController[xSize, ySize];

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    CreateTile(x, y);
        }

        private void CreateTile(int x, int y)
        {
            float startX = transform.position.x;
            float startY = transform.position.y;

            float xOffset = _tilesOffset.x;
            float yOffset = _tilesOffset.y;

            GameObject newTile = Instantiate(tilePrefab,
                new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0),
                tilePrefab.transform.rotation);
            _tiles[x, y] = newTile.GetComponent<TileController>();
            newTile.transform.parent = transform;
            ConfigureTile( _tiles[x, y], x, y);
        }
        
        private bool CanSpawnPowerUp()
        {
            return Random.Range(1, 100) <= powerUpProbability;
        }

        private void ConfigureTile(TileController newlyCreatedTile, int x, int y)
        {
            PowerUpEntry powerUp = GetAvailableRandomPowerUpSprite();
            TileEntry tileEntry = GetAvailableRandomTileSprite(x, y);
            
            Tile.TileType type = powerUp?.type ?? tileEntry.type;
            Sprite sprite = powerUp?.sprite ?? tileEntry.sprite;
            PowerUp.Type powerUpType = powerUp?.powerUpType ?? PowerUp.Type.None;
            
            newlyCreatedTile.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
            newlyCreatedTile.Init(x, y, type, powerUpType);
            newlyCreatedTile.onActionCompleted += ProcessEndActionCallback;
        }
        
        private PowerUpEntry GetAvailableRandomPowerUpSprite()
        {
            if (!CanSpawnPowerUp() || availablePowerUpTiles.Count == 0) return null;
            return availablePowerUpTiles[Random.Range(0, availablePowerUpTiles.Count)];
        }
        private TileEntry GetAvailableRandomTileSprite(int x, int y)
        {
            _currentAvailableTileTypes.Clear();
            _currentAvailableTileTypes.AddRange(availableSpriteTiles.Select(tileSprite => tileSprite.type));

            if (x > 0 && _tiles[x - 1, y] != null)
                _currentAvailableTileTypes.Remove(_tiles[x - 1, y].GetTileType());
            if (x < xSize - 1 && _tiles[x + 1, y] != null)
                _currentAvailableTileTypes.Remove(_tiles[x + 1, y].GetTileType());
            if (y > 0 && _tiles[x, y - 1] != null)
                _currentAvailableTileTypes.Remove(_tiles[x, y - 1].GetTileType());

            var randomAvailableType = Tile.TileType.Type1; 
            if(_currentAvailableTileTypes.Count > 0)
                randomAvailableType = _currentAvailableTileTypes[Random.Range(0, _currentAvailableTileTypes.Count)];
            return availableSpriteTiles
                .FirstOrDefault(spriteTile =>
                    spriteTile.type == randomAvailableType);
        }

        private void ClearBoard()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (_tiles[x, y] == null) continue;
                    _tiles[x, y].onActionCompleted -= ProcessEndActionCallback;
                    _tiles[x, y] = null;
                }
            }
        }

        private IEnumerator FindNullTiles()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (_tiles[x, y] != null) continue;
                    ShiftDown(x, y);
                    break;
                }
            }

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (_tiles[x, y] != null) continue;
                    yield return new WaitForSeconds(0.15f); //wait for all tiles to shift down
                    Refill(x, y);
                    break;
                }
            }
            yield return StartCoroutine(ClearAllMatchesForBoard());
        }

        private void ShiftDown(int x, int yStart)
        {
            int nullCount = 0;

            //shift down above match items
            for (int y = yStart; y < ySize; ++y)
            {
                TileController controller = _tiles[x, y];
                if (controller == null)
                {
                    nullCount++;
                }
                else
                {
                    Vector3 shiftedPosition = controller.transform.position;
                    shiftedPosition.y -= _tilesOffset.y * nullCount;
                    _tiles[x, y - nullCount] = controller;
                    _tiles[x, y] = null;
                    controller.UpdatePositionInBoard(x, y - nullCount);
                    controller.SetMoveAction(TileController.TileAction.Shift, shiftedPosition);
                }
            }
        }
        
        private void Refill(int xStart, int yStart)
        {
            for (int y = yStart; y < ySize; y++)
                CreateTile(xStart, y);
        }

        private bool TryToSwapSelectedWith(TileController otherTile)
        {
            if (!SelectedTile) return false;
            bool isOtherNearby = SelectedTile.IsTileAdjacentTo(otherTile);
            if (!isOtherNearby) return false;
            if (SelectedTile.GetTileType() == otherTile.GetTileType())
            {
                SelectedTile.Play(TileAnimation.BlockSwap);
                otherTile.Play(TileAnimation.BlockSwap);
                return false;
            }

            if (MatchResolver.CanMatchAnyInPosition(SelectedTile, otherTile.transform.position) ||
                MatchResolver.CanMatchAnyInPosition(otherTile, SelectedTile.transform.position))
            {
                SwapTiles(SelectedTile, otherTile);
                DeselectSelected();
                //block user interaction with board until all processes are finished
                IsBoardBusy = true;
                return true;
            }

            SelectedTile.Play(TileAnimation.BlockSwap);
            otherTile.Play(TileAnimation.BlockSwap);
            return false;
        }

        private void SwapTiles(TileController originTile, TileController destinationTile)
        {
            BoardPoint originPoint = originTile.GetBoardPoint();

            BoardPoint destinationPoint = destinationTile.GetBoardPoint();

            originTile.UpdatePositionInBoard(destinationPoint.x, destinationPoint.y);
            _tiles[destinationPoint.x, destinationPoint.y] = originTile;

            destinationTile.UpdatePositionInBoard(originPoint.x, originPoint.y);
            _tiles[originPoint.x, originPoint.y] = destinationTile;

            originTile.SetMoveAction(TileController.TileAction.Swap, destinationTile.transform.position);
            destinationTile.SetMoveAction(TileController.TileAction.Swap, originTile.transform.position);

            AudioManager.instance.PlayAudio(Clip.Swap);
        }

        private IEnumerator ClearAllMatchesForBoard()
        {
            yield return new WaitForSeconds(.5f);
            if (GameManager.Instance.IsGameOver) yield break;
            List<BoardPoint> tilesWithMatches = new List<BoardPoint>();
            //Check if new matches have been formed
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (_tiles[x, y] != null && MatchResolver.CanMatchAnyInPosition(_tiles[x, y]))
                        tilesWithMatches.Add(_tiles[x, y].GetBoardPoint());
                }
            }

            anyMatchToClear = tilesWithMatches.Count > 0;
            
            for (int i = 0; i < tilesWithMatches.Count; i++)
            {
                var boardPoint = tilesWithMatches[i];
                if (_tiles[boardPoint.x, boardPoint.y] == null) continue;
                ClearAllMatchesForTile(_tiles[boardPoint.x, boardPoint.y]);
            }
            
            DoBoardCheck();
        }

        private IEnumerator CheckIfAnyMovesIsPossibleOnBoard()
        {
            yield return new WaitForSeconds(.2f);
            bool anyAvailableMatch = false;
            for (int x = 0; x < xSize && !anyAvailableMatch; x++)
            {
                for (int y = 0; y < ySize && !anyAvailableMatch; y++)
                {
                    if(_tiles[x,y] == null) continue;
                    if (x - 1 > 0 && _tiles[x - 1, y] != null)
                        anyAvailableMatch = MatchResolver.CanMatchAnyInPosition(_tiles[x, y], _tiles[x - 1, y].transform.position);
                    
                    if (x + 1 < xSize && _tiles[x + 1, y] != null)
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.CanMatchAnyInPosition(_tiles[x, y], _tiles[x + 1, y].transform.position);
                    
                    if (y - 1 > 0 && _tiles[x, y - 1] != null)
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.CanMatchAnyInPosition(_tiles[x, y], _tiles[x, y - 1].transform.position);
                    
                    if (y + 1 < ySize && _tiles[x, y + 1] != null)
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.CanMatchAnyInPosition(_tiles[x, y], _tiles[x, y + 1].transform.position);
                }
            }

            //re-enable user interaction with board
            IsBoardBusy = false;
            
            Debug.Log("GameOver " + !anyAvailableMatch);

            if (!anyAvailableMatch)
                GameManager.Instance.GameOver();
        }
        #endregion

        #region Tile Action Handler
        private void ProcessEndActionCallback(TileController.TileAction action, TileController tileController)
        {
            switch (action)
            {
                case TileController.TileAction.Click:
                    ProcessTileClick(tileController);
                    break;
                case TileController.TileAction.Swap:
                    ClearAllMatchesForTile(tileController);
                    break;
                case TileController.TileAction.Explode:
                    TryToNormalizeBoard();
                    break;
                case TileController.TileAction.Shift:
                    tileController.Play(TileAnimation.ShiftEnd);
                    break;
                default: return;
            }
        }

        private void TryToNormalizeBoard()
        {
            if(normalizeRoutine != null)
                StopCoroutine(normalizeRoutine);
            normalizeRoutine = StartCoroutine(FindNullTiles());
        }
        
        private void ProcessTileClick(TileController clickedTile)
        {
            if (IsBoardBusy || GameManager.Instance.IsGameOver || GameManager.Instance.IsGamePaused) return;
            if (SelectedTile == clickedTile)
                DeselectSelected();
            else
            {
                //no tile previously selected, select this
                if (TryToSelectTile(clickedTile)) return;
                //previously selected tile different from this, try to swap with this
                if (TryToSwapSelectedWith(clickedTile)) return;

                //previously selected tile is far from this, deselect it and select this-
                DeselectSelected();
            }
        }

        private bool TryToSelectTile(TileController selectionCandidate)
        {
            if (SelectedTile) return false;
            SelectedTile = selectionCandidate;
            AudioManager.instance.PlayAudio(Clip.Select);
            SelectedTile.Select();
            return true;
        }

        private void DeselectSelected()
        {
            if (!SelectedTile) return;
            SelectedTile.Deselect();
            SelectedTile = null;
        }

        private void ClearAllMatchesForTile(TileController matchedTile)
        {
            if (!MatchResolver.ResolveMatch(matchedTile, out var matches)) return;
            
            //handle power up if any in matches or matched tile itself
            handler.HandlePowerUps(matchedTile, matches);
                
            GameManager.Instance.UpdateScore(matches.Count + 1); //+1 from matched tile
                
            for (int i = 0; i < matches.Count; ++i)
            {
                if(matches[i] == null) continue;
                BoardPoint matchBoardPoint = matches[i].GetBoardPoint();
                _tiles[matchBoardPoint.x, matchBoardPoint.y] = null;
                matches[i].Play(TileAnimation.Explode);
            }

            BoardPoint matchedTileBoardPoint = matchedTile.GetBoardPoint();
            _tiles[matchedTileBoardPoint.x, matchedTileBoardPoint.y] = null;
            matchedTile.SetAction(TileController.TileAction.Explode);
            matchedTile.Play(TileAnimation.Explode);
            AudioManager.instance.PlayAudio(Clip.Clear);
        }
        #endregion

        #region SuperPower Handler

        private void GetPreferredPowerUp()
        {
            string preferredPowerUp = PlayerPrefs.GetString(PlayerPrefHelper.PowerUpKey, null);
            if (string.IsNullOrEmpty(preferredPowerUp)) return;
            var powerUpEntry = handler.GetEntryFor(PowerUp.FromString(preferredPowerUp));
            if(powerUpEntry != null)
                availablePowerUpTiles.Add(powerUpEntry);
        }

        #endregion
      
    }
}