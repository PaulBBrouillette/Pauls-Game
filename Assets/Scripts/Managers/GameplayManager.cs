using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

public enum Direction { Top, Bottom, Left, Right, None }

public class GameplayManager : MonoBehaviour {
    public static GameplayManager Instance;

    [Header("Prefabs & References")]
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject ghostPiecePrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject PieceDisplayPrefab;
    [SerializeField] private GameObject CardDisplayPrefab;
    [SerializeField] private GameObject mainUIPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private TextMeshProUGUI currentPlayerUI;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button capitulateButton;
    [SerializeField] private Button backFromShopButton;
    [SerializeField] private Image attackerIcon;
    [SerializeField] private Image defenderIcon;
    [SerializeField] private Transform playerPieceStockContent; // Player's stock of pieces
    [SerializeField] private Transform playerCardStockContent; // Player's stock of cards
    [SerializeField] private GameObject cardStockObject;
    [SerializeField] private GameObject pieceStockObject;
    [SerializeField] private GameObject shopItem;
    private GameObject currentPlayerGO;
    public Player currentPlayerScript;

    [Header("Settings")]
    public int playerCount;
    public float pieceOffset = 0.25f;

    [Header("Shop Settings")]
    public List<PieceData> availablePieces;
    private PieceData selectedPieceToBuy;   // Which one the player clicked in the UI
    private int selectedPieceIndex; // Unique stock index for deletion
    
    public List<CardData> availableCards;
    private CardData selectedCardToBuy;
    private int selectedCardIndex;

    [Header("Combat References")]
    private Piece attackerPiece;
    private Piece defenderPiece;

    // State Management
    public List<GameObject> players = new List<GameObject>();
    private TurnPhase phase = TurnPhase.Start;

    private GameObject activeGhost; // Transparent icon when placing a bought piece
    private GameObject currentMovingPiece; // Global reference to a piece that was placed but is currently being moved
    private Vector3 originalPieceLocation; // When unsuccessfully moving a piece, it will be placed back here, its original location
    private Dictionary<TileSide, int> debugReachable; // Available places to place a piece when moving it
    public List<Piece> piecesOnBoard; // List of every piece currently on the map
    private HashSet<Tile> touchingTiles; // Friendly tiles touching a specified tile
    private int initAvgTiles; // The starting average number of tiles per player at game start
    public int basePrice; // Base price for scaling item prices
    private bool roundOneOver = false;

    // A simple struct to hold what the mouse is currently looking at
    private struct PointerInfo {
        public Tile tile;
        public Direction direction;
        public TileSide side;
        public bool isTileA;
        public bool isOccupied;
        public Vector3 worldPoint;
    }

    private PointerInfo tempAttackInfo;
    private bool isSneakAttack = false;

    void Awake() => Instance = this;

    void Start() {

        // Should never be null, but for testing purposes 
        // just do sum slight like this
        if (BetweenScene.Instance == null) {
            this.AddComponent<BetweenScene>();
        }
        Debug.Log("GameplayManager Start - START");
        playerCount = BetweenScene.Instance.numPlayers;
        InitializeGhost();
        InitializePlayers();
        piecesOnBoard = new List<Piece>();

        CloseCardStock();
        ClosePieceStock();
        shopPanel.SetActive(false);
        battlePanel.SetActive(false);
        shopButton.onClick.AddListener(OpenShop);
        capitulateButton.onClick.AddListener(Capitulate);
        backFromShopButton.onClick.AddListener(BackToWait);
        GridManager.Instance.InitializeMap();
        CameraManager.Instance.SetupCamera(BetweenScene.Instance.gridWidth, BetweenScene.Instance.gridHeight);
        SetupShop();
        SetPhase(TurnPhase.RoundOneStart);
        DisableShop();
        Debug.Log("GameplayManager Start - END");
    }

    void Update() {
        HandleCurrentPhase();
        UpdateUI();
    }

    #region Phase Handling

    private void HandleCurrentPhase() {
        PointerInfo info = GetPointerInfo();
        switch (phase) {
            case TurnPhase.RoundOneStart:
                Debug.Log($"{currentPlayerGO.name} Start!");
                currentPlayerScript.ResetMoves();
                LoadNewPlayerInfo();
                SetPhase(TurnPhase.RoundOneWait);
                break;

            // During each player's first turn, give some pieces, and allow them to place them but not move
            case TurnPhase.RoundOneWait:
                activeGhost.SetActive(false);
                break;

            case TurnPhase.RoundOneTurnEnd:
                NextPlayer();
                if (roundOneOver) {
                    SetPhase(TurnPhase.Start);
                }
                else {
                    SetPhase(TurnPhase.RoundOneStart);
                }
                break;

            case TurnPhase.Start:
                Debug.Log($"{currentPlayerGO.name} Start!");
                currentPlayerScript.ResetMoves();
                LoadNewPlayerInfo();
                SetPhase(TurnPhase.Wait);
                EnableShop();
                break;

            case TurnPhase.Wait:
                activeGhost.SetActive(false);

                // Transition to Move if we grab a piece
                if (InputManager.Controls.Player.Select.triggered && TryGrabPiece(info)) {
                    if (currentMovingPiece != null) {
                        TogglePieceColliders(false);
                    }
                    SetPhase(TurnPhase.MovePiece);
                }
                break;

            case TurnPhase.Buy:
                break;

            case TurnPhase.PlacePiece:
                HandlePlacementGhost(info);
                // Right click to cancel
                if (InputManager.Controls.Player.Unselect.triggered) {
                    SetPhase(TurnPhase.Wait);
                }
                if (InputManager.Controls.Player.Select.triggered && info.tile != null && !info.isOccupied) {
                    PlacePiece(info.tile, info.direction, info.isTileA, info.side);
                }
                break;

            case TurnPhase.ChooseCardTarget:
                // Right click to cancel
                if (InputManager.Controls.Player.Unselect.triggered) {
                    SetPhase(TurnPhase.Wait);
                }
                if (InputManager.Controls.Player.Select.triggered) {
                    TargetType type = selectedCardToBuy.targetType;
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    switch (type) {
                        case TargetType.FriendlyPiece:
                            // Check that the piece you are selecting is a friendly one
                            if (Physics.Raycast(ray, out RaycastHit hit)) {
                                if (hit.collider.CompareTag("Piece")) {
                                    Piece p = hit.collider.GetComponent<Piece>();
                                    if (p != null && p.team == currentPlayerScript.team) {
                                        Debug.Log("This is a friendly piece!");
                                        selectedCardToBuy.PlayCard(currentPlayerScript, null, p, null, null, null);
                                        RemoveCardFromStock();
                                        SetPhase(TurnPhase.Wait);
                                    }
                                }
                            }
                            break;

                        case TargetType.EnemyPiece:
                            if (Physics.Raycast(ray, out RaycastHit hit3)) {
                                if (hit3.collider.CompareTag("Piece")) {
                                    Piece p = hit3.collider.GetComponent<Piece>();
                                    if (p != null && p.team != currentPlayerScript.team) {
                                        Debug.Log("This is an enemy piece!");
                                        selectedCardToBuy.PlayCard(currentPlayerScript, null, p, null, null, null);
                                        RemoveCardFromStock();
                                        SetPhase(TurnPhase.Wait);

                                        // Special cases:
                                        // Erase: 

                                    }
                                }
                            }
                            break;

                        case TargetType.EnemyTile:
                            if (Physics.Raycast(ray, out RaycastHit hit2)) {
                                if (hit2.collider.CompareTag("Tile")) {
                                    Tile t = hit2.collider.GetComponent<Tile>();
                                    if (t != null && t.team != currentPlayerScript.team) {
                                        if (selectedCardToBuy.cardType == CardType.Imbue) {
                                            RemoveCardFromStock();
                                            SetPhase(TurnPhase.Wait);
                                        }
                                        else {
                                            Team te = info.tile.team;
                                            Player p = currentPlayerScript;
                                            for (int i = 0; i < players.Count; i++) {
                                                if (players[i].GetComponent<Player>().team == te) {
                                                    p = players[i].GetComponent<Player>();
                                                }
                                            }
                                            selectedCardToBuy.PlayCard(currentPlayerScript, p, null, null, t, null);
                                            RemoveCardFromStock();
                                            SetPhase(TurnPhase.Wait);
                                        }
                                    }
                                }
                            }
                            break;

                        case TargetType.FriendlyEnemyTile:

                            break;
                    }
                    
                }
                break;

            case TurnPhase.MovePiece:

                UpdatePieceDrag(info);
                if (InputManager.Controls.Player.Select.triggered) { // Press again to place
                    FinishPieceMove(info);
                }
                break;

                // When a existing piece is able to reach a piece of an opposing team, the attacker and defender are locked
                // into a battle scene where dice are rolled, any abilities and cards are used and the loser is destroyed
            case TurnPhase.Attack:
                if (InputManager.Controls.Player.BattleAdvance.WasPressedThisFrame()) {
                    CombatManager.Instance.Attack(attackerPiece, defenderPiece, isSneakAttack);
                }
                break;

            case TurnPhase.BattleStart:
                battlePanel.SetActive(true);
                attackerIcon.sprite = attackerPiece.data.shopIcon;
                defenderIcon.sprite = defenderPiece.data.shopIcon;
                mainUIPanel.SetActive(false);
                SetPhase(TurnPhase.Attack);
                break;

            // Wrap up battle
            case TurnPhase.BattleEnd:
                if (InputManager.Controls.Player.BattleAdvance.WasPressedThisFrame()) {
                    battlePanel.SetActive(false);
                    attackerIcon.sprite = null;
                    defenderIcon.sprite = null;
                    mainUIPanel.SetActive(true);
                    CombatManager.Instance.ResetUI();
                    SetPhase(TurnPhase.Wait);
                }
                break;

            case TurnPhase.TurnEnd:
                // Clean up and load UI for next player including pieces in stock among other things TBD...
                NextPlayer();
                SetPhase(TurnPhase.Start);
                break;

            case TurnPhase.AfterCaptureCheck:
                for (int i = players.Count - 1; i >= 0; i--) {
                    Player p = players[i].GetComponent<Player>();
                    if (p != null) {
                        if (p.ownedTiles <= 0) {
                            players.Remove(players[i]);
                            Debug.Log("Removing " + p.name + " because they have no tiles left");
                        }
                    }
                }

                if (players.Count == 1) {
                    SetPhase(TurnPhase.GameEnd);
                }
                else {
                    SetPhase(TurnPhase.Wait);
                }
                break;

            case TurnPhase.GameEnd:
                capitulateButton.enabled = false;
                shopButton.enabled = false;
                break;
        }
    }

    #endregion

    #region Piece Placement & Movement

    public void ChoosePieceToPlace(PieceData p, int index) {
        CloseCardStock();
        ClosePieceStock();
        selectedPieceToBuy = p;
        selectedPieceIndex = index;
        SetPhase(TurnPhase.PlacePiece);
    }

    public void ChooseCardToPlace(CardData p, int index) {
        CloseCardStock();
        ClosePieceStock();
        selectedCardToBuy = p;
        selectedCardIndex = index;
        Debug.Log($"You can place this card on a {p.targetType}");
        SetPhase(TurnPhase.ChooseCardTarget);
    }

    void PlacePiece(Tile tile, Direction direction, bool isThisTileA, TileSide side) {
        Vector3 spawnPos = DetermineOffset(tile, direction);

        GameObject pieceGO = Instantiate(piecePrefab, spawnPos, Quaternion.identity);

        // INITIALIZE the piece with our data
        Piece p = pieceGO.GetComponent<Piece>();
        p.Initialize(selectedPieceToBuy, currentPlayerScript.getTeam());
        p.SetPosition(side, isThisTileA);
        p.currentHealth = selectedPieceToBuy.initMaxHealth;
        p.currentTile = tile;
        pieceGO.transform.parent = tile.transform;

        // Save the reference to the correct slot
        if (isThisTileA) {
            side.occupantA = pieceGO;
        }
        else {
            side.occupantB = pieceGO;
        }
        piecesOnBoard.Add(p);
        // After setting a piece, go back to waiting
        currentPlayerScript.RemovePieceFromStock(selectedPieceToBuy);
        foreach (Transform child in playerPieceStockContent) {
            PieceDisplay p2 = child.GetComponent<PieceDisplay>();
            if (p2 != null) {
                if (p2.id == selectedPieceIndex) {
                    Destroy(child.gameObject);
                }
            }
        }
        if (roundOneOver) {
            SetPhase(TurnPhase.Wait);
        }
        else {
            SetPhase(TurnPhase.RoundOneWait);
        }
        
    }

    private void HandlePlacementGhost(PointerInfo info) {
        if (info.tile != null && info.tile.team == currentPlayerScript.getTeam() && !info.isOccupied) {
            activeGhost.transform.position = GetWorldPosition(info.tile, info.direction);
            activeGhost.SetActive(true);
        }
        else {
            activeGhost.SetActive(false);
        }
    }

    private bool TryGrabPiece(PointerInfo info) {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.collider.CompareTag("Piece")) {
                Piece p = hit.collider.GetComponent<Piece>();
                if (p != null && p.getTeam() == currentPlayerScript.getTeam()) {
                    currentMovingPiece = hit.collider.gameObject;
                    originalPieceLocation = currentMovingPiece.transform.position;
                    GetTouchingTiles(currentMovingPiece.GetComponent<Piece>().currentTile);
                    debugReachable = GetReachableWithCost(p.currentSide, p.getTotalMoveRange());
                    return true;
                }
            }
        }
        return false;
    }

    // Move the piece to where the mouse is dragging it
    private void UpdatePieceDrag(PointerInfo info) {
        if (currentMovingPiece != null) {
            if (info.tile == null) {
                currentMovingPiece.transform.position = originalPieceLocation;
            }
            else {
                currentMovingPiece.transform.position = new Vector3(info.worldPoint.x, 0.5f, info.worldPoint.z);
            }
        }
    }

    private void FinishPieceMove(PointerInfo info) {
        // Turn collisions back on
        TogglePieceColliders(true);

        if (info.side != null) {
            Piece piece = currentMovingPiece.GetComponent<Piece>();

            bool canMove = CanMoveTo(piece, info);

            if (canMove) {
                MovePiece(piece, info.side, info.isTileA, info.tile);
                Debug.Log($"Moved piece {piece.name} to {info.tile.name}");
            }
            else {
                currentMovingPiece.transform.position = originalPieceLocation;
            }
        }
        else {
            currentMovingPiece.transform.position = originalPieceLocation;
            Debug.Log("Could not move piece, info.side is null");
        }
        debugReachable.Clear();

        // If a battle is not being started, check grid conditions
        if (phase != TurnPhase.BattleStart) {
            currentMovingPiece = null;
            SetPhase(TurnPhase.AfterCaptureCheck);
        }
    }

    bool CanMoveTo(Piece piece, PointerInfo info) {
        Debug.Log("CanMoveTo: START");
        TileSide target = info.side;
        Piece movingPiece = currentMovingPiece.GetComponent<Piece>();
        bool isEnemyTile = info.tile.team != movingPiece.getTeam() ? true : false;
        debugReachable.TryGetValue(target, out int moveDistanceI);
        // General checks irrespective of team
        // If moveDistance is 0, that means the player is flipping across a TileSide
        if (moveDistanceI == 0) moveDistanceI = 1;
        // Check if piece has enough movement left to move
        if (movingPiece.getTotalMoveRange() - moveDistanceI < 0) {
            Debug.Log("CanMoveTo: Piece doesn't have enough moves to move this distance");
            return false;
        }
        // Check if the player has enough moves left to even move this piece
        if (currentPlayerScript.remainingPieceMoves <= 0) {
            Debug.Log("CanMoveTo: Player doesn't have enough moves left to move");
            return false;
        }
        // Check if the spot to move to is reachable via connected team tiles
        if (!debugReachable.ContainsKey(info.side)) {
            Debug.Log("CanMoveTo: The spot you're trying to move to is not reachable via team tiles");
            return false;
        }

        if (isEnemyTile) {
            Debug.Log("Attemping move on enemy tile...");
            // You can only move to an enemy tile if there is something to fight
            var enemyPieces = info.tile.GetAllPiecesOnTile();
            Debug.Log("Pieces on enemy tile: " + enemyPieces.Count);
            // Capture enemy tile
            if (enemyPieces.Count == 0) {
                Debug.Log("CanMoveTo: Cannot move to enemy tile: No pieces to attack! So capturing instead...");
                CaptureTile(info.tile, movingPiece, currentPlayerScript);
                return true; // Move the piece to its new home
            }
            // Attack piece, attacking can only be done through tile flips
            else {
                if (piece.currentSide == target && piece.isOnSideA != info.isTileA) {
                    // If there are pieces, it's a valid Attack Move, return false because even if the attacker
                    // piece defeats the defender, put them back in their place afterwards
                    Debug.Log("CanMoveTo: Attacking enemy piece...");
                    attackerPiece = currentMovingPiece.GetComponent<Piece>();

                    GameObject directOccupant = info.isTileA ? info.side.occupantA : info.side.occupantB;

                    if (directOccupant != null) {
                        defenderPiece = directOccupant.GetComponent<Piece>();
                        isSneakAttack = false;
                    }
                    else {
                        isSneakAttack = true;
                        List<Piece> otherPieces = info.tile.GetAllPiecesOnTile();
                        if (otherPieces.Count > 0) {
                            defenderPiece = otherPieces[UnityEngine.Random.Range(0, otherPieces.Count)];
                        }
                    }

                    tempAttackInfo = new PointerInfo { tile = info.tile, isTileA = info.isTileA, side = info.side };

                    SetPhase(TurnPhase.BattleStart);
                    Debug.Log("CanMoveTo: Cannot move to spot because this piece is attacking instead");
                }
                else {
                    Debug.Log("CanMoveTo: Trying to attack tile but not a tile flip");
                }
                // Can't move either way
                return false;
            }
        }
        else {
            // Check if spot is occupied
            if (info.isOccupied) {
                Debug.Log("CanMoveTo: Friendly spot is already occupied");
                return false;
            }
            // Friendly tile is unoccupied, the player has enough moves, this piece has enough moves, and this spot is reachable
            return true;
        }
    }

    void MovePiece(Piece piece, TileSide newSide, bool isTileA, Tile newTile) {
        // Clear old slot
        if (piece.isOnSideA) {
            piece.currentSide.occupantA = null;
        }
        else {
            piece.currentSide.occupantB = null;
        }
        // Change piece's parent to the new tile it moved to
        piece.transform.parent = newTile.transform;

        // Assign new slot
        if (isTileA)
            newSide.occupantA = piece.gameObject;
        else
            newSide.occupantB = piece.gameObject;

        debugReachable.TryGetValue(newSide, out int moveDistance);
        // If moveDistance is 0, that means the player is flipping across a TileSide
        if (moveDistance == 0) moveDistance = 1;
        currentPlayerScript.remainingPieceMoves -= moveDistance;
        piece.movesRemaining -= moveDistance;
        piece.currentTile = newTile;
        piece.SetPosition(newSide, isTileA);
        // Snap transform
        piece.transform.position = DetermineOffset(
            isTileA ? newSide.tileA : newSide.tileB, GetDirectionFromSide(newSide, isTileA));
    }

    private Direction GetDirectionFromSide(TileSide side, bool isTileA) {
        Tile tile = isTileA ? side.tileA : side.tileB;

        if (tile.topSide == side) return Direction.Top;
        if (tile.rightSide == side) return Direction.Right;
        if (tile.bottomSide == side) return Direction.Bottom;
        if (tile.leftSide == side) return Direction.Left;

        return Direction.Top;
    }

    // No more references to this as it was commented out in CanMoveTo()
    [Obsolete]
    public List<TileSide> GetReachable(TileSide start, int maxDistance) {
        var reachable = new List<TileSide>();
        var queue = new Queue<(TileSide side, int cost)>();
        var visited = new Dictionary<TileSide, int>();

        queue.Enqueue((start, 0));
        visited[start] = 0;

        while (queue.Count > 0) {
            var (current, costSoFar) = queue.Dequeue();

            foreach (var connection in current.connections) {
                int newCost = costSoFar + connection.cost;

                if (newCost > maxDistance)
                    continue;

                if (!visited.ContainsKey(connection.target) || visited[connection.target] > newCost) {
                    visited[connection.target] = newCost;
                    queue.Enqueue((connection.target, newCost));
                    reachable.Add(connection.target);
                }
            }
        }
        return reachable;
    }

    public Dictionary<TileSide, int> GetReachableWithCost(TileSide start, int maxDistance) {
        var queue = new Queue<(TileSide side, int cost)>();
        var visited = new Dictionary<TileSide, int>();

        queue.Enqueue((start, 0));
        visited[start] = 0;

        while (queue.Count > 0) {
            var (current, costSoFar) = queue.Dequeue();

            foreach (var connection in current.connections) {
                TileSide target = connection.target;
                
                int newCost = costSoFar + connection.cost;

                if (newCost > maxDistance)
                    continue;

                if ((!visited.ContainsKey(target) || visited[target] > newCost) && (touchingTiles.Contains(target.tileA) || touchingTiles.Contains(target.tileB))) {
                    visited[target] = newCost;
                    queue.Enqueue((target, newCost));
                }
            }
        }
        return visited;
    }

    // Determine how far to set the offset from the side
    private Vector3 DetermineOffset(Tile tile, Direction direction) {
        switch (direction) {
            case Direction.Top: return new Vector3(tile.transform.position.x, 0, tile.transform.position.z + .25f);
            case Direction.Bottom: return new Vector3(tile.transform.position.x, 0, tile.transform.position.z - .25f);
            case Direction.Left: return new Vector3(tile.transform.position.x - .25f, 0, tile.transform.position.z);
            case Direction.Right: return new Vector3(tile.transform.position.x + .25f, 0, tile.transform.position.z);
            default: return new Vector3(0, 0, 0);
        }
    }

    [Obsolete]
    private bool HasSideNeighbor(TileSide side, bool isThisTileA) {
        if (isThisTileA) {
            if (side.occupantB != null) {
                Debug.Log($"Neighbor on tile {side.tileB}, they are team {side.tileB.team}");
                return true;
            }
            else {
                return false;
            }
        }
        else {
            if (side.occupantA != null) {
                Debug.Log($"Neighbor on tile {side.tileA}, they are team {side.tileA.team}");
                return true;
            }
            else {
                return false;
            }
        }
    }

    // Placeholder; When moving a piece, draw available places to move the piece to within move distance
    void OnDrawGizmos() {
        if (debugReachable == null) {
            return;
        }

        foreach (var kvp in debugReachable) {
            TileSide side = kvp.Key;
            int cost = kvp.Value;

            Vector3 pos = GetSideWorldPosition(side);

            // Color based on cost
            Gizmos.color = Color.Lerp(Color.green, Color.red, (float)cost / 3.5f);

            Gizmos.DrawSphere(pos, 0.15f);
        }
    }

    #endregion

    #region Combat

    void CaptureTile(Tile tile, Piece piece, Player capturingPlayer) {
        Team teamToLose = tile.team;
        foreach (GameObject p1 in players) {
            Player p2 = p1.GetComponent<Player>();
            if (p2.team == teamToLose) {
                p2.ownedTiles--;
                break;
            }
        }
        capturingPlayer.ownedTiles++;
        tile.setTeam(piece.team);
    }

    #endregion

    #region Initialization & Utility

    void Capitulate() {
        CloseCardStock();
        ClosePieceStock();
        if (phase == TurnPhase.RoundOneWait) {
            SetPhase(TurnPhase.RoundOneTurnEnd);
        }
        else {
            SetPhase(TurnPhase.TurnEnd);
        }
    }

    public void SetPhase(TurnPhase newPhase) {
        Debug.Log("Setting phase to " + newPhase);
        phase = newPhase;
    }

    Vector3 GetSideWorldPosition(TileSide side) {
        Vector3 a = side.tileA.transform.position;

        if (side.tileB != null) {
            Vector3 b = side.tileB.transform.position;
            return (a + b) / 2f; // midpoint between tiles
        }

        // Edge case: side only has one tile
        Direction dir = GetDirectionFromSide(side, true);

        switch (dir) {
            case Direction.Top: return a + new Vector3(0, 0, 0.5f);
            case Direction.Bottom: return a + new Vector3(0, 0, -0.5f);
            case Direction.Left: return a + new Vector3(-0.5f, 0, 0);
            case Direction.Right: return a + new Vector3(0.5f, 0, 0);
        }

        return a;
    }

    private void InitializePlayers() {
        Team[] teamOrder = { Team.One, Team.Two, Team.Three, Team.Four };
       
        Debug.Log("Making " + playerCount + " players");
        for (int i = 0; i < playerCount; i++) {
            GameObject pGO = Instantiate(playerPrefab);
            Player p = pGO.GetComponent<Player>();

            p.setTeam(teamOrder[i]);
            p.setMoney(0);
            p.setPlayerType(PlayerType.Human);

            // Get 2 random normal pieces
            List<PieceData> randomPieces = GetXRandomPieces(2, Rank.Normal);
            foreach (PieceData piece in randomPieces) {
                p.AddPieceToStock(piece);
            }

            pGO.name = $"Player_{p.getTeam()}";
            players.Add(pGO);
        }

        currentPlayerGO = players[0];
        currentPlayerScript = currentPlayerGO.GetComponent<Player>();
    }

    /// <summary>
    /// On setup, assign each player object their initial number of owned tiles
    /// </summary>
    public void AssignTileAmtsToPlayers(int t1Tiles, int t2Tiles, int t3Tiles, int t4Tiles) {
        int[] tileOrder = { t1Tiles, t2Tiles, t3Tiles, t4Tiles };
        int avg = 0;
        for (int i = 0; i < playerCount; i++) {
            players[i].GetComponent<Player>().ownedTiles = tileOrder[i];
            avg += tileOrder[i];
        }
        initAvgTiles = avg / playerCount;
        basePrice = (int)Mathf.Floor((float)((initAvgTiles / playerCount) * .88));
    }

    private Vector3 GetWorldPosition(Tile tile, Direction dir) {
        Vector3 pos = tile.transform.position;
        switch (dir) {
            case Direction.Top: return new Vector3(pos.x, 0, pos.z + pieceOffset);
            case Direction.Bottom: return new Vector3(pos.x, 0, pos.z - pieceOffset);
            case Direction.Left: return new Vector3(pos.x - pieceOffset, 0, pos.z);
            case Direction.Right: return new Vector3(pos.x + pieceOffset, 0, pos.z);
            default: return pos;
        }
    }

    private TileSide GetSide(Tile tile, Direction dir) {
        return dir switch {
            Direction.Top => tile.topSide,
            Direction.Bottom => tile.bottomSide,
            Direction.Left => tile.leftSide,
            Direction.Right => tile.rightSide,
            _ => null
        };
    }

    private void InitializeGhost() {
        activeGhost = Instantiate(ghostPiecePrefab);
        activeGhost.SetActive(false);
    }

    public List<GameObject> getPlayers() {
        return players;
    }

    // For now, load in this new player's piece stock to the scrollview content
    void LoadNewPlayerInfo() {
        foreach (PieceData piece in currentPlayerScript.piecesInStock) {
            GameObject newItem = Instantiate(PieceDisplayPrefab);
            PieceDisplay pieceDisplay = newItem.GetComponent<PieceDisplay>();
            if (pieceDisplay != null) {
                pieceDisplay.Initialize(piece);
            }
            newItem.transform.SetParent(playerPieceStockContent, false);
        }
        foreach (CardData card in currentPlayerScript.cardsInStock) {
            GameObject newItem = Instantiate(CardDisplayPrefab);
            CardDisplay cardDisplay = newItem.GetComponent<CardDisplay>();
            if (cardDisplay != null) {
                cardDisplay.Initialize(card);
            }
            newItem.transform.SetParent(playerCardStockContent, false);
        }
        // For now, just default salary to basePrice
        currentPlayerScript.AddMoney(basePrice);
        GetRewardValue();
    }

    void NextPlayer() {
        int index = players.IndexOf(currentPlayerGO);
        if (index >= players.Count - 1) {
            // Goes back to "first" player and ticks down status effects and resets each piece's remaining moves
            TickDownEffects();
            foreach (Piece p in piecesOnBoard) {
                p.ResetMoves();
            }
            roundOneOver = true;
            currentPlayerGO = players[0];
        }
        else {
            currentPlayerGO = players[index + 1];
        }
        currentPlayerScript = currentPlayerGO.GetComponent<Player>();
        currentPlayerUI.text = $"Current Player: {currentPlayerGO.name}";
        CleanUp();
    }

    // For right now, clear this player's piece stock. Each player will just use the same one with their own data
    void CleanUp() {
        foreach (Transform child in playerPieceStockContent) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in playerCardStockContent) {
            Destroy(child.gameObject);
        }

        // More stuff coming soon on video & DVD....
    }

    public void TickDownEffects() {
        StatusHost[] allHosts = FindObjectsByType<StatusHost>(FindObjectsSortMode.None);
        foreach (var host in allHosts) {
            host.ProcessTurn();
        }
    }

    private PointerInfo GetPointerInfo() {
        PointerInfo info = new PointerInfo { direction = Direction.None };
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit)) {
            info.worldPoint = hit.point;
            info.tile = hit.collider.GetComponent<Tile>();

            if (info.tile != null) {
                // Calculate Direction
                Vector3 localPos = hit.point - info.tile.transform.position;
                if (Mathf.Abs(localPos.x) > Mathf.Abs(localPos.z))
                    info.direction = localPos.x > 0 ? Direction.Right : Direction.Left;
                else
                    info.direction = localPos.z > 0 ? Direction.Top : Direction.Bottom;

                // Get Side Data
                info.side = GetSide(info.tile, info.direction);
                if (info.side != null) {
                    info.isTileA = (info.side.tileA == info.tile);
                    info.isOccupied = info.isTileA ? (info.side.occupantA != null) : (info.side.occupantB != null);
                }
            }
        }
        return info;
    }

    private void GetTouchingTiles(Tile startTile) {
        string tileNames = "";
        HashSet<Tile> tiles = new HashSet<Tile>();
        Tile currentTile = startTile;

        FloodFillTeamTiles(tiles, currentTile, currentMovingPiece.GetComponent<Piece>().team);

        foreach (Tile tile in tiles) {
            tileNames += tile.name + " ";
        }
        Debug.Log(tileNames);
        touchingTiles = tiles;
    }

    /// <summary>
    /// Recursively find same team tiles that are touching one another
    /// </summary>
    private bool FloodFillTeamTiles(HashSet<Tile> tiles, Tile currentTile, Team team) {
        if (currentTile == null || tiles.Contains(currentTile) || currentTile.team != team) {
            return false;
        }
        tiles.Add(currentTile);

        if (currentTile.topNeighbor != null) FloodFillTeamTiles(tiles, currentTile.topNeighbor, team);
        if (currentTile.rightNeighbor != null) FloodFillTeamTiles(tiles, currentTile.rightNeighbor, team);
        if (currentTile.bottomNeighbor != null) FloodFillTeamTiles(tiles, currentTile.bottomNeighbor, team);
        if (currentTile.leftNeighbor != null) FloodFillTeamTiles(tiles, currentTile.leftNeighbor, team);
        return true;
    }

    private void UpdateUI() {
        currentPlayerUI.text = $"Player: {currentPlayerGO.name}\nPhase: {phase}\nMoves remaining: {currentPlayerScript.remainingPieceMoves}";
    }

    private double GetRewardValue() {
        double value = 1.0;

        // Tiles
        int tiles = currentPlayerScript.ownedTiles;
        value += (1 - (tiles / initAvgTiles)) * 1.25;

        // Money
        int money = currentPlayerScript.money;
        value += (1 - (money / basePrice)) / 3;
        Debug.Log("Reward: " + value);
        return value;
    }

    private void TogglePieceColliders(bool toggle) {
        foreach (Piece p in piecesOnBoard) { p.GetComponent<CapsuleCollider>().enabled = toggle; }
    }

    #endregion

    #region Shop

    /// <summary>
    /// Instantiate buttons, as well as set prices for items based on basePrice
    /// </summary>
    void SetupShop() {
        // Pieces
        foreach (PieceData piece in availablePieces) {
            GameObject button = Instantiate(shopItem);

            piece.cost = Mathf.CeilToInt(piece.multiplier * basePrice);

            button.transform.SetParent(shopPanel.transform);
            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>();
            if (tmpText != null) {
                tmpText.text = piece.name + " $" + piece.cost;
            }
            Button buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null) {
                buttonComponent.onClick.AddListener(() => BuyItemFromShop(piece));
            }
        }
        // Cards
        foreach (CardData card in availableCards) {
            GameObject button = Instantiate(shopItem);

            card.cost = Mathf.CeilToInt(card.multiplier * basePrice);

            button.transform.SetParent(shopPanel.transform);
            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>();
            if (tmpText != null) {
                tmpText.text = card.name + " $" + card.cost;
            }
            Button buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null) {
                buttonComponent.onClick.AddListener(() => BuyItemFromShop(card));
            }
        }
    }

    List<PieceData> GetXRandomPieces(int numPieces, Rank rank) {
        System.Random rand = new System.Random();
        return availablePieces
        .Where(p => p.rank == rank) 
        .OrderBy(_ => rand.Next())
        .Take(numPieces) 
        .ToList();
    }

    private void BuyItemFromShop(ScriptableObject data) {
        if (data.GetType() == typeof(PieceData)) {
            Debug.Log("Piece");
            selectedPieceToBuy = (PieceData)data;
            int cost = selectedPieceToBuy.cost;

            if (currentPlayerScript.money - cost < 0) {
                Debug.Log("Not enough moolah!");
                return;
            }
            currentPlayerScript.money -= cost;

            Debug.Log($"Selected {selectedPieceToBuy.pieceName} for ${selectedPieceToBuy.cost}");
            currentPlayerScript.AddPieceToStock(selectedPieceToBuy);
            GameObject newItem = Instantiate(PieceDisplayPrefab);
            PieceDisplay pc = newItem.GetComponent<PieceDisplay>();
            if (pc != null) {
                pc.Initialize(selectedPieceToBuy);
            }
            newItem.transform.SetParent(playerPieceStockContent, false);

        }
        else {
            Debug.Log("Card");
            selectedCardToBuy = (CardData)data;
            int cost = selectedCardToBuy.cost;
            if (currentPlayerScript.money - cost < 0) {
                Debug.Log("Not enough moolah!");
                return;
            }
            currentPlayerScript.money -= cost;
            Debug.Log($"Selected {selectedCardToBuy.name} for ${selectedCardToBuy.cost}");
            currentPlayerScript.AddCardToStock(selectedCardToBuy);
            GameObject newItem = Instantiate(CardDisplayPrefab);
            CardDisplay pc = newItem.GetComponent<CardDisplay>();
            if (pc != null) {
                pc.Initialize(selectedCardToBuy);
            }
            newItem.transform.SetParent(playerCardStockContent, false);
        }
    }

    void OpenShop() {
        CloseCardStock();
        ClosePieceStock();
        shopPanel.SetActive(true);
        mainUIPanel.SetActive(false);
        SetPhase(TurnPhase.Buy);
    }

    // From shop, go to main screen
    void BackToWait() {
        shopPanel.SetActive(false);
        mainUIPanel.SetActive(true);
        SetPhase(TurnPhase.Wait);
    }

    void RemoveCardFromStock() {
        currentPlayerScript.RemoveCardFromStock(selectedCardToBuy);

        foreach (Transform child in playerCardStockContent) {
            CardDisplay p2 = child.GetComponent<CardDisplay>();
            if (p2 != null) {
                if (p2.id == selectedCardIndex) {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public void ToggleCardStock() {
        cardStockObject.SetActive(!cardStockObject.activeSelf);
        if (cardStockObject.activeSelf) {
            TogglePieceColliders(false);
        }
    }

    public void TogglePieceStock() {
        pieceStockObject.SetActive(!pieceStockObject.activeSelf);
        if (pieceStockObject.activeSelf) {
            TogglePieceColliders(false);
        }
    }

    void CloseCardStock() {
        cardStockObject.SetActive(false);
        TogglePieceColliders(true);
    }

    void ClosePieceStock() {
        pieceStockObject.SetActive(false);
        TogglePieceColliders(true);
    }

    void DisableShop() {
        shopButton.enabled = false;
    }

    void EnableShop() {
        shopButton.enabled = true;
    }

    #endregion
}