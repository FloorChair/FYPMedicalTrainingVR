using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ToolScoring))]
public class ToolScoringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ToolScoring ts = (ToolScoring)target;
        serializedObject.Update();

        // --- Time ---
        EditorGUILayout.PropertyField(serializedObject.FindProperty("scoreTime"));
        if (ts.scoreTime)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAllowedTime"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // --- Steadiness ---
        EditorGUILayout.PropertyField(serializedObject.FindProperty("scoreSteadiness"));
        if (ts.scoreSteadiness)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("needleLocalDirection"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAllowedDeviation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deadzone"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // --- Accuracy ---
        EditorGUILayout.PropertyField(serializedObject.FindProperty("scoreAccuracy"));
        if (ts.scoreAccuracy)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accuracyMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetGuide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toolTip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAllowedDistance"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // --- Conservation ---
        EditorGUILayout.PropertyField(serializedObject.FindProperty("scoreConservation"));
        if (ts.scoreConservation)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idealActionCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxExtraActions"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // --- UI ---
        EditorGUILayout.LabelField("Results UI", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resultsText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepLabel"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("procedureGenerator"));

        serializedObject.ApplyModifiedProperties();
    }
}