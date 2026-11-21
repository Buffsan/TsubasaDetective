# 月の巡礼者ハルバード使い（LunarPurgeSquad_axe）

## 追加データ
- 敵Prefab: Assets/Character/Enemy/EnemyPrefabs/Diana/LunarPurgeSquad_axe.prefab
- AI: Assets/Character/Enemy/Enemys/DIANA/LunarPurgeSquad_axe/LunarPurgeSquad_axe_AI.cs
- AI調整Prefab: 同フォルダの LunarPurgeSquad_axe_MoveBase.prefab
- 斬撃波: LunarPurgeSquad_axe_Wave.prefab / LunarPurgeSquad_axe_Wave.cs
- 攻撃1/2の割合: heavyChance=0.2（斬撃系80％、振り下ろし20％）
- 斬撃の接近ステップは攻撃範囲外、後退ステップは攻撃範囲内でも抽選。
- 旋回は時計/反時計ランダム、時々横ステップ。
- 攻撃2: 1秒の予告。最初の0.5秒は狙いを追尾して残り0.5秒は方向固定。斬撃+斬撃波を同時に出す。
- 斬撃波: 進むほど加速。高速時のすり抜けを防ぐBoxCast付き。共通EnemyAttackで防御・陣営を処理。
- 警告の幅はwaveHitboxSize.x、長さはwaveRange + waveHitboxSize.yに依存。
- 斬撃波スプライト: Assets/Character/Player/Weapon/DK_Halberd/無題649_20250201222702.png
- 絵の向きはWave.prefabのspriteRotationOffsetDegreesで調整可能。
- charadataは同派閥のKnightと同じ暫定ステータスです。Veteran=3。正式パラメータはInspectorで設定してください。

## Unityでの確認
1. ブランチ 20261121-lunar-purge-halberd を取得し、コンソールのコンパイルエラーを確認。
2. EnemyPrefabs/Diana/LunarPurgeSquad_axe.prefab を EnemySpawnData等の敵Spawn設定に登録。
3. 停止→旋回→横ステップ→接近斬撃→振り下ろしの遷移と演出を確認。
4. 攻撃2の警告が前半0.5秒だけ動き、後半は方向固定されることを確認。
5. PlayerOnly / PlayerAndEnemy / EnemyOnly、魅了、スタン/混乱、障害物に沿う地上移動を確認。
6. 斬撃波の画面上の回転・大きさ、攻撃判定・警告幅/長さを調整。

注意: GitHubへの追加のみで、Unity Editorでの実行・ビルドは未検証。
