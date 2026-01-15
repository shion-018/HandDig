#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneLoader))]
public class SceneLoaderEditor : Editor
{
    private SerializedProperty _overlayBackgroundProp;
    private SerializedProperty _overlayLoadingTextProp;
    private SerializedProperty _autoLoadOnStartProp;
    private SerializedProperty _loadingDelayMillisecondProp;

    private void OnEnable()
    {
        _overlayBackgroundProp = serializedObject.FindProperty("_overlay_Background");
        _overlayLoadingTextProp = serializedObject.FindProperty("_overlay_LoadingText");
        _autoLoadOnStartProp = serializedObject.FindProperty("_autoLoadOnStart");
        _loadingDelayMillisecondProp = serializedObject.FindProperty("_loadingDelayMillisecond");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Overlay設定", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_overlayBackgroundProp, new GUIContent("背景用Overlay"));
        EditorGUILayout.PropertyField(_overlayLoadingTextProp, new GUIContent("ローディングテキスト用Overlay"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("初期化設定", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(_autoLoadOnStartProp, new GUIContent("起動時に自動初期化"));
        EditorGUILayout.PropertyField(_loadingDelayMillisecondProp, new GUIContent("ローディング遅延時間（ミリ秒）"));
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("このスクリプトは、同じシーン内でオーバーレイを表示しながら、MC_Worldなどのオブジェクトの初期化を待機します。", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
