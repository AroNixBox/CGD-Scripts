# CGD-Scripts: Public C# Script Library

Welcome! This repository hosts the shared C# scripts for our Unity online multiplayer project. Although the main project is private, these scripts are open for everyone to explore, learn from, and contribute to.

## 🤝 Who We Are
We are a group of media informatics students working together on a networked Unity game. We open-source our scripting layer to share knowledge and invite community contributions.

## 📂 What’s Here
All game logic, networking utilities, and helper classes live in this package:
```
Assets/_Scripts/  ← meant to be used exactly at this path in the main project
```

## 🚀 Installation
1. In your Unity project, add as a submodule under `Assets/_Scripts`:
   ```bash
   git submodule add https://github.com/AroNixBox/CGD-Scripts.git Assets/_Scripts
   git submodule update --init
   ```
2. In existing clones:
   ```bash
   git submodule update --init --recursive
   ```
3. Unity will automatically import the scripts on next launch.

## 🔧 How to Contribute (Code)
1. Fork this repo.
2. Create a branch:  
   `git checkout -b feature/YourFeatureName`
3. Make changes to existing scripts or add new ones.
4. Commit with prefix tags:
    - `ADDED:` new features
    - `FIXED:` bug fixes
    - `UPDATED:` improvements
    - `REMOVED:` deletions
5. Submit a Pull Request against `main`.
6. Respond to review comments promptly.

Feel free to reuse, modify, and learn from these scripts. We appreciate your feedback and contributions!  