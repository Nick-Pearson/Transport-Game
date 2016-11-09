using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour, ICameraObserver
{
    [System.Serializable]
    public struct TreeBlueprint
    {
        public GameObject prefab;
        public float bendFactor;
    }

    [System.Serializable]
    public struct Biome
    {
        public TerrainTextureBlueprint grassTexture;
        public TerrainTextureBlueprint rockyTexture;
        public TerrainTextureBlueprint cliffTexture;
        public TerrainTextureBlueprint riverbankTexture;
        public TerrainTextureBlueprint snowTexture;
    }

    public float waterThreshold = 30;
    public float snowThreshold = 125;

    [System.Serializable]
    public struct TerrainTextureBlueprint
    {
        public Texture2D albedo;
        public Texture2D normal;
        public int metalic;
        public Vector2 size;
        public Vector2 offset;
    }

    Transform projector;

    List<Vector2> loadedChunks = new List<Vector2>();
    Vector2[] visibleChunks = null;
    Terrain[] chunkGraphics = new Terrain[9];

    const int chunkSizeX = 512;
    const int chunkSizeY = 512;

    Vector2 currentChunkIndex = new Vector2();

    public static Vector2 VECTOR_WILDCARD = new Vector2(-10000, -10000);
    public static float TERRAIN_HEIGHT_WILDCARD = -1;

    SplatPrototype[] terrainTextures;
    TreePrototype[] terrainTrees;
    public Biome[] biomeTextures;
    public TreeBlueprint[] trees;

    public Transform waterPlanePrefab;

    public int resolution { get { return chunkSizeX / 2; } }

    // Use this for initialization
    void Start()
    {
        projector = GameObject.Find("BrushSizeProjector").transform;

        Camera.main.GetComponent<RTSCamera>().Subscribe(this);

        terrainTextures = new SplatPrototype[biomeTextures.Length * 5];
        terrainTrees = new TreePrototype[trees.Length];

        for (int i = 0; i < biomeTextures.Length; i++)
        {
            terrainTextures[i] = BlueprintToSplatPrototype(biomeTextures[i].grassTexture);
            terrainTextures[i + 1] = BlueprintToSplatPrototype(biomeTextures[i].rockyTexture);
            terrainTextures[i + 2] = BlueprintToSplatPrototype(biomeTextures[i].cliffTexture);
            terrainTextures[i + 3] = BlueprintToSplatPrototype(biomeTextures[i].riverbankTexture);
            terrainTextures[i + 4] = BlueprintToSplatPrototype(biomeTextures[i].snowTexture);
        }

        for(int i = 0; i < trees.Length; i++)
        {
            terrainTrees[i] = BlueprintToTreePrototype(trees[i]);
        }

        for (int i = 0; i < chunkGraphics.Length; i++)
        {
            GameObject go = new GameObject();
            go.name = "Chunk_" + i;

            chunkGraphics[i] = go.AddComponent<Terrain>();

            chunkGraphics[i].terrainData = new TerrainData();

            go.AddComponent<TerrainCollider>().terrainData = chunkGraphics[i].terrainData;

            chunkGraphics[i].terrainData.size = new Vector3((int)(chunkSizeX / 8), 600, (int)(chunkSizeY / 8));
            chunkGraphics[i].terrainData.heightmapResolution = (int)(chunkSizeX / 2);
            chunkGraphics[i].terrainData.splatPrototypes = terrainTextures;
            chunkGraphics[i].terrainData.treePrototypes = terrainTrees;

            Transform waterPlane = GameObject.Instantiate(waterPlanePrefab, go.transform.position, Quaternion.identity) as Transform;
            waterPlane.transform.position = new Vector3(waterPlane.position.x + (chunkSizeX / 2), waterThreshold, waterPlane.position.z + (chunkSizeY / 2));
            waterPlane.localScale = new Vector3(5.1f, 1, 5.1f);
            waterPlane.SetParent(go.transform);
        }

        OnCameraMove(Camera.main.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButton(0))
            {
                ModifyTerrain(hit.point, 0.001f, 20);
            }

            projector.position = new Vector3(hit.point.x, projector.position.y, hit.point.z);
        }


    }

    void ModifyTerrain(Vector3 position, float amount, int diameter)
    {
        int TerrainIndex = GetTerrainFromPos(position);
        Terrain mainTerrain = chunkGraphics[TerrainIndex];

        position = GetRelativePosition(position);

        float[,] heights = mainTerrain.terrainData.GetHeights(0, 0, resolution, resolution);

        int terrainPosX = (int)((position.x / mainTerrain.terrainData.size.x) * resolution);
        int terrainPosY = (int)((position.z / mainTerrain.terrainData.size.z) * resolution);

        float[,] heightChange = new float[diameter, diameter];

        int radius = (int)(diameter / 2);

        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                int x2 = x - radius;
                int y2 = y - radius;

                if (terrainPosY + y2 < 0 || terrainPosY + y2 >= resolution || terrainPosX + x2 < 0 || terrainPosX + x2 >= resolution)
                    continue;

                float distance = Mathf.Sqrt((x2 * x2) + (y2 * y2));

                if (distance > radius)
                {
                    heightChange[y, x] = heights[terrainPosY + y2, terrainPosX + x2];
                }
                else
                {
                    heightChange[y, x] = heights[terrainPosY + y2, terrainPosX + x2] + (amount - (amount * (distance / radius)));
                    heights[terrainPosY + y2, terrainPosX + x2] = heightChange[y, x];
                }


            }
        }

        //FIXME Does not work when array is larger than current terrain
        Debug.Log(position.x);
        mainTerrain.terrainData.SetHeights(terrainPosX - radius, terrainPosY - radius, heightChange);
    }
    
    //get an index into our terrain array
    int GetTerrainFromPos(Vector3 position)
    {
        int xValue = Mathf.FloorToInt(position.x / chunkSizeX);
        int yValue = Mathf.FloorToInt(position.y / chunkSizeY);

        for(int i = 0; i < visibleChunks.Length; i++)
        {
            if (visibleChunks[i].x == xValue && visibleChunks[i].y == yValue)
                return i;
        }
        
        throw new System.Exception("Could not find that chunk!");
    }

    //convert world position to terrain position
    Vector3 GetRelativePosition(Vector3 postion)
    {
        return new Vector3(modulus(postion.x, chunkSizeX), postion.y, modulus(postion.z, chunkSizeY));
    }

    float modulus(float f1, float f2)
    {
        return f1 - f2 * Mathf.FloorToInt(f1 / f2);
    }

    public void OnCameraMove(Vector3 newCameraPosition)
    {
        int chunkIndexX = Mathf.FloorToInt(newCameraPosition.x / chunkSizeX);
        int chunkIndexY = Mathf.FloorToInt(newCameraPosition.z / chunkSizeY);

        if (currentChunkIndex.x == chunkIndexX && currentChunkIndex.y == chunkIndexY)
        {
            return;
        }

        currentChunkIndex.x = chunkIndexX;
        currentChunkIndex.y = chunkIndexY;

        Vector2[] newVisibleChunks = new Vector2[9];

        newVisibleChunks[0] = new Vector2(chunkIndexX - 1, chunkIndexY + 1);
        newVisibleChunks[1] = new Vector2(chunkIndexX, chunkIndexY + 1);
        newVisibleChunks[2] = new Vector2(chunkIndexX + 1, chunkIndexY + 1);

        newVisibleChunks[3] = new Vector2(chunkIndexX - 1, chunkIndexY);
        newVisibleChunks[4] = new Vector2(chunkIndexX, chunkIndexY);
        newVisibleChunks[5] = new Vector2(chunkIndexX + 1, chunkIndexY);

        newVisibleChunks[6] = new Vector2(chunkIndexX - 1, chunkIndexY - 1);
        newVisibleChunks[7] = new Vector2(chunkIndexX, chunkIndexY - 1);
        newVisibleChunks[8] = new Vector2(chunkIndexX + 1, chunkIndexY - 1);


        Terrain[] newChunkGraphics = new Terrain[chunkGraphics.Length];

        List<int> freeTerrains = new List<int>();
        List<int> loadingIndexes = new List<int>();

        for (int i = 0; i < 9; i++)
        {
            bool found = false;

            for (int j = 0; j < newVisibleChunks.Length; j++)
            {
                if (visibleChunks == null)
                    break;

                if (newVisibleChunks[i].Equals(visibleChunks[j]))
                {
                    visibleChunks[j] = VECTOR_WILDCARD;
                    newChunkGraphics[i] = chunkGraphics[j];

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                loadingIndexes.Add(i);
            }
        }



        if (visibleChunks != null)
        {
            for (int i = 0; i < 9; i++)
            {
                if (visibleChunks[i] != VECTOR_WILDCARD)
                {
                    freeTerrains.Add(i);
                    SaveChunkToMemory(chunkGraphics[i], visibleChunks[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                freeTerrains.Add(i);
            }
        }

        visibleChunks = newVisibleChunks;

        for (int i = 0; i < loadingIndexes.Count; i++)
        {
            newChunkGraphics[loadingIndexes[i]] = chunkGraphics[freeTerrains[i]];
        }

        chunkGraphics = newChunkGraphics;

        for (int i = 0; i < loadingIndexes.Count; i++)
        {
            LoadChunkFromMemory(visibleChunks[loadingIndexes[i]], loadingIndexes[i]);
        }

    }

    void LoadChunkFromMemory(Vector2 cordinateIndex, int graphicIndex)
    {

        bool found = false;

        foreach (Vector2 v in loadedChunks)
        {
            if (v == cordinateIndex)
            {
                found = true;
                break;
            }
        }

        GameObject terrainGO;

        if (!found)
        {
            terrainGO = GenerateChunk(cordinateIndex, graphicIndex);
        }
        else
        {
            //Load the chunk from memory
            Debug.Log("Loading (" + cordinateIndex.x + "," + cordinateIndex.y + ")");

            terrainGO = chunkGraphics[graphicIndex].gameObject;
        }

        terrainGO.transform.position = new Vector3(chunkSizeX * cordinateIndex.x, 0, chunkSizeY * cordinateIndex.y);
    }

    GameObject GenerateChunk(Vector2 cordinateIndex, int graphicIndex)
    {
        Debug.Log("Generating (" + cordinateIndex.x + "," + cordinateIndex.y + ")");

        GameObject terrainGO = chunkGraphics[graphicIndex].gameObject;

        loadedChunks.Add(cordinateIndex);

        SetTerrainHeightmap(terrainGO.GetComponent<Terrain>(), cordinateIndex);

        SetTerrainTextures(terrainGO.GetComponent<Terrain>(), cordinateIndex);

        SetTerrainTrees(terrainGO.GetComponent<Terrain>());

        return terrainGO;
    }

    void SetTerrainHeightmap(Terrain terrain, Vector2 cordinateIndex)
    {
        float[,] heights = new float[terrain.terrainData.heightmapHeight, terrain.terrainData.heightmapWidth];

        for (int x = 0; x < heights.GetLength(1); x++)
        {
            for (int y = 0; y < heights.GetLength(0); y++)
            {
                heights[y, x] = TERRAIN_HEIGHT_WILDCARD;
            }
        }

        bool left = false;
        bool right = false;
        bool top = false;
        bool bottom = false;

        float[,] hm = GetTerrainHeightmap(new Vector2(cordinateIndex.x - 1, cordinateIndex.y));

        if (hm != null)
        {
            left = true;

            for (int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, 0] = hm[i, hm.GetLength(1) - 1];
            }
        }

        hm = GetTerrainHeightmap(new Vector2(cordinateIndex.x + 1, cordinateIndex.y));

        if (hm != null)
        {
            right = true;

            for (int i = 0; i < hm.GetLength(0); i++)
            {
                heights[i, heights.GetLength(1) - 1] = hm[i, 0];
            }
        }

        hm = GetTerrainHeightmap(new Vector2(cordinateIndex.x, cordinateIndex.y - 1));

        if (hm != null)
        {
            top = true;

            for (int i = 0; i < hm.GetLength(1); i++)
            {
                heights[0, i] = hm[hm.GetLength(0) - 1, i];
            }
        }

        hm = GetTerrainHeightmap(new Vector2(cordinateIndex.x, cordinateIndex.y + 1));

        if (hm != null)
        {
            bottom = true;

            for (int i = 0; i < hm.GetLength(1); i++)
            {
                heights[heights.GetLength(0) - 1, i] = hm[0, i];
            }
        }

        if (!top && !left)
            heights[0, 0] = 0.2f;

        if (!left && !bottom)
            heights[terrain.terrainData.heightmapHeight - 1, 0] = 0.2f;

        if (!top && !right)
            heights[0, terrain.terrainData.heightmapWidth - 1] = 0.2f;

        if (!bottom && !right)
            heights[terrain.terrainData.heightmapHeight - 1, terrain.terrainData.heightmapWidth - 1] = 0.2f;

        heights = DiamondSquare(heights, 0, 0, terrain.terrainData.heightmapWidth - 1, 0);

        terrain.terrainData.SetHeights(0, 0, heights);
    }

    void SetTerrainTextures(Terrain terrain, Vector2 cordinateIndex)
    {
        int currentBiome = 0;
        float[,,] alphamap = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, terrainTextures.Length];

        for(int x = 0; x < alphamap.GetLength(0); x++)
        {
            for(int y = 0; y < alphamap.GetLength(1); y++)
            {
                //Cliff Texture - steep places
                //Grass Texture - flat places
                float normX = x *  1.0f / (alphamap.GetLength(0) - 1);
                float normY = y * 1.0f / (alphamap.GetLength(1) - 1);

                float steepness = terrain.terrainData.GetSteepness(normX, normY);
                float height = terrain.terrainData.GetHeight((int)(normX * terrain.terrainData.heightmapWidth), (int)(normY * terrain.terrainData.heightmapHeight)); //0 - 1
                
                float isCliff = Mathf.Clamp(steepness - 50, 0, 10) / 10.0f; //40 -  50
                float isSnow = Mathf.Clamp(height - snowThreshold, 0, 30) / 30.0f;
                float isRiverbank = Mathf.Clamp(waterThreshold - height + 15, 0, 10) / 10.0f;

                //are we rocky?
                float isRocky = Mathf.Clamp(steepness - 60, 0, 10) / 10.0f;

                //move up
                alphamap[y, x, currentBiome + 2] = isCliff;
                alphamap[y, x, currentBiome + 1] = isRocky - isCliff;
                alphamap[y, x, currentBiome + 3] = isRiverbank - isCliff - isRocky;
                alphamap[y, x, currentBiome + 4] = isSnow - -isRocky - isCliff - isRiverbank;
                alphamap[y, x, currentBiome] = 1 - isCliff - isRocky - isSnow - isRiverbank;
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, alphamap);
    }

    void SetTerrainTrees(Terrain terrain)
    {
        terrain.terrainData.treeInstances = new TreeInstance[0];
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y+=20)
            {
                float height = terrain.terrainData.GetHeight(x, y);
                float steepness = terrain.terrainData.GetSteepness((float)x / (float)resolution, (float)(y) / (float)resolution);

                if (Random.value > (0.3f + (steepness / 30)) && height >= waterThreshold - 10)
                {
                    TreeInstance instance = new TreeInstance();
                    instance.prototypeIndex = 0;
                    instance.position = new Vector3((float)x / (float)resolution, 0, (float)(y + Random.Range(0,10)) / (float)resolution);
                    instance.widthScale = 1.0f;
                    instance.heightScale = 1.0f;
                    instance.color = Color.white;
                    instance.rotation = 0.0f;

                    terrain.AddTreeInstance(instance);
                    x += 20;
                }
            }
        }
    }

    float[,] GetTerrainHeightmap(Vector2 cordinateIndex)
    {
        if (loadedChunks.Contains(cordinateIndex))
        {
            for (int i = 0; i < visibleChunks.Length; i++)
            {
                if (visibleChunks[i].x == cordinateIndex.x && visibleChunks[i].y == cordinateIndex.y)
                {
                    return chunkGraphics[i].terrainData.GetHeights(0, 0, chunkGraphics[i].terrainData.heightmapWidth, chunkGraphics[i].terrainData.heightmapHeight);
                }
            }

            return LoadHeightmapFromMemory(cordinateIndex);
        }
        else
        {
            return null;
        }
    }

    float[,] DiamondSquare(float[,] heights, int offsetX, int offsetY, int squareSize, int depth)
    {
        if (squareSize == 1)
            return heights;

        float topLeft = heights[offsetY, offsetX];
        float topRight = heights[offsetY, offsetX + squareSize];
        float bottomLeft = heights[offsetY + squareSize, offsetX];
        float bottomRight = heights[offsetY + squareSize, offsetX + squareSize];

        if (topLeft == TERRAIN_HEIGHT_WILDCARD || topRight == TERRAIN_HEIGHT_WILDCARD || bottomLeft == TERRAIN_HEIGHT_WILDCARD || bottomRight == TERRAIN_HEIGHT_WILDCARD)
            Debug.LogError("One or more Corner Seed Values is not set");

        if (heights[offsetY + (squareSize / 2), offsetX + (squareSize / 2)] == TERRAIN_HEIGHT_WILDCARD)
            heights[offsetY + (squareSize / 2), offsetX + (squareSize / 2)] = GetRandomHeight(depth) + AveragePoints(topLeft, topRight, bottomLeft, bottomRight);

        float centrePoint = heights[offsetY + (squareSize / 2), offsetX + (squareSize / 2)];

        //left diamond
        float runningAverage = AveragePoints(topLeft, centrePoint, bottomLeft);

        if (offsetX - (squareSize / 2) > 0 && heights[offsetY + (squareSize / 2), offsetX - (squareSize / 2)] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(topLeft, centrePoint, bottomLeft, heights[offsetY + (squareSize / 2), offsetX - (squareSize / 2)]);
        }

        if (heights[offsetY + (squareSize / 2), offsetX] == TERRAIN_HEIGHT_WILDCARD)
            heights[offsetY + (squareSize / 2), offsetX] = runningAverage + GetRandomHeight(depth);

        //right diamond
        runningAverage = AveragePoints(topRight, centrePoint, bottomRight);

        if (offsetX + (squareSize * 1.5f) < heights.GetLength(1) && heights[offsetY + (squareSize / 2), offsetX + (int)(squareSize * 1.5f)] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(topRight, centrePoint, bottomRight, heights[offsetY + (squareSize / 2), offsetX + (int)(squareSize * 1.5f)]);
        }

        if (heights[offsetY + (squareSize / 2), offsetX + squareSize] == TERRAIN_HEIGHT_WILDCARD)
            heights[offsetY + (squareSize / 2), offsetX + squareSize] = runningAverage + GetRandomHeight(depth);

        //top diamond
        runningAverage = AveragePoints(topLeft, centrePoint, topRight);

        if (offsetY - (squareSize / 2) > 0 && heights[offsetY - (squareSize / 2), offsetX + (squareSize / 2)] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(topLeft, centrePoint, topRight, heights[offsetY - (squareSize / 2), offsetX + (squareSize / 2)]);
        }

        if (heights[offsetY, offsetX + (squareSize / 2)] == TERRAIN_HEIGHT_WILDCARD)
            heights[offsetY, offsetX + (squareSize / 2)] = runningAverage + GetRandomHeight(depth);

        //bottom diamond
        runningAverage = AveragePoints(bottomRight, centrePoint, bottomLeft);

        if (offsetY + (squareSize * 1.5f) < heights.GetLength(0) && heights[offsetY + (int)(squareSize * 1.5f), offsetX + (squareSize / 2)] != TERRAIN_HEIGHT_WILDCARD)
        {
            runningAverage = AveragePoints(bottomRight, centrePoint, topRight, heights[offsetY + (int)(squareSize * 1.5f), offsetX + (squareSize / 2)]);
        }

        if (heights[offsetY + squareSize, offsetX + (squareSize / 2)] == TERRAIN_HEIGHT_WILDCARD)
            heights[offsetY + squareSize, offsetX + (squareSize / 2)] = runningAverage + GetRandomHeight(depth);

        heights = DiamondSquare(heights, offsetX, offsetY, squareSize / 2, depth + 1);
        heights = DiamondSquare(heights, offsetX + (squareSize / 2), offsetY, squareSize / 2, depth + 1);
        heights = DiamondSquare(heights, offsetX, offsetY + (squareSize / 2), squareSize / 2, depth + 1);
        heights = DiamondSquare(heights, offsetX + (squareSize / 2), offsetY + (squareSize / 2), squareSize / 2, depth + 1);

        return heights;
    }

    float AveragePoints(float p1, float p2, float p3, float p4)
    {
        return (p1 + p2 + p3 + p4) * 0.25f;
    }

    float AveragePoints(float p1, float p2, float p3)
    {
        return (p1 + p2 + p3) * 0.33f;
    }

    float GetRandomHeight(int depth)
    {
        return Random.Range(-0.2f, 0.2f) / Mathf.Pow(2, depth);
    }

    float[,] LoadHeightmapFromMemory(Vector2 cordinateIndex)
    {
        return null;
    }

    void SaveChunkToMemory(Terrain chunk, Vector2 index)
    {
        Debug.Log("Unloading Chunk (" + index.x + "," + index.y + ")");
    }

    SplatPrototype BlueprintToSplatPrototype(TerrainTextureBlueprint blueprint)
    {
        SplatPrototype prototype = new SplatPrototype();
        prototype.texture = blueprint.albedo;
        prototype.normalMap = blueprint.normal;
        prototype.metallic = blueprint.metalic;
        prototype.tileSize = blueprint.size;
        prototype.tileOffset = blueprint.offset;

        return prototype;
    }

    TreePrototype BlueprintToTreePrototype(TreeBlueprint blueprint)
    {
        TreePrototype prototype = new TreePrototype();
        prototype.prefab = blueprint.prefab;
        prototype.bendFactor = blueprint.bendFactor;
        return prototype;
    }
}
