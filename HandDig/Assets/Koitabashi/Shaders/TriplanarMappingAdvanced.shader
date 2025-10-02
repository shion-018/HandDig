Shader "Custom/TriplanarMappingAdvanced"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Tiling ("Tiling", Float) = 1.0
        _BlendSharpness ("Blend Sharpness", Range(1, 10)) = 2.0
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        float _Tiling;
        float _BlendSharpness;
        float _NormalStrength;
        fixed4 _Color;
        half _Metallic;
        half _Smoothness;
        
        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
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
            
            // 各面の法線マップをサンプリング
            fixed3 normalX = UnpackNormal(tex2D(_NormalMap, uvX));
            fixed3 normalY = UnpackNormal(tex2D(_NormalMap, uvY));
            fixed3 normalZ = UnpackNormal(tex2D(_NormalMap, uvZ));
            
            // 法線ベクトルの各成分の絶対値を取得
            float3 blendWeights = abs(IN.worldNormal);
            
            // ブレンドウェイトを正規化
            blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
            
            // ブレンドの鋭さを調整
            blendWeights = pow(blendWeights, _BlendSharpness);
            blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
            
            // トリプラナーブレンド（アルベド）
            fixed4 finalColor = texX * blendWeights.x + texY * blendWeights.y + texZ * blendWeights.z;
            
            // トリプラナーブレンド（法線）
            // 各面の法線をワールド空間に変換
            float3 worldNormalX = normalize(float3(0, normalX.y, normalX.z));
            float3 worldNormalY = normalize(float3(normalY.x, 0, normalY.z));
            float3 worldNormalZ = normalize(float3(normalZ.x, normalZ.y, 0));
            
            float3 blendedNormal = worldNormalX * blendWeights.x + 
                                  worldNormalY * blendWeights.y + 
                                  worldNormalZ * blendWeights.z;
            
            o.Albedo = finalColor.rgb * _Color.rgb;
            o.Normal = normalize(blendedNormal) * _NormalStrength;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = finalColor.a * _Color.a;
        }
        ENDCG
    }
    
    FallBack "Diffuse"
}






