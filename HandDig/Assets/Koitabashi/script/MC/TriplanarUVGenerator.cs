using UnityEngine;

public static class TriplanarUVGenerator
{
    /// <summary>
    /// トリプラナーマッピング用のUV座標を生成
    /// 各面（XY, XZ, YZ）に対してUV座標を計算
    /// </summary>
    /// <param name="vertex">頂点座標</param>
    /// <param name="normal">法線ベクトル</param>
    /// <param name="uvScale">UVスケール</param>
    /// <returns>トリプラナーマッピング用のUV座標（XY, XZ, YZの順）</returns>
    public static Vector3[] GenerateTriplanarUVs(Vector3 vertex, Vector3 normal, float uvScale = 0.1f)
    {
        Vector3[] triplanarUVs = new Vector3[3];
        
        // XY平面用のUV座標
        triplanarUVs[0] = new Vector3(vertex.x * uvScale, vertex.y * uvScale, 0);
        
        // XZ平面用のUV座標
        triplanarUVs[1] = new Vector3(vertex.x * uvScale, vertex.z * uvScale, 0);
        
        // YZ平面用のUV座標
        triplanarUVs[2] = new Vector3(vertex.y * uvScale, vertex.z * uvScale, 0);
        
        return triplanarUVs;
    }
    
    /// <summary>
    /// 法線ベクトルに基づいてトリプラナーブレンドウェイトを計算
    /// </summary>
    /// <param name="normal">法線ベクトル</param>
    /// <returns>各面のブレンドウェイト（XY, XZ, YZの順）</returns>
    public static Vector3 CalculateBlendWeights(Vector3 normal)
    {
        // 法線の各成分の絶対値を取得
        Vector3 absNormal = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
        
        // 正規化してブレンドウェイトを計算
        float total = absNormal.x + absNormal.y + absNormal.z;
        if (total > 0)
        {
            absNormal /= total;
        }
        
        // XY, XZ, YZの順でウェイトを返す
        return new Vector3(absNormal.z, absNormal.y, absNormal.x);
    }
    
    /// <summary>
    /// 頂点の法線を推定（隣接する頂点から計算）
    /// マーチングキューブ法では正確な法線が得られない場合があるため
    /// </summary>
    /// <param name="vertex">頂点座標</param>
    /// <param name="neighbors">隣接頂点のリスト</param>
    /// <returns>推定された法線ベクトル</returns>
    public static Vector3 EstimateNormal(Vector3 vertex, Vector3[] neighbors)
    {
        if (neighbors.Length < 3)
        {
            // 隣接頂点が不足している場合は、ワールド座標から推定
            return new Vector3(0, 1, 0); // デフォルトは上向き
        }
        
        Vector3 normal = Vector3.zero;
        
        // 隣接頂点から法線を計算
        for (int i = 0; i < neighbors.Length; i++)
        {
            Vector3 v1 = neighbors[i] - vertex;
            Vector3 v2 = neighbors[(i + 1) % neighbors.Length] - vertex;
            normal += Vector3.Cross(v1, v2);
        }
        
        return normal.normalized;
    }
}



