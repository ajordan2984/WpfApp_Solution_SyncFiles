# 🗂️ WpfApp_Solution_SyncFiles Utility

[![Status](https://img.shields.io/badge/status-active-brightgreen)](#)
[![Project](https://img.shields.io/badge/project-maintained-blue)](#)
[![Version](https://img.shields.io/badge/version-2.1.0-blue)](#)

---

## 🔍 Overview

This utility makes it simple to back up files from a selected PC folder to one or more external drives **without creating duplicate files**. It only copies over **new** or **updated** files that have been added to the PC folder. This keeps your external backups clean and up to date.

![SyncUserFiles](https://github.com/user-attachments/assets/dc64fb20-958b-4082-a208-e691ffdc00a4)

---

## ⚙️ How It Works

> [!NOTE]
> The selected or typed PC folder must match the external folder name or it will not run. For example, if you select or type **`C:\Users\johndoe\Pictures`** on your PC, the external folder must end with **`Pictures`**.

- The program scans a selected folder on your PC.
- It then **compares** the contents of that PC folder to an external backup folder.
- On the **first run**, it creates a `.txt` file listing **all files** currently in the external folder.
  - On **subsequent runs**, instead of re-scanning the external folder (which can be slow with thousands of files), the utility simply **reads from this file** to get an instant snapshot of what's already backed up and then updates the `.txt` file at the end with any file changes.
- It then compares both file structures:
  - **New files** or **Updated files** in the PC folder are **copied over** to the external folder.
  - If a **file or folder is renamed** on the PC, the utility will:
    - Copy the renamed item.
    - Move the old version on the external drive to a **`/Quarantine`** folder (for safe review and cleanup).
- This smart scanning approach reduces external drive reads, saving time and wear on your hardware.
- Finally, a log file is created on the external drive detailing any changes made.

> [!TIP]
> If you want to skip any subfolder inside the selected folder on your PC and external drives, simply select or type the name of the subfolder in the field under **`(Optional) Please add or select the folders from inside the selected PC folder to excluded from backup`** and then click the button **`Add Excluded Folder`**.

> [!NOTE]
> When skipping a subfolder, any subfolder with either that name or part of that name will be skipped. For example, if you want to skip the **`MyExample`** subfolder, **`MyExample1`**, **`MyExample2`**, **`MyExample3`** will all be skipped as they match part of the name **`MyExample`**. If you would like to change this, you can in the **`GetAllDirectories`**  function.

✅ You no longer need to manually copy files to every external drive—just update your PC folder, and let the utility sync them all efficiently.

---

## 🔓 Version
## Compatibility

**v2.1.0** - Windows 7 (64-bit recommended)
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
