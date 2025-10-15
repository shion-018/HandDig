Shader "Custom/TriplanarMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 1.0
        _BlendSharpness ("Blend Sharpness", Range(1, 10)) = 2.0
        _Color ("Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        float _Tiling;
        float _BlendSharpness;
        fixed4 _Color;
        
        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // ワールド座標をトリプラナーマッピング用に変換
            float3 worldPos = IN.worldPos * _Tiling;
            
            // 各面のUV座標を計算
            float2 uvX = worldPos.yz; // X軸に垂直な面（YZ平面）
            float2 uvY = worldPos.xz; // Y軸に垂直な面（XZ平面）
            float2 uvZ = worldPos.xy; // Z軸に垂直な面（XY平面）
            
            // 各面のテクスチャをサンプリング
            fixed4 texX = tex2D(_MainTex, uvX);
            fixed4 texY = tex2D(_MainTex, uvY);
            fixed4 texZ = tex2D(_MainTex, uvZ);
            
            // 法線ベクトルの各成分の絶対値を取得
            float3 blendWeights = abs(IN.worldNormal);
            
            // ブレンドウェイトを正規化
            blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
            
            // ブレンドの鋭さを調整
            blendWeights = pow(blendWeights, _BlendSharpness);
            blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
            
            // トリプラナーブレンド
            fixed4 finalColor = texX * blendWeights.x + texY * blendWeights.y + texZ * blendWeights.z;
            
            o.Albedo = finalColor.rgb * _Color.rgb;
            o.Alpha = finalColor.a * _Color.a;
        }
        ENDCG
    }
    
    FallBack "Diffuse"
}










