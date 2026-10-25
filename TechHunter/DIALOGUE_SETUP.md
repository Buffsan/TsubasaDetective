# 会話システム / Dialogue UI

対象ブランチ: `20261025_会話機能追加`

## 最初のセットアップ（Unity Editor）
1. Unityでプロジェクトを開き、コンパイルが完了するのを確認。
2. メニューの **Tools > TechHunter > Dialogue > Create and Place Dialogue UI** を一度実行。
   - `Assets/System/UI/Dialogue/DialogueCanvas.prefab` を生成し、現在のシーンに配置。
   - プレハブがすでにあれば再生成・上書きしない。
3. シーンを保存。ほかのシーンで使う場合も同じプレハブを配置。

## 会話の作成と呼び出し
1. Projectウィンドウで **Create > TechHunter > Dialogue > Conversation** を選び、DialogueDataを作成。
2. `Characters` に表示名と立ち絵Spriteを左から順に登録（Scale・Flip Horizontallyも設定可能）。
3. `Lines` に `Speaker Index`、`Show Speaker Name`、`Text` を登録。
   - `Speaker Index = 0` は最初の立ち絵、`1` は2人目、`-1` は地の文（全員暗いまま）。
   - 文章が枠に入り切らない場合は1つのLineを分割する。
4. イベント用のGameObjectに `DialogueEventTrigger` を追加し、DialogueDataをセット。
   - `StartDialogue()` をUnityEventやほかのスクリプトから呼べる。
   - 任意でPlay On Start、Collider2Dでプレイヤーが侵入したときの起動が可能。
   - `On Dialogue Finished` で次のイベントに接続可能。

コードからは `DialogueUIController.Instance.Play(dialogueData, callback);` で直接起動できる。

## 操作と挙動
- 決定ボタン: 文章表示中は全文表示、全文表示後は次のセリフへ。
- 会話中の移動・攻撃・スキル・回避入力はPlayerController内で遮断。
- `MoveInput` をイベント側から設定するなどのシステム移動は維持（movetypeとgameModeを変更しない）。
- `DialogueUIController` の `Pause World While Talking` は初期値false。戦闘を止めたい会話だけtrueに変更する。true時はTime.timeScaleを一時的に0にするため、通常の物理移動は停止するが文字送りは継続する。
- TMP文字送りは `maxVisibleCharacters` を使用。日本語用フォントは既存の `Honoka-Shin-Antique-Maru_R SDF.asset` を優先。

## UIの調整
- プレハブ内の `DialogueBorder`、`MessageBackground`、`SpeakerNamePanel` に画像を設定すると参考画像のような装飾へ変更可能。
- 立ち絵は `Portraits` のHorizontalLayoutGroupで横並び。会話によって登録人数が変わっても対応。
- ポートレートの暗さと文字送り速度は `DialogueUIController` のInspectorから変更。
