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

        EditorGUILayout.LabelField("�y�����T�C�Y�z", EditorStyles.boldLabel);
        stats.baseRadius = EditorGUILayout.FloatField("�x�[�X", stats.baseRadius);
        stats.stage2Radius = EditorGUILayout.FloatField("��͂�2�i�K��", stats.stage2Radius);
        stats.stage3Radius = EditorGUILayout.FloatField("��͂�3�i�K��", stats.stage3Radius);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("�y�ő勭���T�C�Y�z", EditorStyles.boldLabel);
        stats.baseRadiusMax = EditorGUILayout.FloatField("�x�[�X Max", stats.baseRadiusMax);
        stats.stage2RadiusMax = EditorGUILayout.FloatField("2�i�K�� Max", stats.stage2RadiusMax);
        stats.stage3RadiusMax = EditorGUILayout.FloatField("3�i�K�� Max", stats.stage3RadiusMax);

        EditorGUILayout.Space();

        stats.upgradeSteps = EditorGUILayout.IntSlider("�����i�K��", stats.upgradeSteps, 1, 10);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("�y�i�K���Ƃ̑����ʁz", EditorStyles.boldLabel);
        if (stats.upgradeSteps > 1)
        {
            float baseStep = (stats.baseRadiusMax - stats.baseRadius) / (stats.upgradeSteps - 1);
            float stage2Step = (stats.stage2RadiusMax - stats.stage2Radius) / (stats.upgradeSteps - 1);
            float stage3Step = (stats.stage3RadiusMax - stats.stage3Radius) / (stats.upgradeSteps - 1);

            EditorGUILayout.LabelField($"�x�[�X: +{baseStep:F3} / �i�K");
            EditorGUILayout.LabelField($"2�i�K��: +{stage2Step:F3} / �i�K");
            EditorGUILayout.LabelField($"3�i�K��: +{stage3Step:F3} / �i�K");
        }
        else
        {
            EditorGUILayout.HelpBox("�����i�K��1�̂��߁A�����ʂ͂���܂���B", MessageType.Info);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(stats);
        }

        serializedObject.ApplyModifiedProperties();
    }
}