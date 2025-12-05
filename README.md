# SpaceJumperRep

**Overview of the Game**
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

Project Parts
**Scripts** 
- CameraMoving – used for rotation and zooming of the camera
- ChangeScore – used for updating the UI
- EatFood – used to keep track of collisions with the food and updating score
- EnemyBehaviour – used for enemy movement and tracking enemy collisions with the player and the world
- MoveCharacter – used for moving the character using rigidbody physics and rotate the movement based on camera position
- ObjectSpawner – used for spawning enemies, keeping tracking of a timer and changing the difficulty of the game
- ScoreKeeper – keeps reference to the player lives, the score and if the game ending is triggered.

**Models & Prefabs**
- A model of the cheese downloaded from sketchfab
- Rat and cat models made with Unity primitives
