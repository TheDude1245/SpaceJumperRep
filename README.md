# SpaceJumperRep

## **Overview of the Game**
The idea of the project is a "remaster" (but worse) of the popular game-franchise called Skylanders. The player is set in a world that feels open, filled with life, including various monsters which the player has the defeat. The player guides a fire-element-character in a 3D-world, using keyboard controls (WASD+space + h,j,k,l for attacks). The goal of the game is to explore, collect up to 20.000 coins, and defeat the inal boss. 

**The main parts of the game are:**
- Player – _Custom Fire-Furnace Character, moved with the keyboard WASD_
- Enemies – _Various Custom Enemies, they spawn in set places, spread around the field and moves towards the position of the player utilising NavMesh. Kill them, or die yourself_
- Playable Areas – _Mulitple Custom Islands and Custom Airship. They player CAN go out of the boundaries_

**Game features:**
- The difficulty of the game changes with the enemy and amount, increasing when getting further into the game, therefore making it harder
- The game keeps track of a coin-score

**Running It**
- Download Unity >= 60002.7f2
- Clone or Download the project
- The game requires a computer with a keyboard

## Project Parts
**Main/Important Scripts** 
- PlayerAttack – _Script to handle attacks, and allows the use of Blender file-prefabs directly as attacks_
- PlayerMovement – _Handles movement, and purposly slows the rotation to mimic skylanders_
- Damage – _Handles damage universally_
- Health – _Calculates health for each objetc it's arracged to, based on the set value in the inspector_
- EnemyChase – _Script to handle how enemies moves (speed, etc.)_

**Models & Prefabs**
- All models were created in the 3D-modelling program called Blender

| **Task**                                                                     | **Time it Took (in hours)** |
|------------------------------------------------------------------------------|------------------------------------|
|     Setting up Unity, making a project in GitHub                             |     0.25                           |
|     Research and conceptualization of game idea                              |     1.5                            |
|     Creating the 3D models - player, enemies, islands, attacks, ect.         |     2.5                            |
|     Making player movement controls and enemie script                        |     1                              |
|     Setting up the islands and creating the NavMesh                          |     1.25                           |
|     Building/Placing the player, enemies, and coin-bags                      |     2                              |
|     Setting up the difficulties of each enemies (hp, dmg, speed, etc.)       |     1                              |
|     Making and setting up UI elements                                        |     1                              |
|     Setting up camerzones that moves the camera (rotates like in skylanders) |     0.5                            |
|     Collisions and bugfixing errors like camerazones                         |     0.5                            |
|     Playtesting and bugfixing fringe cases in rigidbody incorrect physics    |     0.25                           |
|     Making readme                                                            |     0.5                            |
|     **Total**                                                                |     **12.25**                      |

