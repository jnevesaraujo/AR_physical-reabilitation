# AR Motor Rehabilitation — Unity Prototype

A mobile augmented reality application for upper-body physiotherapy, built with Unity and MediaPipe Pose. Developed as an academic final-year project (*Projeto Final de Licenciatura*), the system guides patients through supervised exercises using markerless skeleton tracking and real-time AR visual feedback.

> **Status:** Academic prototype — not a medical device. Not validated for clinical use.

---

## Table of Contents

- [Overview](#overview)
- [Exercises Implemented](#exercises-implemented)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Configuration](#configuration)
- [Running in the Editor](#running-in-the-editor)
- [Build (Android)](#build-android)
- [Known Limitations](#known-limitations)
- [Not Implemented](#not-implemented)
- [License](#license)

---

## Overview

The application renders an AR overlay on the device's live camera feed. A MediaPipe-based skeleton tracker localises body landmarks in real time; per-exercise evaluators consume those landmarks to count repetitions, enforce posture constraints, and deliver corrective feedback to the patient via overlaid 3D guides and HUD messages.

Session data (repetition count, duration, completion status) is persisted to Firebase Firestore under the authenticated user's document. A researcher can assign a `subjectId` to a user profile for pseudonymised data collection.

---

## Exercises Implemented

| ID | Clinical Name | Code Identifier | Tracking Model |
|---|---|---|---|
| 1 | Rotação Cervical | `NeckRotation` | MediaPipe Pose |
| 2 | Deslizar do Braço ¹ | `ShoulderSlide` | MediaPipe Pose |
| 3 | Flexão de Punho (Pinça) | `HandGrip` | MediaPipe Hands |
| 4 | Flexão do Cotovelo | `ElbowFlexion` | MediaPipe Pose |

> ¹ The exercise menu labels this *"Deslizar Ombro"* for brevity. The clinically correct term used in the report is *"Deslizar do Braço"*. The code identifier `ShoulderSlide` predates the clinical naming decision and was retained to avoid breaking scene references.

---

## Architecture

The application follows a layered architecture with a View/Controller separation for the 2D UI (Unity UI Toolkit) and a dedicated Vision pipeline for AR tracking and evaluation.

```
┌──────────────────────────────────────────────────┐
│                    App.UI Layer                  │
│  Views (UXML/USS) ←→ Controllers                 │
│  UINavigationManager  SessionContext (shared)    │
└────────────────────────┬─────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────┐
│                  App.Vision Layer                │
│  BaseExerciseExtractor (abstract)                │
│    ├── NeckRotationExtractor                     │
│    ├── ShoulderSlideExtractor                    │
│    ├── HandGripExtractor                         │
│    └── ElbowFlexionExtractor                     │
│  Evaluators (pure C# — stateful FSM per exercise)│
│  ARExerciseVisualizer → IExerciseGuide prefabs   │
│  OneEuroFilterV3 (per-landmark smoothing)        │
└────────────────────────┬─────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────┐
│                  App.Services Layer              │
│  AuthService  ProfileService  SessionService     │
│  Firebase Auth + Firestore                       │
└──────────────────────────────────────────────────┘
```

**Key design decisions:**

- `BaseExerciseExtractor` centralises landmark caching, One Euro Filter management, HUD wiring, and rep counting. Per-exercise subclasses implement only `OnInitialize`, `CalibrateAndStart`, `OnEvaluateFrame`, and `OnSessionComplete`.
- Evaluators are plain C# classes (no `MonoBehaviour`), making their state-machine logic independently testable.
- `IExerciseGuide` decouples the 3D visual feedback prefabs from the extraction logic; `ARExerciseVisualizer` holds only the active guide reference.
- `SessionContext` is a static class used as an in-memory cross-scene data bus. It does not persist across application restarts.

---

## Technology Stack

| Component | Version / Notes |
|---|---|
| Unity | 2022.3 LTS (URP) |
| MediaPipe Unity Plugin | Homuler's plugin — body pose (33 landmarks) and hand (21 landmarks) |
| Firebase Auth | Firebase Unity SDK |
| Firebase Firestore | Firebase Unity SDK |
| Unity UI Toolkit | UXML / USS — runtime panels |
| Target Platform | Android (ARMv7 / ARM64) |
| Font | Manrope (open licence) |

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── SessionContext.cs          # Cross-scene state bus
│   │   └── Utils/
│   │       └── PerformanceLogger.cs   # CSV profiling helper (optional)
│   ├── Controllers/                   # UI controllers (MVC pattern)
│   ├── Data/
│   │   ├── Models/                    # SessionRecord, UserProfile
│   │   └── ScriptableObjects/         # ExerciseDefinition subclasses
│   ├── Services/                      # AuthService, ProfileService, SessionService
│   ├── UI/
│   │   ├── Toolkit/                   # UINavigationManager, PanelSettings assets
│   │   └── Views/                     # View MonoBehaviours (UXML bindings)
│   └── Vision/
│       ├── Evaluators/                # Per-exercise evaluation FSMs
│       ├── Extractors/                # Per-exercise landmark extractors
│       ├── Guides/                    # IExerciseGuide + concrete guides
│       ├── AngleCalculator.cs         # Stateless biomechanical utilities
│       ├── ARExerciseVisualizer.cs    # Guide lifecycle manager
│       └── OneEuroFilter.cs          # Adaptive low-pass filter
├── UI/
│   └── Views/
│       ├── RehabAppEditor.uxml        # Main app layout (all panels)
│       ├── ExerciseHUD.uxml           # In-exercise overlay
│       ├── RehabApp_Style.uss
│       ├── fonts/                     # Manrope TTF variants
│       ├── icons/                     # UI icon PNGs
│       └── instructions/              # Per-exercise tutorial images
└── [MediaPipe, Firebase, StreamingAssets — managed by Package Manager / manual import]
```

---

## Prerequisites

- **Unity 2022.3 LTS** with Android Build Support module installed.
- **Android SDK / NDK** configured in Unity Preferences → External Tools.
- **MediaPipe Unity Plugin** — follow the [plugin author's setup guide](https://github.com/homuler/MediaPipeUnityPlugin). The plugin is **not** included in this repository.
- **Firebase project** with Authentication (Email/Password) and Firestore enabled. Download `google-services.json` from the Firebase Console and place it under `Assets/`.
- Git LFS — binary assets (fonts, PNGs, instruction images) are tracked via LFS. Run `git lfs install && git lfs pull` after cloning.

---

## Setup

```bash
# 1. Clone the repository
git clone <repo-url>
cd <repo-folder>

# 2. Pull LFS objects
git lfs install
git lfs pull

# 3. Open the project in Unity 2022.3 LTS
#    Unity will import packages on first open — this may take several minutes.

# 4. Import MediaPipe Unity Plugin manually (see plugin documentation)

# 5. Place google-services.json in Assets/
#    File → Build Settings → Android → Player Settings → Other Settings
#    Confirm Package Name matches your Firebase project.
```

---

## Configuration

### Firebase

- Enable **Email/Password** sign-in in Firebase Authentication.
- Create a Firestore database in **production mode** (or test mode for development) with the following collection structure:

```
users/{userId}
  ├── firstName, lastName, email, subjectId, affectedSide, surgeryDate
  ├── registrationDate (ISO 8601 string)
  └── totalSessionsCompleted (int)
  └── sessions/{sessionId}
        ├── sessionTimestamp (ISO 8601 string)
        ├── exerciseId, completedReps, targetReps
        ├── accuracyScore, durationSeconds, isCompleted
        └── subjectId
```

> **Important:** `GetSessionHistoryAsync` in `SessionService` orders results by the field `"sessionTimestamp"`. If this field is missing or named differently in existing documents, the query will return no results without throwing an error.

### Exercise Definitions (ScriptableObjects)

Each exercise is configured via a ScriptableObject asset (`ExerciseDefinition` subclass) in the Project window:

- `NeckRotationDefinition` — rotation amplitude, neutral zone radius, pacer speed.
- `ShoulderSlideDefinition` — horizontal tolerance, minimum discovery amplitude, target side (`isLeftArm`). **Note:** the `isLeftArm` field on the asset can diverge from `SessionContext.CurrentUser.affectedSide` at runtime if the asset is mutated in Play Mode. Runtime side selection is driven by `affectedSide` on the user profile.
- `HandGripDefinition` — grip/release distance thresholds, isometric hold duration.
- `ElbowFlexionDefinition` — peak/rest angle thresholds, horizontal tolerance, expected ROM.

Assign `visualGuidePrefab` on each definition to the corresponding guide prefab (`NeckGuide`, `ShoulderGuide`, `HandGripGuide`, `ElbowGuide`).

---

## Running in the Editor

An `ExerciseAppManager` inspector field (`debugExerciseDefinition`) allows bypassing scene-to-scene `SessionContext` passing during Editor testing. Assign any `ExerciseDefinition` asset there; it will be used automatically when `SessionContext.CurrentExercise` is null (Editor-only `#if UNITY_EDITOR` guard).

Set `SessionContext.debugMode = true` in code (or expose it via an Editor menu) to enable verbose per-frame logging in the evaluators.

---

## Build (Android)

1. File → Build Settings → Switch Platform → Android.
2. Player Settings → Other Settings → Scripting Backend: **IL2CPP**; Target Architectures: **ARMv7 + ARM64**.
3. Ensure Internet Access is set to **Required** (Firebase dependency).
4. Build and deploy via USB (`adb install`) or Build and Run.

> Camera permission is required at runtime. The application uses the **front camera** (selfie mode). MediaPipe's landmark handedness is mirrored: MediaPipe's "left" landmarks correspond to the patient's anatomical **right** side.

---

## Known Limitations

- **One Euro Filter warm-up:** The filter requires a warm-up period after calibration. Filters are reset on calibration (`ResetLandmarkFilters`) and on pause/resume to avoid timestamp discontinuities, but the first ~10 frames post-calibration may exhibit slightly elevated noise.
- **`OneEuroFilterV3.SetParameters`** does not apply new `minCutoff`/`beta` values at runtime — it only calls `Reset()`. Reconstructing the filter instances is required for mid-session parameter changes.
- **`GetSessionHistoryAsync` ordering field:** The Firestore query orders by `"date"` (the field name used in older documents) rather than `"sessionTimestamp"`. Documents written by the current version use `"sessionTimestamp"`. Mixed collections will produce incorrect sort order.
- **ScriptableObject Play Mode mutation:** Modifying a `ScriptableObject` asset field during Play Mode persists the change to the asset on disk. The `isLeftArm` field on `ShoulderSlideDefinition` / `ElbowFlexionDefinition` is not read at runtime for side selection — `SessionContext.CurrentUser.affectedSide` is used instead — but the asset field can cause confusion if inspected post-session.
- **SUS evaluation (n=5):** The usability evaluation was conducted with five participants. This sample size is insufficient for statistically generalisable conclusions; results should be interpreted as indicative only.
- **No accessibility features** for low-vision users or motor-impaired users beyond the exercise's own feedback mechanism.
- **Session history display** is not implemented in the current UI; data is written to Firestore but not surfaced in the application.

---

## Not Implemented

The following requirements were descoped prior to submission due to deadline constraints. They are marked **Não Implementado** in the requirements appendix of the technical report.

| ID | Description |
|---|---|
| R2.5 | Calibration ROM metric persistence |
| R3.3 | Session history screen |
| R3.4 | Progress charts / trend visualisation |
| R4.2 | Therapist remote monitoring portal |
| R4.5 | Push notification reminders |
| R6.3 | Offline mode with sync-on-reconnect |
| R6.4 | Accessibility settings (font size, contrast) |
| R7.2 | Automated regression tests |
| R7.5 | Localisation beyond European Portuguese |
| R7.6 | iPad / tablet layout adaptation |

---

## License

This project was developed for academic evaluation purposes. No open-source licence has been applied at this time. Reuse of any part of this codebase requires explicit written permission from the author.

The **Manrope** font is distributed under the [SIL Open Font License 1.1](https://scripts.sil.org/OFL).

The **Shadow.cs** custom UI element is adapted from work by David Tattersall and is distributed under the MIT License (see licence header in `Assets/Scripts/UI/Toolkit/Elements/Shadow.cs`).

---

*Developed as a final-year undergraduate project (Projeto Final de Licenciatura), 2025–2026.*
