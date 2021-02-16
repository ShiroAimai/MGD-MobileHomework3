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
        [SerializeField] private List<Tile> tiles = new List<Tile>();
        private readonly List<TileState.TileType> _currentAvailableTileTypes = new List<TileState.TileType>();

        [Header("Board config")]
        [SerializeField] private int xSize;
        [SerializeField] private int ySize;

        private TileController[,] _tiles;

        private TileController SelectedTile;

        private bool IsBoardBusy = false;
        private bool anyMatchToClear = false;
        private int shiftingTileCounter = 0;

        /*
         * Routine used to normalize board    
         */
        private Coroutine normalizeRoutine;
        /*
         * Routine used to check if board contains at least one possible match
         */
        private Coroutine checkRoutine;
        
        #region Lifecycle

        private void Start()
        {
            GetPreferredPowerUp();
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
            checkRoutine = StartCoroutine(IsGameOver());
        }
        
        /**
	    * Creates a Board starting from bottom left
	    * And goes ahead completing row per row
	    * X are the columns, Y are the rows
	    */
        private void CreateBoard()
        {
            _tiles = new TileController[xSize, ySize];

            bool boardEstablished = false;
            while (!boardEstablished)
            {
                ClearBoard();
                for (int x = 0; x < xSize; x++)
                    for (int y = 0; y < ySize; y++)
                        //during board creation we do not spawn power up in order to avoid not resolved matches
                        CreateTile(x, y, true);

                boardEstablished = AnyMovesIsPossibleOnBoard();
            }
        }

        private void CreateTile(int x, int y, bool isCreatingBoard)
        {
            float startX = transform.position.x;
            float startY = transform.position.y;

            TileConfig tileConfigToSpawn = isCreatingBoard ? 
                tiles.Where(tile => tile.enabled).First(tile => tile.config.powerUp == PowerUp.Type.None).config
                : GetTileToSpawn();

            var _tilesOffset = tileConfigToSpawn.prefab.GetComponentInChildren<SpriteRenderer>().bounds.size;
            
            float xOffset = _tilesOffset.x;
            float yOffset = _tilesOffset.y;

            GameObject newTile = Instantiate(tileConfigToSpawn.prefab,
                new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0),
                tileConfigToSpawn.prefab.transform.rotation);
            newTile.transform.parent = transform;
            ConfigureTile(newTile, tileConfigToSpawn, x, y);
        }

        private void ConfigureTile(GameObject justCreatedTile, TileConfig config, int x, int y)
        {
            TileEntry entry = GetAvailableRandomTileEntry(config.entries, x, y);
            
            if (entry == null) return;
            
            _tiles[x, y] = justCreatedTile.GetComponent<TileController>();
            _tiles[x, y].Init(x, y, entry);
            _tiles[x, y].onActionCompleted += ProcessEndActionCallback;
        }

        private TileConfig GetTileToSpawn()
        {
            List<TileConfig> activeTiles = tiles.Where(tile => tile.enabled).Select(tile => tile.config).ToList();

            var maxWeight = activeTiles.Sum(config => config.spawnProbability);
            var spawnProbability = Random.Range(0, maxWeight);

            TileConfig selection = null;
            for (int i = 0; i < activeTiles.Count && selection == null; ++i)
            {
                if (spawnProbability < activeTiles[i].spawnProbability) 
                    selection = activeTiles[i];
                spawnProbability -= activeTiles[i].spawnProbability;
            }

            return selection;
        }
        
        private TileEntry GetAvailableRandomTileEntry(List<TileEntry> entries,int x, int y)
        {
            if (entries.Count <= 0) return null;
            
            _currentAvailableTileTypes.Clear();
            _currentAvailableTileTypes.AddRange(entries.Select(tileSprite => tileSprite.type));
            
            if (x > 0 && IsTileValidAtPoint(x - 1, y))
                _currentAvailableTileTypes.Remove(_tiles[x - 1, y].GetTileType());
            if (x < xSize - 1 && IsTileValidAtPoint(x + 1, y))
                _currentAvailableTileTypes.Remove(_tiles[x + 1, y].GetTileType());
            if (y > 0 && IsTileValidAtPoint(x, y - 1))
                _currentAvailableTileTypes.Remove(_tiles[x, y - 1].GetTileType());

            TileState.TileType randomType = _currentAvailableTileTypes.Count > 0
                ? _currentAvailableTileTypes[Random.Range(0, _currentAvailableTileTypes.Count)]
                : entries.First().type;
            
            return entries
                .FirstOrDefault(spriteTile =>
                    spriteTile.type == randomType);
        }

        private void ResetTileAtPoint(int x, int y)
        {
            _tiles[x, y] = null;
        }

        private bool IsTileValidAtPoint(int x, int y)
        {
            return _tiles[x, y] != null;
        }

        private void ClearBoard()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (!IsTileValidAtPoint(x, y)) continue;
                    _tiles[x, y].onActionCompleted -= ProcessEndActionCallback;
                    if(_tiles[x, y].gameObject)
                        Destroy(_tiles[x, y].gameObject);
                    ResetTileAtPoint(x, y);
                }
            }
        }

        private IEnumerator FindNullTiles()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (IsTileValidAtPoint(x, y)) continue;
                    ShiftDown(x, y);
                    break;
                }
            }

            yield return new WaitUntil(() => shiftingTileCounter == 0);
            var wait = new WaitForSeconds(0.15f);//wait for all tiles to shift down
            
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (IsTileValidAtPoint(x, y)) continue;
                    yield return wait; 
                    Refill(x, y);
                    break;
                }
            }
            
            yield return new WaitForSeconds(0.35f);
            ClearAllMatchesForBoard();
        }

        private void ShiftDown(int x, int yStart)
        {
            int nullCount = 0;

            //shift down above match items
            for (int y = yStart; y < ySize; ++y)
            {
                if (!IsTileValidAtPoint(x, y))
                {
                    nullCount++;
                }
                else
                {
                    shiftingTileCounter++;
                    TileController controller = _tiles[x, y];

                    Vector3 shiftedPosition = controller.transform.position;

                    var _tilesOffset = controller.gameObject.GetComponentInChildren<SpriteRenderer>().bounds.size;
                    shiftedPosition.y -= _tilesOffset.y * nullCount;
                    _tiles[x, y - nullCount] = controller;
                    ResetTileAtPoint(x, y);
                    controller.UpdatePositionInBoard(x, y - nullCount);
                    controller.SetMoveAction(TileController.TileAction.Shift, shiftedPosition);
                }
            }
        }
        
        private void Refill(int xStart, int yStart)
        {
            for (int y = yStart; y < ySize; y++)
                CreateTile(xStart, y, false);
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

            if (MatchResolver.AreThereAnyMatchesInPosition(SelectedTile, otherTile.transform.position) ||
                MatchResolver.AreThereAnyMatchesInPosition(otherTile, SelectedTile.transform.position))
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

        private void ClearAllMatchesForBoard()
        {
            if (GameManager.Instance.IsGameOver) return;
            
            List<BoardPoint> tilesWithMatches = new List<BoardPoint>();
            //Check if new matches have been formed
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (IsTileValidAtPoint(x, y) && MatchResolver.AreThereAnyMatchesInPosition(_tiles[x, y]))
                        tilesWithMatches.Add(_tiles[x, y].GetBoardPoint());
                }
            }

            anyMatchToClear = tilesWithMatches.Count > 0;
            
            for (int i = 0; i < tilesWithMatches.Count; i++)
            {
                var boardPoint = tilesWithMatches[i];
                if (!IsTileValidAtPoint(boardPoint.x, boardPoint.y)) continue; //already included in another match
                ClearAllMatchesForTile(_tiles[boardPoint.x, boardPoint.y]);
            }
            
            DoBoardCheck();
        }

        private bool AnyMovesIsPossibleOnBoard()
        {
            bool anyAvailableMatch = false;
            for (int x = 0; x < xSize && !anyAvailableMatch; x++)
            {
                for (int y = 0; y < ySize && !anyAvailableMatch; y++)
                {
                    if(!IsTileValidAtPoint(x, y)) continue;
                    if (x - 1 > 0 && IsTileValidAtPoint(x - 1, y))
                        anyAvailableMatch = MatchResolver.AreThereAnyMatchesInPosition(_tiles[x, y], _tiles[x - 1, y].transform.position);
                    
                    if (x + 1 < xSize && IsTileValidAtPoint(x + 1, y))
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.AreThereAnyMatchesInPosition(_tiles[x, y], _tiles[x + 1, y].transform.position);
                    
                    if (y - 1 > 0 && IsTileValidAtPoint(x, y - 1))
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.AreThereAnyMatchesInPosition(_tiles[x, y], _tiles[x, y - 1].transform.position);
                    
                    if (y + 1 < ySize && IsTileValidAtPoint(x, y + 1))
                        anyAvailableMatch = anyAvailableMatch || MatchResolver.AreThereAnyMatchesInPosition(_tiles[x, y], _tiles[x, y + 1].transform.position);
                }
            }

            return anyAvailableMatch;
        }
        private IEnumerator IsGameOver()
        {
            yield return new WaitForSeconds(.2f);

            bool anyAvailableMatch = AnyMovesIsPossibleOnBoard();
            
            if (!anyAvailableMatch)
                GameManager.Instance.GameOver();
            else
            {
                //re-enable user interaction with board
                IsBoardBusy = false;
            }

            Debug.Log("GameOver " + !anyAvailableMatch);
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
                    shiftingTileCounter--;
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
            ExecutePowerUpsBehavior(matchedTile, matches);

            int effectiveTilesForMatch = 0;
            for (int i = 0; i < matches.Count; ++i)
            {
                if(matches[i] == null) continue; //tile already consumed by another match
                BoardPoint matchBoardPoint = matches[i].GetBoardPoint();
                if(!IsTileValidAtPoint(matchBoardPoint.x, matchBoardPoint.y)) continue; //tile already consumed by another match
                effectiveTilesForMatch++;
                ResetTileAtPoint(matchBoardPoint.x, matchBoardPoint.y);
                matches[i].Play(TileAnimation.Explode);
            }

            effectiveTilesForMatch++;
            BoardPoint matchedTileBoardPoint = matchedTile.GetBoardPoint();
            ResetTileAtPoint(matchedTileBoardPoint.x, matchedTileBoardPoint.y);
            matchedTile.SetAction(TileController.TileAction.Explode);
            matchedTile.Play(TileAnimation.Explode);
            AudioManager.instance.PlayAudio(Clip.Clear);

            GameManager.Instance.UpdateScore(effectiveTilesForMatch);
        }
        #endregion

        #region SuperPower Handler

        private void GetPreferredPowerUp()
        {
            string preferredPowerUp = PlayerPrefs.GetString(PlayerPrefHelper.PowerUpKey, null);
            if (string.IsNullOrEmpty(preferredPowerUp)) return;
            var powerUpToEnable = tiles.FirstOrDefault(tile => tile.config.powerUp == PowerUp.FromString(preferredPowerUp));
            if (powerUpToEnable != null)
                powerUpToEnable.enabled = true;
        }

        private void ExecutePowerUpsBehavior(TileController matchedTile, List<TileController> matches)
        {
            if (!matchedTile.IsPowerUpTile() && !matches.Any(tile => tile.IsPowerUpTile())) return;
            {
                List<TileController> powerUps = matches.Where(tile => tile.IsPowerUpTile()).ToList();
                //also check matchedTile
                if(matchedTile.IsPowerUpTile())
                    powerUps.Add(matchedTile);
                
                MatchContext context = MatchContext.Create(matchedTile, matches, powerUps);
                for (int i = 0; i < powerUps.Count; ++i)
                    powerUps[i].GetComponent<PowerUp>()?.Execute(context);

            }
        }
        #endregion
      
    }
}