# 4種類のステータスカード強化：既存StatusUPs対応

対象ブランチ: `20261025_会話機能追加`

## 反映済み

- `PlayerController : PlayerStatus` の4つの補正値を利用し、既存の `StatusCardEffects.Apply()` から強化値を加算。
- `PlayerController.isAddStatus()` を従来どおり呼び出し、更新処理を重複させない。
- `SkillController.StatusUPs` 以外からカード種類を自動登録する処理は撤去。
- `StatusMenu` はシリアライズされたTextMeshPro参照を使う。実行時のUIオブジェクト生成を廃止。
- ダメージ、回復、獲得資金の計算処理は既にブランチに実装済み。

## 一度だけ必要なUnity Editor操作

大きな `SampleScene.unity` 内にある既存の `SkillController.StatusUPs` を安全に更新するため、シーンのシリアライズデータを直接書き換えずUnity Editor上でカード4種類を登録します。

1. Unityを開いて **SampleScene**（またはSkillControllerを含む利用シーン）をロードする。
2. メニュー **Tools > TechHunter > 4種類の強化カードを既存StatusUPsに登録** を実行する。
3. `SkillController` のInspectorで `StatusUPs` に4種類が保存されたことを確認する。
4. **シーンとPrefabの変更を保存**し、Gitにコミットする。
5. ステータス画面の追加4行の位置・背景サイズ・カード画像を確認する。

このコマンドは同じ名前のカードを重複追加しません。カードのレアリティ・補正値・説明・画像は、以後Inspectorから個別に変更できます。

| カード名 | Rarity | 初期上昇量 |
|---|---|---|
| 獲得資金増加 | 1 | +20% |
| 混乱時ダメージ | 1 | +20% |
| 回復量増加 | 1 | +20% |
| 幸運ダメージ | 2 | +20% |

**注意:** Unityのコンパイル、プレイテスト、Prefabレイアウト調整はまだ未実施です。開いているシーンの状態により追加4行の位置は微調整してください。
