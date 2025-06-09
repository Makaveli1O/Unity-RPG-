# Unity RPG (Archived)

The main task of this bachelor thesis is to create a 2D RPG with procedural elements that has a bird's eye view. The game is implemented in the Unity game engine. The content of this thesis is composed of theoretical information about video games, procedural content generation and information about game engines and Unity. The thesis also includes the design of the solution and the implementation part of the game. Perlin noise was used for the procedural generation of the world and its post-processing using Whittaker diagram. The thesis describes several systems whose role is to enhance the enjoyment of the game itself. Finally, the thesis includes testing and evaluation using a short questionnaire.

> ⚠️ This project is archived and no longer maintained.  
> It requires **Unity 2021.2.13f1** to run correctly.  
> Upgrading to a newer Unity version is not supported due to complexity and breaking changes.

## Overview

A basic RPG project built in Unity. Includes systems for:

- Character movement
- Combat mechanics
- Inventory management
- Simple UI elements

This project was created as part of a **Bachelor's Thesis**. You can read the full thesis paper [here](https://www.fit.vut.cz/study/thesis/24680/.cs).

## Solution Design

The game is an uncompromising **RPG** full of **action**, **combat**, and **challenge**. The map is **procedurally generated**, making each playthrough unique in layout and design. The main goal is to find and collect **four missing keys**, hidden inside crystals scattered across the map. This objective is called **The Main Quest**. Players encounter various enemies, each differing in **speed**, **attack patterns**, and **strength**, adding to the game’s difficulty and variety.


![image](https://github.com/user-attachments/assets/38b40354-74cf-454e-8207-7ad42225a2ba)


### Game rules

The player is thrown into a generated world. Their main objective is to collect all the missing keys to finish the game. Each key is located in a different biome across the map. Every biome contains exactly one key; however, there may be multiple biomes of the same type. Collecting all the keys is required to complete the game. The difficulty varies depending on the biome and the enemies within it. This design aims to make each playthrough more varied as the player explores the world.

⚠️ For a more detailed description of the game, please refer to the thesis text, especially the **Solution Design** section.

### Controls

The gameplay consists of the character’s **movement**, **attacks**, and **ability usage** during combat.

- **Movement** is controlled using the **WASD** keys.
- **Combat** includes a **regular attack**, which is performed by **left-clicking** near the target’s position.
- The player is given **five abilities**, each assigned to a specific key:

  ![healing_spell](https://github.com/user-attachments/assets/579d554d-2fd9-4441-bc44-cd3e682f2382) **Q** – _[Heal]_ heals character after casting of the spell is done.
  
  ![fortify_spell](https://github.com/user-attachments/assets/daf752d2-4114-409a-b529-28fd07c21edf) **RIGHT MOUSE HOLD** – _[Shield]_ absorption of damage.
  
  ![water_spell](https://github.com/user-attachments/assets/d9f08f11-35bc-4d57-b4a1-55e0dd11419b) **F** – _[Tornado invocation]_ instantly casts 8 tornadoes hurting everything in its direction.
  
  ![poison_dagger 1](https://github.com/user-attachments/assets/b40d775d-c445-430a-872d-5d383d97ff23) **E** – _[Sword clash]_ AOE1 spell that spawns swords on top of enemies within range and damages them.
 
  ![Dash](https://github.com/user-attachments/assets/6526445d-75a9-494d-a29f-eae86dbab7a5) **Space** – _[Dash]_ quick movement burst in running direction.

## Requirements

- **Unity 2021.2.13f1**  
  Download from Unity Hub's archive section:  
  https://unity.com/releases/editor/archive

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/Makaveli1O/Unity-RPG-.git
   ```
2. Open the project using Unity Hub with version 2021.2.13f1.

3. Let Unity compile and load the assets.

4. Press Play to test in the editor.

## Status

❌ Archived

✅ Usable with Unity 2021.2.13f1

⚠️ Opening the project in latest unity is not advised as it generates hundreds of errors and is just too much work to move the project after 3 years of stagnation.

## License

MIT 
