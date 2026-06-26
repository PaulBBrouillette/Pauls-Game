using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {
    public static GridManager Instance;
    public int mapHeight;
    public int mapWidth;
    public GameObject tilePrefab;
    public GameObject tileSidePrefab;
    public GameplayManager gameplayManager;
    private Tile[,] map;
    private MapLayout layout;
    private int tileCount;

    void Awake() => Instance = this;

    public void InitializeMap() {
        Debug.Log("GridManager Start - START");
        mapHeight = BetweenScene.Instance.gridHeight;
        mapWidth = BetweenScene.Instance.gridWidth;
        layout = BetweenScene.Instance.mapLayout;
        tileCount = mapHeight * mapWidth;
        map = new Tile[mapHeight, mapWidth];
        int teamNumber = gameplayManager.getPlayers().Count;
        int[] tiles = new int[4];

        var tPerTeam = GetTilesPerTeam(teamNumber);

        tiles[0] = tPerTeam.t1Tiles;
        tiles[1] = tPerTeam.t2Tiles;
        tiles[2] = tPerTeam.t3Tiles;
        tiles[3] = tPerTeam.t4Tiles;
        Debug.Log($"Map size is {mapWidth}x{mapHeight}");

        bool validIndex = false;
        // Random tiles in random places
        if (layout == MapLayout.Scrambled) {
            for (int i = 0; i < mapHeight; i++) {
                for (int j = 0; j < mapWidth; j++) {
                    validIndex = false;
                    GameObject tileGO = Instantiate(tilePrefab, new Vector3(j, 0, i), Quaternion.identity);

                    tileGO.transform.parent = this.transform;

                    tileGO.name = $"Tile_{i}_{j}";

                    Tile tileScript = tileGO.GetComponent<Tile>();
                    if (tileScript != null) {

                        while (!validIndex) {
                            int rand = Random.Range(0, teamNumber);
                            if (tiles[rand] > 0) {
                                validIndex = true;
                                tiles[rand]--;
                                Team team = Team.One;
                                switch (rand) {
                                    case 0: team = Team.One; break;
                                    case 1: team = Team.Two; break;
                                    case 2: team = Team.Three; break;
                                    case 3: team = Team.Four; break;
                                }
                                tileScript.setTeam(team);
                                map[i, j] = tileScript;
                                tileScript.gridPos = new Vector2Int(i, j);
                                break;
                            }
                        }
                    }
                }
            }
            // Assign num tiles to each player
            GameplayManager.Instance.AssignTileAmtsToPlayers(tPerTeam.t1Tiles, tPerTeam.t2Tiles, tPerTeam.t3Tiles, tPerTeam.t4Tiles);
        }
        else {
            // 1. Generate the data beforehand
            Team[,] generatedMap = GenerateMapData(mapWidth, mapHeight, teamNumber);
            int red = 0;
            int blue = 0;
            int green = 0;
            int yellow = 0;
            foreach (Team i in generatedMap) {
                if (i == Team.One) red++;
                if (i == Team.Two) blue++;
                if (i == Team.Three) green++;
                if (i == Team.Four) yellow++;
            }
            Debug.Log($"{red} {blue} {green} {yellow}");

            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    GameObject tileGO = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                    Tile tileScript = tileGO.GetComponent<Tile>();
                    tileGO.transform.parent = this.transform;
                    tileGO.name = $"Tile_{y}_{x}";
                    // Use [x, y] to match the [width, height] definition
                    // If your array was defined as [width, height], use [x, y]
                    tileScript.setTeam(generatedMap[x, y]);

                    tileScript.gridPos = new Vector2Int(x, y);
                    map[y, x] = tileScript;
                }
            }
            // Assign num tiles to each player
            GameplayManager.Instance.AssignTileAmtsToPlayers(red, blue, green, yellow);
        }

        AssignNeighbors();
        BuildConnections();
        Debug.Log("GridManager Start - END");
    }

    void AssignNeighbors() {
        for (int i = 0; i < mapHeight; i++) {
            for (int j = 0; j < mapWidth; j++) {
                Tile current = map[i, j];

                // --- HORIZONTAL SIDES (Check Left Neighbor) ---
                if (j > 0) {
                    // Not on the left edge? Steal the 'RightSide' of the neighbor to our left
                    current.leftSide = map[i, j - 1].rightSide;
                    current.leftNeighbor = map[i, j - 1];
                    current.leftSide.tileB = current; // Assign ourselves as the second tile
                }
                else {
                    // On the far left border? Create a fresh side
                    current.leftSide = new TileSide { tileA = current };
                }

                // --- VERTICAL SIDES (Check Bottom Neighbor) ---
                if (i > 0) {
                    // Not on the bottom edge? Steal the 'TopSide' of the neighbor below us
                    current.bottomSide = map[i - 1, j].topSide;
                    current.bottomNeighbor = map[i - 1, j];
                    current.bottomSide.tileB = current;
                }
                else {
                    // On the bottom border? Create a fresh side
                    current.bottomSide = new TileSide { tileA = current };
                }

                // --- CREATE UNKNOWN SIDES (Top and Right) ---
                // These will be "stolen" by the next tiles in the loop
                current.topSide = new TileSide { tileA = current };
                if ((i + 1 < mapHeight) && map[i + 1, j] != null) {
                    current.topNeighbor = map[i + 1, j];
                }
                
                current.rightSide = new TileSide { tileA = current };
                if ((j + 1 < mapWidth) && map[i, j + 1] != null) {
                    current.rightNeighbor = map[i, j + 1];
                }
            }
        }
    }

    void BuildConnections() {
        for (int i = 0; i < mapHeight; i++) {
            for (int j = 0; j < mapWidth; j++) {
                Tile tile = map[i, j];

                TileSide top = tile.topSide;
                TileSide right = tile.rightSide;
                TileSide bottom = tile.bottomSide;
                TileSide left = tile.leftSide;

                // --- SAME TILE CONNECTIONS ---

                // Adjacent sides (cost 1)
                Connect(top, right, 1);
                Connect(right, bottom, 1);
                Connect(bottom, left, 1);
                Connect(left, top, 1);

                // Opposite sides (cost 1, across tile)
                Connect(top, bottom, 2);
                Connect(left, right, 2);
            }
        }
    }

    void Connect(TileSide a, TileSide b, int cost) {
        if (a == null || b == null) return;
        a.connections.Add(new TileSideConnection { target = b, cost = cost });
        b.connections.Add(new TileSideConnection { target = a, cost = cost });
    }

    /**
     * Based on the total amount of tiles and the number of teams, get an equal amount of tiles
     * per team. If (totalTiles % teamNumber) != 0, the remaining tiles get assigned to the players
     * in reverse turn order to compensate for their turns happening after other players
     */
    private (int t1Tiles, int t2Tiles, int t3Tiles, int t4Tiles) GetTilesPerTeam(int teamNumber) {
        int t1Tiles = 0;
        int t2Tiles = 0;
        int t3Tiles = 0;
        int t4Tiles = 0;
        if (teamNumber == 2) {
            if (tileCount % 2 == 0) {
                t1Tiles = tileCount / 2;
                t2Tiles = tileCount / 2;
            }
            else {
                t1Tiles = tileCount / 2;
                t2Tiles = tileCount / 2 + 1;
            }
        }
        else if (teamNumber == 3) {
            if (tileCount % 3 == 0) {
                t1Tiles = tileCount / 3;
                t2Tiles = tileCount / 3;
                t3Tiles = tileCount / 3;
            }
            else if (tileCount % 3 == 1) {
                t1Tiles = tileCount / 3;
                t2Tiles = tileCount / 3;
                t3Tiles = tileCount / 3 + 1;
            }
            else {
                t1Tiles = tileCount / 3;
                t2Tiles = tileCount / 3 + 1;
                t3Tiles = tileCount / 3 + 1;
            }
        }
        else if (teamNumber == 4) {
            if (tileCount % 4 == 0) {
                t1Tiles = tileCount / 4;
                t2Tiles = tileCount / 4;
                t3Tiles = tileCount / 4;
                t4Tiles = tileCount / 4;
            }
            else if (tileCount % 4 == 1) {
                t1Tiles = tileCount / 4;
                t2Tiles = tileCount / 4;
                t3Tiles = tileCount / 4;
                t4Tiles = tileCount / 4 + 1;
            }
            else if (tileCount % 4 == 2) {
                t1Tiles = tileCount / 4;
                t2Tiles = tileCount / 4;
                t3Tiles = tileCount / 4 + 1;
                t4Tiles = tileCount / 4 + 1;
            }
            else {
                t1Tiles = tileCount / 4;
                t2Tiles = tileCount / 4 + 1;
                t3Tiles = tileCount / 4 + 1;
                t4Tiles = tileCount / 4 + 1;
            }
        }
        return (t1Tiles, t2Tiles, t3Tiles, t4Tiles);
    }

    public Team[,] GenerateMapData(int width, int height, int numTeams) {
        Team[,] mapData = new Team[width, height];
        bool[,] assigned = new bool[width, height];
        Queue<Vector2Int>[] teamQueues = new Queue<Vector2Int>[numTeams];
        int[] teamCounts = new int[numTeams];

        // 1. Calculate quotas
        int totalTiles = width * height;
        int baseQuota = totalTiles / numTeams;
        int remainder = totalTiles % numTeams;

        var starts = GetSeedStarts(numTeams, width, height);
        Vector2Int[] seeds = new[] { starts.a, starts.b, starts.c, starts.d };

        // 2. Initialize seeds
        for (int i = 0; i < numTeams; i++) {
            teamQueues[i] = new Queue<Vector2Int>();
            // Assign a specific quota for this team (including remainder if applicable)
            int target = baseQuota + (i < remainder ? 1 : 0);

            // Find seed... (same as before)
            Vector2Int seed = seeds[i];
            mapData[seed.x, seed.y] = (Team)i;
            assigned[seed.x, seed.y] = true;
            teamQueues[i].Enqueue(seed);
            teamCounts[i]++;
        }

        // 3. Grow with Quotas
        bool tilesLeft = true;
        while (tilesLeft) {
            tilesLeft = false;
            for (int i = 0; i < numTeams; i++) {
                // Determine quota for this specific team
                int target = baseQuota + (i < remainder ? 1 : 0);

                // Only expand if we have tiles left to place AND this team hasn't met their quota
                if (teamQueues[i].Count > 0 && teamCounts[i] < target) {
                    tilesLeft = true;
                    Vector2Int current = teamQueues[i].Dequeue();

                    // Try to find a valid neighbor to claim
                    Vector2Int[] neighbors = {
                    current + Vector2Int.up, current + Vector2Int.down,
                    current + Vector2Int.left, current + Vector2Int.right
                };

                    foreach (var neighbor in neighbors) {
                        // Check if team is still under quota before claiming the neighbor
                        if (IsWithinBounds(neighbor, width, height) && !assigned[neighbor.x, neighbor.y] && teamCounts[i] < target) {
                            assigned[neighbor.x, neighbor.y] = true;
                            mapData[neighbor.x, neighbor.y] = (Team)i;
                            teamQueues[i].Enqueue(neighbor);
                            teamCounts[i]++;
                        }
                    }
                }
            }
        }
        return mapData;
    }

    private (Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d) GetSeedStarts(int teamNumber, int width, int height) {
        Vector2Int seed1 = new Vector2Int();
        Vector2Int seed2 = new Vector2Int();
        Vector2Int seed3 = new Vector2Int();
        Vector2Int seed4 = new Vector2Int();

        // Opposite corners
        if (teamNumber == 2) {
            seed1 = new Vector2Int(0, 0);
            seed2 = new Vector2Int(width - 1, height - 1);
            return (seed1, seed2, seed3, seed4);
        }
        // Top middle, two thirds down the left side, two thirds down the right side
        else if (teamNumber == 3) {
            seed1 = new Vector2Int(width / 2, height - 1);
            seed2 = new Vector2Int(0, (int)(height * (2f / 3f)));
            seed3 = new Vector2Int(width - 1, (int)(height * (2f / 3f)));
            return (seed1, seed2, seed3, seed4);
        }
        // Corners
        else if (teamNumber == 4) {
            seed1 = new Vector2Int(0, 0);
            seed2 = new Vector2Int(width - 1, height - 1);
            seed3 = new Vector2Int(0, height - 1);
            seed4 = new Vector2Int(width - 1, 0);
            return (seed1, seed2, seed3, seed4);
        }
        // Done goofed
        else {
            return (seed1, seed2, seed3, seed4);
        }
    }

    private bool IsWithinBounds(Vector2Int pos, int w, int h) => pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h;
}