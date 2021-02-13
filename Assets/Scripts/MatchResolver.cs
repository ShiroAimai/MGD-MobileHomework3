using System.Collections.Generic;
using System.Linq;
using Controllers;
using Models;
using UnityEngine;
using Utils;

public class MatchContext
{
    public List<TileController> _matches;
    public List<TileController> _powerUps;
    private MatchContext()
    {
    }

    public static MatchContext Create(List<TileController> matches, List<TileController> powerUps)
    {
        return new MatchContext() { _matches = matches, _powerUps = powerUps};
    }
}
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
    private static readonly int MatchMinLength = 2;

    #region Public

    public static bool ResolveMatch(TileController tile, out List<TileController> matches)
    {
        matches = new List<TileController>();
        if (tile.IsPowerUpTile())
            ResolveMatchesForPowerUpTile(tile, matches);
        else
            ResolveMatchesForStandardTile(tile, matches);

        return matches.Count > 0;
    }

    public static bool AreThereAnyMatchesInPosition(TileController tile, Vector3? position = null)
    {
        return tile.IsPowerUpTile()
            ? CheckMatchesForPowerUpTile(tile, position)
            : CheckMatchesForStandardTile(tile, position);
    }

    #endregion

    #region Private

    private static void ResolveMatchesForPowerUpTile(TileController powerUpTile, List<TileController> matches)
    {
        ResolveMatchInPathForPowerUpTile(powerUpTile, AdjacentDirections.Bundle.Horizontal, ref matches);
        ResolveMatchInPathForPowerUpTile(powerUpTile, AdjacentDirections.Bundle.Vertical, ref matches);
    }

    private static void ResolveMatchInPathForPowerUpTile(TileController powerUpTile, Vector2[] path,
        ref List<TileController> matches)
    {
        if (path.Length != AdjacentDirections.Bundle.Vertical.Length ||
            path.Length != AdjacentDirections.Bundle.Horizontal.Length)
        {
            Debug.Log("Expected a path containing a direction and its opposite");
            return;
        }

        var matchesInPath = FindAllMatchesInPathFromPositionForTile(powerUpTile, path);

        var matchesInDirection = matchesInPath
            .First(candidate => candidate.direction == path[0]).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        var matchesInOppositeDirection = matchesInPath
            .First(candidate => candidate.direction == path[1]).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();

        if (matchesInDirection.Count <= 0 && matchesInOppositeDirection.Count <= 0) return;

        if ((matchesInDirection.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType() ==
             matchesInOppositeDirection.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType()) ||
            (matchesInDirection.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true ||
             matchesInOppositeDirection.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() == true)
        )
        {
            matches.AddRange(matchesInDirection);
            matches.AddRange(matchesInOppositeDirection);
        }
        else
        {
            if (matchesInDirection.Count >= MatchMinLength)
                matches.AddRange(matchesInDirection);
            if (matchesInOppositeDirection.Count >= MatchMinLength)
                matches.AddRange(matchesInOppositeDirection);
        }
    }

    private static void ResolveMatchesForStandardTile(TileController tile, List<TileController> matches)
    {
        ResolveMatchInPathForStandardTile(tile, AdjacentDirections.Bundle.Horizontal, ref matches);
        ResolveMatchInPathForStandardTile(tile, AdjacentDirections.Bundle.Vertical, ref matches);
    }

    private static void ResolveMatchInPathForStandardTile(TileController tile, Vector2[] path,
        ref List<TileController> matches)
    {
        var matchesInPath = FindAllMatchesInPathFromPositionForTile(tile, path)
            .SelectMany(candidate => candidate.candidates.Select(el => el.GetComponent<TileController>()))
            .ToList();
        if (matchesInPath.Count >= MatchMinLength)
            matches.AddRange(matchesInPath);
    }

    private static bool CheckMatchesInPathForPowerUpTile(TileController powerUpTile, Vector2[] path,
        Vector3? position = null)
    {
        if (path.Length != AdjacentDirections.Bundle.Vertical.Length ||
            path.Length != AdjacentDirections.Bundle.Horizontal.Length)
        {
            Debug.Log("Expected a path containing a direction and its opposite");
            return false;
        }

        bool anyMatchesInPath = false;

        var matchesInPath = FindAllMatchesInPathFromPositionForTile(powerUpTile, path, position);
        var matchesInDirection = matchesInPath
            .First(candidate => candidate.direction == path[0]).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();
        var matchesInOppositeDirection = matchesInPath
            .First(candidate => candidate.direction == path[1]).candidates
            .Select(el => el.GetComponent<TileController>())
            .ToList();

        if (matchesInDirection.Count > 0 || matchesInOppositeDirection.Count > 0)
        {
            anyMatchesInPath = (matchesInDirection.FirstOrDefault()?.GetComponent<TileController>()?.GetTileType() ==
                                matchesInOppositeDirection.FirstOrDefault()?.GetComponent<TileController>()
                                    ?.GetTileType()) ||
                               (matchesInDirection.FirstOrDefault()?.GetComponent<TileController>()?.IsPowerUpTile() ==
                                true ||
                                matchesInOppositeDirection.FirstOrDefault()?.GetComponent<TileController>()
                                    ?.IsPowerUpTile() == true) ||
                               matchesInDirection.Count >= MatchMinLength ||
                               matchesInOppositeDirection.Count >= MatchMinLength;
        }

        return anyMatchesInPath;
    }

    private static bool CheckMatchesForPowerUpTile(TileController powerUpTile, Vector3? position = null)
    {
        return CheckMatchesInPathForPowerUpTile(powerUpTile, AdjacentDirections.Bundle.Horizontal, position) ||
               CheckMatchesInPathForPowerUpTile(powerUpTile, AdjacentDirections.Bundle.Vertical, position);
    }

    private static bool CheckMatchesInPathForStandardTile(TileController tile, Vector2[] path, Vector3? position = null)
    {
        return FindAllMatchesInPathFromPositionForTile(tile, path, position)
            .Select(candidate => candidate.candidates.Count)
            .Sum() >= MatchMinLength;
    }

    private static bool CheckMatchesForStandardTile(TileController tile, Vector3? position = null)
    {
        return CheckMatchesInPathForStandardTile(tile, AdjacentDirections.Bundle.Horizontal, position) ||
               CheckMatchesInPathForStandardTile(tile, AdjacentDirections.Bundle.Vertical, position);
    }

    private static List<MatchCandidate> FindAllMatchesInPathFromPositionForTile(TileController tile, Vector2[] path,
        Vector3? position = null)
    {
        var startPosition = position ?? tile.transform.position;
        return path
            .Select(dir => FindMatchInDirectionFromPositionForTile(tile, startPosition, dir))
            .ToList();
    }

    private static MatchCandidate FindMatchInDirectionFromPositionForTile(TileController tile, Vector3 position,
        Vector2 castDir)
    {
        TileState.TileType tileType = tile.GetTileType();

        MatchCandidate matchCandidate = MatchCandidate.Create(castDir);

        RaycastHit2D hit = Physics2D.Raycast(position, castDir);
        while (hit.collider != null &&
               hit.collider.gameObject != tile.gameObject &&
               (tileType == TileState.TileType.PowerUp ||
                hit.collider.gameObject.GetComponent<TileController>().GetTileType() == tileType ||
                hit.collider.gameObject.GetComponent<TileController>().IsPowerUpTile())
        )
        {
            //take the first item found as default type if power up
            if (tileType == TileState.TileType.PowerUp)
                tileType = hit.collider.gameObject.GetComponent<TileController>().GetTileType();

            matchCandidate.candidates.Add(hit.collider.gameObject);
            hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
        }

        return matchCandidate;
    }

    #endregion
}