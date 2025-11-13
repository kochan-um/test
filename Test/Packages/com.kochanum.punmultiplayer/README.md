## Kochan PUN Multiplayer (Unity Package)

Reusable Photon PUN helpers to speed up multiplayer setup without using Resources.

- Custom `IPunPrefabPool` (explicit prefab references via Inspector)
- `PlayerSpawner` (connect 竊・join/create room 竊・spawn)
- `NetworkPlayer` (ownership gating, transform sync, Cinemachine follow)

### Requirements
- Unity 2021.3+ (URP/Standard both ok)
- Photon PUN 2 imported in the project (`Assets/Photon/...`)
- Optional: Cinemachine (for camera follow), Starter Assets ThirdPersonController

### Install
Option A. Git URL (embedded package)
- Unity 竊・Window 竊・Package Manager 竊・+ 竊・Add package from git URL窶ｦ
- URL: `https://github.com/kochan-um/test.git?path=Test/Packages/com.kochanum.punmultiplayer#main`
- Project Settings 竊・Player 竊・Scripting Define Symbols 竊・Add: `KOCHAN_PUN_MODULE`
- (Optional) Enable tests: `Packages/manifest.json` 竊・add
  {
    "testables": ["com.kochanum.punmultiplayer"]
  }

Option B. Copy into `Packages/`
- Copy folder `com.kochanum.punmultiplayer` to your `<Project>/Packages/`
- Add `KOCHAN_PUN_MODULE` to Scripting Define Symbols
- (Optional) add `testables` as above

### Setup Steps (no asset reference errors)
1) Photon AppId
- Open `PhotonServerSettings` and set AppId.

2) Scene bootstrap
- Create empty `NetworkBootstrap` GameObject
- Add components:
  - `PhotonPrefabPool`
    - Entries: add your player prefab as
      - Name: `PlayerArmature` (or any ID you prefer)
      - Prefab: your Player prefab (NOT in `Resources/`)
  - `PlayerSpawner`
    - Player Prefab Id: same as above (e.g., `PlayerArmature`)
    - SpawnPoints (optional): assign transforms

3) Player prefab
- Ensure it contains or is compatible with:
  - `Rigidbody` (optional)
  - `ThirdPersonController` + `StarterAssetsInputs` + `PlayerInput` (optional)
  - A child named `PlayerCameraRoot` (recommended) or `CinemachineCameraTarget`
  - `PhotonView` is auto-added by `PhotonPrefabPool` if missing

4) Camera
- Main Camera has `CinemachineBrain`
- Scene contains `CinemachineVirtualCamera` (recommended name: `PlayerFollowCamera`)
- `NetworkPlayer` auto binds the vcam's `Follow/LookAt` to `PlayerCameraRoot` (or `CinemachineCameraTarget`)

5) Play
- Enter Play mode 竊・connects to Photon 竊・joins/creates `Default` room 竊・spawns local player

### API Notes
- Instantiate more prefabs (bullets, etc.)
  - Register in `PhotonPrefabPool` Entries with Name `Bullet` and Prefab
  - Call: `PhotonNetwork.Instantiate("Bullet", pos, rot);`

### Animation Sync\n- Add `AnimatorParamSync` to your player (or let `NetworkPlayer` auto-add when Animator exists).\n- AutoDiscover: syncs all float/int/bool parameters automatically.\n- Triggers: call `AnimatorParamSync.SetTrigger("YourTrigger")` instead of `Animator.SetTrigger` to propagate.\n\n### Tests
- Included under `Tests/Runtime` (requires `testables` entry)
- Covers `PhotonPrefabPool` mapping and basic instantiation behavior

### Uninstall / Coexistence
- Package assembly compiles only when `KOCHAN_PUN_MODULE` scripting define symbol is present.
- This avoids duplicate types if you already have similar scripts in your project.

### Changelog
- 0.1.0 Initial release

