using System.Collections.Generic;
using Unity.Mathematics; //int2
using Random = UnityEngine.Random;

/// <summary>
/// Structure holding map dimensions intel
/// </summary>
[System.Serializable]
public struct TDMap 
{
    /*public List<TDTile> forestTiles;    //all forest tiles
    public List<TDTile> ashlandTiles;   //all ashland tiles
    public List<TDTile> desertTiles;    //all desert tiles
    public List<TDTile> jungleTiles;    //all jungle tiles*/
    public Dictionary<string, List<TDTile>> biomeTiles;
    public Dictionary<int2, WorldChunk> chunks;
    public int width;
    public int height;
    public int renderDistance;

    // FIXME doc
    public void InitializeTileLists(){
        biomeTiles = new Dictionary<string, List<TDTile>>();
        this.biomeTiles.Add("ashland", new List<TDTile>());
        this.biomeTiles.Add("forest", new List<TDTile>());
        this.biomeTiles.Add("desert", new List<TDTile>());
        this.biomeTiles.Add("jungle", new List<TDTile>());
    }
    //FIXME doc
    public void ShuffleArrays(){
        foreach (List<TDTile> list in biomeTiles.Values)
        {
            list.Fisher_Yates_shuffle();
        }
    }
}
