using UnityEditor;

[CustomEditor(typeof(SmartImageEffect))]
public class SmartImageEffectEditor : Editor
{
    private SerializedProperty effect;
    private SerializedProperty shineColor;
    private SerializedProperty shineWidth;
    private SerializedProperty shineAngle;
    private SerializedProperty shineSpeed;
    private SerializedProperty tilingScale;
    private SerializedProperty tilingSpeed;

    private void OnEnable()
    {
        effect = serializedObject.FindProperty("effect");
        shineColor = serializedObject.FindProperty("shineColor");
        shineWidth = serializedObject.FindProperty("shineWidth");
        shineAngle = serializedObject.FindProperty("shineAngle");
        shineSpeed = serializedObject.FindProperty("shineSpeed");
        tilingScale = serializedObject.FindProperty("tilingScale");
        tilingSpeed = serializedObject.FindProperty("tilingSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(effect);
        EditorGUILayout.Space(4f);

        switch (effect.enumValueIndex)
        {
            case 1:
                EditorGUILayout.PropertyField(shineColor);
                EditorGUILayout.PropertyField(shineWidth);
                EditorGUILayout.PropertyField(shineAngle);
                EditorGUILayout.PropertyField(shineSpeed);
                break;
            case 2:
                EditorGUILayout.PropertyField(tilingScale);
                EditorGUILayout.PropertyField(tilingSpeed);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
