using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder
{
    
    public Cell[,] Matrix;
    
    public void CreateGrid(int width, int height, BoundsInt bounds, Tilemap tilemap, TileBase redTile)
    {
        // Initialise la matrice
        Matrix = new Cell[width, height];

        // Remplis la grille selon la Tilemap
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Créé la Cell
                Matrix[x, y] = new Cell();
                Matrix[x, y].Cost = int.MaxValue;
                Matrix[x, y].Direction = Vector2Int.zero;

                // Position de la tuile
                Vector3Int tilePos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                TileBase tile = tilemap.GetTile(tilePos);

                // Position dans le monde (centrée)
                Matrix[x, y].Position = tilemap.CellToWorld(tilePos) + tilemap.cellSize / 2f;

                // Case bloquée ou libre
                if (tile == redTile)
                    Matrix[x, y].Valid = false;
                else
                    Matrix[x, y].Valid = true;
            }
        }
    }

    public void GenerateBFS(Vector2Int targetIndex, int width, int height)
    {
        if (Matrix == null) return;
        
        // 🔁 Réinitialise tous les coûts avant de relancer le BFS
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Matrix[x, y].Cost = int.MaxValue;
            }
        }

        //Créer une queue pour gérer toutes les cellules à explorer
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        //Vérifie qu’on est dans les bornes
        if (targetIndex.x < 0 || targetIndex.x >= width || targetIndex.y < 0 || targetIndex.y >= height)
            return;

        //Lui donner un cost de 0
        Matrix[targetIndex.x, targetIndex.y].Cost = 0;
        queue.Enqueue(targetIndex);

        //Boucle principale BFS
        while (queue.Count > 0)
        {
            Vector2Int currentIndex = queue.Dequeue();
            int currentCost = Matrix[currentIndex.x, currentIndex.y].Cost;

            foreach (Vector2Int neighbor in GetNeighbors(currentIndex, width, height))
            {
                if (!Matrix[neighbor.x, neighbor.y].Valid) continue;

                if (Matrix[neighbor.x, neighbor.y].Cost == int.MaxValue)
                {
                    Matrix[neighbor.x, neighbor.y].Cost = currentCost + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    public void GenerateFlowField()
    {
        //Récupérer la matrice
        if (Matrix == null) return;
        int width = Matrix.GetLength(0);
        int height = Matrix.GetLength(1);
        //Pour chaque case récupérer sa valeur de cout
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = Matrix[x, y];
                //Si elle n'est pas Valide, direction à zero
                if (!cell.Valid)
                {
                    cell.Direction = Vector2Int.zero;
                    continue;
                }
                
                int cost = cell.Cost;
                //Si son cost n'est pas défini, direction zéro
                if (cost == int.MaxValue)
                {
                    cell.Direction = Vector2Int.zero;
                    continue;
                }
                //Chercher dans les cases adjacentes un cout plus faible
                List<Vector3> lowerDirections = new List<Vector3>();
                Vector2Int currentIndex = new Vector2Int(x, y);
                foreach (Vector2Int neighbor in GetNeighbors(currentIndex, width, height))
                {
                    Cell neighborCell = Matrix[neighbor.x, neighbor.y];
                    if (!neighborCell.Valid) continue;
                    int neighborCost = neighborCell.Cost;
                    if (neighborCost < cost)
                    {
                        Vector3 direction = neighborCell.Position - cell.Position;
                        Debug.Log("cell"+ cell.Position + "Direction = " + direction);
                        if (direction.sqrMagnitude > 0f)
                        {
                            direction.Normalize();
                            lowerDirections.Add(direction);
                        }
                    }

                    if (lowerDirections.Count == 0)
                    {
                        cell.Direction = Vector2Int.zero;
                    }
                }

                Matrix[x, y] = cell;
            }
        }
    }

    private List<Vector2Int> GetNeighbors(Vector2Int currentIndex,  int width, int height)
    {
        var directions = new List<Vector2Int>
        {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0),                         new Vector2Int(1, 0),
            new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
        };

        var neighbors = new List<Vector2Int>();
        foreach (var dir in directions)
        {
            Vector2Int n = currentIndex + dir;
            if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height)
                neighbors.Add(n);
        }
        return neighbors;
    }
}