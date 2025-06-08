# Chicken Runner

A Unity-based open world exploration game where players explore different biomes, interact with creatures, and discover hidden features.

## Game Features

- Open world environment with procedurally generated terrain
- Multiple unique biomes (Nice, Lake, Swamp, Corrupted, Dead)
- Interactive creatures with different behaviors (aggressive and passive)
- Minimap system for navigation
- Dynamic water system with reflections
- Flower interaction system

## Required Assets

This project uses several asset packages that need to be downloaded separately from the Unity Asset Store:

1. ABKaspo Games Assets
   - Location: `/Assets/ABKaspo Games Assets/`
   - Features: Advanced water rendering and reflections
   - [Link to Asset Store]

2. SimpleLowPolyNature
   - Location: `/Assets/SimpleLowPolyNature/`
   - Contains: Low-poly nature models, materials, and prefabs
   - Used for: Environmental decoration
   - [Link to Asset Store]

3. Character Assets
   - LowpolyCharacterRio - Main player character
   - Location: `/Assets/DownloadedPacks/LowpolyCharacterRio/`
   - [Link to Asset Store]

4. Additional Map Assets
   - Location: `/Assets/DownloadedPacks/mapassets/`
   - Includes:
     - 2D Pixel Fantasy Tilemap
     - Handpainted Grass and Ground Textures
     - Kenney City Kit Suburban
     - Kenney Pirate Pack
     - Kenney Top-down Tanks
     - Stylize Tree & Grasses samples

## Project Structure

```
Assets/
├── Scripts/              # Core game scripts
│   ├── CreatureHandling/ # Creature AI and behavior
│   ├── Map/             # Map generation and management
│   └── MiniMap/         # Minimap functionality
├── Prefabs/             # Game object prefabs
├── Materials/           # Custom materials
└── Scenes/              # Game scenes
```

## Getting Started

1. Clone this repository
2. Download required assets from Unity Asset Store
3. Import assets into the project
4. Open the project in Unity (recommended version: [specify version])
5. Load the main scene from `/Assets/Scenes/AdvancedMap.unity`

## Development Guidelines

- Use the provided folder structure for new assets
- Follow the established naming conventions
- Document any new features or significant changes

