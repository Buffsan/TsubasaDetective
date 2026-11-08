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

