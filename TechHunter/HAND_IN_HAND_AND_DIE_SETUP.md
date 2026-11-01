# 名画「手と手を取り合って死ね」

- Resources/Relics/HandInHandAndDie.asset を遺物プールへ自動登録するため、既存のInspectorリストの変更は不要です。
- スキル3 (Q) と同一のSkillCardDataを保持するPlayerAttackによるHPダメージを魅了値へ変換します。
- 魅了値は防御後ダメージ相当です。対象の現在HP以上になると永続的に味方化します。
- 混乱中の通常の混乱ダメージ・混乱値の付与は継続します。
- 味方化した敵はAllEnemyから除外し、スポットを移動すると消えます。
- **画像**: 遺物イラストはまだ割り当てていません。後からRelicImageへSpriteを指定してください。
- **注意**: 同じSkillCardDataがQ以外にも設定されている場合、PlayerAttack側の情報だけでは実際の生成元スロットを完全には区別できません。
- **注意**: Playerを直接参照して行動する特殊AIやEnemyAttack以外の固有攻撃は、必要に応じ個別の改修が必要です。
- Unity Editorによるコンパイル、混乱・魅了・勝利判定・マップ遷移・同種対戦の実動作確認が必要です。

## 魅了の状態異常管理

- `Assets/Character/Player/StatusEffect.cs` の `StatusEffectType.Charm` を使用します。
- `Assets/System/Resalt/Relic/RelicBaseDatas/HandInHandAndDie/Charm_HandInHandAndDie.asset` が専用StatusEffectAttackです。`number` は与えるはずだったダメージにかける魅了倍率（初期値1）です。
- 遺物Prefabの `Relic_HandInHandAndDie.charmEffect` が上記SOを参照します。
- 魅了値は `charaDamage.buffs` 内の `ActiveBuff.accumulatedValue` に格納。複数の当たり判定を持つ敵も本体の受け口へ集約します。
- Charmは永久効果であり、時間経過では消失しません。蓄積値が**現在HP以上になった時**、`EnemyBase.BecomeCharmedAlly()` により陣営変更・勝利判定側の登録変更を行います。
- この変更では完成済みの敵VS敵ターゲット設定は追加変更していません。

## 実行時HPバーと魅了後の衰弱（2026-11-01）

- 魅了が初めて蓄積した敵は、`charaDamage.EnsureCharmHPVisual()` から既存の `EnemyBase.HP_Slider` に `CharmHPBarVisual` を自動追加します。
- 既存Sliderの `fillRect` とFillのImageからSprite・Material・透明度を取得し、子の `CharmFill_Runtime` を実行時生成してピンク色に設定します。追加のPrefab編集・SerializeField設定は不要です。
- 魅了成立前：現在HPバーの**右端から**蓄積量ぶんピンクで覆います。通常HP減少時も現在HPと魅了値の比率で自動更新します。
- 魅了成立後：残HPすべてをピンクで覆い、残HPが減るとバーも縮みます。既存HP Sliderはそのまま使用します。
- 元Fillが `Image.Type.Filled` の場合と、SliderがRect幅で表示する場合の両方式に対応します。FillにUnity UI Imageが無ければ警告して表示だけスキップします。
- 魅了成立時に蓄積タイマーをリセットし、その後は `Charm_HandInHandAndDie.asset` の `TickTime` 間隔（初期値1秒）で毒の実効ダメージ × `CharmedDamageMultiplier`（初期値0.5）をHPから差し引きます。最低ダメージと防御力の扱いは既存毒処理に準じます。
- 敵HPが0以下なら、既存の `charaDamage.Death()` に入ります。
- 古いPrefab・Variantの継承関係は変更していません。HP Slider/Fill Imageの参照がない場合は、その敵のUIだけ要確認です。
- Unity Editor上でのコンパイルと実動作確認は未実施です。
