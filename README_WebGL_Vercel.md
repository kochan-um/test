# Unity WebGL を Vercel で公開する手順

このプロジェクトを GitHub → Vercel で静的ホスティングするための手順です。2通りの方法を用意しています。

- A) GitHub Actions で自動ビルド → `vercel` ブランチに静的ファイルを公開（推奨）
- B) Unity エディタで手動ビルド → `vercel` ブランチに静的ファイルをコミット

## 前提
- Vercel アカウント（無料プランでOK）
- このリポジトリ（GitHub）
- Unity（ローカル手動ビルドをする場合のみ）

---

## A) GitHub Actions で自動ビルド（推奨）
1) GitHub Secrets に Unity ライセンスを設定
- リポジトリ → Settings → Secrets and variables → Actions → New repository secret
  - Name: `UNITY_LICENSE`
  - Value: Unity Personal/Plus/Pro の ULF（game-ci の手順に従ってエクスポート）
  - 参考: https://game.ci/docs/github/activation

2) Actions を実行
- GitHub の Actions タブ → `Build WebGL and publish to Vercel branch` → `Run workflow`
- 成功すると、`vercel` ブランチに WebGL のビルド成果物が出力されます（静的ファイルのみ）。

3) Vercel で Git 連携
- Vercel ダッシュボード → Add New → Project → `kochan-um/test` を選択
- Framework preset: `Other`
- Root Directory: ルート（変更不要）
- Build & Output Settings:
  - Build Command: 空（ビルド不要）
  - Output Directory: `.`（ルート直下に静的ファイルが存在）
- Git: Production Branch を `vercel` に変更
- Deploy を実行

以後、Actions を走らせる度に `vercel` ブランチが更新され、自動的に Vercel 側が再デプロイします。

---

## B) Unity エディタで手動ビルド→ブランチ公開
1) Unity から WebGL ビルド
- メニュー: `Tools > Build > WebGL (Vercel)` を実行
- 出力先: `<プロジェクト>/Test/WebGLBuild`

2) `vercel` ブランチにコミット
- 新規ブランチ作成: `git checkout -B vercel`
- ビルド出力だけを残して、他ファイルを削除（例）
  - 安全策として作業フォルダを別途コピーするか、下記 `git` コマンドを活用
- 例（PowerShell）:
```
# ルートに移動
cd <repo-root>
# 全削除 → WebGLBuild だけを残す（慎重に実行）
git rm -r --cached .
Get-ChildItem -File -Recurse | Remove-Item -Force -Recurse
Get-ChildItem -Directory | Where-Object { $_.Name -ne 'WebGLBuild' } | Remove-Item -Recurse -Force
# WebGLBuild の中身をルートへ移動（Vercel でルート配信にするため）
robocopy Test\WebGLBuild . /E
# 不要になった空ディレクトリ削除（任意）
Remove-Item -Recurse -Force Test
# コミット & プッシュ
git add -A
git commit -m "chore: publish WebGL build to vercel branch"
git push -u origin vercel -f
```

3) Vercel 設定
- A) と同様に Git 連携し、Production Branch を `vercel` に設定、Build Command を空、Output Directory を `.` にしてデプロイ

---

## メモ
- マルチスレッドを有効にする場合は COOP/COEP ヘッダが必要です（Vercel でも設定可能）。現在のエディタビルドスクリプトはスレッドを無効化しています（互換重視）。
- 大容量の静的ファイル（.data/.wasm）は Vercel のキャッシュで高速配信されます。必要に応じて `vercel.json` でヘッダ/キャッシュ制御を追加してください。
- Unity のビルドがうまくいかない場合は、EditorBuildSettings のシーン設定を確認し、エラー内容を Issue で共有してください。
