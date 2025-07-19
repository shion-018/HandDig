# パーティクルシステム トラブルシューティングガイド

## ピンクのエフェクトが表示される問題

### 原因
ピンクのエフェクトが表示される主な原因は以下の通りです：

1. **シェーダーが見つからない**: マテリアルが参照しているシェーダーが存在しない
2. **lilToonシェーダーの問題**: lilToonシェーダーがパーティクルシステムと互換性がない
3. **マテリアルの設定不備**: 透明度やブレンドモードの設定が正しくない

### 解決方法

#### 1. 自動修正（推奨）
`ParticleMaterialFixer`スクリプトを使用：

1. パーティクルプレハブに`ParticleMaterialFixer`スクリプトをアタッチ
2. `autoFixOnStart`を有効にする
3. プレイモードで自動的にマテリアルが修正される

#### 2. 手動修正

##### パーティクルプレハブの修正
1. パーティクルプレハブを開く
2. `ParticleSystemRenderer`コンポーネントを選択
3. マテリアルを以下のいずれかに変更：
   - `Particles/Standard Unlit`
   - `Standard`（フォールバック）

##### マテリアルの設定
1. マテリアルを選択
2. シェーダーを`Particles/Standard Unlit`に変更
3. レンダリングモードを`Transparent`に設定
4. ブレンドモードを`Alpha Blend`に設定

#### 3. コードからの修正

```csharp
// パーティクルシステムのマテリアルを修正
ParticleSystem ps = GetComponent<ParticleSystem>();
var renderer = ps.GetComponent<ParticleSystemRenderer>();
if (renderer != null)
{
    Material material = new Material(Shader.Find("Particles/Standard Unlit"));
    if (material.shader == null)
    {
        material = new Material(Shader.Find("Standard"));
    }
    
    // 透明度設定
    material.SetFloat("_Mode", 3);
    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    material.SetInt("_ZWrite", 0);
    material.DisableKeyword("_ALPHATEST_ON");
    material.EnableKeyword("_ALPHABLEND_ON");
    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    material.renderQueue = 3000;
    
    renderer.material = material;
}
```

### デバッグ方法

#### 1. マテリアル情報の確認
```csharp
// マテリアル情報をログ出力
var renderer = GetComponent<ParticleSystemRenderer>();
if (renderer != null && renderer.material != null)
{
    Debug.Log($"シェーダー: {renderer.material.shader?.name}");
    Debug.Log($"色: {renderer.material.color}");
    Debug.Log($"レンダリングモード: {renderer.material.GetFloat("_Mode")}");
}
```

#### 2. シェーダーの存在確認
```csharp
// 利用可能なシェーダーを確認
Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
foreach (var shader in shaders)
{
    if (shader.name.Contains("Particle"))
    {
        Debug.Log($"パーティクルシェーダー: {shader.name}");
    }
}
```

### よくある問題と解決策

#### 問題1: lilToonシェーダーが原因
**解決策**: 
- パーティクルシステムにはlilToonシェーダーを使用しない
- `Particles/Standard Unlit`または`Standard`シェーダーを使用

#### 問題2: マテリアルが共有されている
**解決策**:
```csharp
// マテリアルのインスタンスを作成
Material newMaterial = new Material(originalMaterial);
renderer.material = newMaterial;
```

#### 問題3: シェーダーが見つからない
**解決策**:
```csharp
// フォールバックシェーダーを使用
Material material = new Material(Shader.Find("Particles/Standard Unlit"));
if (material.shader == null)
{
    material = new Material(Shader.Find("Standard"));
}
```

### 推奨設定

#### パーティクルシステム用マテリアル設定
```
シェーダー: Particles/Standard Unlit
レンダリングモード: Transparent
ブレンドモード: Alpha Blend
ZWrite: Off
RenderQueue: 3000
```

#### パーティクルシステム設定
```
Start Color: 希望の色
Color over Lifetime: 有効
Size over Lifetime: 有効
```

### 予防策

1. **プレハブ作成時**: 最初から正しいシェーダーを使用
2. **プロジェクト設定**: パーティクル用のマテリアルテンプレートを作成
3. **コード修正**: `DigEffectManager`で自動的にマテリアルを修正

### トラブルシューティングチェックリスト

- [ ] パーティクルシステムのマテリアルが正しいシェーダーを使用しているか
- [ ] マテリアルの透明度設定が正しいか
- [ ] lilToonシェーダーを使用していないか
- [ ] マテリアルが共有されていないか
- [ ] レンダリングキューが適切に設定されているか

### 追加のデバッグ情報

問題が解決しない場合は、以下の情報を確認してください：

1. **コンソールエラー**: シェーダー関連のエラーメッセージ
2. **マテリアル設定**: インスペクターでのマテリアル設定
3. **プロジェクト設定**: グラフィックスAPIの設定
4. **Unity バージョン**: 使用しているUnityのバージョン 