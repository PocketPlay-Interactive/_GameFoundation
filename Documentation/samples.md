# Samples

Source samples are kept under `Samples`. Binary package builds copy their README files and code into `Documentation/Samples`; code uses `.cs.txt` so Unity cannot compile it.

## EditorCools

Scene: `Samples/EditorCools/Scenes/Demo.unity`

Shows inspector helpers:

- dropdown values from a method.
- min/max slider.
- inspector buttons.
- grouped button rows.

## ExtensionData

Scene: `Samples/ExtensionData/Scenes/Demo.unity`

Shows `RuntimeStorageData.PLAYER.ExtensionData` with:

- string, int, float and bool values.
- JSON data through `JsonUtility`.
- save/load with `RuntimeStorageData.SaveAllData()`.

Run the scene and read the Console logs.
