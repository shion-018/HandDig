# 鍵ドアシステム（回転ベース）

鍵や特定のタグが付いたオブジェクトを指定の数取ったらドアが開くシステムです。
ドアの開閉は回転で制御され、右側と左側のドアを個別に設定できます。

## スクリプト構成

### 1. DoorController.cs
ドアの開閉を制御するメインスクリプト（回転ベース）

**主な機能:**
- 鍵の収集状況を管理
- ドアの回転による開閉アニメーション
- 右側・左側ドアの個別設定
- テスト用キー入力（Oキーで開く、Cキーで閉じる）
- 特定オブジェクトの管理

**設定項目:**
- `leftDoor`: 左ドアの設定（DoorPanel）
- `rightDoor`: 右ドアの設定（DoorPanel）
- `useBothDoors`: 両方のドアを使用するかどうか
- `requiredKeyCount`: 必要な鍵の数
- `keyTag`: 鍵として認識するタグ
- `specificObjects`: 鍵として認識する特定のオブジェクトリスト
- `testOpenKey`: テスト用開くキー（デフォルト：O）
- `testCloseKey`: テスト用閉じるキー（デフォルト：C）
- `enableTestMode`: テストモードの有効/無効

**DoorPanel設定項目:**
- `doorTransform`: ドアのTransform
- `rotationAxis`: 回転軸（通常はVector3.up）
- `openAngle`: 開く角度（度）
- `closedAngle`: 閉じた角度（度）
- `openSpeed`: 開く速度（度/秒）
- `closeSpeed`: 閉じる速度（度/秒）
- `pivotOffset`: 回転中心からのオフセット

### 2. KeyItem.cs
鍵アイテムのスクリプト

**主な機能:**
- プレイヤーとの接触検知
- 収集時のエフェクト・音再生
- 収集後のオブジェクト削除

**設定項目:**
- `canBeCollected`: 収集可能かどうか
- `destroyOnCollection`: 収集後にオブジェクトを削除するか
- `collectionDelay`: 収集から削除までの遅延時間
- `collectionEffect`: 収集時のエフェクト
- `collectionSound`: 収集時の音

### 3. KeyDoorManager.cs
鍵とドアの連携を管理するマネージャー

**主な機能:**
- シーン内の鍵・ドアの自動検出
- 鍵とドアの連携設定
- 全体の管理機能

**設定項目:**
- `autoDetectKeys`: 鍵の自動検出
- `autoDetectDoors`: ドアの自動検出
- `keyTag`: 鍵のタグ
- `doorTag`: ドアのタグ
- `manualKeys`: 手動設定する鍵リスト
- `manualDoors`: 手動設定するドアリスト

## 使用方法

### 基本的なセットアップ

1. **ドアの設定**
   ```
   1. ドアオブジェクトにDoorControllerスクリプトをアタッチ
   2. 左ドアの設定：
      - doorTransform: 左ドアのTransformを設定
      - rotationAxis: 回転軸を設定（通常はVector3.up）
      - openAngle: 開く角度を設定（例：90度）
      - closedAngle: 閉じた角度を設定（通常は0度）
   3. 右ドアの設定（両開きの場合）：
      - doorTransform: 右ドアのTransformを設定
      - rotationAxis: 回転軸を設定（通常はVector3.up）
      - openAngle: 開く角度を設定（例：-90度）
      - closedAngle: 閉じた角度を設定（通常は0度）
   4. useBothDoors: 両開きドアの場合はtrueに設定
   5. requiredKeyCount: 必要な鍵の数を設定
   ```

2. **鍵の設定**
   ```
   1. 鍵オブジェクトにKeyItemスクリプトをアタッチ
   2. コライダーを設定（TriggerまたはCollision）
   3. 必要に応じてタグを設定
   ```

3. **マネージャーの設定**
   ```
   1. 空のGameObjectにKeyDoorManagerスクリプトをアタッチ
   2. 自動検出設定を調整
   3. 必要に応じて手動設定を追加
   ```

### タグを使用する場合

1. 鍵オブジェクトに「Key」タグを設定
2. ドアオブジェクトに「Door」タグを設定
3. DoorControllerのkeyTagを「Key」に設定
4. KeyDoorManagerのkeyTagとdoorTagを設定

### 特定オブジェクトを使用する場合

1. DoorControllerのspecificObjectsリストに鍵オブジェクトを追加
2. タグの設定は不要

### テスト機能

- **Oキー**: ドアを開く（鍵の数に関係なく）
- **Cキー**: ドアを閉じる
- **KeyItemのContext Menu**: 「テスト用：鍵を収集」で手動収集

## イベント

### DoorController
- `OnDoorOpened`: ドアが開いた時
- `OnDoorClosed`: ドアが閉じた時
- `OnKeyCollected(int)`: 鍵が収集された時（現在の鍵数）

### KeyItem
- `OnKeyCollected(KeyItem)`: 鍵が収集された時

### KeyDoorManager
- `OnKeyRegistered(KeyItem)`: 鍵が登録された時
- `OnDoorRegistered(DoorController)`: ドアが登録された時

## デバッグ機能

- 各スクリプトに`showDebugInfo`フラグがあり、コンソールに詳細なログを出力
- SceneビューでGizmosが表示される（選択時）
- 鍵の位置、ドアの開閉位置が可視化される

## 注意事項

1. プレイヤーオブジェクトには「Player」タグが必要
2. 鍵オブジェクトにはコライダーが必要
3. ドアの開閉はTransformのlocalRotationを使用
4. 鍵の重複収集は防止される
5. マネージャーはシーンに1つだけ配置することを推奨
6. 回転軸は通常Y軸（Vector3.up）を使用
7. 左ドアは正の角度、右ドアは負の角度で開くことが一般的

## 拡張例

### カスタム収集条件
KeyItemスクリプトを継承して、特定の条件でのみ収集可能にする

### 複数ドア対応
1つの鍵で複数のドアを開く場合は、KeyDoorManagerで管理

### 鍵の種類別管理
異なる種類の鍵で異なるドアを開く場合は、DoorControllerを複数作成し、それぞれ異なる条件を設定
