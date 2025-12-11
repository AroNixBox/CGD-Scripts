# CollectData - Usage Guide

This class collects game data during runtime and saves it to a JSON file.

## Setup

1. Add the `CollectData` component to a GameObject in your scene (e.g., GameManager)
2. Configure the save settings in the Inspector:
   - **Save File Name**: Base name for the JSON file (default: "GameData")
   - **Append Timestamp**: Whether to add a timestamp to the filename (default: true)

## Usage Flow

### 1. Match Start
```csharp
CollectData dataCollector = GetComponent<CollectData>();

// Initialize match data collection
dataCollector.StartMatchDataCollection();

// Initialize all players
dataCollector.InitializeUser("Player1");
dataCollector.InitializeUser("Player2");

// Initialize all weapons
dataCollector.InitializeWeapon("Rifle");
dataCollector.InitializeWeapon("Pistol");
```

### 2. During Gameplay

#### Turn Management
```csharp
// At the start of each turn
dataCollector.IncrementTurn();
```

#### User/Player Data
```csharp
// Record player position
dataCollector.RecordUserPosition("Player1", transform.position);

// Record damage dealt
dataCollector.RecordDamage("Player1", 50.0);

// Record weapon used
dataCollector.RecordWeaponUsed("Player1", "Rifle");

// Record movement percentage (0.0 to 1.0)
dataCollector.RecordMovementPercentage("Player1", 0.75);

// Record turn duration
TimeSpan turnTime = DateTime.Now - turnStartTime;
dataCollector.RecordTurnDuration("Player1", turnTime);

// Record kill
dataCollector.RecordKill("Player1");

// Record death
dataCollector.RecordDeath("Player2", killerPosition);

// Record fairness rating (at match end)
dataCollector.RecordFairnessRating("Player1", true);
```

#### Weapon Data
```csharp
// Record weapon usage
string weaponName = "Rifle";
double damage = 45.0;
double distance = Vector3.Distance(shooter.position, target.position);
bool hit = true; // or false if missed

dataCollector.RecordWeaponUsage(weaponName, damage, distance, hit);
```

### 3. Match End
```csharp
// Set winner information
dataCollector.SetWinner("Player1", "Last player standing");

// Set kill distance (distance of final kill)
double finalKillDistance = Vector3.Distance(winner.position, loser.position);
dataCollector.SetKillDistance(finalKillDistance);

// Finalize match data
dataCollector.EndMatchDataCollection();

// Save all data to JSON
dataCollector.SaveDataToJson();

// Optional: Clear data for next match
dataCollector.ClearData();
```

## Output

The data is saved to `Application.persistentDataPath` with the structure:
- Windows: `C:\Users\{username}\AppData\LocalLow\{CompanyName}\{ProductName}\`
- The file will be named: `GameData_YYYY-MM-DD_HH-mm-ss.json` (if timestamp is enabled)

## JSON Structure

```json
{
  "MatchData": {
    "MatchDuration": "00:15:30",
    "TotalTurns": 25,
    "Winner": "Player1",
    "WinReason": "Last player standing",
    "KillDistance": 15.5
  },
  "UserDatas": [
    {
      "UserName": "Player1",
      "Positions": [...],
      "DamagePerTurn": [50.0, 35.0, ...],
      "UsedWeaponPerTurn": ["Rifle", "Pistol", ...],
      "MovementPercentagePerTurn": [0.75, 0.80, ...],
      "TurnDurations": ["00:30", "00:45", ...],
      "TotalKills": 1,
      "Died": false,
      "KillerPositionOnDeath": {"x": 0, "y": 0, "z": 0},
      "UserRatedMatchAsFair": true
    }
  ],
  "WeaponDatas": [
    {
      "WeaponName": "Rifle",
      "UsageCount": 15,
      "HitCount": 12,
      "DamagePerUsage": [45.0, 50.0, ...],
      "DistanceToTargetPerUsage": [10.5, 15.3, ...]
    }
  ]
}
```

## Example Integration

```csharp
public class GameManager : MonoBehaviour {
    private CollectData dataCollector;
    
    private void Start() {
        dataCollector = GetComponent<CollectData>();
        StartMatch();
    }
    
    private void StartMatch() {
        dataCollector.StartMatchDataCollection();
        
        // Initialize players
        foreach (var player in players) {
            dataCollector.InitializeUser(player.name);
        }
        
        // Initialize weapons
        foreach (var weapon in availableWeapons) {
            dataCollector.InitializeWeapon(weapon.name);
        }
    }
    
    private void OnMatchEnd(Player winner) {
        dataCollector.SetWinner(winner.name, "Victory condition met");
        dataCollector.EndMatchDataCollection();
        dataCollector.SaveDataToJson();
        
        Debug.Log($"Match data saved to: {Application.persistentDataPath}");
    }
}
```

## Notes

- All data is stored in memory during gameplay and saved at the end
- Make sure to call `SaveDataToJson()` before the application quits or when transitioning scenes
- Use `ClearData()` if you want to start collecting data for a new match without restarting
- The `OnApplicationQuit()` method has an optional auto-save feature (currently commented out)

