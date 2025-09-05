# 掘削音声システム 設定ガイド

## 概要
このシステムは、つるはし、ドリル、手掘り、およびつるはしの爆発採掘それぞれに異なる音を再生する機能を提供します。

## 実装された機能

### 1. 音声の種類
- **つるはし**: 通常掘削音 + コンボ段階別の音（3段階）
- **ドリル**: 連続掘削音
- **手掘り**: 手掘り専用音
- **つるはし爆発**: マーカー設置音 + 爆発音

### 2. 音声管理システム
- 3D音声対応（位置に応じた音量変化）
- ピッチ変動機能（音の変化）
- 音量調整機能
- UIによる音量制御

## 設定手順

### Step 1: DigSoundManagerの設定

1. **空のGameObjectを作成**
   - 名前: "DigSoundManager"
   - 位置: シーンの適当な場所

2. **DigSoundManagerコンポーネントを追加**
   - 作成したGameObjectに`DigSoundManager.cs`をアタッチ

3. **音声ファイルを設定**
   ```
   [つるはしの音]
   - Pickaxe Dig Sound: 通常掘削音
   - Pickaxe Combo1 Sound: コンボ段階1の音
   - Pickaxe Combo2 Sound: コンボ段階2の音
   - Pickaxe Combo3 Sound: コンボ段階3の音
   - Pickaxe Explosion Marker Sound: 爆発マーカー設置音
   - Pickaxe Explosion Sound: 爆発音

   [ドリルの音]
   - Drill Dig Sound: ドリル掘削音

   [手掘りの音]
   - Hand Dig Sound: 手掘り音
   ```

4. **音声設定を調整**
   - Volume: 音量（0-1）
   - Pitch Variation: ピッチ変動範囲（0-0.5）

### Step 2: VRDigToolManagerの設定

1. **VRDigToolManagerを開く**
   - 既存のVRDigToolManagerオブジェクトを選択

2. **DigSoundManagerを参照設定**
   - DigSoundManagerフィールドに、Step 1で作成したDigSoundManagerをドラッグ&ドロップ

### Step 3: UI設定（オプション）

1. **Canvasを作成**
   - UI > Canvas で新しいCanvasを作成

2. **音声設定UIを作成**
   ```
   Canvas
   └── SoundSettingsPanel (Panel)
       ├── VolumeSlider (Slider)
       ├── VolumeText (Text)
       └── SoundToggle (Toggle)
   ```

3. **DigSoundUIコンポーネントを追加**
   - SoundSettingsPanelに`DigSoundUI.cs`をアタッチ

4. **UI要素を参照設定**
   - Volume Slider: 音量スライダー
   - Volume Text: 音量表示テキスト
   - Sound Toggle: 音声ON/OFFトグル

## 使用方法

### 基本操作
- **Mキー**: 音声設定UIの表示/非表示切り替え
- **スライダー**: 音量調整（0-100%）
- **トグル**: 音声ON/OFF

### 音声の特徴
- **3D音声**: 掘削位置から音が再生される
- **ピッチ変動**: 同じ音でも少しずつ異なるピッチで再生
- **自動再生**: 掘削時に自動で音が再生される

## 推奨音声ファイル

### つるはし
- **通常掘削**: 金属的な打撃音
- **コンボ音**: 段階に応じて重厚感を増す音
- **爆発マーカー**: 設置音（軽い音）
- **爆発音**: 大きな爆発音

### ドリル
- **掘削音**: 連続的な機械音

### 手掘り
- **掘削音**: 土を掘る音

## トラブルシューティング

### 音が鳴らない場合
1. DigSoundManagerが正しく設定されているか確認
2. 音声ファイルがアタッチされているか確認
3. 音量が0になっていないか確認
4. AudioSourceコンポーネントが正常に動作しているか確認

### 音が重複する場合
- 一時的なAudioSourceが自動で削除されるため、通常は問題ありません
- パフォーマンスに問題がある場合は、`voxelsPerFrame`を調整してください

### 音の遅延がある場合
- 音声ファイルのサイズを確認
- 音声ファイルを適切な形式（.wav, .mp3）に変換
- 音声ファイルの圧縮設定を調整

## カスタマイズ

### 新しい音を追加
1. DigSoundManagerに新しいAudioClipフィールドを追加
2. 対応する再生メソッドを作成
3. 該当するツールクラスで音を再生するコードを追加

### 音の効果を調整
- `pitchVariation`: ピッチ変動の範囲を調整
- `volume`: デフォルト音量を調整
- AudioSourceの設定: 3D音声の距離減衰を調整

## パフォーマンス最適化

- 音声ファイルは適切なサイズに圧縮
- 同時再生数を制限（必要に応じて）
- 距離に応じた音の再生制御（必要に応じて実装）

