using Unity.Mathematics; //int2
using System.Collections.Generic;
/// <summary>
/// TDTile struct is holding intel about each tile.
/// This intel is later used to determine it's type,
/// position in the world etc.
/// </summary>
[System.Serializable]
public class TDTile : IHeapItem<TDTile>
{  
    private int heapIndex;
    public bool partial = false; //indicates partial tile only ( cliff ends )
    public int z_index;
    public int2 pos;
    [System.NonSerialized]
    public BiomePreset biome;
    public float temperature;
    public float height;
    public float precipitation;
    public bool landmass;
    public string waterType;
    /*
        This comes in handy for things, such as creating paths, bitmasking, or flood filling
    */
    public TDTile left;
    public TDTile topLeft;
    public TDTile top;
    public TDTile topRight;
    public TDTile right;
    public TDTile bottomRight;
    public TDTile bottom;
    public TDTile bottomLeft;
    //type of edge between biomes (for smooth transition)
    public EdgeType edgeType;
    //type of edge of hill
    public EdgeType hillEdge;
    //trees
    public bool stair = false;

    /* pathfinding stuff */
    public TDTile cameFrom;
    private bool walkable;
    public bool IsWalkable
    {
        set{walkable = value;}
        get{return walkable;}
    }

    public int gCost;
    public int hCost;
    /// <summary>
    /// Calculating f cost from hCost and gcost (a* algo)
    /// </summary>
    /// <value>reeturned heurestic cost</value>
    public int fCost{
        get{return hCost + gCost;}
    }

    /// <summary>
    /// Heap indexvalue used in heap generic class
    /// </summary>
    /// <value>integet index</value>
    public int HeapIndex{
        get{
            return heapIndex;
        }
        set{
            heapIndex = value;
        }
    }

    /// <summary>
    /// Compare function implementinf heapitem interface
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>1 if current item has higher priority, 0 otherwise</returns>
    public int CompareTo(TDTile tile){
        int compare = fCost.CompareTo(tile.fCost);
        //if both are equal, ise heurestic
        if (compare == 0)
        {
            compare = hCost.CompareTo(tile.hCost);
        }
        return -compare;    //int compare to works opposite way 
    }


    /// <summary>
    /// Return true if on this tile can actually be placed object. If tile is in the
    /// water, or is cliff return false.
    /// </summary>
    /// <returns></returns>
    public bool IsPlacable(){
        if (this.biome.type != "ocean" &&  this.biome.type != "water" 
        && this.hillEdge == EdgeType.none &&  this.edgeType == EdgeType.none){
            return true;
        }else{
            return false;
        }
    }

    /// <summary>
    /// Returns list of neighbours ( for a* algorithm )
    /// </summary>
    /// <returns>List of all neighbours</returns>
    public List<TDTile> GetNeighbourList(){
        List<TDTile> ret = new List<TDTile>();
        ret.Add(left);
        ret.Add(topLeft);
        ret.Add(top);
        ret.Add(topRight);
        ret.Add(right);
        ret.Add(bottomRight);
        ret.Add(bottom);
        ret.Add(bottomLeft);
        return ret;
    }

    /// <summary>
    /// Returns dict of neighbours ( for a* algorithm )
    /// </summary>
    /// <returns>List of all neighbours</returns>
    public Dictionary<string, TDTile> GetNeighbourDict(){
        Dictionary<string,TDTile> ret = new Dictionary<string,TDTile>();
        ret["left"] = this.left;
        ret["topLeft"] = this.topLeft;
        ret["top"] = this.top;
        ret["topRight"] = this.topRight;
        ret["right"] = this.right;
        ret["bottomRight"] = this.bottomRight;
        ret["bottom"] = this.bottom;
        ret["bottomLeft"] = this.bottomLeft;

        return ret;
    }
}
public enum EdgeType
{
    //regular
    none,
    left,   
    top,    
    right,  
    bot,
    //two sides
    botLeft,
    topLeft,
    botRight,
    topRight,
    //corners
    botLeftOnly,
    topLeftOnly,
    botRightOnly,
    topRightOnly,
    //rare cases
    rareTRB, //TRB -> top, right, bottom
    rareLTR,
    rareRBL,
    rareBLT,
    rareTB,
    //cliffs
    cliff,
    cliffLeft,
    cliffEndLeft,
    cliffRight,
    cliffEndRight,
    cliffBot,
    cliffEndBot,
    staircase,
    staircaseTop,
    staircaseBot
}
