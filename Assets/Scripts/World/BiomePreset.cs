using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;

/// <summary>
/// Holds information and functionality for single biome.
/// </summary>
[CreateAssetMenu(fileName = "Biome Preset", menuName = "New Biome Preset")]
public class BiomePreset : ScriptableObject
{
    public string type;
    public Sprite testTile;
    public Sprite[] tiles;
    public Sprite[] specialTiles;
    public EdgeTile[] edgeTiles;
    public Objects[] objects;
    public EnemyPreset[] enemies;
    public float height;
    public float temperature; //min
    public float maxTemperature;
    public float precipitation;//min
    public float maxPrecipitation;
    public float treeRadius; //bigger radius - > less dense
    public HashSet<TDTile> possibleStairs = new HashSet<TDTile>();

    /// <returns>Returns random assigned tile to this biome.</returns>
    public Sprite GetRandomSprite(){
        return tiles[Random.Range(0,tiles.Length)];
    }

    public Sprite GetRandomSpecial(){
        try{
            return specialTiles[Random.Range(0,specialTiles.Length)];
        }catch{
            return tiles[Random.Range(0,tiles.Length)];
        }
    }
    public Sprite GetTestSprite(){
        return testTile;
    }

    /// <summary>
    /// Check entire surroundings of given tile, and then determines which tile to pick accordingly.
    /// </summary>
    /// <param name="tile">Tile reference</param>
    /// <returns>Sprite for given tile</returns>
    public Sprite GetTileSprite(TDTile tile){
        bool water = false;
        Sprite tileToReturn;
        float chance = Random.value; // <0,1> 
        //spawn random flower, little rock etc. merged with tile
        if(chance < 0.98f){
            tileToReturn = GetRandomSprite();
        }else{
            tileToReturn = GetRandomSpecial();
        }
        
        //assign correct water type
        if (tile.biome.type == "water"){
            tileToReturn = GetWaterType(tile);
            water = true;
        }
        SetWalkability(tile);

 
        string tileName = "";
        bool rare = false; //rare occasions (3 sided tile)
        //cliffs for mountains first
        if(tile.hillEdge != EdgeType.none || (tile.hillEdge == EdgeType.none && tile.bottom.hillEdge == EdgeType.bot)){
            tileName = GetCliffTile(tile);
        }
        if (tileName == "")
        {
            switch (tile.edgeType)
            {
                //* regular edged tiles 
                case EdgeType.left:
                    tileName = tile.left.biome.type + "_left";
                    break;
                case EdgeType.right:
                    tileName = tile.right.biome.type + "_right";
                    break;
                case EdgeType.top:
                    tileName = tile.top.biome.type + "_top";
                    break;
                case EdgeType.bot:
                    tileName = tile.bottom.biome.type + "_bot";
                    break;
                //* 2-sided edge tiles 
                case EdgeType.botLeft:
                    tileName = tile.left.biome.type + "_bot_left";
                    break;
                case EdgeType.botRight:
                    tileName = tile.right.biome.type + "_bot_right";
                    break;
                case EdgeType.topLeft:
                    tileName = tile.left.biome.type + "_top_left";
                    break;
                case EdgeType.topRight:
                    tileName = tile.right.biome.type + "_top_right";
                    break;
                // corners 
                case EdgeType.botLeftOnly:
                    tileName = tile.bottomLeft.biome.type + "_corner_bot_left";
                    break;
                case EdgeType.botRightOnly:
                    tileName = tile.bottomRight.biome.type + "_corner_bot_right";
                    break;
                case EdgeType.topLeftOnly:
                    tileName = tile.topLeft.biome.type + "_corner_top_left";
                    break;
                case EdgeType.topRightOnly:
                    tileName = tile.topRight.biome.type + "_corner_top_right";
                    break;
                // 3 side surrounded -> rare case 
                case EdgeType.rareBLT:
                    tileName = tile.bottom.biome.type + "_rare_BLT";
                    rare= true;
                    break;
                case EdgeType.rareTRB:
                    tileName = tile.right.biome.type + "_rare_TRB";
                    rare= true;
                    break;
                case EdgeType.rareLTR:
                    tileName = tile.top.biome.type + "_rare_LTR";
                    rare= true;
                    break;
                case EdgeType.rareRBL:
                    tileName = tile.left.biome.type + "_rare_RBL";
                    rare= true;
                    break;
                case EdgeType.rareTB:
                    tile.biome = tile.bottom.biome;
                    tile.edgeType = EdgeType.none;
                    rare= true;
                    break;
            }    
        }

        if (edgeTiles.Length != 0) // temporary during testing
        {
            foreach (var t in edgeTiles){
                if (t.name == tileName){
                    tile.IsWalkable = t.walkable;
                    tileToReturn = t.sprite;
                } 
            }
        }

        //height levels
        tileToReturn = MountainsRenderer(tile, tileToReturn, rare, water);
        
        

        return tileToReturn;
    }

    private void SetWalkability(TDTile tile){
        if (this.type == "ocean" || this.type == "water")
        {
            tile.IsWalkable = false;
        }else{
            tile.IsWalkable = true;
        }
    }

    /// <summary>
    /// This functions handles problematic mountain biome rendering. It gets rid of problematic tiles,
    /// (outside of mountain etc. ..) and set correct edges (cliffs, so on..).
    /// </summary>
    /// <param name="tile">Tile reference</param>
    /// <param name="tileToReturn">Sprite to return</param>
    /// <param name="rare">Does tile match specific conditions</param>
    /// <param name="water">Rendering water</param>
    /// <returns>Returns correct sprite for given tile.</returns>
    private Sprite MountainsRenderer(TDTile tile, Sprite tileToReturn, bool rare, bool water){
        //rare occurences are eleminated within mountain biome
        if (rare){
            tile.biome = (tile.biome != tile.left.biome) ? tile.left.biome : tile.right.biome;
            tile.edgeType = EdgeType.none;
            if (!water)
            {
                tileToReturn = tile.biome.GetRandomSprite();
            }
        }else{
            tileToReturn = GetMountainEdgeTile(tile, tileToReturn);
        }
        return tileToReturn;
    }
    /*
        @function GetMountainEdgeTile
        ----------------------------------

    */
    /// <summary>
    /// Handles assigning correct texture to mountains. Mountains are 2 tiles high, si edges must be
    /// accordingly assigned. Since rendering is done in upwards direction, checking bottom tile is used to
    /// determine if current tile should be just cliff or it's already top of the mountain.
    /// </summary>
    /// <param name="tile">Tile reference</param>
    /// <param name="spriteToReturn">Sprite to return</param>
    /// <param name="recursive">Recursive call flag</param>
    /// <returns>Sprite for given tile</returns>
    private Sprite GetMountainEdgeTile(TDTile tile, Sprite spriteToReturn, TDTile recursive = null){
        string tileName = "";
        EdgeType et = EdgeType.none;

        //in recursive calls is bottomRight used to determine future tile edgeType
        if (recursive !=null){
            et = recursive.hillEdge;
        }else{
            if (tile.edgeType == EdgeType.staircase){
                et = tile.hillEdge;
            }
            et = tile.bottom.hillEdge;
        }
        
        switch (et)
        {
            case EdgeType.staircase:
                tileName = "staircase";
                break;
            case EdgeType.staircaseBot:
                tileName = "staircase_bot";
                break;
            case EdgeType.staircaseTop:
                tileName = "staircase_top";
                break;
            //second tile of the mountain to appear higher but on the edges of mountains(left)
            case EdgeType.botLeft:
                tile.hillEdge = EdgeType.cliffLeft;
                tileName = "cliff_left";
                break;
            //second tile of the mountain to appear higher but on the edges of mountains(right)
            case EdgeType.botRight:
                tile.hillEdge = EdgeType.cliffRight;
                tileName = "cliff_right";
                break;
            //corners of cliffs
            case EdgeType.cliffLeft:
                tile.hillEdge = EdgeType.cliffEndLeft;
                tile.partial = true;
                tileName = "cliff_end_botLeft";
                break;
            case EdgeType.cliffRight:
                tile.hillEdge = EdgeType.cliffEndRight;
                tile.partial = true;
                tileName = "cliff_end_botRight";
                break;
            //cliff purely on the left side of the mountain
            case EdgeType.cliffEndLeft:

                //very left bottom corner only cliff
                if (tile.left.hillEdge == EdgeType.cliffLeft ||
                    tile.left.hillEdge == EdgeType.botLeft   ||
                    tile.top.hillEdge == EdgeType.topLeft){
                    tile.partial = true;
                    tile.hillEdge = EdgeType.left;
                    tileName = "cliff_end_left";
                }
                break;
            //right side must be checket in advance
            case EdgeType.cliffEndRight:
                //determine right neighbour in advance
                try{ //FIXME not necessary anymore?
                    GetMountainEdgeTile(tile.right, null, tile.bottomRight);
                    GetMountainEdgeTile(tile.top, null, tile);
                }catch{
                   
                }

                if (tile.right.hillEdge == EdgeType.cliffRight 
                || tile.top.hillEdge == EdgeType.right
                || tile.right.hillEdge == EdgeType.botRight){
                    tile.hillEdge = EdgeType.right;
                    tile.partial = true;
                    tileName = "cliff_end_right";
                }
                break;
            //bottom edge of the cliff
            case EdgeType.cliff:
                if (tile.left.hillEdge != EdgeType.cliffLeft)
                {
                    tile.hillEdge = EdgeType.cliffEndBot;
                    tile.partial = true;
                    tileName = "cliff_end_bot";
                }
                break;
            case EdgeType.cliffBot:
                tile.hillEdge = EdgeType.cliff;
                tileName = "cliff";
                break;
            //left edge of the mountain is beneath this tile
            case EdgeType.left:
                if (tile.left.hillEdge == EdgeType.cliffLeft
                 || tile.left.hillEdge == EdgeType.botLeft)
                {
                    tile.hillEdge = EdgeType.left;
                    tile.partial = true;
                    tileName = "cliff_end_left";
                }
                break;
            //right edge of the mountain is beneath this tile
            case EdgeType.right:
                try{
                    GetMountainEdgeTile(tile.right, null, tile.bottomRight); //throws null exepction for some reason 
                    if (tile.right.hillEdge == EdgeType.cliffRight || tile.bottomRight.hillEdge == EdgeType.botRight)
                    {
                        tile.hillEdge = EdgeType.right;
                        tile.partial = true;
                        tileName = "cliff_end_right";
                    }
                }catch{
                    tile.hillEdge = EdgeType.right;
                    tile.partial = true;
                    tileName = "cliff_end_right";
                }
                break;

        } 

        //find according tiles by name in array
        foreach (var t in edgeTiles){
            if (t.name == tileName){
                tile.IsWalkable = t.walkable;
                spriteToReturn = t.sprite;
            } 
        }


        return spriteToReturn;
    }

    /// <summary>
    /// Handles edges of cliffs while generating mountains.
    /// </summary>
    /// <param name="tile">Tile refetence</param>
    /// <return>Tile name string.</return>
    private string GetCliffTile(TDTile tile){
        string tileName = "";
        if (tile.bottom.hillEdge == EdgeType.bot && tile.hillEdge == EdgeType.none)
        {

            tile.hillEdge = EdgeType.cliff;
            tileName = "cliff";
        }

        switch (tile.hillEdge)
        {
            //* regular edged tiles 
            case EdgeType.left:
                tile.partial = true;
                tileName = "cliff_end_left";
                break;
            case EdgeType.right:
                tile.partial = true;
                tileName = "cliff_end_right";
                break;
            case EdgeType.top:
                tile.partial = true;
                tileName = "cliff_end_top";
                break;
            case EdgeType.bot:
                //if this is not here, end cliff wont recognize
                tile.hillEdge = EdgeType.cliffBot;
                tileName = "cliff_bot";
                break;
            //this is to recognize case above, when tile is loaded again, since it would not be recognized properly
            case EdgeType.cliffBot:
                tileName = "cliff_bot";
                break;
            case EdgeType.cliffEndBot:
                tile.partial = true;
                tileName = "cliff_end_bot";
                break;
            //* 2-sided edge tiles 
            case EdgeType.botLeft:
                tile.partial = true;
                tileName = "cliff_botLeft";
                break;
            case EdgeType.botRight:
                tile.partial = true;
                tileName = "cliff_botRight";
                break;
            case EdgeType.topLeft:
                tileName = "cliff_topLeft";
                tile.partial = true;
                break;
            case EdgeType.topRight:
                tileName = "cliff_topRight";
                tile.partial = true;
                break;
            // corners 
            case EdgeType.botLeftOnly:
                if (tile.bottom.hillEdge == EdgeType.none)
                {
                    tile.hillEdge = EdgeType.bot;
                    tileName = "cliff_bot";
                // fixing bug
                }else if (tile.bottom.hillEdge == EdgeType.cliffEndLeft || tile.bottom.hillEdge == EdgeType.left){

                    tile.top.hillEdge = EdgeType.cliffEndLeft;
                    tile.hillEdge = EdgeType.cliffEndLeft;
                    tile.partial = true;
                    tileName = "cliff_end_left";
                }else{
                    tile.hillEdge = EdgeType.botLeft;
                    tileName = "cliff_botLeft";
                }
                break;
            case EdgeType.botRightOnly:
                tile.hillEdge = EdgeType.cliffEndRight;
                tile.partial = true;
                tileName = "cliff_end_right"; 
                break;
            case EdgeType.topLeftOnly:
                tile.partial = true;
                tileName = "cliff_corner_topLeft";
                break;
            case EdgeType.topRightOnly:
                tile.partial = true;
                tileName = "cliff_corner_topRight";
                break;
            //fixing bug
            case EdgeType.cliffEndLeft:
                    tile.partial = true;
                    tileName = "cliff_end_left";
                break;
        }    

        return tileName;
    }
    

    /// <summary>
    /// Accordingly find type of suiting water to specific biome surrounding it
    /// </summary>
    /// <param name="tile">Tile reference</param>
    /// <returns>Sprite type for water.</returns>
    private Sprite GetWaterType(TDTile tile){
        if (tile.temperature < 0.25)
        {
            tile.waterType = "ashland";
            return tiles[1]; //lava
        }else if (tile.temperature > 0.6)
        {
            tile.waterType= "rainforest";
            return tiles[2]; //green water
        }else{
            tile.waterType = "forest";
            return tiles[0]; //regular forest water
        }
    }


    /// <summary>
    /// Takes 2 arguments, temperature and moisture of given tile. Returns euclidean distance which is 
    /// urther used in biome picking.
    /// </summary>
    /// <param name="noiseTemperature">tile temperature value</param>
    /// <param name="noisePrecipitation">tile precipitation value</param>
    /// <returns>Euclidean distance to this biome</returns>
    public float EuclideanDistance(float noiseTemperature, float noisePrecipitation){
        float averageTemperature = (temperature + maxTemperature) / 2;
        float averagePrecipitation = (precipitation + maxPrecipitation) / 2;
        float eucVal = 
        (Mathf.Pow(noiseTemperature - averageTemperature,2) + Mathf.Pow(noisePrecipitation - averagePrecipitation,2));
        return eucVal;
    }


    /// <summary>
    ///         Takes 2 arguments(temperature and moisture) of given tile, and check whenever they meet conditions for biome.
    /// </summary>
    /// <param name="noiseTemperature">tile temperature value</param>
    /// <param name="noisePrecipitation">tile precipitation value</param>
    /// <returns>Returns true or false depending on answer.</returns>
    public bool MatchCondition(float noiseTemperature, float noisePrecipitation){
        
        if (noiseTemperature > temperature && noiseTemperature < maxTemperature) //matches biome's temperature
        {
            if (noisePrecipitation > precipitation && noisePrecipitation < maxPrecipitation) //matcher biome's precipitation -> this biome
            {
                return true;
            }
        }
        return false;
    }
    
    /*
        objects used in ChunkCreator tree pool
    */

    /// <summary>
    /// Return random tree from this biome
    /// </summary>
    /// <returns></returns>
    public Sprite GetRandomTree(){
            foreach (Objects itm in objects)
            {
                if(itm.type == "trees"){
                    return itm.sprites[Random.Range(0, itm.sprites.Length)];
                }  
            } 
            Debug.Log("Object not found");
            return null;
    }

    /// <summary>
    /// Return requested tree from this biome
    /// </summary>
    /// <param name="name">Name of requested tree sprite</param>
    /// <returns>Sprite of tree</returns>
    /// //FIXME rework gettree and getobj to one function, same with random
    public Sprite GetTree(string name){
        foreach (Objects itm in objects){
            if(itm.type == "trees"){
                foreach (Sprite sprite in itm.sprites)
                {
                    if (sprite.name == name)
                    {
                        return sprite;
                    }
                }
            }
        }
        Debug.Log("No tree found");
        return null;
    }
    
    /// <summary>
    /// Get specific object of this biome, from given name.
    /// </summary>
    /// <param name="name">Name of desired object.</param>
    /// <returns></returns>
    public Sprite GetObj(string name){
        foreach (Objects itm in objects){
            if(itm.type == "miscellaneous"){
                foreach (Sprite sprite in itm.sprites)
                {
                    if (sprite != null && sprite.name == name)
                    {
                        return sprite;
                    }
                }
            }
        }
        Debug.Log("No objects found");
        return null;
    }

    /// <summary>
    /// Returns random object from this biome.
    /// </summary>
    /// <returns>Returns random object from this biome.</returns>
    public Sprite GetRandomObj(){
            foreach (Objects itm in objects)
            {
                if(itm.type == "miscellaneous"){
                    return itm.sprites[Random.Range(0, itm.sprites.Length)];
                }  
            } 
            Debug.Log("Object not found");
            return null;
    }
}

/// <summary>
/// Structure holding tiles edging with other biomes.
/// </summary>
 [System.Serializable]
 public struct EdgeTile {
     public string name;
     public bool walkable;
     public Sprite sprite;
 }

[System.Serializable]
 public struct Objects {
     public string type;
     public Sprite[] sprites;
 }