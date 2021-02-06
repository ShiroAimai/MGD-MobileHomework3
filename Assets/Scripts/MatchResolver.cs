using System.Collections.Generic;
using System.Linq;
using Controllers;
using Models;
using UnityEngine;
using Utils;

public class MatchCandidate
{
    public Vector2 direction;
    public List<GameObject> candidates;

    private MatchCandidate()
    {
    }

    public static MatchCandidate Create(Vector2 _direction)
    {
        return new MatchCandidate() {direction = _direction, candidates = new List<GameObject>()};
    }
}

public static class MatchResolver
{
    private static int MatchMinLength = 2;

    public static bool ResolveMatch(TileController tile, out List<TileController> matches)
    {
        matches = new List<TileController>();
        if (tile.IsPowerUpTile())
            ResolveMatchForPowerUpTile(tile, matches);
        else
            ResolveMatchForStandardTile(tile, matches);

        return matches.Count > 0;
    }

    private static void ResolveMatchForPowerUpTile(TileController powerUpTile, List<TileController> matches)
    {
        var horizontalMatches = FindAllMatchesInPathFromPositionForTile(powerUpTile, AdjacentDirections.Bundle.Horizontal);
        var leftMatches = horizontalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Left).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        var rightMatches = horizontalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Right).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();

        if (leftMatches.Count > 0 || rightMatches.Count > 0)
        {
            if ((leftMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType() ==
                 rightMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType()) ||
                (leftMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true ||
                 rightMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true)
            )
            {
                matches.AddRange(leftMatches);
                matches.AddRange(rightMatches);
            }
            else
            {
                if (leftMatches.Count >= MatchMinLength)
                    matches.AddRange(leftMatches);
                if (rightMatches.Count >= MatchMinLength)
                    matches.AddRange(rightMatches);
            }
        }

        var verticalMatches = FindAllMatchesInPathFromPositionForTile(powerUpTile, AdjacentDirections.Bundle.Vertical);
        var upMatches = verticalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Up).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        ;
        var downMatches = verticalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Down).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        ;
        if (upMatches.Count > 0 || downMatches.Count > 0)
        {
            if ((upMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType() ==
                 downMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType()) ||
                (upMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true ||
                 downMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true)
            )
            {
                matches.AddRange(upMatches);
                matches.AddRange(downMatches);
            }
            else
            {
                if (upMatches.Count >= MatchMinLength)
                    matches.AddRange(upMatches);
                if (downMatches.Count >= MatchMinLength)
                    matches.AddRange(downMatches);
            }
        }
    }

    private static void ResolveMatchForStandardTile(TileController tile, List<TileController> matches)
    {
        var horizontalMatchCandidates = FindAllMatchesInPathFromPositionForTile(tile, AdjacentDirections.Bundle.Horizontal)
            .SelectMany(candidate => candidate.candidates.Select(el => el.GetComponent<TileController>()))
            .ToList();
        if (horizontalMatchCandidates.Count >= MatchMinLength)
            matches.AddRange(horizontalMatchCandidates);
        var verticalMatchCandidates = FindAllMatchesInPathFromPositionForTile(tile, AdjacentDirections.Bundle.Vertical)
            .SelectMany(candidate => candidate.candidates.Select(el => el.GetComponent<TileController>()))
            .ToList();
        if (verticalMatchCandidates.Count >= MatchMinLength)
            matches.AddRange(verticalMatchCandidates);
    }

    public static bool CanMatchAnyInPosition(TileController tile, Vector3? position = null)
    {
        return tile.IsPowerUpTile()
            ? CanMatchAnyForPowerUpTile(tile, position)
            : CanMatchAnyForStandardTile(tile, position);
    }

    private static bool CanMatchAnyForPowerUpTile(TileController powerUpTile, Vector3? position = null)
    {
        bool anyHorizontalMatch = false;
        var horizontalMatches = FindAllMatchesInPathFromPositionForTile(powerUpTile, AdjacentDirections.Bundle.Horizontal, position);
        var leftMatches = horizontalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Left).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        var rightMatches = horizontalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Right).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();

        if (leftMatches.Count > 0 || rightMatches.Count > 0)
        {
            anyHorizontalMatch = (leftMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType() ==
                                  rightMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType()) ||
                                 (leftMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true ||
                                 rightMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true) ||
                                 leftMatches.Count >= MatchMinLength ||
                                 rightMatches.Count >= MatchMinLength;
        }

        bool anyVerticalMatch = false;
        var verticalMatches = FindAllMatchesInPathFromPositionForTile(powerUpTile, AdjacentDirections.Bundle.Vertical, position);
        var upMatches = verticalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Up).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        ;
        var downMatches = verticalMatches
            .First(candidate => candidate.direction == AdjacentDirections.Down).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        ;
        if (upMatches.Count > 0 || downMatches.Count > 0)
        {
            anyVerticalMatch = (upMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType() ==
                                downMatches.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType()) ||
                               (upMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true ||
                                downMatches.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true) ||
                               upMatches.Count >= MatchMinLength ||
                               downMatches.Count >= MatchMinLength;
        }

        return anyHorizontalMatch || anyVerticalMatch;
    }

    private static bool CanMatchAnyForStandardTile(TileController tile, Vector3? position = null)
    {
        bool horizontalMatches = FindAllMatchesInPathFromPositionForTile(tile, AdjacentDirections.Bundle.Horizontal, position)
            .Select(candidate => candidate.candidates.Count)
            .Sum() >= MatchMinLength;
        bool verticalMatches = FindAllMatchesInPathFromPositionForTile(tile, AdjacentDirections.Bundle.Vertical, position)
            .Select(candidate => candidate.candidates.Count)
            .Sum() >= MatchMinLength;
        return horizontalMatches || verticalMatches;
    }
    
    private static List<MatchCandidate> FindAllMatchesInPathFromPositionForTile(TileController tile, Vector2[] path, Vector3? position = null)
    {
        var startPosition = position ?? tile.transform.position;
        return path
            .Select(dir => FindMatchInDirectionFromPositionForTile(tile, startPosition, dir))
            .ToList();
    }
        
    private static MatchCandidate FindMatchInDirectionFromPositionForTile(TileController tile, Vector3 position, Vector2 castDir)
    {
        Tile.TileType tileType = tile.GetTileType();
            
        MatchCandidate matchCandidate = MatchCandidate.Create(castDir);
            
        RaycastHit2D hit = Physics2D.Raycast(position, castDir);
        while (hit.collider != null &&
               hit.collider.gameObject != tile.gameObject && 
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
}