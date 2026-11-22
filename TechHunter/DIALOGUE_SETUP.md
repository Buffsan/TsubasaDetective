# 汎用会話システム / Dialogue System

## 構造
- `-----GameSysem-----.prefab` の子オブジェクト `DialogueSystem` に `DialogueRuntimeBootstrap` を設定。通常ゲームシステムが存在するあらゆるステージで共通の会話UIを起動します。
- `SharedCharacters.asset` は共通のキャラクター登録台帳で、Vastor（`ShopVastorCharacter.asset`）を最初から登録しています。
- 会話内容は `DialogueData` ScriptableObjectで保存し、イベント/NPC/ショップなど任意の呼び出し元から `DialogueUIController.Instance.Play(dialogueData)` を呼び出せます。
- `DialogueEventTrigger` はシーン上のイベントObjectに貼り、任意の `DialogueData` を指定できます。
- 通常のゲームプレイを止めずにプレイヤーの手動操作だけを停止します。
- ScriptableObjectでキャラID / 名前 / 表情差分を管理。会話NodeはキャラIDと表情だけを設定します。

## 会話データの編集
`Assets/System/UI/Dialogue/VastorPurchaseConversation.asset` を選択すると、ショップ専用ではない汎用会話テーブルを使ってVastorの販売セリフを編集できます。

1. Participants に表示キャラIDを左から登録。
2. NodesにNode IDとセリフを設定。Next Node ID=-1は次の行、数値は指定Nodeへジャンプ。
3. Choice NodeのChoicesに選択肢Label/Target Node IDを設定。分岐の末尾から同じNode IDにジャンプすると合流。
4. 購入に使う選択肢はこのショップではNode=10 の0番目（購入する）です。変更する場合は ShopManager の Purchase Choice Node Id / Purchase Option Indexも変えます。
5. 商品情報は従来のShopManager.ShopItemsが正規データ。説明欄では `{itemName}` / `{itemPrice}` / `{itemDescription}` を挿入するとリアルタイムの選択商品情報が入ります。
6. `{purchaseResult}` は販売側から渡す文字列変数です。残高が足りないときも同じ会話ノードで別の文章を表示します。
7. 選択肢で購入した場合、会話終了後に既存のShopManager.BuyItemとカード付与処理を呼びます。キャンセル時は何もしません。

## 対象プレハブ
- ShopMapからUI生成コンポーネントを外しました。今は共通 GameSystemだけが会話UIを作成します。
- ShopMapの ShopManager に `VastorPurchaseConversation.asset` を割り当て済み。
- VastorShop_AIによる旧テスト会話は共通会話UIがある場合停止。頭部/首の移動はそのまま残します。
- GameSystemとShopMapが同じシーンに存在することが前提です。SampleSceneでは共通 GameSystem prefabが使用されています。

## ショップ以外への適用
- 任意のNPC/イベントに `DialogueEventTrigger` と新規Conversationアセットを割り当てればすぐ使えます。
- `DialogueUIController.Instance.SetTextVariable("key", "value")` をPlay前に呼ぶと、どの会話でも `{key}` の値を差し替えできます。終了時に変数をリセットします。
- `Tools > TechHunter > Dialogue > Create and Place Dialogue UI` はプレビューや単体シーン向け。**通常ゲームでは手動配置不要**です。

Unity上でのコンパイル/Play Mode検証は未実施です。

## ALL_SystemManagerからの共通会話参照

- 共通の `-----GameSysem-----.prefab` に常設する `DialogueSystem` GameObjectが `DialogueRuntimeBootstrap` を持ちます。ショップ内には会話システム本体を置きません。
- `ALL_SystemManager` に `dialogueRuntimeBootstrap`、`dialogueUIController`、`dialogueCharacterDatabase` を登録できる欄を追加しました。
- `DialogueSceneReferenceBinder` がUnity Editorでシーンを開いたとき／スクリプトの再読み込み後に、共通プレハブに `DialogueSystem` が存在するか確認し、なければ子オブジェクトとして作成します。
- `SampleScene` の `ALL_SystemManager` に、同じ `-----GameSysem-----` 配下の `DialogueRuntimeBootstrap` と `SharedCharacters.asset` を割り当てます。**シーンが事前に未保存状態でなければ参照の変更を自動保存**します。別件の未保存変更がある場合は上書きせず、シーンを手動保存してください。
- `Tools > TechHunter > Dialogue > Bind GameSystem References` でも再登録できます。
- `dialogueUIController` はPlay開始時に生成されるため、非再生時は空欄のままで正常です。Play開始時に `DialogueRuntimeBootstrap` が登録します。
- 会話の呼び出し例：`ALL_SystemManager.Instance.dialogueUIController.Play(dialogueData)`
- `ALL_SystemManager.Instance.dialogueUIController.IsPlaying` で会話中の判定が可能です。
- 立ち絵登録用の `SharedCharacters.asset` を各会話で使い回します。

## Inspectorでキャラクターを追加（C#のenum編集不要）

1. `Assets/System/UI/Dialogue/SharedCharacters.asset` を選択します。
2. Inspectorの **Register New Character** を押すと、新しい `DialogueCharacterProfile` アセットを自動作成・登録し、未使用の数値IDを割り当てます。
3. 作成したアセットの **Display Name** と各表情のSpriteを設定します。発行後、IDは変更しないでください。
4. 会話の `DialogueData` の **Participants**（立ち絵の表示順）および各ノードの **Speaker Id** は、登録済みキャラクター名のプルダウンで選択できます。
5. 既存のProfileをProfiles欄に手動追加しても、不足・重複したIDは自動採番します。必要なら **Repair Missing / Duplicate IDs** を実行してください。
6. プロフィール削除やリスト並び替えではほかのIDは変更しません。既に発行したIDは再利用しません。

既存のバストールのID=10と`VastorPurchaseConversation.asset`の参照は維持しています。


## 近づいて話しかけられるNPC

1. NPCのGameObjectに `DialogueNPCInteractable` を追加します（Collider2Dは必須ではありません）。
2. `Conversation` に `DialogueData` を設定します。登録済み会話IDを使う場合は `Conversation Id` に入力してください（こちらが優先）。
3. `Interaction Distance` をInspectorで調節します。NPC選択時にはSceneに緑色の範囲が表示されます。`Interaction Point`を設定した場合はその座標が中心です。
4. `Prompt Anchor` は任意の頭部Transformです。未設定ならNPCの座標に `Prompt Offset`（初期値は上方向1.2）を足してアイコンを表示します。`Prompt World Height`でサイズを指定できます。
5. `Prompt Sprite` はコンポーネント追加時、`Assets/System/UI/Dialogue/resource/会話待機用コメント.PNG` を自動参照します（空欄ならInspectorから手動で割り当ててください）。
6. プレイヤーが指定距離に入ると頭上にアイコンが表示され、左クリック（Decide）で会話が始まります。会話中はアイコンが消え、会話終了後は距離内で再表示します。`Only Once`をオンにすると初回会話後は再表示されません。
7. NPCが複数いるときは最も近い会話可能なNPCが選ばれます。NPCの会話範囲内では左クリックを会話に優先し、同じ入力で通常攻撃が発生しないようにしています。他のスキルやゲームパッド右トリガーの通常攻撃は従来どおりです。
8. `On Dialogue Finished` と `On Choice Selected` のUnityEventも使えます。

この方法はイベント用の `DialogueEventTrigger` に影響せず、会話の呼び出しには既存 `DialogueUIController.Play` / `PlayById` を使用します。
画像ファイルはUnityの特殊な `Resources` フォルダではなく `Assets/System/UI/Dialogue/resource` にあるため、エディタのResetで直接Sprite参照を保存します。ビルド時に `Resources.Load` は使いません。


### NPC会話の左クリック判定
NPCに接近すると、プレイヤー位置とInteraction Point（またはNPC位置）の距離だけで会話可能か判定します。カーソルがNPCの画像やColliderに重なっている必要はありません。会話可能なときは画面内のどこを左クリックしても会話が始まります。UIへのポインタRaycastによる会話開始禁止も撤廃しました。
`PlayerController.OnDecide`から既存の共通会話を開始し、PlayerInput側でDecideが通知されない場合に備え、`PlayerController.Update`で左クリックの押下を補助的に検知します。同一フレームの二重実行は抑止します。


## 会話グラフエディタ（Animator形式）

### 開き方
- `DialogueData` アセットをダブルクリック、またはInspectorの **会話グラフを開く** ボタン。
- メニュー `Tools > TechHunter > Dialogue > Open Graph Editor` からも開けます。
- 上部の **新規作成** は会話SOを作成し、`SharedConversations.asset` に自動登録します。既存会話は **ID登録** から登録できます。
- 編集内容は元の `DialogueData` SOに保存されます。**保存**ボタンでAssetDatabase.SaveAssets()を実行します。Undo/Redoも使えます。

### 操作
1. グリッド上を右クリックし、**追加/セリフ**、**追加/選択肢**、**追加/条件分岐**、**追加/購入処理**、**追加/終了**のいずれかを選びます。
2. `START` の出力端子を開始ノードの入力端子にドラッグして接続します。
3. 各ノードの端子から次のノードへ線を接続します。1つの出力先は1つ、複数のノードから同じ入力ノードへ合流できます。
4. ノードをタイトル部分からドラッグすると配置が保存されます。線を選択してDeleteで切断、ノードを選択してDeleteで削除できます。
5. **全体表示**で全ノードを見渡せます。**接続を検証**でSTART未接続、無効な遷移、選択肢不足、到達不能なノードなどを検査します。

### ノードの種類
- **セリフ**：話者・表情・文章・表示名を設定。次のセリフへ1本の線。
- **選択肢**：会話文と選択肢一覧。各選択肢にTrue側、条件False側の接続端子。条件キーを空にするとTrue側だけ使います。
- **条件分岐**：`Bool変数キー`を指定してTrue/Falseへ進みます。変数は従来どおり`DialogueUIController.SetBoolVariable("key", value)`でセットします。
- **購入処理**：ショップで選択中の商品をその場でピア残高を確認して購入し、**購入成功**／**残高不足（購入できない）**の2本に分岐。表示ノードではないので自動で次のセリフへ進みます。
- **終了**：必要なら最後のセリフを表示し、次の決定入力で会話終了。線は接続しません。

### ショップ購入の重要な仕様
- 実際のショップの商品と価格は従来の `ShopController` / `ShopManager` が正規データです。グラフ上の購入ノードは商品・価格・付与処理の重複実装を持ちません。
- 購入ノードに到達すると`ShopManager`が**その時点の**価格と`PlayerController.AllCoins`（ピア）を比較。足りる場合のみ代金を引き、購入数を更新して購入成功の線へ、足りなければ代金・購入数を変えず残高不足の線へ進みます。
- 購入したスキル／遺物などの**選択UIは会話終了後**に起動します。会話UIと選択UIが重ならないようにするためです。
- `VastorPurchaseConversation.asset` は新形式へ変換済みです。**購入する → 購入処理 →（成功 / 不足）→ セリフ → 終了**のサンプルになっています。キャンセル時は購入ノードを通らないので支払いません。
- 購入処理は`ShopManager`が呼び出す会話向けです。一般NPCやイベントから単独で呼び出すと購入処理の受け口がないため失敗側に進みます。
- 古い購入ノードなしの会話は引き続き従来の「会話終了時に選択肢番号で購入」方式も動作します。ただし新規ショップ会話では**購入処理ノードを推奨**します。

### 旧データの移行
- `graphVersion=0`の旧アセットは、最初にグラフを開いたとき1回だけ変換されます。`Next Node ID=-1`で配列順に進んでいたセリフは明示的な遷移線へ変更します。既存のキャラクターID、本文、選択肢や遷移先IDは維持します。
- 新形式では`-1`は**未接続**を意味します。配列順の暗黙遷移は行いません。
- グラフ編集前に会話SOを複製しておくと、旧設定との比較が容易です。
- 会話中の立ち絵は `DialogueData.participants` の順に表示され、グラフでも同じ一覧を使います（Inspectorから設定）。

### 検証
コード上の静的整合性は確認していますが、Unity 2022 Editor上のコンパイル・グラフ操作・Play Mode・ショップ購入実行は、実際のUnity環境で別途確認が必要です。

## NPCランダム会話（接近して話しかけるときの抽選）

NPCの`DialogueNPCInteractable`コンポーネントにある`Random Conversations`一覧へ候補を追加します。会話のグラフエディタや元の会話データ構造は変更不要です。

1. 従来どおり`Conversation`または`Conversation Id`に**通常会話**を設定します。
2. `Random Conversations`のサイズを増やし、各要素の`Probability Percent`（0～100）と`Conversation`または`Conversation Id`を設定します。会話IDが指定されている場合、同じ候補の直接アセット参照よりIDを優先します。
3. 各候補の確率は**絶対確率（％）**です。設定した有効な候補の合計が100％以下になるようにしてください。残った確率は通常会話に割り当てられます。例：別会話A=20%、別会話B=10%なら通常会話=70%。
4. Inspectorの`Random Conversation Chances`欄で候補合計と通常会話の確率を確認できます。100%を超える場合は警告を出し、実行時はランダム抽選せず通常会話を使います。
5. 抽選は**左クリックで実際に話しかけた瞬間**に一度だけ実行し、同じ会話中に再抽選しません。次回話しかけたときは再抽選します（`Only Once`がオンなら一度だけ）。
6. 会話SO/IDがない候補は抽選対象外となり、Inspectorに警告を表示します。選ばれた別会話が無効で再生できなかった場合は通常会話を試みます。

既存の接近距離、頭上アイコン、左クリック入力、会話終了イベント、NPCごとの一度きりフラグは変更ありません。バストールのショップ会話にも変更はありません。
