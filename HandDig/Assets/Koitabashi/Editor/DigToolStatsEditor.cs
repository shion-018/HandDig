using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DigToolStats))]
public class DigToolStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var stats = (DigToolStats)target;

        EditorGUILayout.LabelField("y‰ŠúƒTƒCƒYz", EditorStyles.boldLabel);
        stats.baseRadius = EditorGUILayout.FloatField("ƒx[ƒX", stats.baseRadius);
        stats.stage2Radius = EditorGUILayout.FloatField("‚Â‚é‚Í‚µ2’iŠK–Ú", stats.stage2Radius);
        stats.stage3Radius = EditorGUILayout.FloatField("‚Â‚é‚Í‚µ3’iŠK–Ú", stats.stage3Radius);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("yÅ‘å‹­‰»ƒTƒCƒYz", EditorStyles.boldLabel);
        stats.baseRadiusMax = EditorGUILayout.FloatField("ƒx[ƒX Max", stats.baseRadiusMax);
        stats.stage2RadiusMax = EditorGUILayout.FloatField("2’iŠK–Ú Max", stats.stage2RadiusMax);
        stats.stage3RadiusMax = EditorGUILayout.FloatField("3’iŠK–Ú Max", stats.stage3RadiusMax);

        EditorGUILayout.Space();

        stats.upgradeSteps = EditorGUILayout.IntSlider("‹­‰»’iŠK”", stats.upgradeSteps, 1, 10);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("y’iŠK‚²‚Æ‚Ì‘‰Á—Êz", EditorStyles.boldLabel);
        if (stats.upgradeSteps > 1)
        {
            float baseStep = (stats.baseRadiusMax - stats.baseRadius) / (stats.upgradeSteps - 1);
            float stage2Step = (stats.stage2RadiusMax - stats.stage2Radius) / (stats.upgradeSteps - 1);
            float stage3Step = (stats.stage3RadiusMax - stats.stage3Radius) / (stats.upgradeSteps - 1);

            EditorGUILayout.LabelField($"ƒx[ƒX: +{baseStep:F3} / ’iŠK");
            EditorGUILayout.LabelField($"2’iŠK–Ú: +{stage2Step:F3} / ’iŠK");
            EditorGUILayout.LabelField($"3’iŠK–Ú: +{stage3Step:F3} / ’iŠK");
        }
        else
        {
            EditorGUILayout.HelpBox("‹­‰»’iŠK‚ª1‚Ì‚½‚ßA‘‰Á—Ê‚Í‚ ‚è‚Ü‚¹‚ñB", MessageType.Info);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(stats);
        }

        serializedObject.ApplyModifiedProperties();
    }
}