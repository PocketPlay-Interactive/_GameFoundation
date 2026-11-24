using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AddSmartComponentMenu
{
    private const string Root = "GameObject/GameFoundation/Add Component/";

    [MenuItem(Root + "Smart Move", false, 10)]
    private static void AddMove(MenuCommand command) => Add<SmartMove>(command);

    [MenuItem(Root + "Smart Rotation", false, 11)]
    private static void AddRotation(MenuCommand command) => Add<SmartRotation>(command);

    [MenuItem(Root + "Smart Reset Object", false, 12)]
    private static void AddReset(MenuCommand command) => Add<SmartResetObject>(command);

    [MenuItem(Root + "Smart UI Animation", false, 13)]
    private static void AddAnimation(MenuCommand command) => Add<SmartUIAnimation>(command);

    [MenuItem(Root + "Smart Dropdown", false, 14)]
    private static void AddDropdown(MenuCommand command) => Add<SmartDropdown>(command);

    [MenuItem(Root + "Smart Image Effect", false, 15)]
    private static void AddImageEffect(MenuCommand command) => Add<SmartImageEffect>(command);

    [MenuItem(Root + "Smart Typewriter", false, 16)]
    private static void AddTypewriter(MenuCommand command) => Add<SmartTypewriter>(command);

    [MenuItem(Root + "Button Safe", false, 20)]
    private static void AddButtonSafe(MenuCommand command) => Add<ButtonSafe>(command);

    [MenuItem(Root + "Button Sound", false, 21)]
    private static void AddButtonSound(MenuCommand command) => Add<ButtonSound>(command);

    [MenuItem(Root + "Smart Button", false, 22)]
    private static void AddSmartButton(MenuCommand command) => Add<SmartButton>(command);

    [MenuItem(Root + "Smart Move", true)]
    private static bool CanAddMove() => CanAdd<SmartMove>();

    [MenuItem(Root + "Smart Rotation", true)]
    private static bool CanAddRotation() => CanAdd<SmartRotation>();

    [MenuItem(Root + "Smart Reset Object", true)]
    private static bool CanAddReset() => CanAdd<SmartResetObject>();

    [MenuItem(Root + "Smart UI Animation", true)]
    private static bool CanAddAnimation() => CanAdd<SmartUIAnimation>();

    [MenuItem(Root + "Smart Dropdown", true)]
    private static bool CanAddDropdown() => CanAdd<SmartDropdown>() && SelectedHas<RectTransform>();

    [MenuItem(Root + "Smart Image Effect", true)]
    private static bool CanAddImageEffect() => CanAdd<SmartImageEffect>() && SelectedHas<Graphic>();

    [MenuItem(Root + "Smart Typewriter", true)]
    private static bool CanAddTypewriter() => CanAdd<SmartTypewriter>() && SelectedHas<TMP_Text>();

    [MenuItem(Root + "Button Safe", true)]
    private static bool CanAddButtonSafe() => CanAdd<ButtonSafe>() && SelectedHas<Button>();

    [MenuItem(Root + "Button Sound", true)]
    private static bool CanAddButtonSound() => CanAdd<ButtonSound>() && SelectedHas<Button>();

    [MenuItem(Root + "Smart Button", true)]
    private static bool CanAddSmartButton() => CanAdd<SmartButton>() && SelectedHas<Button>();

    private static bool CanAdd<T>() where T : Component =>
        !EditorApplication.isPlayingOrWillChangePlaymode && Selection.activeGameObject != null &&
        Selection.activeGameObject.scene.IsValid() && Selection.activeGameObject.GetComponent<T>() == null;

    private static bool SelectedHas<T>() where T : Component =>
        Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<T>() != null;

    private static void Add<T>(MenuCommand command) where T : Component
    {
        GameObject target = command.context as GameObject ?? Selection.activeGameObject;
        if (target == null || target.GetComponent<T>() != null) return;
        Undo.AddComponent<T>(target);
        Selection.activeGameObject = target;
    }
}
