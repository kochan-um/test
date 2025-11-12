# Unity ThirdPersonController を Photon マルチプレイ化（Resources 不使用）

このリポジトリには、Starter Assets の ThirdPersonController を Photon PUN でマルチプレイ化する最低限の仕組みを追加済みです。プレハブは `Resources` では管理せず、Inspector で明示登録するカスタム PrefabPool を使います。

## 追加済みスクリプト
- `Test/Assets/Scripts/Multiplayer/PhotonPrefabPool.cs`
  - `IPunPrefabPool` 実装。名前→Prefab の対応を Inspector で設定し、`PhotonNetwork.PrefabPool` に適用します。
- `Test/Assets/Scripts/Multiplayer/NetworkPlayer.cs`
  - 所有者のみ入力・コントローラ有効化。非所有者は Transform を補間同期。
- `Test/Assets/Scripts/Multiplayer/PlayerSpawner.cs`
  - Photon に接続→既定ルーム参加→登録済みプレハブを `PhotonNetwork.Instantiate` でスポーン。

## 前提
- Unity（Starter Assets: ThirdPersonController が含まれていること）
- Photon PUN 2 インポート済み（`Assets/Photon/PhotonUnityNetworking` がある）
- シーン例: `Test/Assets/StarterAssets/ThirdPersonController/Scenes/Playground.unity`

## クイックセットアップ（5分）
1. プロジェクト `Test` を Unity で開く。
2. Photon AppId を設定。
   - メニュー: Window → Photon Unity Networking → PUN Wizard（または `PhotonServerSettings` アセット）で AppId を入力。
3. 起動シーン（例: Playground.unity）を開く。
4. 空の GameObject を作成（例: `NetworkBootstrap`）。
5. コンポーネントを追加:
   - `PhotonPrefabPool`
     - Entries に 1 件追加:
       - Name: `PlayerArmature`
       - Prefab: `Test/Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerArmature.prefab`
   - `PlayerSpawner`
     - `Player Prefab Id` を `PlayerArmature`（デフォルト）にする。
     - 任意で `SpawnPoints` に Transform を複数指定可能。
6. カメラを配置。
   - Main Camera に Cinemachine の Virtual Camera をアタッチしている場合、`NetworkPlayer` がローカルプレイヤーの `CinemachineCameraTarget` を Follow/LookAt に自動設定します。
7. 再生（Play）。
   - 自動で Photon 接続→`Default` ルーム参加→プレイヤーがスポーンします。

## Resources を使わないプレハブ管理
- カスタム `PhotonPrefabPool` の `Entries` に「名前→Prefab」を登録し、コード側ではその「名前」で生成します。
- 例（別スクリプトから生成する場合）:
```csharp
// プレハブを事前に PhotonPrefabPool の Entries に登録しておくこと
var go = PhotonNetwork.Instantiate("PlayerArmature", spawnPos, spawnRot);
```
- 追加プレハブ（弾丸等）も同様に、`Entries` に `Bullet` などを登録 → `PhotonNetwork.Instantiate("Bullet", ...)` で生成。

## 動作仕様（既定値）
- ルーム: `Default` に JoinOrCreate（`PlayerSpawner.maxPlayers` デフォルト 8）
- 同期レート: `PhotonNetwork.SendRate = 30`, `SerializationRate = 15`
- 所有権:
  - ローカル: ThirdPersonController/Input/物理を有効
  - リモート: 入力を停止し、位置・回転のみ補間同期

## トラブルシュート
- 生成に失敗: `Failed to instantiate 'PlayerArmature'`
  - `PhotonPrefabPool` の `Entries` に `PlayerArmature` が登録されているか確認。
- 接続しない/入室しない
  - AppId 未設定、またはネットワーク制限を確認。`OnConnectedToMaster` が呼ばれていれば自動で入室します。
- リモートプレイヤーが操作できてしまう
  - Player プレハブ直下に ThirdPersonController/StarterAssetsInputs/PlayerInput があることを確認（`NetworkPlayer` が自動で無効化）。
- カメラが追従しない
  - Main Camera に Cinemachine Virtual Camera があるか、プレイヤーに `CinemachineCameraTarget` が存在するか確認。

## 拡張のヒント
- アニメーター同期: Animator のパラメータを `OnPhotonSerializeView` で送受信、または `PhotonAnimatorView` を検討。
- より滑らかな補間: `NetworkPlayer` の `_lerpSpeed` を調整。
- スポーン管理: `PlayerSpawner` の `spawnPoints` に開始位置を複数設定。

---
このドキュメントに沿ってセットアップすれば、ThirdPersonController の基本的なマルチプレイが動作します。追加要望（弾丸のネット同期、HP/リスポーン、ボイスチャット等）があればお知らせください。
