# 📦 Parcel Panic!
![image7](https://github.com/user-attachments/assets/3b5bd403-bc49-4f90-b491-94460186a90d)

**Parcel Panic!** is a chaotic, comedic Unity game where you take on the role of a rookie courier trying to deliver parcels under extreme (and hilarious) pressure. Rush through city alleys, dodge cars, and yeet parcels at targets before time runs out!

Inspired by party games like *Overcooked* and *Fall Guys*, this project focuses on creating an expressive, fast-paced, and funny player experience.

---


## 🛠 Project Summary

- 🎮 Developed in **Unity**
- 🧊 Game Style: 3D cartoon-style environment with exaggerated animations
- ⏱ Core Loop: Pick up ➤ Navigate ➤ Throw ➤ Beat the timer!
- 🧰 Built using Unity 
- 📦 Final build includes 3 playable levels, main menu, and level selection

---

## 🔗 Play It
https://play.unity.com/en/games/245d67e2-1d22-4ee5-b92d-050c0c74845d/parcelpanic
<img width="1457" height="802" alt="image8" src="https://github.com/user-attachments/assets/f527bd99-3543-4195-916f-bee5edfe1f6e" />

---

## 🧩 Core Game Mechanics

- Pick up parcels using interaction input
- Hold left-click to charge and throw
- Avoid obstacles and cars
- Deliver parcels to the target zone before time runs out
- Fail state: Timer runs out or hit by car (resets player position)

---

## 🗂 Scenes Overview

| Scene Name | Description |
|------------|-------------|
| `MainMenuScene` | The starting scene with buttons for level selection and game instructions |
| `SelectLevelScene` | Allows players to choose between Level 1, 2, or 3 |
| `Level1Scene` | Tutorial-style map with manual popup and basic obstacles |
| `Level2Scene` | A reverse of level 1 with lower time limit |
| `Level3Scene` | Hardest level with more challenging parkour |
| `Demos/` | Contains experimental test scenes that were used for prototyping but are not included in the final build |
<img width="1457" height="816" alt="image5" src="https://github.com/user-attachments/assets/aed77d74-bb9b-4283-ac1a-ca1192b58f67" />

---

## 🎨 Assets & Art Style

- **Toon City** 3D building models were used for the environment design
- Sound effects and music were added to enhance comedic theme
---

## 📁 Project Structure (Important Folders)

- `Assets/Assets/` – Core assets including models, textures, sounds, and scripts
- `Assets/Scenes/` – All playable scenes (Main Menu, Levels, etc.)
- `Assets/Prefabs/` – Modular prefabs for players, parcels, and other game objects.
- `Assets/Scripts/` – C# scripts controlling game logic (pickup, throw, collision, UI etc.)
- `Assets/Toon City/` – Contains city building models used in-game

---

## 📜 License & Credits

This project was developed by 3 members (Core Game Mechanic https://github.com/BothBosu, Level Design https://github.com/szuchihsu, Game Assets https://github.com/pawin6380216) as part of Game Development coursework at Mahidol University International College.

