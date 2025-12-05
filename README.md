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

| **Task**                                                                | **Time it Took (in hours)** |
|--------------------------------------------------------------------------------|------------------------------------|
|     Setting up Unity, making a project in GitHub                             |     0.2                            |
|     Research and conceptualization of game idea                              |     1.5                            |
|     Creating the 3D models - cheese                                          |     0.5                            |
|     Building 3D models from scratch -cat, rat, field                         |     1                              |
|     Making camera movement controls and initial testing                      |     1                              |
|     Player movement                                                          |     0.5                            |
|     Combining player movement with camera orientation, bugfixing             |     1.5                            |
|     Building the random spawning of cheeses and fixing spawning bugs         |     1                              |
|     Building enemy random spawners, randomizing starting positions           |     2                              |
|     Making timers and connecting enemy spawning and game difficulty          |     1.5                            |
|     Making UI elements and research into TextMesh Pro                        |     1.5                            |
|     Collisions and bugfixing error with multiple collision all at once       |     0.5                            |
|     Playtesting and bugfixing fringe cases in rigidbody incorrect physics    |     1.5                            |
|     Code documentation                                                       |     1                              |
|     Making readme                                                            |     0.5                            |
|     **All**                                                                        |     **15.5**                           |

