# お宝UIシステム 設定ガイド

## 概要
このシステムは、お宝の取得状況、強化段階、爆発採掘システムの状態、総取得お宝数を表示するUIシステムです。

## 実装された機能

### 1. 表示項目
- **通常強化お宝**: 現在のツールの強化段階 (例: 3/5)
- **ドリル速度お宝**: ドリルの速度レベル (例: 2/5)
- **爆発採掘システム**: 爆発モード状態と残りチャージ数
- **総取得お宝数**: 取得したお宝の総数
- **お宝取得通知**: お宝取得時のポップアップ通知

### 2. お宝の種類と通知メッセージ
- **通常強化お宝**: "ツール強化お宝を取得！"
- **つるはし判定数増加お宝**: "つるはし判定数増加お宝を取得！"
- **ドリル判定数増加お宝**: "ドリル判定数増加お宝を取得！"
- **ドリル速度増加お宝**: "ドリル速度増加お宝を取得！"
- **爆発採掘お宝**: "爆発採掘お宝を取得！"

## 設定手順

### Step 1: 基本設定

1. **空のGameObjectを作成**
   - 名前: "TreasureUIManager"
   - 位置: シーンの適当な場所

2. **TreasureUIManagerコンポーネントを追加**
   - 作成したGameObjectに`TreasureUIManager.cs`をアタッチ

3. **TreasureNotificationManagerも作成**
   - 名前: "TreasureNotificationManager"
   - `TreasureNotificationManager.cs`をアタッチ

### Step 2: UI設定（自動生成の場合）

両方のマネージャーは自動的にUIを作成しますが、手動で設定することも可能です。

#### TreasureUIManagerの設定
```
[表示設定]
- Update Interval: 0.1 (UI更新間隔)
- Show Explosion System After First Get: true (初回取得後に爆発システムを表示)

[テキスト設定]
- Text Color: White (テキストの色)
- Text Size: 24 (テキストサイズ)
```

#### TreasureNotificationManagerの設定
```
[表示設定]
- Display Duration: 3.0 (表示時間)
- Fade In Duration: 0.5 (フェードイン時間)
- Fade Out Duration: 0.5 (フェードアウト時間)

[お宝メッセージ設定]
- Normal Treasure Message: "ツール強化お宝を取得！"
- Pickaxe Hit Zone Message: "つるはし判定数増加お宝を取得！"
- Drill Hit Zone Message: "ドリル判定数増加お宝を取得！"
- Drill Speed Message: "ドリル速度増加お宝を取得！"
- Explosive Message: "爆発採掘お宝を取得！"
```

### Step 3: 手動UI設定（オプション）

自動生成されたUIをカスタマイズしたい場合：

1. **Canvasを作成**
   - UI > Canvas で新しいCanvasを作成
   - Render Mode: Screen Space - Overlay
   - Sort Order: 100 (TreasureUIManager用), 1000 (TreasureNotificationManager用)

2. **UI要素を作成**
   ```
   TreasureCanvas
   ├── NormalTreasureDisplay (TextMeshPro)
   ├── DrillSpeedDisplay (TextMeshPro)
   ├── ExplosionSystemPanel (Image)
   │   ├── ExplosionModeText (TextMeshPro)
   │   └── ExplosionChargesText (TextMeshPro)
   └── TotalTreasureDisplay (TextMeshPro)
   ```

3. **マネージャーにUI要素を設定**
   - TreasureUIManagerの各フィールドに対応するUI要素をドラッグ&ドロップ

## 使用方法

### 基本的な使用方法
1. シーンに両方のマネージャーを配置
2. ゲームを実行
3. お宝を取得すると自動的に通知が表示され、UIが更新される

### カスタマイズ方法

#### メッセージの変更
```csharp
// TreasureNotificationManagerのインスペクターで変更
normalTreasureMessage = "カスタムメッセージ！";
```

#### UIの表示/非表示
```csharp
// コードから制御
TreasureUIManager.Instance.SetUIVisible(false);
TreasureUIManager.Instance.SetExplosionSystemVisible(false);
```

#### 通知の表示
```csharp
// カスタム通知を表示
TreasureNotificationManager.Instance.ShowNotification("カスタム通知！");
```

## 表示位置

### TreasureUIManager
- 画面左上に表示
- 通常強化: 上から10%
- ドリル速度: 上から15%
- 爆発システム: 上から25%
- 総取得数: 上から30%

### TreasureNotificationManager
- 画面上部中央に表示
- フェードイン/アウト付きのポップアップ

## 注意事項

1. **VRDigToolManagerが必要**: お宝の状態を取得するためにVRDigToolManagerが必要です
2. **自動生成**: UIは自動的に生成されますが、手動設定も可能です
3. **シングルトン**: 両方のマネージャーはシングルトンパターンを使用しています
4. **爆発システム**: 初回爆発お宝取得後に爆発システムの表示が開始されます

## トラブルシューティング

### UIが表示されない
- VRDigToolManagerがシーンに存在するか確認
- CanvasのSort Orderが正しく設定されているか確認

### 通知が表示されない
- TreasureNotificationManagerがシーンに存在するか確認
- お宝のタグが"Treasure"に設定されているか確認

### 爆発システムが表示されない
- 爆発お宝を1つ以上取得しているか確認
- `showExplosionSystemAfterFirstGet`がtrueに設定されているか確認






