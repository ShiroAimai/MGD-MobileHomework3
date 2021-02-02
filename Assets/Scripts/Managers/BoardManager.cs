using System;
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
        private List<TileSprite> characters = new List<TileSprite>();

        private readonly List<Tile.TileType> currentAvailableCharacters = new List<Tile.TileType>();


        [Header("Board config")] [SerializeField]
        private GameObject tile;

        [SerializeField] private int xSize, ySize;
        private Vector2 tilesOffset;

        private TileController[,] tiles;

        public TileController SelectedTile { get; set; }

        public bool IsShifting { get; private set; }

        void Start()
        {
            instance = GetComponent<BoardManager>();

            tilesOffset = tile.GetComponent<SpriteRenderer>().bounds.size;
            CreateBoard(tilesOffset.x, tilesOffset.y);
        }

        private void OnDestroy()
        {
            ClearBoard();
        }

        /**
	 * Creates a Board starting from bottom left
	 * And goes ahead completing row per row
	 * X are the columns, Y are the rows
	 */
        private void CreateBoard(float xOffset, float yOffset)
        {
            tiles = new TileController[xSize, ySize];

            float startX = transform.position.x;
            float startY = transform.position.y;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    currentAvailableCharacters.Clear();
                    GameObject newTile = Instantiate(tile,
                        new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0),
                        tile.transform.rotation);
                    var tileController = newTile.GetComponent<TileController>();
                    tiles[x, y] = tileController;
                    newTile.transform.parent = transform;
                    TileSprite tileSprite = GetAvailableRandomTileSprite(x, y);
                    newTile.GetComponent<SpriteRenderer>().sprite = tileSprite.sprite;
                    tileController.Init(x, y, tileSprite.type);
                    tileController.onSwapped += ClearAllMatchesForTile;
                }
            }
        }

        private void ClearBoard()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y].onSwapped -= ClearAllMatchesForTile;
                        tiles[x, y] = null;
                    }
                }
            }
        }

        /* public IEnumerator FindNullTiles()
         {
             for (int x = 0; x < xSize; x++)
             {
                 for (int y = 0; y < ySize; y++)
                 {
                     if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
                     {
                         yield return StartCoroutine(ShiftTilesDown(x, y));
                         break;
                     }
                 }
             }
 
             //Check if new matches have been formed
             for (int x = 0; x < xSize; x++)
             {
                 for (int y = 0; y < ySize; y++)
                 {
                     tiles[x, y].GetComponent<TileController>().ClearAllMatches();
                 }
             }
         }
 
         private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f)
         {
             IsShifting = true;
             List<SpriteRenderer> renders = new List<SpriteRenderer>();
             int nullCount = 0;
 
             for (int y = yStart; y < ySize; y++)
             {
                 SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
                 if (render.sprite == null)
                     nullCount++;
                 renders.Add(render);
             }
 
             for (int i = 0; i < nullCount; i++)
             {
                 //GUIManager.instance.Score += 50;
                 yield return new WaitForSeconds(shiftDelay);
                 for (int k = 0; k < renders.Count - 1; k++)
                 {
                     renders[k].sprite = renders[k + 1].sprite;
                     renders[k + 1].sprite = GetAvailableRandomSprite(x, ySize - 1);
                 }
             }
 
             IsShifting = false;
         }
 */
        private TileSprite GetAvailableRandomTileSprite(int x, int y)
        {
            currentAvailableCharacters.Clear();
            currentAvailableCharacters.AddRange(characters.Select(tileSprite => tileSprite.type));

            if (x > 0 && tiles[x - 1, y] != null)
                currentAvailableCharacters.Remove(tiles[x - 1, y].GetTileType());
            if (x < xSize - 1 && tiles[x + 1, y] != null)
                currentAvailableCharacters.Remove(tiles[x + 1, y].GetTileType());
            if (y > 0 && tiles[x, y - 1] != null)
                currentAvailableCharacters.Remove(tiles[x, y - 1].GetTileType());

            var randomAvailableType = currentAvailableCharacters[Random.Range(0, currentAvailableCharacters.Count)];
            return characters
                .FirstOrDefault(spriteTile =>
                    spriteTile.type == randomAvailableType);
        }

        public void ProcessTileClick(TileController clickedTile)
        {
            if (IsShifting) return;

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
            int originX = originTile.GetPositionXInBoard();
            int originY = originTile.GetPositionYInBoard();

            originTile.swapPosition = destinationTile.transform.position;
            destinationTile.swapPosition = originTile.transform.position;

            originTile.UpdatePositionInBoard(destinationTile.GetPositionXInBoard(),
                destinationTile.GetPositionYInBoard());
            destinationTile.UpdatePositionInBoard(originX, originY);

            AudioManager.instance.PlayAudio(Clip.Swap);
        }


        private void ClearAllMatchesForTile(TileController matchedTile)
        {
            bool horizontalMatches = ClearMatchForTile(matchedTile, AdjacentDirections.Bundle.Horizontal);
            bool verticalMatches = ClearMatchForTile(matchedTile, AdjacentDirections.Bundle.Vertical);

            if (horizontalMatches || verticalMatches)
            {
                Destroy(matchedTile.gameObject);
            }

            //StopCoroutine(BoardManager.instance.FindNullTiles());
            //StartCoroutine(BoardManager.instance.FindNullTiles());
            AudioManager.instance.PlayAudio(Clip.Clear);
        }

        private bool ClearMatchForTile(TileController matchedTile, Vector2[] paths)
        {
            List<GameObject> matches = matchedTile.FindAllMatches();

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
    }
}