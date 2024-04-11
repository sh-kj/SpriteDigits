# Sprite Digits for Unity-R3

0-9までの画像を使って数値を表示するためのシステムです。

Unityの`RectTransform`を利用して、必ずRectの内部に収まるようスケールして数値を表示します。  
数値の画像は別で用意し、差し替えることができます。

画像表示の実体に使うシステムは`UnityEngine.SpriteRenderer`または`UnityEngine.UI.Image`を選択できます。

## git URLからのインストール

前提としてR3 https://github.com/Cysharp/R3.git のインストールが必要です。

Package Managerの`Add package from git URL`より  
https://github.com/sh-kj/SpriteDigits.git?path=Unity-R3/SpriteDigits を指定してインストールしてください。

# 使用法

## 数値画像のSpriteを用意

AssetsメニューもしくはProject右クリックより `Create/radiants/SpriteDigits/Make Digits ScriptableObject` でDigitsのScriptableObjectを作成します。  
0-9までの数値とマイナス、小数点の画像をDigitsに登録してください。

## 整数の表示

GameObjectに`SpriteDigits`コンポーネントをアタッチしてください。

Inspectorの`Digits`に作成したScriptableObjectのDigitsを登録すると、`Value`に設定した数値が表示されます。

### パラメータ(整数)

- MaxDigitNum  
0で無制限となります。1以上の場合、表示上限を超えると`99999`のような表示でカウンターストップします。
- Padding Mode  
Zero-Fillに設定して数値の桁数がMaxDigitNum(>2)未満の場合、`00123`のような表示となり上位の桁が0で埋まります。

## 小数点数の表示

GameObjectに`SpriteDigitsFloat`コンポーネントをアタッチし、`Digits`を登録してください。

`Value`はdouble型となり、小数点以下の桁数が固定された表示となります。

### パラメータ(小数点数)

- DisplayDecimalPlaces  
小数点以下の表示桁数。桁数以下は四捨五入されます。

## 共通パラメータ

### DisplayModeの設定

`Sprite`では表示実体に`UnityEngine.SpriteRenderer`が、`Image`では`UnityEngine.UI.Image`が使用されます。

`Sprite` の方がパフォーマンスが良く、動かしてもCanvasの再演算が発生しません。また重ね順を数値で自由に制御できます。

`Image` ではuGUIのMaskやCanvasGroup等が有効となり、重ね順もuGUIの法則に従うので設定の手間が省けるメリットがあります。

### その他のパラメータ
- Color  
頂点カラーです。
- SortingLayerID, OrderInLayer(Spriteモードのみ有効)  
SpriteRendererのソート順を指定できます。
- Custom Material  
カスタムシェーダーを使う場合デフォルトからMaterialを差し替えることができます。シェーダーはSpriteモードなら`Sprites/Default`、Imageモードなら`UI/Default`をベースに作成してください。  
- Size  
数値のスケールを指定できます。Rectをはみ出るようなサイズにはなりません。
- Spacing  
数値同士のスペーシングを指定できます。
- Horizontal/Vertical Pivot  
Rect内での数値の揃え方を指定できます。

# License

MIT