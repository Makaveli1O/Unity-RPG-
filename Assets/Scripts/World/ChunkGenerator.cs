using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics; //int2 and perlin noise

/// <summary>
/// Class generating perlin maps for  individual chunks
/// </summary>
public class ChunkGenerator : MonoBehaviour
{
    Map map;
    /// <summary>
    /// Main function retrieving all neccessary intel about creating perlin maps. From this
    /// method generating of height, heat and moisture maps are called. Each one needs it's own
    /// seed, octave, persistence and lacunarity values to be adjustable. On beginning dictionary
    /// is appended when new key is found and then is filled with generated values.
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="chunks">chunks dictionary map</param>
    /// <param name="globalSeed">seed of global map</param>
    /// <param name="heightSeed">seed for height only</param>
    /// <param name="precipitationSeed">seed for precipitation only</param>
    /// <param name="scale">scale of whole map</param>
    /// <param name="heightOctaves">number of octaves for height noise</param>
    /// <param name="precipitationOctaves">number of octaves for moisture noise</param>
    /// <param name="heightFrequency">Persistance value for height.</param>
    /// <param name="precipitationPersistance">Persistance value for moisture.</param>
    /// <param name="temperatureMultiplier">Persistance value for temperature(multiplier of heat)</param>
    /// <param name="heightExp">Lacunarity of height</param>
    /// <param name="precipitationLacunarity">Lacunarity of moisture</param>
    /// <param name="temperatureLoss">Temperature loss(how much is temperature decreasing with latitude.</param>
    /// <param name="dimensions">Map widht and height</param>
    /// <returns></returns>
    public Dictionary<int2, WorldChunk> GenerateChunks( int x,int y , Dictionary<int2, WorldChunk> chunks,
    int globalSeed, int heightSeed, int precipitationSeed,
    float scale, int heightOctaves, int precipitationOctaves,
    float heightFrequency, float precipitationPersistance, float temperatureMultiplier,
    float heightExp, float precipitationLacunarity, float temperatureLoss, int2 dimensions,
    float treeScale
    ){
        var key = new int2(x,y);
        //chunk is not yet loaded
        if (!chunks.ContainsKey(key))
        {
            //append dictionary
            chunks.Add(key, new WorldChunk());
            chunks[key].position = new int2(x,y);
        }

        chunks[key] = GenerateLandMass(chunks[key], heightOctaves, heightFrequency, heightExp, scale,globalSeed, heightSeed, dimensions);
        chunks[key] = GeneratePrecipitationMap(chunks[key],globalSeed, precipitationSeed, scale, precipitationOctaves, precipitationPersistance, precipitationLacunarity, dimensions);
        chunks[key] = GenerateHeatMap(chunks[key], dimensions, temperatureMultiplier, temperatureLoss); 
        chunks[key] = GenerateTrees(chunks[key], globalSeed, scale, treeScale);
        
        return chunks;

    }

    /// <summary>
    /// Function that creates map holding information of where trees should actually be spawned. Pelin noise
    /// is used in this method, and if value exceeds treshold, spawn location is marked as 1.
    /// </summary>
    /// <param name="chunk">Working chunk</param>
    /// <param name="globalSeed">World seed</param>
    /// <param name="scale">World scale</param>
    /// <param name="dimensions">World dimensions width and height</param>
    /// <param name="treeScale">Scale for perlin noise map</param>
    /// <returns></returns>
    public WorldChunk GenerateTrees(WorldChunk chunk, int globalSeed, float scale, float treeScale) {
        
        float [,] treeMap = GenerateNoiseMap(chunk, globalSeed, treeScale);

        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                float treeValue = treeMap[x,y];
                TDTile tile = map.GetTile(new int2(x,y), chunk.position);
                //get tree radius specified for this biome
                float neighborRadius = map.GetBiome(tile.height, tile.precipitation, tile.temperature, chunk.position, new int2(x,y)).treeRadius;

                // compares the current tree noise value to the neighbours
                int neighbourYBegin = (int)Mathf.Max (0, y - neighborRadius);
                int neighbourYEnd = (int)Mathf.Min (Const.CHUNK_SIZE-1, y + neighborRadius);
                int neighbourXBegin = (int)Mathf.Max (0, x - neighborRadius);
                int neighbourXEnd = (int)Mathf.Min (Const.CHUNK_SIZE-1, x + neighborRadius);

                //find maximum value for radius neighbourhood
                float maxValue = 0f;
                for (int neighbourY = neighbourYBegin; neighbourY <= neighbourYEnd; neighbourY++) {
                    for (int neighbourX = neighbourXBegin; neighbourX <= neighbourXEnd; neighbourX++) {

                        float neighbourValue = treeMap [neighbourY, neighbourX];

                        // saves the maximum tree noise value in the radius
                        if (neighbourValue >= maxValue) {
                            maxValue = neighbourValue;
                        }
                    }
                }
                // if the current tree noise value is the maximum one, place a tree mark
                if (treeValue == maxValue) {
                    chunk.treeMap[x,y] = 1;
                    //FIXME this causes enemies to avoid  trees so its probably aight
                    tile.IsWalkable = false;
                }
            }
        }

        return chunk;
    }


    /// <summary>
    /// Creates moisture map for 1 chunk, depending on given seeds, scale, octaves, persistance and lucanarity.
    /// More octaves will result in more processed noise.
    /// </summary>
    /// <param name="chunk">Operated chunk</param>
    /// <param name="globalSeed">Seed of world</param>
    /// <param name="seed">Seed of precipitation</param>
    /// <param name="scale">World scale</param>
    /// <param name="octaves">Numer of loops to perform noise</param>
    /// <param name="persistance">Persistance is affecting length of frequency of function.</param>
    /// <param name="lacunarity">Lacunarity is affecting volume of function.</param>
    /// <param name="dimensions">Map width and height.</param>
    /// <param name="temperature">Temperature is affecting generated moisture.</param>
    /// <returns>Chunk filled with precipitation noise data.</returns>
    public WorldChunk GeneratePrecipitationMap(WorldChunk chunk, int globalSeed, int seed, float scale, int octaves, float persistance, float lacunarity, int2 dimensions, bool temperature = false){
        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                float frequency = 1f; //higher frequency -> further apart sample points height values will change more rapidly
                float amplitude = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i<octaves; i++){
                    float px = (float)(x + chunk.position.x) / Const.CHUNK_SIZE * scale * frequency;//* 1f + xOffset;
                    float py = (float)(y + chunk.position.y) / Const.CHUNK_SIZE * scale * frequency;//*1f + yOffset;
                    float perlinValue = Mathf.PerlinNoise(px + globalSeed + seed,py + globalSeed + seed);
                    noiseHeight += perlinValue * amplitude; 
                    amplitude *= persistance; //decreases
                    frequency *= lacunarity; //increases
                }
                chunk.sample[x,y].precipitation = noiseHeight;
            }
        }
        return chunk;
    }

    /// <summary>
    /// This function generates landmass(island like structure) with perlin noise. First whole map is land
    /// generated. After that rectangle in the middle is calculated, and based on theshold land is subtracted
    /// from the edges resulting in rectangle haight island in the middle, while edges are mostly 0 valued.
    /// </summary>
    /// <param name="chunk">Operated chunk</param>
    /// <param name="octaves">Number of loops</param>
    /// <param name="frequency">Freqency of perlin function.</param>
    /// <param name="exp">Volume of perlin function</param>
    /// <param name="scale">World scale</param>
    /// <param name="globalSeed">World seed</param>
    /// <param name="seed">Height seed</param>
    /// <param name="dimensions">Map widht and height</param>
    /// <returns>Chunk filled with height noise data.</returns>
    public WorldChunk GenerateLandMass(WorldChunk chunk, int octaves, float frequency, float exp, float scale, int globalSeed, int seed, int2 dimensions){
        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                //create new tdtile instance
                chunk.sample[x,y] = new TDTile(); //create new tile instance
                float elevation = 0f;
                float amplitude = 1f;
                float f = frequency;
                float amplitudeSum = 0f;
                for (int i = 0; i<octaves; i++){
                    float px = (float)(x + chunk.position.x) / Const.CHUNK_SIZE * scale;
                    float py = (float)(y + chunk.position.y) / Const.CHUNK_SIZE * scale;
                    elevation = Mathf.PerlinNoise(px * f+seed+globalSeed,py * f+seed+globalSeed); 

                    //run perlin values through function tat will make square island like shape

                    elevation = MakeIslandMask(dimensions.x, dimensions.y,x+chunk.position.x, y + chunk.position.y, elevation,chunk.sample[x,y]);
                    f = Mathf.Pow(f, 2);
                    if (f == 1f)
                    {
                        f = 2f;
                    }
                    amplitudeSum += amplitude;
                    amplitude /= 2;
                }

                elevation = elevation / amplitudeSum;
                //redistribution and initialization of tile
                chunk.sample[x,y].pos.x = chunk.position.x + x;
                chunk.sample[x,y].pos.y = chunk.position.y + y;
                chunk.sample[x,y].height = Mathf.Pow(elevation,exp);
                chunk.sample[x,y].z_index = SetZ_Index(elevation);
                chunk.zIndexMap[x,y] = SetZ_Index(elevation);

                // numbers like -0,00115059^1.48 returns nan dunno why NaN
                if (float.IsNaN(chunk.sample[x,y].height))
                {
                   chunk.sample[x,y].height = Mathf.Pow(Mathf.Abs(elevation),exp);
                }
            }
        }
        
        return chunk;
    }
    /// <summary>
    /// Set z-index (pseudo z coordinate in 2d world) based on give elevation
    /// </summary>
    /// <param name="elevation">Single tile elevation</param>
    /// <returns>Calculated z index</returns>
    private int SetZ_Index(float elevation){
        if (elevation > 0.25 && elevation < 0.7){        //1st level
            return 1;
        }else if (elevation > 0.7 && elevation < 1){  //2nd level
            return 2;
        }else {
            return 0;                                   //water level 0th level
        }
    }


    /// <summary>
    /// Creates part of heat map with elevation/latitude relationship. The higher latitude(distance from equator) is
    /// the lower temperature will be. Elevation also decreases temperature of area, how much tho depends on temperature_multiplier argument.
    /// (Gradient - cold on poles and hot on equator)
    /// </summary>
    /// <param name="chunk">Operated chunk</param>
    /// <param name="dimensions">Map width and height</param>
    /// <param name="temperature_multiplier">Multiplying value for temperature.</param>
    /// <param name="temperature_loss">Temperature lowering coefficient for latitude.</param>
    /// <returns>Chunk filled with perlin noise and adjusted with earth like (poles) conditions.</returns>
    public WorldChunk GenerateHeatMap(WorldChunk chunk, int2 dimensions, float temperature_multiplier, float temperature_loss){
        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                float elevation = chunk.sample[x,y].height;

                int latitude = GetLatitude(chunk.position.y + y, dimensions);
                float heatValue = GenerateHeatValue(latitude, dimensions.y, elevation,temperature_multiplier, temperature_loss);
                chunk.sample[x,y].temperature = heatValue;
            }
        }
        return chunk;
    }
    
    /// <summary>
    /// Adjusting values to earth like conditions. Poles on the top and bottom(cold), equator in the middle(hot)
    /// Formula for calculating heat of given tile:
    /// heat = ((latitude / (map_height)) *-temperature_multiplier) - ((elevation / temperature_height) * temperature_loss) + base_temperature;
    /// or
    /// float retval = ((latitude / map_height) * temperature_multiplier) - ((elevation / temperature_height) * temperature_loss); 
    /// ! base_temperature -> is the highest temperature at the equator   
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="map_height"></param>
    /// <param name="elevation"></param>
    /// <param name="temperature_multiplier">multiplier decreasing the temperature value with increasing latitude</param>
    /// <param name="temperature_loss">is the amount of temperature that should be lost for each height step.</param>
    /// <returns>Float heat value for single tile.</returns>
    private static float GenerateHeatValue(int latitude, int map_height, float elevation, float temperature_multiplier,float temperature_loss){
        float tmp = (float) (latitude / (map_height/2f) * temperature_multiplier - (elevation * temperature_loss));
        float retval = 1f - tmp;
        retval = Mathf.Clamp(retval, 0.0f, 1.0f);

        return retval;
    }


    /// <param name="y">Y coord</param>
    /// <param name="dimensions">Map height and width</param>
    /// <returns>Latitude of given y coord.</returns>
    private static int GetLatitude(int y, int2 dimensions){
        int equator = dimensions.y / 2;
        int latitude = EuclidDistance(y, equator);
        return latitude;
    }

    /// <summary>
    /// Calculates distance between two points(on same axis).
    /// </summary>
    /// <param name="y1">One point on axis</param>
    /// <param name="y2">Second point on axis</param>
    /// <returns>Euclidean distance</returns>
    private static int EuclidDistance(int y1, int y2){
        return Mathf.Abs(y1-y2);
    }


    /// <summary>
    /// Calculates island from height perlin data. Distances to edges are calculated and compared with
    /// treshold min value. After that ideal distance is chosen and returned with factored value.
    /// </summary>
    /// <param name="width">map width</param>
    /// <param name="height">map height</param>
    /// <param name="posX">x coord</param>
    /// <param name="posY">y coord</param>
    /// <param name="oldValue">Previously calculated value</param>
    /// <param name="tile">tile reference</param>
    /// <returns>Height value(trimmed for island) for given tile.</returns>
     public static float MakeIslandMask(int width, int height, int posX, int posY, float oldValue, TDTile tile) {
        int minVal =(((height + width )/2)/ 100 * 2 );
        int maxVal = (((height + width)/2)/ 100 * 10 );
        if(GetDistanceToEdge(posX, posY, width, height) <= minVal){
            tile.landmass = false;
            return 0;
        }else if(GetDistanceToEdge(posX, posY, width, height) >= maxVal){
            tile.landmass = true;
            return oldValue;
        }else{
            float factor = GetFactor(GetDistanceToEdge(posX, posY, width, height), minVal, maxVal);
            //0.1 on 512x512 , 0.15 on 256x256
            float treshold = 0.1f;
            if((width >= 512 && height >= 512)){
                treshold = 0.1f;
            }else if((width >= 256 && height >= 256)){
                treshold = 0.15f;
            }
            if (factor * oldValue < treshold){
                tile.landmass = false;
            }else{
                tile.landmass = true;
            }
            return oldValue * factor;
        }
    }
 

    /// <summary>
    /// Finds out factor of given value. Used in creating island-like maps.
    /// </summary>
    /// <param name="val">Value to be factored</param>
    /// <param name="min">Minimum</param>
    /// <param name="max">Maximum</param>
    /// <returns>Factor of value.</returns>
    private static float GetFactor( int val, int min, int max ) {
        int full = max - min;
        int part = val - min;
        float factor = (float)part / (float)full;
        return factor;
    }

    /// <summary>
    /// Finds minimal distance to the edge. Used in creating island-like  maps.
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="width">widht of map</param>
    /// <param name="height">height of map</param>
    /// <returns>Minimal distance to the edge</returns>
    private static int GetDistanceToEdge(int x, int y, int width, int height) {
        int[] distances = new int[]{y, x,(width - x), (height - y)};
        int min = distances[ 0 ];
        foreach(var val in distances) {
            if(val < min) {
                min = val;
            }
        }
        return min;
    }

    /// <summary>
    /// Generate basic perlin noise 2d map, with given seed and scale for 1 chunk.
    /// </summary>
    /// <param name="chunk">Processed chunk</param>
    /// <param name="globalSeed">World seed</param>
    /// <param name="scale">World scale</param>
    /// <returns></returns>
    private float[,] GenerateNoiseMap(WorldChunk chunk, int globalSeed, float scale){
        float[,] noiseMap = new float[Const.CHUNK_SIZE,Const.CHUNK_SIZE];
        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                float px = (float)(x + chunk.position.x) / Const.CHUNK_SIZE * scale;//* 1f + xOffset;
                float py = (float)(y + chunk.position.y) / Const.CHUNK_SIZE * scale;//*1f + yOffset;
                float perlinValue = Mathf.PerlinNoise(px + globalSeed,py + globalSeed);

                noiseMap[x,y] = perlinValue;
            }
        }
        return noiseMap;
    }

    private void Awake(){
        map = GetComponent<Map>();
    }
}