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
