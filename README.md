# Pixel-Post Unity Project

## Setup
1. Klone das Repository
2. Öffne das Projekt in Unity Version [deine Unity Version]
3. Die folgenden Assets müssen manuell importiert werden:

### Benötigte Assets
Diese Assets werden für das Projekt benötigt, sind aber zu groß für Git:

- ABKaspo Games Assets
  - Download: [Asset Store Link]
  - Platzierung: `Assets/ABKaspo Games Assets/`

- DownloadedPacks
  - Download: [Link/Beschreibung wo zu finden]
  - Platzierung: `Assets/DownloadedPacks/`

- IgniteCoders Simple Water Shader
  - Download: [Asset Store Link]
  - Platzierung: `Assets/IgniteCoders/`

- SimpleLowPolyNature
  - Download: [Asset Store Link]
  - Platzierung: `Assets/SimpleLowPolyNature/`

## Asset Management
- Große Assets werden nicht im Git Repository gespeichert
- Diese Assets müssen separat heruntergeladen und in die entsprechenden Verzeichnisse kopiert werden
- Die .gitignore ist bereits entsprechend konfiguriert

## Entwicklungs-Workflow
1. Stelle sicher, dass alle benötigten Assets installiert sind
2. Führe nach dem Klonen aus:
```bash
git lfs install
git lfs pull