using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager instance;

        /**
         * Elements
         */
        [Header("Tile config")] [SerializeField]
        private List<TileSprite> availableSpriteTiles = new List<TileSprite>();

        private readonly List<Tile.TileType> _currentAvailableTileTypes = new List<Tile.TileType>();


        [Header("Board config")] [SerializeField]
        private GameObject tile;

        [SerializeField] private int xSize, ySize;
        private Vector2 tilesOffset;

        private TileController[,] tiles;

        public TileController SelectedTile { get; private set; }

        public bool IsShifting { get; private set; }
        public bool IsRefilling { get; private set; }

        private int _clearedMatches = 0;
        private int _allClearMatches = -1;
        private bool HasClearedMatches => _clearedMatches == _allClearMatches;

        void Start()
        {
            instance = GetComponent<BoardManager>();

            tilesOffset = tile.GetComponent<SpriteRenderer>().bounds.size;
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
        }

        /**
	    * Creates a Board starting from bottom left
	    * And goes ahead completing row per row
	    * X are the columns, Y are the rows
	    */
        private void CreateBoard()
        {
            tiles = new TileController[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                CreateTile(x, y);
        }

        private void CreateTile(int x, int y)
        {
            float startX = transform.position.x;
            float startY = transform.position.y;

            float xOffset = tilesOffset.x;
            float yOffset = tilesOffset.y;

            _currentAvailableTileTypes.Clear();
            GameObject newTile = Instantiate(tile,
                new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0),
                tile.transform.rotation);
            var tileController = newTile.GetComponent<TileController>();
            tiles[x, y] = tileController;
            newTile.transform.parent = transform;
            TileSprite tileSprite = GetAvailableRandomTileSprite(x, y);
            newTile.GetComponent<SpriteRenderer>().sprite = tileSprite.sprite;
            tileController.Init(x, y, tileSprite.type);
            tileController.onTargetPositionReached += ProcessEndActionCallback;
        }

        private void ClearBoard()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y].onTargetPositionReached -= ProcessEndActionCallback;
                        tiles[x, y] = null;
                    }
                }
            }
        }

        private IEnumerator FindNullTiles()
        {
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

            yield return StartCoroutine(ClearAllMatchesForBoard());
        }

        private IEnumerator ShiftDown(int x, int yStart, float shiftDelay = .03f)
        {
            IsShifting = true;
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
                    controller.SetAction(TileController.TileAction.Shift, shiftedPosition);
                }
            }

            IsShifting = false;
        }
        
        private IEnumerator Refill(int xStart, int yStart, float refillDelay = 0.2f)
        {
            IsRefilling = true;
            yield return new WaitForSeconds(refillDelay);
            for (int y = yStart; y < ySize; y++)
                CreateTile(xStart, y);

            IsRefilling = false;
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

        private void ProcessEndActionCallback(TileController.TileAction action, TileController tileController)
        {
            switch (action)
            {
                case TileController.TileAction.Swap:
                    ClearAllMatchesForTile(tileController);
                    break;
                case TileController.TileAction.Shift:
                    break;
                default: return;
            }
        }

        public void ProcessTileClick(TileController clickedTile)
        {
            if (IsShifting || IsRefilling) return;

            if (SelectedTile == clickedTile)
                DeselectSelected();
            else
            {
                //no tile previously selected, select this
                if (TryToSelectTile(clickedTile))
                    return;
                //previously selected tile different from this, try to swap with this
                if (TryToSwapSelectedWith(clickedTile))
                {
                    //selectedTile.ClearAllMatches();
                    //ClearAllMatches();
                    return;
                }

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

        private bool TryToSwapSelectedWith(TileController otherTile)
        {
            if (!SelectedTile) return false;
            bool isOtherNearby = SelectedTile.IsTileAdjacentTo(otherTile);
            if (!isOtherNearby) return false;
            if (SelectedTile.GetTileType() == otherTile.GetTileType())
            {
                //can't swap animation
                return false;
            }

            if (SelectedTile.CanMatchAnyInPosition(otherTile.transform.position) ||
                otherTile.CanMatchAnyInPosition(SelectedTile.transform.position))
            {
                //can swap
                SwapTiles(SelectedTile, otherTile);
                DeselectSelected();
                //GUIManager.instance.MoveCounter--;
                return true;
            }

            //can't swap, no match found
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

            originTile.SetAction(TileController.TileAction.Swap, destinationTile.transform.position);
            destinationTile.SetAction(TileController.TileAction.Swap, originTile.transform.position);

            AudioManager.instance.PlayAudio(Clip.Swap);
        }


        private void ClearAllMatchesForTile(TileController matchedTile)
        {
            bool horizontalMatches = ClearMatchForTile(matchedTile, AdjacentDirections.Bundle.Horizontal);
            bool verticalMatches = ClearMatchForTile(matchedTile, AdjacentDirections.Bundle.Vertical);

            if (horizontalMatches || verticalMatches)
            {
                tiles[matchedTile.GetPositionXInBoard(), matchedTile.GetPositionYInBoard()] = null;
                Destroy(matchedTile.gameObject);
                AudioManager.instance.PlayAudio(Clip.Clear);
            }
            _clearedMatches++;
        }

        private bool ClearMatchForTile(TileController matchedTile, Vector2[] paths)
        {
            List<GameObject> matches = matchedTile.FindAllMatchesInPath(paths);

            if (matches.Count < Match.MatchMinLength) return false;

            for (int i = 0; i < matches.Count; ++i)
            {
                TileController tileMatch = matches[i].GetComponent<TileController>();
                tiles[tileMatch.GetPositionXInBoard(), tileMatch.GetPositionYInBoard()] = null;
                Destroy(matches[i]);
                //matchTile.animator.SetTrigger("Explode");
            }

            return true;
        }

        private IEnumerator ClearAllMatchesForBoard()
        {
            List<BoardEntry> tilesWithMatches = new List<BoardEntry>();
            //Check if new matches have been formed
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tiles[x, y].CanMatchAnyInPosition(tiles[x, y].transform.position))
                        tilesWithMatches.Add(new BoardEntry(x, y));
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
                yield return new WaitForSeconds(.05f);
                ClearAllMatchesForTile(tiles[boardEntry.x, boardEntry.y]);
            }
        }

        private struct BoardEntry
        {
            public int x;
            public int y;

            public BoardEntry(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}