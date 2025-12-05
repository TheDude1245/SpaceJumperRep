# SpaceJumperRep

**Overview of the Game**
The idea of the project is a "remaster" (but worse) of the popular game-franchise called Skylanders. The player is set in a world that feels open, filled with life, including various monsters which the player has the defeat. The player guides a fire-element-character in a 3D-world, using keyboard controls (WASD+space + h,j,k,l for attacks). The goal of the game is to explore, collect up to 20.000 coins, and defeat the inal boss. 

**The main parts of the game are:**
Player – _rat, moved with the keyboard WASD or arrow keys_
Camera – _pivoting around the center of the playfield and rotated around with the mouse. Zooming is done with the mouse scroll_
Food – _cheese objects are spawned on the field – one in the beginning and then another one each time a player gathers the previous one. Each cheese gives 1 point to the player._
Enemies – _cats, they are spawn in random places at the edge of the play field and moved towards the position of the player at the time of their spawning. Take 1 live from the player on collision and are destroyed if they touch the edges of the play field_
Playfield – _close off space where the player can freely move. They player cannot go out of the field._
Lives – _the player starts with 3 lives, once all lives are removed the game ends_
Game features:

Positions of food and enemies are randomly selected each time helping with replayability.
The difficulty of the game changes with time, making it harder
The game keeps track of a score
Running It
Download Unity >= 2021.2.14f
Clone or Download the project
The game requires a computer with a mouse and keyboard
Project Parts
Scripts
CameraMoving – used for rotation and zooming of the camera
ChangeScore – used for updating the UI
EatFood – used to keep track of collisions with the food and updating score
EnemyBehaviour – used for enemy movement and tracking enemy collisions with the player and the world
MoveCharacter – used for moving the character using rigidbody physics and rotate the movement based on camera position
ObjectSpawner – used for spawning enemies, keeping tracking of a timer and changing the difficulty of the game
ScoreKeeper – keeps reference to the player lives, the score and if the game ending is triggered.
Models & Prefabs
A model of the cheese downloaded from sketchfab
Rat and cat models made with Unity primitives
