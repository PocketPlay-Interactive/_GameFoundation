using System.Collections.Generic;
using System.Text;
using Pathfinding;
using UnityEditor;
using UnityEngine;

public class AStarPathFinderObjectMenu : EditorWindow
{
    private const float WindowWidth = 560f;
    private const float WindowHeight = 460f;
    private const float SelectionListHeight = 190f;

    private Vector2 scrollPosition;
    private string resultMessage;
    private MessageType resultMessageType = MessageType.Info;

    [MenuItem("Pathfinding/Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<AStarPathFinderObjectMenu>();
        window.titleContent = new GUIContent("Pathfinding Connector");
        window.minSize = new Vector2(WindowWidth, WindowHeight);
        window.Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(WindowWidth, WindowHeight);
    }

    private void OnSelectionChange()
    {
        resultMessage = string.Empty;
        Repaint();
    }

    private void OnGUI()
    {
        var selectedObjects = Selection.gameObjects;
        var validNodes = new List<PointNode>();
        var invalidObjects = new List<GameObject>();
        CollectSelection(selectedObjects, validNodes, invalidObjects);

        DrawHeader();

        EditorGUILayout.Space(10f);
        DrawSelectionSummary(selectedObjects.Length, validNodes.Count, invalidObjects.Count);

        EditorGUILayout.Space(8f);
        DrawSelectionList(selectedObjects);

        EditorGUILayout.Space(8f);
        DrawValidationMessage(selectedObjects, validNodes, invalidObjects);

        if (!string.IsNullOrEmpty(resultMessage))
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(resultMessage, resultMessageType);
        }

        GUILayout.FlexibleSpace();
        DrawActions(selectedObjects, validNodes, invalidObjects);
    }

    private static void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("POINT NODE CONNECTOR", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Chọn từ 2 GameObject có component PointNode để tạo kết nối hai chiều giữa tất cả node đã chọn.",
            EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
    }

    private static void DrawSelectionSummary(int selectedCount, int validCount, int invalidCount)
    {
        EditorGUILayout.BeginHorizontal();
        DrawStat("Đã chọn", selectedCount.ToString());
        DrawStat("PointNode hợp lệ", validCount.ToString());
        DrawStat("Không hợp lệ", invalidCount.ToString());
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawStat(string label, string value)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(48f));
        GUILayout.Label(value, new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15
        });
        GUILayout.Label(label, new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        });
        EditorGUILayout.EndVertical();
    }

    private void DrawSelectionList(GameObject[] selectedObjects)
    {
        GUILayout.Label("Danh sách lựa chọn", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition,
            EditorStyles.helpBox,
            GUILayout.Height(SelectionListHeight));

        if (selectedObjects.Length == 0)
        {
            EditorGUILayout.LabelField("Chưa có GameObject nào được chọn.", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            foreach (var selectedObject in selectedObjects)
            {
                var hasPointNode = selectedObject.TryGetComponent<PointNode>(out _);

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(selectedObject, typeof(GameObject), true);
                }
                GUILayout.Label(
                    hasPointNode ? "Hợp lệ" : "Thiếu PointNode",
                    hasPointNode ? EditorStyles.miniLabel : EditorStyles.boldLabel,
                    GUILayout.Width(105f));
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private static void DrawValidationMessage(
        GameObject[] selectedObjects,
        List<PointNode> validNodes,
        List<GameObject> invalidObjects)
    {
        if (selectedObjects.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "Không thể kết nối: chưa chọn GameObject nào. Hãy chọn ít nhất 2 GameObject trong Hierarchy hoặc Scene.",
                MessageType.Info);
            return;
        }

        if (selectedObjects.Length == 1)
        {
            EditorGUILayout.HelpBox(
                $"Không thể kết nối: mới chọn 1 GameObject ('{selectedObjects[0].name}'). Cần chọn ít nhất 2 GameObject có PointNode.",
                MessageType.Warning);
            return;
        }

        if (invalidObjects.Count > 0)
        {
            EditorGUILayout.HelpBox(BuildInvalidSelectionMessage(invalidObjects), MessageType.Error);
            return;
        }

        var pairCount = validNodes.Count * (validNodes.Count - 1) / 2;
        EditorGUILayout.HelpBox(
            $"Sẵn sàng tạo tối đa {pairCount} cặp kết nối hai chiều. Các kết nối đã tồn tại sẽ được giữ nguyên.",
            MessageType.Info);
    }

    private void DrawActions(
        GameObject[] selectedObjects,
        List<PointNode> validNodes,
        List<GameObject> invalidObjects)
    {
        var canConnect = selectedObjects.Length >= 2
            && validNodes.Count >= 2
            && invalidObjects.Count == 0;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Chọn lại", GUILayout.Height(34f), GUILayout.Width(100f)))
        {
            Selection.objects = new Object[0];
        }

        using (new EditorGUI.DisabledScope(!canConnect))
        {
            if (GUILayout.Button("Kết nối các PointNode", GUILayout.Height(34f)))
            {
                ConnectNodes(validNodes);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ConnectNodes(List<PointNode> nodes)
    {
        if (nodes == null || nodes.Count < 2)
        {
            SetResult(
                "Kết nối thất bại: danh sách node hợp lệ không đủ 2 phần tử. Hãy kiểm tra lại lựa chọn.",
                MessageType.Error,
                true);
            return;
        }

        Undo.RecordObjects(nodes.ToArray(), "Connect Point Nodes");

        var connectionsAdded = 0;
        var existingConnections = 0;

        foreach (var node in nodes)
        {
            if (node.neighbors == null)
            {
                node.neighbors = new List<PointNode>();
            }
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            for (var j = i + 1; j < nodes.Count; j++)
            {
                AddNeighbor(nodes[i], nodes[j], ref connectionsAdded, ref existingConnections);
                AddNeighbor(nodes[j], nodes[i], ref connectionsAdded, ref existingConnections);
            }

            EditorUtility.SetDirty(nodes[i]);
        }

        var pairCount = nodes.Count * (nodes.Count - 1) / 2;
        SetResult(
            $"Kết nối hoàn tất cho {nodes.Count} node ({pairCount} cặp): "
            + $"đã thêm {connectionsAdded} liên kết một chiều, bỏ qua {existingConnections} liên kết đã tồn tại. "
            + "Có thể dùng Undo để hoàn tác.",
            MessageType.Info,
            false);
    }

    private static void AddNeighbor(
        PointNode source,
        PointNode target,
        ref int connectionsAdded,
        ref int existingConnections)
    {
        if (source.neighbors.Contains(target))
        {
            existingConnections++;
            return;
        }

        source.neighbors.Add(target);
        connectionsAdded++;
    }

    private void SetResult(string message, MessageType messageType, bool logAsError)
    {
        resultMessage = message;
        resultMessageType = messageType;

        if (logAsError)
        {
            Debug.LogError($"[Pathfinding Connector] {message}");
        }
        else
        {
            Debug.Log($"[Pathfinding Connector] {message}");
        }
    }

    private static void CollectSelection(
        GameObject[] selectedObjects,
        List<PointNode> validNodes,
        List<GameObject> invalidObjects)
    {
        foreach (var selectedObject in selectedObjects)
        {
            if (selectedObject.TryGetComponent<PointNode>(out var pointNode))
            {
                validNodes.Add(pointNode);
            }
            else
            {
                invalidObjects.Add(selectedObject);
            }
        }
    }

    private static string BuildInvalidSelectionMessage(List<GameObject> invalidObjects)
    {
        var message = new StringBuilder();
        message.Append("Không thể kết nối vì ")
            .Append(invalidObjects.Count)
            .Append(" GameObject sau không có component PointNode:");

        foreach (var invalidObject in invalidObjects)
        {
            message.Append("\n• ").Append(GetHierarchyPath(invalidObject.transform));
        }

        message.Append("\nHãy thêm PointNode hoặc bỏ các GameObject này khỏi lựa chọn rồi thử lại.");
        return message.ToString();
    }

    private static string GetHierarchyPath(Transform target)
    {
        var path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}
