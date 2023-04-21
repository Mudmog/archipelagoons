using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public MenuManager mm;
    public static event Action<GamePhase> OnGamePhaseChange;
    private GameState _currentState;
    private GamePhase _currentPhase;
    private PlayerTurn _currentTurn;
    public HexMapEditor map;

    public HexGrid grid;
    public Player[] players;
    public Player _currentPlayer;
    [SerializeField]
    Unit selectedUnit;
    [SerializeField]
    public CardList gamesCardList;


    public enum PlayerTurn {
        PLAYER1,
        PLAYER2
    }

    public enum GameState {
        NONE,
        LOADING, 
        RUNNING, 
        END, 
        RESTART 
    }

    public enum GamePhase {

        STARTUP,
        UPKEEP, 
        BUILD, 
        RECRUIT,
        AUCTION,
        ARMY,
        ROUNDEND
    }

    void Awake() {
        Instance = this;
        _currentPlayer = players[0];
    }
    void Start()
    {
        UpdateState(GameState.LOADING);
        map.Load();
        map.enabled = false;
        UpdatePhase(GamePhase.STARTUP);
        placeBeginnerUnit(grid);
        foreach (Player player in players) {
            player.assignUnits(gamesCardList);
            player.changePearls(10);
            player.changeMaxHammers(5);
            player.changeMaxOrders(7);
            player.changeHammers(5);
            player.changeOrders(7);
        }
    }
    void Update() {
        UpdatePhase(_currentPhase);
    }
    void UpdateState(GameState state) {

        _currentState = state;
        switch(_currentState) {

            case GameState.NONE: 
                //Will be used for something haven't actually figured that out yet
                break;
            
            case GameState.LOADING:
                //For when players need to load and stuff
                break;
            
            case GameState.RUNNING: 
                //game is running
                break;
            
            case GameState.END:
                break;

            case GameState.RESTART:
                break;
            
            default:
                break;

        }
    }

    void UpdatePhase(GamePhase phase) {
        _currentPhase = phase;

        switch(_currentPhase) {

            case GamePhase.STARTUP:
                HandleStartUp();
                //Should only be used at the beginning.
                //Make sure that everyone is connected.
                //Goes to upkeep when done.
                break;

            case GamePhase.UPKEEP:
                HandleUpkeep();
                //each client simotaniously moves
                //make sure they are done before moving on
                break;
            
            case GamePhase.BUILD:
                HandleBuild();
                //each client simotaniously moves
                //make sure they are done before moving on
                break;
            
            case GamePhase.RECRUIT:
                HandleRecruit();
                //basically auction phase
                //lots of waiting on players 
                //cool
                break;

            case GamePhase.AUCTION:
                HandleAuction();
                break;

            case GamePhase.ARMY:
                HandleArmy();
                //turn based round
                //each player goes
                //goto roundend
                break;

            case GamePhase.ROUNDEND:
                HandleRoundEnd();
                //make sure everything is correct
                //and updated
                //decide if winner
                //if not goto next round
                //if so endgame
                break;
            
            default:
                break;
        }
        OnGamePhaseChange?.Invoke(phase);
    }
    public void ChangeTurn(PlayerTurn newTurn) {
        _currentTurn = newTurn;

        switch(newTurn) {
            case PlayerTurn.PLAYER1:
                _currentPlayer = players[0];
                mm.HandleTurnChange(players[0]);
                HandlePhaseChange();
                break;


            case PlayerTurn.PLAYER2:
                _currentPlayer = players[1];
                mm.HandleTurnChange(players[1]);
                break;
            
            default:
                break;
        }

    }

    private void HandleStartUp() {
    }
    private void HandleUpkeep() {
    }
    private void HandleBuild() {
    }
    private void HandleRecruit() {
        mm.HandleRecruitMenuChange();

    }

    private void HandleAuction()
    {
        mm.HandleAuctionMenuChange();
    }
    private void HandleArmy() {
        mm.HandleArmyMenuChange();
        if (_currentPlayer.getOrders() > 0) {
            if (Input.GetMouseButtonUp(0))
            {
                if (selectedUnit != null) {
                    HandleUnitMovement(grid, selectedUnit);
                }
                else {
                    HandleUnitSelection();
                }     
            }
        }
    }
    private void HandleRoundEnd() {
    }
    public void HandlePhaseChange() {
        switch(_currentPhase) {

            case GamePhase.STARTUP:
                UpdatePhase(GamePhase.UPKEEP);
                break;

            case GamePhase.UPKEEP:
                 UpdatePhase(GamePhase.BUILD);
                break;
            
            case GamePhase.BUILD:
                 UpdatePhase(GamePhase.RECRUIT);
                break;
            
            case GamePhase.RECRUIT:
                 UpdatePhase(GamePhase.AUCTION);
                break;

            case GamePhase.AUCTION:
                 UpdatePhase(GamePhase.ARMY);
                break;

            case GamePhase.ARMY:
                 UpdatePhase(GamePhase.ROUNDEND);
                break;

            case GamePhase.ROUNDEND:
                foreach (Player player in players) {
                    player.resetOrders();
                    player.resetHammers();
                 }
                UpdatePhase(GamePhase.UPKEEP);
                break;
            
            default:
                break;
        }
        Debug.Log("Phase Updated From: " + _currentPhase.ToString());
    }
    void StartLoading() {
        
    }

    public void placeBeginnerUnit(HexGrid hexgrid) {
        players[0].getUnitList().placeFirstUnit(hexgrid.GetCell(new Vector3(69, 9, 87)), players[0]);
        players[1].getUnitList().placeFirstUnit(hexgrid.GetCell(new Vector3(277, 8, 89)), players[1]);
    }

    public void HandleUnitMovement(HexGrid hexGrid, Unit unit)
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit) && hexGrid.GetCell(hit.point).IsUnderwater 
        && hit.transform.gameObject.tag is not "Unit")
		{
			unit.updatePosition(hexGrid.GetCell(hit.point), mm);
            selectedUnit = null;
		}
        else if (unit.checkIsNeighbors(hit.transform.gameObject.GetComponentInParent<Unit>().getCurrentCell())) {
            Debug.Log("Would you like to attack?");
        }
	}

    public void HandleUnitSelection() {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit) && hit.transform.gameObject.tag.Equals("Unit"))
		{
            if (hit.transform.gameObject.GetComponentInParent<Player>() != _currentPlayer) {
                Debug.Log("Not Your Unit");
            }
            else {
                selectedUnit = hit.transform.gameObject.GetComponentInParent<Unit>();
                Debug.Log("Selected Unit: " + selectedUnit.ToString());
            }
		}
    }

    public PlayerTurn getNextTurn(Player currentPlayer) {
        if (currentPlayer == players[0]) {
            return PlayerTurn.PLAYER2; 
        }
        else if (currentPlayer == players[1]) {
            return PlayerTurn.PLAYER1;
        }
        Debug.Log("Error getting next player");
        return PlayerTurn.PLAYER1;
    }

    public GamePhase gamePhase {
        get {
            return _currentPhase;
        }
    }
}
