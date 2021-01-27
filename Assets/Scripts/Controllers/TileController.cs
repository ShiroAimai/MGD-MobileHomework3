using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace Controllers
{
    internal static class AdjacentDirections
    {
        public readonly static Vector2 Up = Vector2.up;
        public readonly static Vector2 Down = Vector2.down;
        public readonly static Vector2 Left = Vector2.left;
        public readonly static Vector2 Right = Vector2.right;
        
        internal static class Bundle
        {
            public static readonly Vector2[] Horizontal = new Vector2[] {Left, Right};
            public static readonly Vector2[] Vertical = new Vector2[] {Up, Down};
            public static readonly Vector2[] All = new Vector2[] {Up, Down, Left, Right};
        }
    }

    public class TileController : MonoBehaviour
    {
        [SerializeField]
        private Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
        private static TileController previousSelected = null;

        private SpriteRenderer render;
        private bool isSelected = false;

        private bool matchFound = false;
    
        void Awake()
        {
            render = GetComponent<SpriteRenderer>();
        }
    
        private void OnMouseDown()
        {
            if (render.sprite == null || BoardManager.instance.IsShifting) return;

            if (isSelected)
                Deselect();
            else
            {
                if (previousSelected == null)
                    Select();
                else if(GetAllAdjacentTiles().Contains(previousSelected.gameObject)) {
                    SwapWith(previousSelected.render);
                    previousSelected.ClearAllMatches();
                    previousSelected.Deselect();
                    ClearAllMatches();
                }
                else
                {
                    previousSelected.Deselect();
                    Select();
                }
            }
        }

        #region Public

        public void ClearAllMatches() {
            if (render.sprite == null)
                return;

            ClearMatch(AdjacentDirections.Bundle.Horizontal);
            ClearMatch(AdjacentDirections.Bundle.Vertical);
        
            if (!matchFound) return;
        
            render.sprite = null;
            matchFound = false;
        
            StopCoroutine(BoardManager.instance.FindNullTiles());
            StartCoroutine(BoardManager.instance.FindNullTiles());
            AudioManager.instance.PlayAudio(Clip.Clear);
        }
        #endregion
    
        #region Private
        private void Select()
        {
            isSelected = true;
            render.color = selectedColor;
            previousSelected = this;
            AudioManager.instance.PlayAudio(Clip.Select);
        }

        private void Deselect()
        {
            isSelected = false;
            render.color = Color.white;
            previousSelected = null;
        }

        private void SwapWith(SpriteRenderer otherSprite)
        {
            if (render.sprite == otherSprite.sprite) return;

            Sprite tempSprite = otherSprite.sprite;
            otherSprite.sprite = render.sprite;
            render.sprite = tempSprite;
            AudioManager.instance.PlayAudio(Clip.Swap);
            //GUIManager.instance.MoveCounter--;
        }
    
        private List<GameObject> GetAllAdjacentTiles()
        {
            return AdjacentDirections.Bundle.All.Select(GetAdjacent).ToList();
        }
    
        private GameObject GetAdjacent(Vector2 castDir)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
            return hit.collider != null ? hit.collider.gameObject : null;
        }
    
        private List<GameObject> FindMatch(Vector2 castDir) { 
            List<GameObject> matchingTiles = new List<GameObject>(); 
            RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir); 
            while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite) { 
                matchingTiles.Add(hit.collider.gameObject);
                hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
            }
            return matchingTiles; 
        }
    
        private void ClearMatch(Vector2[] paths) 
        {
            List<GameObject> matchingTiles = new List<GameObject>();
            for (int i = 0; i < paths.Length; i++) 
            {
                matchingTiles.AddRange(FindMatch(paths[i]));
            }
            if (matchingTiles.Count >= 2) 
            {
                for (int i = 0; i < matchingTiles.Count; i++)
                {
                    matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
                }
                matchFound = true; 
            }
        }
        #endregion

    }
}