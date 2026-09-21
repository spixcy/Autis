# 🌿 Autis — Open World Survival Game

**Autis** is a 3D open-world survival game built in Unity. Explore a lush procedurally-scattered terrain, chop trees, mine rocks, craft gear, fight enemies, and build your way to survival.

---

## 🎮 Gameplay Overview

You wake up alone in a sprawling open world. Gather resources, craft tools and structures, manage your hunger and stamina, and fend off hostile creatures as night falls. Your goal is to survive — and thrive.

---

## ✨ Features

### 🌍 Open World
- Large terrain with forests, lakes, grassy biomes, and scattered rocks
- A dynamic water lake with proper water volume effects
- Trees and shrubs react to being chopped — they wobble, fall, and drop wood
- Rocks can be mined for stone, copper, gold, emerald, and other ores

### 🪓 Gathering & Resources
- Chop trees using axes to collect **Wood**
- Mine rock nodes to collect **Stone**, **Copper**, **Gold**, **Emerald**, **Violet Crystal**, **Bloom Crystal**, and more
- Gather food items like **Apple**, **Pumpkin**, **Coconut**, **Mushroom**, **Strawberry**, **Watermelon**, **Fish**, and more

### 🎒 Inventory System
- 24-slot inventory grid with a **5-slot hotbar**
- Drag-and-drop item management
- Stack items (up to max stack size per item type)
- Split stacks by right-clicking
- Drop items from your hand with **Q**
- Collect items in the world by looking at them and pressing **E**

### ⚒️ Crafting
#### Personal Crafting (Tab)
- Open the crafting menu anywhere, no workbench needed
- Craft the **Crafting Table** itself from 4 Wood

#### Crafting Table UI (right-click the placed table)
- Full categorised recipe book: **Basic**, **Tools**, **Stations**, **Build**
- Browse all recipes with icons
- Click any recipe to see its exact requirements vs. what you own
- **Craft** button (makes 1) and **Auto** button (crafts as many as possible)
- Crafting correctly consumes items from your inventory

#### Available Recipes Include:
| Category | Items |
|----------|-------|
| Basic | Crafting Table, Campfire, Respawn Point, Flint |
| Tools | Stone Axe, Stone Pickaxe, Copper Tools, Gold Tools, Emerald Tools, Violet Tools, Bloom Tools |
| Weapons | Stone Sword, Copper Sword, Gold Sword, Emerald Sword, Violet Sword, Bloom Sword |
| Stations | Campfire, Respawn Point |
| Build | Fence, Barrel, Floor Board |

### 🏥 Survival Stats (HUD)
- **Health bar** — takes damage from enemies; regenerates slowly
- **Stamina bar** — drains while sprinting; refills when idle
- **Hunger bar** — drains over time; eat food to restore it

### 🗺️ World Spawners
- **Tree Spawner** — scatters trees across the terrain (excludes water surfaces)
- **Rock Spawner** — spreads ore-bearing rocks across the world
- **Small Rock Spawner** — populates the terrain with small stone fragments
- **Chest Spawner** — hides 400 loot chests across the map, each containing random items
- **Enemy Spawner** — spawns hostile creatures on land (respects NavMesh and avoids water)

### 👹 Enemies
- Enemies patrol the world using Unity's NavMesh
- They detect and chase the player on sight
- Deal melee damage on contact

### 📦 Chests
- Chests are scattered across the terrain
- Right-click to open and receive random loot
- Items pop out with physics

### 🔊 Audio & Visual
- Footstep sounds while walking
- Hit particles when chopping trees or mining
- Wood break / pickup particle effects
- Item pickup notification UI
- Wobble animation on trees when hit
- Tree fall physics on death

### 🎯 Controls

| Action | Key / Button |
|--------|-------------|
| Move | WASD |
| Look | Mouse |
| Sprint | Left Shift |
| Jump | Space |
| Pick up item | E |
| Attack / Chop / Mine | Left Click |
| Open Chest | Right Click |
| Open Inventory | Tab |
| Select Hotbar Slot | 1–5 or Mouse Scroll |
| Drop Item | Q |

---

## 🛠️ Built With

- **Unity 6** (6000.2.10f1)
- **Universal Render Pipeline (URP)**
- **Unity NavMesh** for enemy AI
- **Unity New Input System**
- **TextMeshPro** for UI
- **Custom C# scripts** for all gameplay systems

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Inventory/       # InventoryManager, CraftingManager, ItemData, ItemSlot, CraftingRecipe
│   ├── UI/              # InventoryUI, CraftingTablePanel, PersonalCraftingPanel, SlotUI, HotbarUI, SurvivalStatsHUD
│   ├── PlayerHealth.cs  # Health, death, respawn
│   ├── PlayerInteract.cs # Raycast interactions (pick up, attack, open chest)
│   ├── TreeNode.cs      # Tree HP, drops, animations
│   ├── TreePhysics.cs   # Tree fall physics
│   ├── RockNode.cs      # Rock HP and ore drops
│   ├── Chest.cs         # Chest loot logic
│   ├── ChestSpawner.cs  # Scatter chests on terrain
│   ├── TreeSpawner.cs   # Scatter trees (excludes water)
│   ├── EnemySpawner.cs  # Spawn enemies on NavMesh (excludes water)
│   ├── DropItem.cs      # World item pickup logic
│   ├── WorldLootItem.cs # Item data attached to world drops
│   └── ...
├── ITEMDATA/
│   ├── master_prefabs/  # Wood item data and world prefab
│   ├── Food/            # Apple, Pumpkin, Coconut, Fish, Mushroom etc.
│   ├── ores/            # Stone, Copper, Gold, Emerald, Violet, Bloom ores + tools
│   ├── Building/        # Barrel, Fence, Floor Board
│   ├── Recpies/         # All CraftingRecipe ScriptableObjects
│   └── Treeessss/       # Tree prefabs (Apple Tree, Shrubs, etc.)
└── ...
```

---

## 🚀 How to Run (Build)

1. Locate `Autis.exe` in the build folder
2. Double-click to launch
3. The game starts directly in the open world

> **System Requirements:** Windows 10/11, DirectX 12 compatible GPU, 4 GB RAM minimum

---

## 🧩 Known Issues / Notes

- The **Water layer** must be assigned to any lake plane in the scene for spawners to correctly avoid spawning on water surfaces
- After placing the lake plane, uncheck **Navigation Static** on it and **Rebake the NavMesh** (Window → AI → Navigation → Bake)
- If items from chests appear **pink**, add the `Outline.shader` to **Project Settings → Graphics → Always Included Shaders**

---

## 👨‍💻 Developer Notes

This project was developed iteratively with a heavy focus on making all systems feel connected:

- The **crafting system** is data-driven — all recipes are Unity ScriptableObjects, so new items can be added without touching code
- The **inventory** uses name-based matching (case-insensitive) so items from different sources (world drops vs. chest loot) always stack correctly
- The **spawners** use terrain height sampling and layer masking to avoid placing objects in water or steep slopes
- The **HUD** bars are fully procedural — built entirely in code with no prefab dependencies

---

## 📜 License

This project is for educational and personal use only.
