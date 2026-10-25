# Dialogue V2 / キャラ共通管理・会話分岐・ショップ連携

## Unityでの更新
Unityを開いたら **Tools > TechHunter > Dialogue > Create and Place Dialogue UI** を再実行してください。
- 会話UIが存在する場合、そのUI階層は維持し、共通台帳だけ差し込みます。
- 自動作成される `Assets/System/UI/Dialogue/DialogueCharacterDatabase.asset` を開きます。
- Project > Create > TechHunter > Dialogue > Character でキャラクターごとにScriptableObjectを作り、ID / Display Name / Normalほかの表情画像を設定。
- 台帳の Profiles に作成したキャラクターデータを登録します。
- 新しいキャラIDは `DialogueCharacterId.cs` のenumに追加します。番号は既存の会話資産が使用しているので再割当てしないでください。

## Conversation（会話ファイル）
Create > TechHunter > Dialogue > Conversation
- Participants: 表示するキャラIDを左から順に入力。キャラ画像や名前の再登録は不要。
- Nodes: Node IDは重複不可。Speaker ID、Expression、Textを行ごとに設定。
- Entry Node ID = -1で最初のNodeから開始。
- Node Type = Line: 会話文。一文字ずつ表示。Next Node ID=-1ならリストの次行、数字なら指定Nodeへジャンプ。
- Node Type = Choice: Text=質問、ChoicesにLabelとTarget Node IDを登録。マウスまたは上下キー・十字キーで選び、Enter/Space/ゲームパッドAで確定。
- 例: ID 0(セリフ) → ID 10(選択: 20 / 30) → ID 20(Aルート、Next=40)、ID 30(Bルート、Next=40) → ID 40(合流セリフ) → 終了。
- その他表情: Expression=Other、Other Expression KeyにCharacterProfileのOthers内のKeyを指定。未設定画像はNormal画像を表示。
- 昔のDialogueDataのLines/Charactersは旧形式再生対応のため残しています（Inspectorでは非表示）。

## Shop（表示連携）
Create > TechHunter > Dialogue > Shop Item で商品データ(名前・値段・説明・画像)を作成。
ショップUIのGameObjectにDialogueShopSelectionを追加し、Itemsに商品を登録。既存のショップUIからSelectByIndex(index)かSetSelectedItem(data)で選択商品を変更。
Item Name Text、Price Text、Description Text、Icon Imageを設定すると表示を差し替えます。
会話本文または選択肢で `{itemName}` `{itemPrice}` `{itemDescription}` を使うと選択商品に置換されます。
ChoiceのSelected Shop Itemに商品をセットして、選択肢決定時に会話の商品情報を変更することもできます。
購入、在庫処理、所持金の加減算は既存ショップ側で行ってください（今回の実装は表示と選択データ連携のみ）。

## コード連携
- DialogueUIController.Instance.Play(dialogueAsset, onFinished, onChoiceSelected);
- DialogueUIController.Instance.SetSelectedShopItem(itemAsset);
- DialogueEventTriggerのOn Choice Selectedには選択肢番号（0開始）が渡されます。
- 会話中のプレイヤー手動操作は禁止、システムからの移動は従来どおり可能。
- 会話選択ボタンはランタイムで生成するため、旧プレハブの見た目はそのまま保持します。

※ Unity Editorによるコンパイル・Play Mode検証は未実施です。


## 既存ショッププレハブとの連携（Vastor）

`Assets/Map/prefab/ShopMap(_=-Stage-=_)----------() (2) Variant.prefab` にある
`ShopManager`、`ShopController`、`VastorShop_AI` を調査し、そのまま流用するブリッジを追加。

- `ShopManager.ShopItems`（元の5商品情報、ItemImage、Explanation、value）を正規データとして利用。
- 価格計算は `ShopController.ChangeUIs()` の既存倍率（購入回数/ステージ）を利用。
- `PlayerArea` が商品に接触したら商品を選択し、商品名・価格・説明を更新。
- `ShopManager` の `Use Dialogue For Purchases` が有効かつ `DialogueUIController` がシーンにある場合、決定ボタンで購入前の会話を起動。
- 「購入する」「やめる」の選択肢から別Nodeへ進み、共通の台詞へ合流。購入承認がある場合だけ、会話終了後に元の `BuyItem()` と `ItemChoiceBuy()` を呼ぶ。
- 会話UIがないシーンは従来どおり即時購入。商人の首・頭・アニメーションなどの `VastorShop_AI` は変更しない。
- 既存の商品説明や値段はPrefabのInspector設定を優先し、重複したScriptableObjectを作らない。
- 商品名は `ShopItemDatas.ItemName`（任意）に入力でき、空欄なら従来の `ItemData` から日本語の商品種名を表示。
- 既存Prefabを破壊的に更新しないため、ShopManager内の新設定 `Use Dialogue For Purchases` の初期値はtrue。DialogueUIがない場合は従来どおり。
- ショップ名・値段・説明の固定表示UIは必要に応じてShopManagerの3つのTextMeshProUGUI参照へ割り当て可能（未設定でも商品近接時に会話本文へ反映）。
- 初期キャラVastorは `Tools > TechHunter > Dialogue > Create and Place Dialogue UI` を再実行すると台帳へ自動登録されます。立ち絵画像はVastor.assetのNormal欄に手動割り当てしてください。

**注意:** ショップの購入コストは表示値と一致するよう四捨五入して整数化、所持金がちょうど同額でも購入可能に変更。
実購入処理のカード選択処理を維持し、既存のショップ用パーティクル・アニメーション・首移動処理には触れていません。

## ShopMapプレハブ実装（自動UI起動）
- `ShopMap(_=-Stage-=_)----------() (2) Variant.prefab` の `ShopManager` GameObjectに `ShopDialogueBootstrap` を追加済み。
- マップ生成時に新会話Canvasが無ければ自動生成します（有れば既存Canvasを再利用）。**シーンへの手動配置は不要**。
- 元の `VastorShop_AI` が実行していた `テスト動作出現` と `テスト動作` は新会話UIがあるとき無効化し、古い試作ショップウィンドウを非表示にします。商人の本体・頭・首のアニメーションは継続します。
- プロジェクト内で最初に共有する商人は `ShopVastorCharacter.asset`、台帳は `ShopVastorDatabase.asset` に登録済み。**ShopVastorCharacterのNormalに会話用の立ち絵を割り当ててください**。
- これらはUnityメニューのUI生成で作る個人作業中の未追跡アセット（DialogueCharacterDatabase.assetやVastor.asset）と衝突しないよう別ファイル名にしました。
- 既存Canvasを先に手配置したシーンでは、そちらの登録台帳が優先されます。
- Prefab内部の元のショップUI/ShopManagerの価格や商品画像/処理は保持しています。
- UI背景・文字のレイアウトは `ShopDialogueBootstrap.CreateDialogueCanvas` が生成するベーシックUIです。開発中の暫定レイアウトなので必要に応じて変更可能です。
