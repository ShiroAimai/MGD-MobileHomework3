# MGD-MobileHomework3


This project is developed using Unity 2019.4.16f1.

## Setup

To play this game go to **Scenes/MainScene** and click play in Unity Editor.

## Introduction
The project is about a match-3 game, very similar to its famous older brothers.
To play the game select your preferred power up (choosing between Bomb and Freeze) and click 
the **New Game** button.
To move around the displayed tiles the player has to click a tile and re-click another adjacent
tile, if the clicked tile is a match then the two tiles will swap position and the match is resolved.
Otherwise, an animation suggests the impossibility to move.
After a swap, all the tiles above the match will shift down to close the gap and new tiles will spawn
in the blank spaces left by the shifted tiles. At this moment, if any new match is created it is resolved
automatically by the board controller. After this period, if the player can't move any of the tiles 
displayed then it's GameOver, otherwise the player can select another tile to match.
During the shift, refill and board check period player's interaction with the board is disabled to prevent
parallels match that would lead to conflicts in the board logic.
Every time a new match is resolved the score is updated, if two or more matches are resolved in a short period of time
then a combo multiplier is applied to the current match score. The combo multiplier is incremented by one every new match
resolved in the specified period. After some time without matches, the combo
multiplier is reverted back to normal.

## Configuration

All the following elements are customizable in the Editor:

- **Bomb's range** : can be customized in bomb's prefab BombPowerUp component;
- **Freeze's duration** : can be customized in freeze's prefab FreezePowerUp component;
- **Points per tile in match** : can be customized in the GameManager;
- **Game time** : can be customized in the GameManager;
- **Combo time to reset** : can be customized in the GameManager;
- **Points per combo / combo multiplier** : can be customized in the GameManager;
- **Available tile's type / power up** : can be customized in the BoardController;
- **Board's size** : can be customized in the BoardController;

