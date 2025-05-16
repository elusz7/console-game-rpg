# Character Management System

A .NET-based console application for managing game characters, abilities, rooms, and more. 
This project demonstrates CRUD operations, entity relationships, and logging.

---

## Table of Contents

- [Overview](#overview)
- [Features by Grade Level](#features-by-grade-level)
  - [Basic Required Functionality](#basic-required-functionality)
  - [C Level (405/500 points)](#c-level-405500-points)
  - [B Level (445/500 points)](#b-level-445500-points)
  - [A Level (475/500 points)](#a-level-475500-points)
  - [A+ Stretch Level (500/500 points)](#a-stretch-level-500500-points)
- [Setup](#setup)
- [Final Comments](#final-comments)

---

## Overview

This application allows users to create, edit, and manage characters, their abilities, and the rooms they inhabit. 
It supports searching, logging, and advanced features such as equipment tracking and room navigation.

---

## Features by Grade Level

### Basic Required Functionality

- **Add a New Character:**  
  Prompt for character details (Name, Health, Attack, Defense) and save to the database.
- **Edit an Existing Character:**  
  Update attributes like Health, Attack, and Defense.
- **Display All Characters:**  
  List all characters with their details.
- **Search for a Character by Name:**  
  Case-insensitive search with detailed output.
- **Logging:**  
  All user interactions (add, edit, display) are logged.

---

### C Level (405/500 points)

- **All Required Features**
- **Add Abilities to a Character:**  
  Add abilities (Name, Attack Bonus, Defense Bonus, etc.) to existing characters and save to the database.
- **Display Character Abilities:**  
  Show all abilities for a selected character.
- **Execute an Ability During Attack:**  
  Abilities are executed and output is displayed during attacks.

---

### B Level (445/500 points)

- **All Required and C Level Features**
- **Add New Room:**  
  Create rooms with name, description, and properties. Optionally add characters to rooms.
- **Display Room Details:**  
  Show all room properties and list inhabitants.
- **Navigate Rooms:**  
  Move characters between rooms and display details upon entry.

---

### A Level (475/500 points)

- **All Required, C, and B Level Features**
- **List Characters in Room by Attribute:**  
  Find characters in a room matching specific criteria (Health, Attack, Name, etc.).
- **List All Rooms with Characters:**  
  Group and display characters by room.
- **Find Equipment and Associated Character/Location:**  
  Search for an item and display the character holding it and their location.

---

### A+ Stretch Level (500/500 points)

- **All C, B, and A Level Features**
- **Stretch?? Feature:**  
  Players have an Archetype, which determines their damage type and skills (previously abilities).

  Monsters can have skills assigned to them as well. Monsters are split into different threat levels which determines how hard they are to fight, and they employ various strategies to attack the player.

  Skills are split into their different effects, damaging skills, support skills (buff/debuff), ultimate skills, and boss monsters have a special skill that scales with the phase they're in.

  Rooms are all established and connected in the database, but the user can manipulate the connections as they desire.

  Items have four types: Weapon, Armor, Valuable, and Consumable. A player can equip 1 Weapon of matching Archetype damage type and up to 4 pieces of armor, 1 per ArmorType. Consumables affects the player's
    archetype resource (stamina/mana), the player's health, and an item's durability. Valuables are items that can be found on monsters during combat. Other item types can be looted from monsters... but beware...

  All of these can be created, edited, and deleted through the administrator menu.


  #Adventuring# - Select an established character and enter a floor generated based on the player's level. Once you clear the floor of monsters, you level up and move onto the next floor.

  #Campaign# - Nearly identical set up to Adventure, but you start with a new player and advance from level 1. Monsters are pre-determined and include boss monsters.

  #Merchant# - She is available in both adventuring and the campaign. She provides many services that can be helpful.

---

## Setup

1. **Clone the repository:**  
   `git clone <your-repo-url>`
2. **Configure the database connection:**  
   Update `appsettings.json` with your connection string.
3. **Run migrations:**  
   Use the .NET CLI or Visual Studio to apply migrations and create the database.
4. **Build and run the application:**  
   Use Visual Studio 2022 or the .NET CLI.

---

## Final Comments

This was a lot of fun and a little frustrating. There's so many more things I want to implement. I liked being able to create functions that I could reuse and call through multiple different classes.

TODO:
- implement elemental damage
- implement status effects
- implement weakness/strengths to different elements
- implement locked rooms
- implement traps??
- implement merchant repair option
- implement monster types?? undead, animal, abomination?? why??
- implement contracts - through merchant?? another NPC??
- implement party play??
  
---

## Submission

- [ ] Video demonstration (approx. 5 minutes)
- [ ] GitHub repository link
- [ ] Database connection string
- [ ] This README file

