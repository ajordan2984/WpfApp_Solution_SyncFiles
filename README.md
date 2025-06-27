# 🗂️ WpfApp_Solution_SyncFiles Utility

[![Status](https://img.shields.io/badge/status-active-brightgreen)](#)
[![Project](https://img.shields.io/badge/project-maintained-blue)](#)
[![Version](https://img.shields.io/badge/version-1.0.0-blue)](#)

---

## 🔍 Overview

This utility makes it simple to back up files from a selected PC folder to one or more external drives **without creating duplicate files**. It only copies over **new files** that have been added to the PC folder. This keeps your external backups clean and up to date.

---

## ⚙️ How It Works

- The program scans a selected folder on your PC.
- It then **compares** the contents of that PC folder to an external backup folder.
- On the **first run**, it creates a `.txt` file listing **all files** currently in the external folder.
  - On **subsequent runs**, instead of re-scanning the external folder (which can be slow with thousands of files), the utility simply **reads from this file** to get an instant snapshot of what's already backed up and then updates the `.txt` file at the end with any file changes.
- It then compares both file structures:
  - **New files** in the PC folder are **copied over** to the external folder.
  - If a **file or folder is renamed** on the PC, the utility will:
    - Copy the renamed item.
    - Move the old version on the external drive to a **`/Quarantine`** folder (for safe review and cleanup).
- This smart scanning approach reduces external drive reads, saving time and wear on your hardware.

✅ You no longer need to manually copy files to every external drive—just update your PC folder, and let the utility sync them all efficiently.

---

## 🔓 Version

**v2.0.0**  
Open Source and free to use.

---

## 💝 Support My Work

If you found this utility helpful and want to support my work—especially as I continue my **cancer journey**—you can send a small donation:

- **Cash App**: [`$ajordan2984`](https://cash.app/$ajordan2984)
- **Venmo**: [`@ajordan2984`](https://venmo.com/ajordan2984)

Every bit of encouragement and support makes a difference. Thank you 🙏

---

## 📰 Follow My Writing

For more updates, behind-the-scenes progress, and reflections from my journey, follow me on [Substack](https://andrewjordansjourney.substack.com).

---

## 📜 License

This project is licensed under the GNU General Public License v3.0 License. See `LICENSE` for details.
