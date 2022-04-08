using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using System.Linq;

/// <summary>
/// This Class is handling creation of each chunk. Mesh is created on whose is applied texture representing tiles.
/// This improves performance a big amount.
/// </summary>
public class ChunkCreator : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    public Map mapReference;
    ChunkLoader chunkLoader;
    ObjectPool treePool;
    ObjectPool objectPool;
    public ObjectPool entityPool;
    GameHandler gameHandler;
    WorldChunk chunk;
    private float tileSize = 1;
    public Vector3[] vertices;
    public Vector2[] uv;
    public int[] triangles;
    public TDTile[,] tiles;
    public int x;
    public int top_x;
    public int y;
    public int top_y;
    private Dictionary<int2, GameObject> renderedTrees = new Dictionary<int2, GameObject>();
    private HashSet<GameObject> renderedObjects = new HashSet<GameObject>();
    private HashSet<GameObject> spawnedEntities = new HashSet<GameObject>();
    private bool treesLoaded = false;
    private bool objectsLoaded = false;
    private bool entitiesSpawned = false;
    private bool containsKeyObj = false;
    private int2 keyObjPos = new int2(0, 0);

    /// <summary>
    /// Function that creates mesh with given width, height and world space. This tile represents
    /// one chunk. Mesh is divided into 32 x 32 quads. Each quad represents 1 world tile. Each tile's
    /// UV is set accordingly to match texture desired. Consists of loop throught the map, that
    /// is creating quads for tiles and setting it's biomes, and sets uvs and texturing accordingly.
    /// </summary>
    /// <param name="width">chunk width</param>
    /// <param name="height">chunk height</param>
    /// <param name="chunkX">key x coord(bot left)</param>
    /// <param name="chunkY">key y coord(bot left)</param>
    /// <returns></returns>
    public Mesh CreateTileMesh(WorldChunk chunk, Mesh mesh)
    {
        this.chunk = chunk;
        x = chunk.position.x;
        y = chunk.position.y;
        top_x = x + 32;
        top_y = y + 32;


        //check for passed cached chunk data
        this.vertices = new Vector3[4 * (Const.CHUNK_SIZE * Const.CHUNK_SIZE)];
        this.uv = new Vector2[4 * (Const.CHUNK_SIZE * Const.CHUNK_SIZE)];
        this.triangles = new int[6 * (Const.CHUNK_SIZE * Const.CHUNK_SIZE)];

        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                int index = x * Const.CHUNK_SIZE + y;
                vertices[index * 4 + 0] = new Vector3(tileSize * x + chunk.position.x, tileSize * y + chunk.position.y);
                vertices[index * 4 + 1] = new Vector3(tileSize * x + chunk.position.x, tileSize * (y + 1) + chunk.position.y);
                vertices[index * 4 + 2] = new Vector3(tileSize * (x + 1) + chunk.position.x, tileSize * (y + 1) + chunk.position.y);
                vertices[index * 4 + 3] = new Vector3(tileSize * (x + 1) + chunk.position.x, tileSize * y + chunk.position.y);

                triangles[index * 6 + 0] = index * 4 + 0;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;

                triangles[index * 6 + 3] = index * 4 + 0;
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;

                // now set UVs and textures accordingly
                Sprite tileSprite = GetTileTexture(x, y, chunk.position);

                //map UVs for each tile to specific texture in atlas
                this.uv[index * 4 + 0] = SetTileTexture(0, tileSprite);
                this.uv[index * 4 + 1] = SetTileTexture(1, tileSprite);
                this.uv[index * 4 + 2] = SetTileTexture(2, tileSprite);
                this.uv[index * 4 + 3] = SetTileTexture(3, tileSprite);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        meshFilter.mesh = mesh;

        this.chunk.chunkMesh = mesh;
        return mesh;
    }

    /// <summary>
    /// Sets tile's neighbourhood(pointers in 8 directions) and sets it's biome accordingly.
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="chunkX">chunk x key</param>
    /// <param name="chunkY">chunk y key</param>
    private void SetTileBiome(int x, int y, int2 key)
    {
        //bool isLeftSame, isTopLeftSame, isTopSame, isTopRightSame, isRightSame, isBotRightSame, isBotSame, isBotLeftSame, sameAround = false;
        mapReference.AssignNeighbours(mapReference.GetTile(new int2(x, y), key), key);
    }

    /// <summary>
    /// Return texture according to given coordinatite
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <param name="chunkX">chunk x key</param>
    /// <param name="chunkY">chunk y key</param>
    /// <returns>Texture for tile</returns>
    private Sprite GetTileTexture(int x, int y, int2 key)
    {
        // get reference to tile working with
        TDTile tile = mapReference.GetTile(new int2(x, y), key);
        //set material texture so we can assign uvs later on 
        SetMaterialTexture(tile.biome.GetRandomSprite().texture);

        Sprite ret = tile.biome.GetTileSprite(tile);
        return ret;
    }

    /// <summary>
    /// Sets UV's for quads within mesh for each tile. 
    /// </summary>
    /// <param name="corner">Corner of quad that is being processed.</param>
    /// <param name="tileSprite">Sprite holding texture</param>
    /// <returns>Vector2 UV for given corner of quad.</returns>
    private Vector2 SetTileTexture(int corner, Sprite tileSprite)
    {
        Rect UVs = tileSprite.rect;
        UVs.x /= tileSprite.texture.width;
        UVs.width /= tileSprite.texture.width;
        UVs.y /= tileSprite.texture.height;
        UVs.height /= tileSprite.texture.height;
        /*
            Return UV for each corner of texture within atlas
        */
        Vector2 retval = new Vector2(0, 0);
        switch (corner)
        {
            case 0: //bottom left
                retval = new Vector2(UVs.x, UVs.y);
                break;
            case 1: //top left
                retval = new Vector2(UVs.x, UVs.y + UVs.height);
                break;
            case 2: //top right
                retval = new Vector2(UVs.x + UVs.width, UVs.y + UVs.height);
                break;
            case 3: //bottom right
                retval = new Vector2(UVs.x + UVs.width, UVs.y);
                break;
        }

        return retval;
    }
    /// <summary>
    /// Set material texture holding all tile textures to be used in meshes.
    /// </summary>
    /// <param name="texture">Tiles texture</param>
    private void SetMaterialTexture(Texture2D texture)
    {
        meshRenderer.sharedMaterials[0].mainTexture = texture;
        return;
    }

    /// <summary>
    /// Spawn trees in loaded chunk. Loops through whole chunk and looks for tiles marked treeMap = 1 which,
    /// which mean tree should be spawned there. (More in chunkGenerator spawning trees). If tile is placable,
    /// object from pool is available and tree is out of another's tree radius, tree can be spawned. When tree
    /// is processed for the first time, picked sprite is saved to WorldChunk structure and saved to json. This
    /// sprite is next time retrieved from JSON so same tree sprite is present next render time. Actual saving
    /// of chunk is done after loading objects.
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="chunkX">Chunk x key</param>
    /// <param name="chunkY">Chunk y key</param>
    private void LoadTrees()
    {
        WorldChunk loaded = LoadChunk();
        for (int x = 0; x < Const.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Const.CHUNK_SIZE; y++)
            {
                int2 relativePos = new int2(x, y);
                TDTile tile = mapReference.GetTile(relativePos, chunk.position);
                if (chunk.treeMap[x, y] == 1 && tile.IsPlacable())
                {
                    int x_coord = chunk.position.x + x;
                    int y_coord = chunk.position.y + y;
                    int2 actualPos = new int2(x_coord, y_coord);
                    GameObject treeP = treePool.GetPooledObject();
                    if (treeP != null && TreeRadius(actualPos))
                    {
                        //treeP.transform.parent = gameObject.transform;
                        treeP.transform.position = new Vector3(chunk.position.x + x, chunk.position.y + y, 0);
                        treeP.SetActive(true);
                        //set correct sprite and save
                        Sprite treeSprite;
                        if(loaded == null){
                            treeSprite = SetTreeSprite(treeP, tile);
                            //mark picked sprite to chunk
                            chunk.trees.Add(treeSprite.name);
                        }else{
                            //pop first sprite from json and assign to tree
                            if (loaded.trees.Count != 0)
                            {
                                treeSprite = SetTreeSprite(treeP, tile, loaded.trees[0]);
                                loaded.trees.RemoveAt(0);
                            }else{
                                continue;
                            }
                        }
                        renderedTrees[actualPos] = treeP;
                        if (treeP != null)
                        {
                            AdjustObjCollider(treeP, treeSprite);
                        }
                    }
                }
            }
        }
        treesLoaded = true;
        return;
    }
    
    /// <summary>
    /// Loading objects of the chunk. If chunk is successfully loaded from JSON, then
    /// LoadedChunkObjects is called, otherwise new object in the chunk are being generated
    /// using GenerateChunkObjects.
    /// </summary>
    private void LoadObjects(){
        WorldChunk loaded = LoadChunk();
        
        if (loaded != null)
        {
            LoadedChunkObjects(loaded);
        //chunk is about to place objects
        }else{
            GenerateChunkObjects();
        }
        //if objects were not loaded(saving would be pointless), save whole chunk
        if (loaded == null){
            SaveChunk();
        }
        objectsLoaded = true;
        return;
    }

    /// <summary>
    /// Loads all objects within given loaded chunk read from JSON file
    /// </summary>
    /// <param name="loaded">Currently processing chunk</param>
    private void LoadedChunkObjects(WorldChunk loaded){
        this.containsKeyObj = loaded.containsKeyObj;
        this.keyObjPos = loaded.keyObjPos;
        foreach (WorldChunk.ObjectsStorage item in loaded.objects){
            List<GameObject> availableObjects = objectPool.GetInactiveObjects();
            if (availableObjects.Count > 0 )
            {
                GameObject obj = availableObjects[0];
                availableObjects.RemoveAt(0);
                renderedObjects.Add(obj);
                int2 relativePos = item.pos;
                int2 absolutePos = new int2(relativePos.x + chunk.position.x, relativePos.y + chunk.position.y);
                obj.transform.position = new Vector3(absolutePos.x, absolutePos.y, 0);
                obj.SetActive(true);
                TDTile tile = mapReference.GetTile(relativePos, chunk.position);
                Sprite sprite = SetObjSprite(obj, tile, item.sprite, true);

                AdjustObjCollider(obj, sprite);
            }else{
                break;
            }
        }
        return;
    }

    /// <summary>
    /// Randomly picks point in the chunk area, if tile on the point is accessible place random
    /// object from tile's biome. Add to the chunk objects and save after every point is properly set.
    /// </summary>
    /// <param name="availableObj">Available child gameobject of chunk</param>
    /// <returns>Picked object sprite</returns>
    private void GenerateChunkObjects(){
        List<GameObject> availableObjects = objectPool.GetInactiveObjects();
        List<int> availableX = Enumerable.Range(0, 31).ToList();
        List<int> availableY = Enumerable.Range(0, 31).ToList();
        foreach( GameObject availableObj in availableObjects){
            int indX = Random.Range(0,availableX.Count);
            int indY = Random.Range(0,availableY.Count);
            Vector3 randomCoords = new Vector3( availableX[indX],
                                                availableY[indY],
                                                0); 
            availableX.RemoveAt(indX);
            availableY.RemoveAt(indY);
            int2 relativePos = new int2((int)randomCoords.x, (int)randomCoords.y);
            TDTile tile = mapReference.GetTile(relativePos, chunk.position);
            //set correct sprite and save
            if (tile.IsPlacable() && chunk.treeMap[relativePos.x, relativePos.y] == 0){
                int2 absolutePos = new int2(relativePos.x + chunk.position.x, relativePos.y + chunk.position.y);
                availableObj.transform.position = new Vector3(absolutePos.x, absolutePos.y, 0);
                availableObj.SetActive(true);
                renderedObjects.Add(availableObj);
                Sprite sprite = SetObjSprite(availableObj, tile, null);
                //mark intel to the chunk
                chunk.objects.Add(new WorldChunk.ObjectsStorage { pos = relativePos, sprite = sprite.name });

                AdjustObjCollider(availableObj, sprite);
            }
        }
        return;
    }
    //FIXME do these unload motheds more abstract for **** sake
    /// <summary>
    /// Unload all trees and clear renderedTree list.
    /// </summary>
    public void UnloadTrees()
    {
        foreach (GameObject tree in renderedTrees.Values)
        {
            tree.SetActive(false);
        }
        //treePool.DeactivateAll();
        renderedTrees.Clear();

        treesLoaded = false;
    }
    /// <summary>
    /// Unload all objects and clear renderedObjects list.
    /// </summary>
    public void UnloadObjects(){
        foreach (GameObject obj in renderedObjects)
        {
            obj.SetActive(false);
        }
        //objectPool.DeactivateAll();
        renderedObjects.Clear();

        objectsLoaded = false;
    }

    /// <summary>
    /// Despawns all spaned entities within chunk.
    /// </summary>
    public void DespawnEntities(){
        foreach (GameObject obj in spawnedEntities)
        {
            obj.SetActive(false);
        }
        //objectPool.DeactivateAll();
        spawnedEntities.Clear();

        entitiesSpawned = false;
    }

    /// <summary>
    /// Saves current chunk stage to json. Used only once, when visiting chunk for the first time
    /// after loading trees and objects
    /// </summary>
    private void SaveChunk(){
        gameHandler.Save<WorldChunk>(this.chunk, ObjType.Chunk, new Vector3(this.x, this.y, 0));
    }
    
    /// <summary>
    /// Load chunk from json in WorldChunk class format
    /// </summary>
    /// <returns>Loaded chunk data</returns>
    private WorldChunk LoadChunk(){
        return gameHandler.Load<WorldChunk>(ObjType.Chunk,null, this.x, this.y);
    }

    /// <summary>
    /// Check if tree about to spawn can be actually spawned. If another tree is already
    /// spawned in radius 2, tree wort spawn. Tree radius from chunk generator works, however
    /// multiple trees spawn on 1 location. This function handles that problem
    /// </summary>
    /// <param name="coords">Coords of root points.</param>
    /// <returns>Boolean value whenever tree can or can not be spawned.</returns>
    private bool TreeRadius(int2 coords)
    {
        foreach (int2 tree in renderedTrees.Keys)
        {
            if (EuclideanDistance(coords, tree) < 4f)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Calculates distance between two points.
    /// </summary>
    /// <param name="p1">Point 1</param>
    /// <param name="p2">Point 2</param>
    /// <returns>Float distance between them.</returns>
    private float EuclideanDistance(int2 p1, int2 p2)
    {
        Vector2 v1 = new Vector2(p1.x, p1.y);
        Vector2 v2 = new Vector2(p2.x, p2.y);
        return Vector2.Distance(v1, v2);
    }

    /// <summary>
    /// Set correct tree sprite for given tile and edit given gameobject to that.
    /// </summary>
    /// <param name="tree">Tree gameobject</param>
    /// <param name="tile">Processed tile</param>
    /// <returns>Sprite that was assigned to tree</returns>
    public Sprite SetTreeSprite(GameObject tree, TDTile tile, string loaded = null)
    {
        SpriteRenderer treeRenderer = tree.GetComponent<SpriteRenderer>();

        if (loaded != null){
            treeRenderer.sprite = tile.biome.GetTree(loaded);
        }else{
            treeRenderer.sprite = tile.biome.GetRandomTree(); 
        }
       
        return treeRenderer.sprite;
    }

    /// <summary>
    /// Set sprite for given prefab gameobject. If loaded gameobject, select according sprite
    /// as given in "name" variable. If not loaded, pick random sprite.
    /// </summary>
    /// <param name="obj">Prefab game object</param>
    /// <param name="tile">tile to be spawned on top of</param>
    /// <param name="name">name of sprite</param>
    /// <param name="loaded">flag determining if sprite is being loaded from json</param>
    /// <returns>Picked sprite</returns>
    public Sprite SetObjSprite(GameObject obj, TDTile tile, string name, bool loaded = false){
        SpriteRenderer objRenderer = obj.GetComponent<SpriteRenderer>();

        //loading storage
        if (loaded){
            objRenderer.sprite = tile.biome.GetObj(name);
        }else{//getting random object
            objRenderer.sprite = tile.biome.GetRandomObj();
        }
        return objRenderer.sprite;
    }

    /// <summary>
    /// Determines size of tree/object collider. Depending on sprite prefix name, bounding box
    /// is calculated differently.
    /// </summary>
    /// <param name="collider2D">colldier object</param>
    /// <param name="boundingBox">bounding box around sprite</param>
    private void AdjustObjCollider(GameObject obj, Sprite sprite)
    {
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        CapsuleCollider2D collider2D = obj.GetComponent<CapsuleCollider2D>();
        Bounds boundingBox = sprite.bounds;
        string name = sprite.name;
        
        int index = name.IndexOf("_");
        string prefix = name.Substring(0, index);
        collider2D.enabled = true;
        spriteRenderer.sortingOrder = 1;
        switch (prefix)
        {
            case "regular":
                collider2D.size = new Vector2(boundingBox.size.x / 2f, collider2D.size.y);
                break;
            case "big":
                collider2D.size = new Vector2(boundingBox.size.x / 1.2f, collider2D.size.y);
                break;
            case "small":
                collider2D.size = new Vector2(boundingBox.size.x / 5f, collider2D.size.y);
                break;
            case "hollow":
                spriteRenderer.sortingOrder = 0;
                collider2D.enabled = false;
                break;
            default:
                collider2D.size = new Vector2(boundingBox.size.x / 2f, collider2D.size.y);
                break;
        }
        return;
    }

    /// <summary>
    /// Prevents Update to call rendering objects and trees multiple times.
    /// </summary>
    private void SpawnChunkObjects(){
        if(!treesLoaded){
            LoadTrees();
        }
        if (!objectsLoaded){
            LoadObjects();
        }
        return;     
    }
    private void Awake()
    {
        var m = GameObject.FindGameObjectWithTag("Map");
        if (m != null){
            mapReference = m.GetComponent<Map>();
            chunkLoader = m.GetComponent<ChunkLoader>();
            gameHandler = m.GetComponent<GameHandler>();
        }
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        //2 pools in this case, one for trees other for objects
        ObjectPool[] pools = GetComponents<ObjectPool>();

        foreach ( ObjectPool pool  in pools){
            //trees
            if (pool.id == 0){
                treePool = pool;
            //objects
            }else if(pool.id == 1){
                objectPool = pool;
            }else{
                entityPool = pool;
            }
        }
    }

    /// <summary>
    /// Spawning entities within the chunk.
    /// </summary>
    /// <param name="chunkKey">Key position of the chunk</param>
    /// <param name="chunkTopCoords">Top x and y coordinates of this chunk</param>
    /// <returns>Boolean if entity was spawned</returns>
    public bool EntitySpawner(int2 chunkKey, int2 chunkTopCoords){
        int spawned = 0;
        int randomNum = Random.Range(0,entityPool.amountToPool);
        //key object within this chunk -> spawn guards
        if (chunk.containsKeyObj){
            while (spawned < 8)
            {
                //pick random valid position around object +/- 5 tiles
                //this can cause exception, when obj is in the corner of chunk, and others are not generated yet
                //int2 spawnPos = new int2(Random.Range(keyObjPos.x - 5, keyObjPos.x + 5), Random.Range(keyObjPos.y - 5, keyObjPos.y + 5));
                int2 spawnPos = new int2(Random.Range(chunkKey.x, chunkTopCoords.x), Random.Range(chunkKey.y, chunkTopCoords.y));
                TDTile tile = mapReference.GetTile(mapReference.TileRelativePos(spawnPos), mapReference.TileChunkPos(spawnPos));
                //tile = mapReference.GetTile(mapReference.TileRelativePos(keyObjPos), mapReference.TileChunkPos(keyObjPos));
               
                //check if tile is fine to spawn entity on
                if(mapReference.isSpawnable(tile)){
                    Spawn(tile, chunk.containsKeyObj);
                    spawned++;
                }
            }
        }else{  //spawn normaly when keyObject is not present
            while (spawned < randomNum)
            {
                int2 spawnPos = new int2(Random.Range(chunkKey.x, chunkTopCoords.x), Random.Range(chunkKey.y, chunkTopCoords.y));
                TDTile tile = mapReference.GetTile(mapReference.TileRelativePos(spawnPos), mapReference.TileChunkPos(spawnPos));
                //check if tile is fine to spawn entity on
                if(mapReference.isSpawnable(tile)){
                    Spawn(tile, this.containsKeyObj);
                    spawned++;
                }
            }
        }
        return true;
    }


    /// <summary>
    /// Function handles spawning entity within given tile. Tile is converted into world coordinates
    /// in Vector3 format. Selected tile is scanned for biome type to instantiate proper entity that
    /// belongs into it.
    /// </summary>
    /// <param name="tile">Proccessed tile.</param>
    private void Spawn(TDTile tile, bool spawnGuards){
        //convert to vector position
        Vector3 pos = new Vector3(tile.pos.x, tile.pos.y , 0);
        //get available object
        GameObject entity = this.entityPool.GetPooledObject();
        EnemyController ec = entity.GetComponent<EnemyController>();
        //set correct enemy
        if (spawnGuards)
        {
            ec.preset = mapReference.guards[Random.Range(0,1)]; 
        }else{
            ec.preset = tile.biome.enemies[Random.Range(0, tile.biome.enemies.Length)];
        }
        //set wander anchor point
        ec.anchorPoint = pos;
        //set correct assetsLibrary
        ec.assetsLibrary = ec.preset.assetsLibrary;

        //set position and activate
        entity.transform.position = pos;
        entity.SetActive(true);
        spawnedEntities.Add(entity);
    }

    private void Update()
    {
        if(!treesLoaded){
            SpawnChunkObjects();
        }

        if (!entitiesSpawned){
            entitiesSpawned = EntitySpawner(new int2(x,y), new int2(top_x, top_y));
        }
    }
}
