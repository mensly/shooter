# Vertical Shooter Game

A vertical scrolling shooter game built with MonoGame and C#.

## Features

- Player ship with fast movement (WASD/Arrow keys or gamepad)
- Gamepad support with Xbox controller (thumbstick movement + A button shooting)
- Six enemy types with unique behaviors:
  - **Basic Enemy** (Red) - Standard enemy, 10 points
  - **Fast Enemy** (Yellow) - Fast moving enemy, 20 points
  - **Tank Enemy** (Blue) - Slow but sturdy enemy, 50 points
  - **Zigzag Enemy** (Green) - Moves in zigzag pattern, 25 points
  - **Kamikaze Enemy** (Orange) - Charges toward player, 15 points
  - **Shooter Enemy** (Purple) - Fires rapidly, 30 points
- Shooting mechanics (Space key or gamepad A button)
- Dark starfield background with scrolling stars
- Fixed-resolution rendering (1920x1080) with F11 fullscreen toggle
- Collision detection
- Score system
- Lives system (3 lives)
- Game over screen with restart functionality

## Requirements

- .NET 8.0 SDK or later
- MonoGame 3.8.1
- MonoGame Content Pipeline Builder (MGCB) - `dotnet tool install -g dotnet-mgcb`

## Building and Running

### Option 1: Using the run script (Recommended)
```bash
./run_game.sh
```

### Option 2: Manual build and run
1. Restore NuGet packages:
```bash
dotnet restore
```

2. Build the game:
```bash
dotnet build
```

3. Run the game:
```bash
dotnet run
```

## Controls

### Keyboard
- **Arrow Keys** or **WASD** - Move player ship
- **Space** - Shoot bullets
- **Escape** - Exit game
- **R** - Restart game (when game over)
- **F11** - Toggle fullscreen

### Gamepad
- **Left Thumbstick** - Move player ship
- **A Button** - Shoot bullets
- **Back Button** - Exit game

## Project Structure

```
shooter/
├── src/                  # Source code directory
│   ├── Entities/        # Game entities (Player, Enemy, Bullet, Sprite)
│   ├── Managers/        # Game managers (EnemyManager, BulletManager, BackgroundManager)
│   └── Game1.cs         # Main game class
├── Content/             # Game assets
│   ├── *.png           # Image assets (player, enemies, bullets)
│   ├── font.spritefont # Font for UI
│   └── Content.mgcb    # Content pipeline definition
├── Program.cs          # Entry point
├── Shooter.csproj      # Project file
├── run_game.sh         # Script to build and run the game
├── .gitignore          # Git ignore file
└── README.md           # This file
```

## Gameplay

Enemies spawn from the top of the screen at regular intervals with increasing frequency. Shoot them to earn points:
- Basic enemies: 10 points
- Fast enemies: 20 points
- Zigzag enemies: 25 points (move side-to-side)
- Shooter enemies: 30 points (fire rapidly)
- Kamikaze enemies: 15 points (charge toward player)
- Tank enemies: 50 points (slow but sturdy)

You have 3 lives. You lose a life when an enemy or enemy bullet collides with your ship. When all lives are lost, the game ends. Press R to restart.

**Note:** The game uses the Liberation Sans font which should be available on most Linux systems. If you encounter font issues, you may need to install it:
```bash
sudo apt-get install fonts-liberation  # Ubuntu/Debian
```

## Assets

All game assets are procedurally generated PNG files:
- `player.png` - Player ship (cyan colored)
- `enemy_basic.png` - Basic enemy (red colored)
- `enemy_fast.png` - Fast enemy (yellow colored)
- `enemy_tank.png` - Tank enemy (blue colored)
- `bullet_player.png` - Player bullets (yellow tipped)
- `bullet_enemy.png` - Enemy bullets (red tipped)

**Note:** The `run_game.sh` script automatically builds the MonoGame content (compiles `.png` and `.spritefont` files into `.xnb` format) and copies them to the output directory. Content is built to the `build/` directory to keep the source `Content/` directory clean.

