# Barcode Comparison System — Version SQLite

Aucune installation de base de données requise !
La base de données est un simple fichier `barcode.db` créé automatiquement au démarrage.

---

## Prérequis (à installer une seule fois)

### 1. .NET 8 SDK
Télécharger : https://dotnet.microsoft.com/download/dotnet/8.0
→ Windows → x64 → Installer

### 2. Node.js
Télécharger : https://nodejs.org
→ Version LTS → Installer

---

## Lancer l'application

### Étape 1 — Backend (PowerShell dans le dossier backend\)

    cd C:\chemin\vers\barcode-system\backend
    dotnet run

Vous devez voir : Now listening on: http://localhost:5000

### Étape 2 — Frontend (second PowerShell dans frontend\)

    cd C:\chemin\vers\barcode-system\frontend
    npm install
    npm start

Vous devez voir : Angular Live Development Server is listening on localhost:4200

### Étape 3 — Ouvrir le navigateur

Aller sur : http://localhost:4200

---

## En cas de problème

"dotnet n'est pas reconnu" → Installer .NET 8 SDK et redémarrer PowerShell
"npm n'est pas reconnu"    → Installer Node.js et redémarrer PowerShell
La page ne s'affiche pas   → Vérifier que les deux PowerShell tournent en même temps
# barcode--system
