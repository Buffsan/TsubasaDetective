# 名画「手と手を取り合って死ね」

- Resources/Relics/HandInHandAndDie.asset を遺物プールへ自動登録するため、既存のInspectorリストの変更は不要です。
- スキル3 (Q) と同一のSkillCardDataを保持するPlayerAttackによるHPダメージを魅了値へ変換します。
- 魅了値は防御後ダメージ相当です。対象の現在HPを超えると永続的に味方化します。
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
- Charmは永久効果であり、時間経過では消失しません。蓄積値が**現在HPを超えた時**、`EnemyBase.BecomeCharmedAlly()` により陣営変更・勝利判定側の登録変更を行います。
- この変更では完成済みの敵VS敵ターゲット設定は追加変更していません。
