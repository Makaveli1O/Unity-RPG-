using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class PathFinding : MonoBehaviour
{
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;
    TDTile startTile;
    TDTile targetTile;
    Map mapRef;
    TDMap map;
    void Awake(){
        try
        {
            GameObject mapObj = GameObject.Find("_Map");
            mapRef = mapObj.GetComponent<Map>();
            this.map = mapRef.map;
        }
        catch
        {
            Debug.Log("_Map reference not found.");
        }
    }

    //FIXME pretypovanie prerobit
    /// <summary>
    /// Converts found path from FindPath into Vector3s
    /// </summary>
    /// <param name="start">Start point(player pos)</param>
    /// <param name="target">End point (mouse pos)</param>
    /// <returns>List of Vector3 path</returns>
    public List<Vector3> FindPathVector(Vector3 start, Vector3 target){
        float offset = 0.5f; //to the center of the tile
        List<TDTile> path = FindPath(new int2((int)start.x,(int) start.y), new int2((int)target.x, (int)target.y));
        
        //path not found
        if (path == null)
        {
            return null;
        }else{
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (TDTile pathTile in path)
            {
                vectorPath.Add(new Vector3(pathTile.pos.x + offset, pathTile.pos.y + offset, 0));
            }

            return vectorPath;
        }
    }

    /// <summary>
    /// Debug function, that vsualizes found path. Verypoor performance !!
    /// </summary>
    /// <param name="start">Staring point(player)</param>
    /// <param name="target">End point(mouse clicked)</param>
    public void DrawPath(Vector3 start, Vector3 target){
        List<Vector3> vectorPath = FindPathVector(start, target);
        if (vectorPath != null)
        {
            Vector3 a = start;
            foreach (Vector3 v in vectorPath)
            {
                Debug.DrawLine(a, v); 
                a = v;               
            }
        }
        return;
    }
    
    /// <summary>
    /// Performs A* algorithm to find shortest path, avoiding obstacles. Each tile has
    /// isWalkable attribute which is used to determine walkable and not walkable tiles. This solution is not
    /// properly optimalized but it might not be necessary.
    /// </summary>
    /// <param name="startPos">Starting point</param>
    /// <param name="targetPos">Ending point</param>
    /// <returns>A* found path list of TDTiles</returns>
    public List<TDTile> FindPath(int2 startPos, int2 targetPos){
        //get correct tile ref
        this.startTile = mapRef.GetTile(mapRef.TileRelativePos(startPos), mapRef.TileChunkPos(startPos));
        this.targetTile = mapRef.GetTile(mapRef.TileRelativePos(targetPos), mapRef.TileChunkPos(targetPos));
        //invalid path
        if (startTile == null || targetTile == null)
        {
            return null;
        }
        
        //two sets
        Heap<TDTile> openSet = new Heap<TDTile>(this.map.getMaxTiles);
        HashSet<TDTile> closedSet = new HashSet<TDTile>();

        openSet.Add(startTile);
        while(openSet.Count > 0){
            TDTile currentTile = openSet.RemoveFirst();
            
            closedSet.Add(currentTile);
            //path found
            if (currentTile == targetTile){
                return RetracePath(startTile, targetTile);
            }
            //check neighbours
            foreach (KeyValuePair<string, TDTile> neighbour in currentTile.GetNeighbourDict())
            {
                //check if diagonal is walkable
                if(!IsRegularDiagonal(neighbour)){
                    continue;
                }
                //check if is not walkable or in closed
                if (!neighbour.Value.IsWalkable || closedSet.Contains(neighbour.Value)){
                    continue;
                }//if newpath to neighbour is shorter OR neighbour is not in OPEN
                int newCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour.Value);
                if (newCostToNeighbour < neighbour.Value.gCost || !openSet.Contains(neighbour.Value)){
                    //set neighbour's fcost
                    neighbour.Value.gCost = newCostToNeighbour;
                    neighbour.Value.hCost = GetDistance(neighbour.Value, targetTile);
                    //set where you came from to this tile
                    neighbour.Value.cameFrom = currentTile;
                    if (!openSet.Contains(neighbour.Value))
                        openSet.Add(neighbour.Value);
                }
            }
        }

        //no path found
        return null;
    }

    //FIXME github popis ako sa to robi nech to mozem dat do bakalarky
    /// <summary>
    /// Checks whenever diagonal attempt is regular or not. If certain conditions aren't matched,
    /// such as there must be gap between tiles, path won't lead through here.
    /// </summary>
    /// <param name="neighbour">Neighbour that is being processed</param>
    /// <returns>True or false if diagonal is passable or not.</returns>
    bool IsRegularDiagonal(KeyValuePair<string, TDTile> neighbour){
        switch (neighbour.Key)
        {
            case "topRight":
                if (!neighbour.Value.left.IsWalkable || !neighbour.Value.bottom.IsWalkable)
                {
                    return false;
                }
                break;
            case "topLeft":
                if (!neighbour.Value.right.IsWalkable || !neighbour.Value.bottom.IsWalkable)
                    return false;
                break;
            case "bottomLeft":
                if (!neighbour.Value.right.IsWalkable || !neighbour.Value.top.IsWalkable)
                    return false;
                break;
            case "bottomRight":
                if (!neighbour.Value.left.IsWalkable || !neighbour.Value.top.IsWalkable)
                    return false;
                break;
        }
        //correct
        return true;
    }

    /// <summary>
    /// Retraces path back to original starting tile. Each tile was marked with
    /// cameFrom that points to previously visited tile.
    /// </summary>
    /// <param name="startTile">Starting tile</param>
    /// <param name="endTile">Ending tile</param>
    /// <returns>Retraced a* path.</returns>
    private List<TDTile> RetracePath(TDTile startTile, TDTile endTile){
        List<TDTile> path = new List<TDTile>();
        TDTile currentTile = endTile;

        //retrace back to beginning
        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.cameFrom;
        }

        path.Reverse();
        return path;
    }

    
    /// <summary>
    /// Calculates cost distance with //14 y + 10(x - y) formula asigned to a* algo.
    /// </summary>
    /// <param name="a">1st tile</param>
    /// <param name="b">2nd tile</param>
    /// <returns>Distance cost value.</returns>
    private int GetDistance(TDTile a, TDTile b) {        
        int xDistance = Mathf.Abs(a.pos.x - b.pos.x);
        int yDistance = Mathf.Abs(a.pos.y - b.pos.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + STRAIGHT_COST * remaining;
    }
}
