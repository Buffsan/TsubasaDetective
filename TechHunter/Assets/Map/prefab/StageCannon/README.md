# 帝国ガレヲ大型主砲 — 導入・確認

## 配置
- StageCannon.prefab をステージプレハブ中央に一基配置する。NPC・EnemySpawnData登録不要。
- MountPoint を床上、土台のコライダーと重ならない位置へ設定する。半径1.5の乗り込み範囲は選択時のGizmoで確認可能。
- Base/Turret にSpriteRendererを置くと仮画像の生成を省略する。砲身の基準方向はローカル+X。FirePoint を先端に調整する。
- BaseのStageレイヤーにBoxCollider2Dをプレハブ上で設定済み。仮コライダーの自動生成はOFF。
- GaugeAnchor には編集時から装填ゲージが表示され、実行時に既存Rendererを更新する。マーカー・砲弾・砲口・爆発の素材は各Prefab欄に任意で設定できる。設定する砲弾/爆発素材は視覚専用（独自のダメージ判定を持たせない）。
- Fire SE / Explosion SE は使用する素材を設定する。素材未設定時は仮の閃光、砲身反動、カメラ揺れで確認可能。

## 操作
Space 0.3秒長押し → 乗り込み・敵混乱・3秒無敵・視野拡張 → マウス照準 → 左クリック → 方向入力で2倍速の通常回避離脱。途中キャンセルは不可。方向入力が既に保持されていれば発射の次のUpdateで離脱を開始する。発射から10秒で再装填する。
戦闘終了で強制解除し、報酬受取後のGameMode.Goalで同じ操作を練習できる。
キーボード/マウスが標準。任意の有効化済みInputActionReferenceをMount/Fireに設定できる。Android向けUI・入力バインドは本変更に含まない。

## ダメージ
ベテラン未満を貫通220、ベテラン/エリート/ボスに貫通220+爆発4100、周囲の敵に爆発4100。味方は階級に関係なく通過。既存weapondataの通常攻撃計算・クリティカル・防御・混乱・プレイヤー攻撃イベントを使用する。スキルカード扱いではないためスキル専用バフ・スキルヒットイベント・Qスロット魅了変換は対象外。テンプレートSOは任意で指定し、基礎攻撃力は主砲Inspector値で上書きする（元アセットは変更しない）。
敵の当たり判定はEnemyArea。高速砲弾はCircleCastで追跡する。列車外の車両を攻撃できるよう地形は通過し、最大飛距離で消滅する。大型敵に着弾した場合のみ爆発する。初期仮値は速度35、距離45、砲弾半径0.18、爆発半径2.5。

## 確認項目
- 通常敵・ベテラン・ボス・味方を射線に置き、貫通/爆発/味方通過を確認する。
- 起動後の無敵切れ、方向未入力時の待機、通常回避との速度比較、カメラ復元、装填ゲージを確認する。
- 報酬画面遷移、ゲームオーバー、スポット移動、主砲の無効化時に操作制限が残らないことを確認する。
- ボスの保護付き攻撃や形態移行中の混乱、既存の炎舞・攻撃遺物との相互作用を重点的に確認する。
- Unity Editor上のコンパイル・Play Modeは別途確認すること。敵車両、列車走行、完成アート/SE、実ステージへの配置は本実装の対象外。

## プレハブ修復・検証
編集画面から砲台・砲身・装填ゲージが確認可能。CannonPrototypeSquare.png は仮表示専用の単一Sprite。
Unityのコンパイル完了後、Tools > Stage Cannon > Validate Prefab で参照・Missing Script・Collider・SpriteRendererを確認できる。
問題があれば Tools > Stage Cannon > Repair Prototype Prefab でUnity標準のPrefabUtilityを使用して修復可能（確認ダイアログあり）。既存の設定・名前付きオブジェクトは読み込める場合に維持する。

## Space入力の競合対策
既存のDodgeもSpaceに割り当てられているため、装填済みの主砲の起動範囲内では回避の発動を保留する。0.3秒未満で離せば通常回避、長押しで乗り込み。範囲外や装填中は従来通り押した瞬間に回避。Gamepadの通常回避は変更しない。起動範囲はMountPointまたは土台Colliderの表面から1.5。複数設置した場合は最寄りを優先する。
Play Mode中、StageCannonControllerのMount Status、Mount Hold Progress、Player Interaction DistanceをInspectorで確認できる。Log Mount DiagnosticsをONにするとSpace押下中の状態変化のみConsoleへ出力する。戦闘中(Nomal)または報酬受取後(Goal)以外の起動制限は維持。
