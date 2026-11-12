# プレイヤー＋味方NPC VS 敵：ステージ設定

既存の敵Prefabを一時的に味方陣営へ変更して召喚します。専用の味方Prefabや新しいEnemyTypeは不要です。

## 設定手順

1. 通常戦闘のステージで使用する EnemySpawnData（ScriptableObject）を選びます。
2. 既存の enemyspawnGroup に敵の種類と数を設定します。
3. 新しい allyspawnGroup に味方として使用する既存の EnemyType と SpawnNumber を登録します。
   - 例：CG_Knight × 2
   - 対応するPrefabは、従来どおり EnemySpawnBase.enemyList のIDで解決します。
4. ゲームを再生すると EnemySpawnBase.isStartClone から敵と味方を同時に召喚します。
   - 味方はプレイヤーの近く（allySpawnRadius：初期値5）にある床タイルを優先します。なければランダムな床に出現します。

## 動作仕様

- 味方NPCは Faction.StageAlly として管理し、魅了（Faction.Charmed）とは別です。
- 既存の EnemyContoroller / Boss_Controller / CG_Knght を利用し、味方は敵を、敵はプレイヤーと味方を狙います。
- PlayerAttack と EnemyAttack の処理で、プレイヤー陣営内の誤射を防ぎます。
- 味方は SystemManager.StageAllies に登録され、AllEnemy（敵残数）には含まれません。
- 味方が倒れてもコインを落とさず、敵が全滅すれば味方が残っていてもクリアできます。
- 次スポット・次フェーズ・再スタート時に味方を破棄します。
- 味方NPCが既存AIで子分を召喚した場合、子分にも味方属性を引き継ぎます。
- allyspawnGroup が空なら、従来の通常戦闘と同じ挙動を想定しています。

## Unityエディター上でのテスト項目

1. プレイヤー＋味方2体、敵3体で出現位置と狙いが正しいか。
2. プレイヤーの通常攻撃・スキル・遺物が味方にダメージを与えないか。
3. 敵の近接・遠距離攻撃が味方に当たり、味方の攻撃が敵に当たるか。
4. 味方が生存していても敵全滅で勝利できるか。
5. 味方が死亡してもコインを落とさず、次のスポットに残らないか。
6. 召喚系の敵・旧式騎士・ボスなどは使用予定のPrefabごとに動作を確認すること。

注：UnityのPlayMode／コンパイル検証はGitHub上の変更だけでは実施できていません。PlayerAttackを経由しない個別スキル・遺物や独自のターゲット参照を持つAIについては、実際に採用するPrefabに応じて追加の対応が必要になる場合があります。
