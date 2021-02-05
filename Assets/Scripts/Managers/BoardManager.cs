using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class BoardEntry
    {
        public int x;
        public int y;

        private BoardEntry() {}

        public static BoardEntry Create(int _x, int _y)
        {
            return new BoardEntry() {x = _x, y = _y};
        }
    }
    
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager instance;

        /**
         * Elements
         */
        [Header("Tile config")] 
        [SerializeField] private List<TileSprite> availableSpriteTiles = new List<TileSprite>();

        private readonly List<Tile.TileType> _currentAvailableTileTypes = new List<Tile.TileType>();
        
        [Header("Board config")] [SerializeField]
        private GameObject tilePrefab;
        [SerializeField] private int xSize, ySize;
        private Vector2 tilesOffset;
        
        [Header("Power ups config")]
        [SerializeField][Range(1, 100)] private int powerUpProbability;
        [SerializeField] private List<PowerUpSprite> availablePowerUpTiles = new List<PowerUpSprite>();

        private TileController[,] tiles;
        
        public TileController SelectedTile { get; private set; }
        
        public bool IsBoardBusy { get; private set; }

        private int _clearedMatches = 0;
        private int _allClearMatches = -1;
        private bool HasClearedMatches => _clearedMatches == _allClearMatches;
        private bool AnyMatchToClear => _allClearMatches > 0;

        #region Lifecycle
        void Start()
        {
            instance = GetComponent<BoardManager>();

            tilesOffset = tilePrefab.GetComponent<SpriteRenderer>().bounds.size;
            CreateBoard();
        }

        private void OnDestroy()
        {
            ClearBoard();
        }

        private void FixedUpdate()
        {
            if (!HasClearedMatches) return;
            _clearedMatches = 0;
            StopCoroutine(FindNullTiles());
            StartCoroutine(FindNullTiles());
            ScheduleBoardCheck();
        }

        private void ScheduleBoardCheck()
        {
            Invoke(nameof(DoBoardCheck), 2f);
        }

        private void DoBoardCheck()
        {
            if (AnyMatchToClear)
            {
                CancelInvoke(nameof(DoBoardCheck));
                ScheduleBoardCheck();
                return;
            }

            StopCoroutine(CheckIfAnyMovesIsPossibleOnBoard());
            StartCoroutine(CheckIfAnyMovesIsPossibleOnBoard());
        }
        #endregion

        #region Board Handler
        /**
	    * Creates a Board starting from bottom left
	    * And goes ahead completing row per row
	    * X are the columns, Y are the rows
	    */
        private void CreateBoard()
        {
            tiles = new TileController[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    CreateTile(x, y);
                }
            }
        }

        private void CreateTile(int x, int y)
        {
            float startX = transform.position.x;
            float startY = transform.position.y;

            float xOffset = tilesOffset.x;
            float yOffset = tilesOffset.y;

            GameObject newTile = Instantiate(tilePrefab,
                new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0),
                tilePrefab.transform.rotation);
            var tileController = newTile.GetComponent<TileController>();
            tiles[x, y] = tileController;
            newTile.transform.parent = transform;
            ConfigureTile(tileController, x, y);
        }

        private void ClearBoard()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y].onActionCompleted -= ProcessEndActionCallback;
                        tiles[x, y] = null;
                    }
                }
            }
        }

        private IEnumerator FindNullTiles()
        {
            IsBoardBusy = true;
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] == null)
                    {
                        yield return StartCoroutine(ShiftDown(x, y));
                        break;
                    }
                }
            }

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] == null)
                    {
                        yield return StartCoroutine(Refill(x, y));
                        break;
                    }
                }
            }
            
            yield return new WaitForSeconds(.5f);

            yield return StartCoroutine(ClearAllMatchesForBoard());
            
            IsBoardBusy = false;
        }

        private IEnumerator ShiftDown(int x, int yStart, float shiftDelay = .03f)
        {
            int nullCount = 0;

            //GUIManager.instance.Score += 50;
            yield return new WaitForSeconds(shiftDelay);

            //shift down above match items
            for (int y = yStart; y < ySize; ++y)
            {
                TileController controller = tiles[x, y];
                if (controller == null)
                {
                    nullCount++;
                }
                else
                {
                    Vector3 shiftedPosition = controller.transform.position;
                    shiftedPosition.y -= tilesOffset.y * nullCount;
                    tiles[x, y - nullCount] = controller;
                    tiles[x, y] = null;
                    controller.UpdatePositionInBoard(x, y - nullCount);
                    controller.SetMoveAction(TileController.TileAction.Shift, shiftedPosition);
                }
            }

        }
        
        private IEnumerator Refill(int xStart, int yStart, float refillDelay = 0.2f)
        {
            yield return new WaitForSeconds(refillDelay);
            for (int y = yStart; y < ySize; y++)
                CreateTile(xStart, y);

        }

        private bool CanSpawnPowerUp()
        {
            return Random.Range(1, 100) <= powerUpProbability;
        }

        private void ConfigureTile(TileController newlyCreatedTile, int x, int y)
        {
            PowerUpSprite powerUp = GetAvailableRandomPowerUpSprite();
            TileSprite tileSprite = GetAvailableRandomTileSprite(x, y);
            
            Tile.TileType type = powerUp?.type ?? tileSprite.type;
            Sprite sprite = powerUp?.sprite ?? tileSprite.sprite;
            PowerUp.Type powerUpType = powerUp?.powerUpType ?? PowerUp.Type.None;
            
            newlyCreatedTile.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            newlyCreatedTile.Init(x, y, type, powerUpType);
            newlyCreatedTile.onActionCompleted += ProcessEndActionCallback;
        }
        
        private PowerUpSprite GetAvailableRandomPowerUpSprite()
        {
            if (!CanSpawnPowerUp() || availablePowerUpTiles.Count == 0) return null;
            return availablePowerUpTiles[Random.Range(0, availablePowerUpTiles.Count)];
        }
        private TileSprite GetAvailableRandomTileSprite(int x, int y)
        {
            _currentAvailableTileTypes.Clear();
            _currentAvailableTileTypes.AddRange(availableSpriteTiles.Select(tileSprite => tileSprite.type));

            if (x > 0 && tiles[x - 1, y] != null)
                _currentAvailableTileTypes.Remove(tiles[x - 1, y].GetTileType());
            if (x < xSize - 1 && tiles[x + 1, y] != null)
                _currentAvailableTileTypes.Remove(tiles[x + 1, y].GetTileType());
            if (y > 0 && tiles[x, y - 1] != null)
                _currentAvailableTileTypes.Remove(tiles[x, y - 1].GetTileType());

            var randomAvailableType = Tile.TileType.Type1; 
            if(_currentAvailableTileTypes.Count > 0)
                randomAvailableType = _currentAvailableTileTypes[Random.Range(0, _currentAvailableTileTypes.Count)];
            return availableSpriteTiles
                .FirstOrDefault(spriteTile =>
                    spriteTile.type == randomAvailableType);
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
                //can swap
                SwapTiles(SelectedTile, otherTile);
                DeselectSelected();
                //GUIManager.instance.MoveCounter--;
                return true;
            }

            SelectedTile.Play(TileAnimation.BlockSwap);
            otherTile.Play(TileAnimation.BlockSwap);
            return false;
        }

        private void SwapTiles(TileController originTile, TileController destinationTile)
        {
            _allClearMatches = 2;
            int originX = originTile.GetPositionXInBoard();
            int originY = originTile.GetPositionYInBoard();

            int destinationX = destinationTile.GetPositionXInBoard();
            int destinationY = destinationTile.GetPositionYInBoard();

            originTile.UpdatePositionInBoard(destinationX, destinationY);
            tiles[destinationX, destinationY] = originTile;

            destinationTile.UpdatePositionInBoard(originX, originY);
            tiles[originX, originY] = destinationTile;

            originTile.SetMoveAction(TileController.TileAction.Swap, destinationTile.transform.position);
            destinationTile.SetMoveAction(TileController.TileAction.Swap, originTile.transform.position);

            AudioManager.instance.PlayAudio(Clip.Swap);
        }

        private IEnumerator ClearAllMatchesForBoard()
        {
            List<BoardEntry> tilesWithMatches = new List<BoardEntry>();
            //Check if new matches have been formed
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (MatchResolver.CanMatchAnyInPosition(tiles[x, y]))
                        tilesWithMatches.Add(BoardEntry.Create(x, y));
                }
            }

            _allClearMatches = tilesWithMatches.Count > 0 ? tilesWithMatches.Count : -1;
            for (int i = 0; i < tilesWithMatches.Count; i++)
            {
                var boardEntry = tilesWithMatches[i];
                if (tiles[boardEntry.x, boardEntry.y] == null)
                {
                    _allClearMatches--;
                    continue;
                }
                yield return new WaitForSeconds(.2f);
                ClearAllMatchesForTile(tiles[boardEntry.x, boardEntry.y]);
            }
        }

        private IEnumerator CheckIfAnyMovesIsPossibleOnBoard()
        {
            yield return new WaitForSeconds(.5f);
            bool anyAvailableMatch = false;
            for (int x = 0; x < xSize && !anyAvailableMatch; x++)
            {
                for (int y = 0; y < ySize && !anyAvailableMatch; y++)
                {
                    if(tiles[x,y] == null) continue;
                    if (x - 1 > 0 && tiles[x - 1, y] != null)
                        anyAvailableMatch = MatchResolver.CanMatchAnyInPosition(tiles[x, y], tiles[x - 1, y].transform.position);
                    
                    if (x + 1 < xSize && tiles[x + 1, y] != null)
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.CanMatchAnyInPosition(tiles[x, y], tiles[x + 1, y].transform.position);
                    
                    if (y - 1 > 0 && tiles[x, y - 1] != null)
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.CanMatchAnyInPosition(tiles[x, y], tiles[x, y - 1].transform.position);
                    
                    if (y + 1 < ySize && tiles[x, y + 1] != null)
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.CanMatchAnyInPosition(tiles[x, y], tiles[x, y + 1].transform.position);
                }
            }
            
            Debug.Log($"AVAILABLE MOVES {anyAvailableMatch}");
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
                    _clearedMatches++;
                    break;
                case TileController.TileAction.Shift:
                    break;
                default: return;
            }
        }

        private void ProcessTileClick(TileController clickedTile)
        {
            if (IsBoardBusy) return;
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
            List<TileController> matches;
            bool isMatchValid = MatchResolver.ResolveMatch(matchedTile, out matches);

            if (isMatchValid)
            {
                //handle power up if any in matches
                if (matchedTile.IsPowerUpTile() || matches.Any(tile => tile.IsPowerUpTile()))
                {
                    List<TileController> powerUps = matches.Where(tile => tile.IsPowerUpTile()).ToList();
                    //also check matchedTile
                    if(matchedTile.IsPowerUpTile())
                        powerUps.Add(matchedTile);
                    for (int i = 0; i < powerUps.Count; ++i)
                    {
                        switch (powerUps[i].GetPowerUpTile())
                        {
                            case PowerUp.Type.Bomb:
                                HandleBombPowerUp(powerUps[i], matches, powerUps);
                                break;
                            case PowerUp.Type.Freeze:
                                HandleFreezePowerUp();
                                break;
                        }
                    }
                }

                for (int i = 0; i < matches.Count; ++i)
                {
                    if(matches[i] == null) continue;
                    tiles[matches[i].GetPositionXInBoard(), matches[i].GetPositionYInBoard()] = null;
                    matches[i].Play(TileAnimation.Explode);
                }
                
                tiles[matchedTile.GetPositionXInBoard(), matchedTile.GetPositionYInBoard()] = null;
                matchedTile.SetAction(TileController.TileAction.Explode);
                matchedTile.Play(TileAnimation.Explode);
                AudioManager.instance.PlayAudio(Clip.Clear);
            }
            else _clearedMatches++;
        }
        #endregion

        #region SuperPower Handler

        private void HandleBombPowerUp(TileController currentPowerUpTile, List<TileController> currentMatches, List<TileController> powerUpLists)
        {
            int startX = currentPowerUpTile.GetPositionXInBoard() - 1;
            int startY = currentPowerUpTile.GetPositionYInBoard() - 1;

            for (int x = startX; x <= currentPowerUpTile.GetPositionXInBoard() + 1; ++x)
            {
                for (int y = startY; y <= currentPowerUpTile.GetPositionYInBoard() + 1; ++y)
                {
                    if(x < 0 || y < 0 || x >= xSize || y >= ySize) continue;
                    var newCandidateToMatch = tiles[x, y];
                    if(newCandidateToMatch == null) continue;
                    if(currentMatches.Contains(newCandidateToMatch)) continue;
                    
                    currentMatches.Add(newCandidateToMatch);
                    if(newCandidateToMatch.IsPowerUpTile())
                        powerUpLists.Add(newCandidateToMatch);
                }
            }
        }

        private void HandleFreezePowerUp()
        {
            
        }

        #endregion
      
    }
}