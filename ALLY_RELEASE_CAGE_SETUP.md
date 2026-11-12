# 敵を解放する檻（AllyReleaseCage）

## 仕様
- 檻の本体は AllyReleaseCage（IDamageable）です。EnemyBase を持たないため、敵AIの探索対象や SystemManager.AllEnemy（敵の討伐対象）に入りません。
- プレイヤー攻撃が EnemyArea タグの Collider2D に命中すると、檻のHPを減らします。
- HPが0になったら、一度だけ設定済みの敵セットを既存Prefabから生成し、味方（Faction.StageAlly）へ切り替えます。
- 味方の通常AIを短時間停止し、檻の中心から放射状に散開させてから戦闘へ移行します。
- 解放された味方は既存の SystemManager.StageAllies に登録し、討伐対象にカウントしません。
- 味方は死亡してもピアを落としません（既存 EnemyBase.DropCoinsOnce の StageAlly 除外処理）。
- 檻自体もピアを落としません。敵全滅によるステージクリアは檻が残っていても成立します。

## 初回：檻Prefabを生成する
Unityのメニュー **Tools > Stages > Create Ally Release Cage Prefab** を押します。
生成物：
- Assets/Character/Enemy/AllyReleaseCage/AllyReleaseCage.prefab
- Assets/Character/Enemy/AllyReleaseCage/CagePlaceholder.png（仮の檻スプライト）

Prefabには EnemyArea タグ、SpriteRenderer、BoxCollider2D（Trigger）、Kinematic Rigidbody2D、
AllyReleaseCage が設定されます。ゲーム用の本番画像に差し替えて使ってください。
既にPrefabがある場合、メニュー操作で上書きされません。

## 檻ごとの解放セット
Prefabまたはシーン上の檻の AllyReleaseCage コンポーネントで設定：
- Max HP / Defense：耐久値と防御値
- Release Enemies：敵の種類（既存 EnemySpawnGroup.EnemyType）と数を指定
- Release Preset：任意の EnemySpawnData（SO）。allyspawnGroup が空でなければそちらを、空なら enemyspawnGroup を使用
- Release Enemies と Release Preset の両方を設定した場合は、両方を解放します
- Spawn Database：任意の EnemySpawnBase。空なら ALL_SystemManager.enemySpawnBASE から取得
- Minimum / Maximum Scatter Radius：散開半径
- Scatter Duration：飛び出し演出時間
- Keep On Floor Tiles：床タイルがある位置への出現を優先
- Break Effect / Each Ally Effect：任意の演出Prefab（自動消去されるパーティクル等を指定）

## ステージへの配置
- **マップに手動設置**：生成した檻PrefabをマップPrefabの子に配置
- **ウェーブから生成**：EnemySpawnData の Cage Spawn Groups に、檻Prefabと出現数を指定
  - EnemySpawnBase.isStartClone() でスポーンします
  - 通常の敵SpawnGroupには登録しません（討伐対象になってしまうため）

檻から出てきた敵は EnemySpawnBase.enemyList 内の同じ EnemyType / ID マッピングを参照します。
個別の敵Prefabをコピーする必要はありません。

## Unityでの検証
1. 通常攻撃とスキルで檻が壊れるか
2. Qスロットの魅了変換スキルでも檻にダメージが入るか
3. 設定したEnemyType / 体数で、味方が飛び出すか
4. 散開後、味方は敵へ、敵はプレイヤーまたは味方へ攻撃するか
5. 解放された味方・檻からピアがドロップしないか
6. 檻・味方が残ったまま敵を全滅させても、クリア判定が正常か
7. ステージを移動すると、残った檻（マップ子）と味方が消えるか

### 注意
- 檻はEnemyBaseを持たないため、プレイヤーの「AllEnemyを探索して標的を選ぶ」形式の自動照準攻撃では対象になりません。通常の当たり判定を使うPlayerAttack系攻撃で壊せます。
- 一部の独自スキル・遺物はcharaDamageを直接参照している可能性があり、すべての攻撃への対応は未確認です。
- Unityエディターでのコンパイル・PlayMode実動作検証は未実施です。
