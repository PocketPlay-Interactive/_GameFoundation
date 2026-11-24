---
name: gf-extensions
description: Catalog of every Game Foundation (GF) extension method (GameObject/Transform show-hide, hierarchy finders, Vector/Quaternion/Color helpers, array/list/LINQ helpers, number/string formatting, RectTransform helpers). Use this skill BEFORE writing or editing any Unity C# that touches GameObjects, Transforms, vectors, collections, random picks, number or time formatting, or UI layout in a project that references GF, and whenever you are about to hand-write something like SetActive, Random.Range over a list, new Vector3 with one changed component, child lookups by name, or "1.2K"-style number text. Reach for it even when the user does not mention extensions, so existing helpers are reused instead of duplicated.
---

# GF extension methods

GF ships 180+ public static helpers (mostly extension methods) in `Runtime/Core/Utilities/Extensions/` (global namespace, no `using` needed).
Hand-writing the same logic again splits behaviour across the codebase and skips the null-safety these helpers add.
So: look here first, then write new code only if nothing fits.

## Workflow

1. Before writing code that does one of the tasks in the table below, check whether a helper exists.
2. For the full signature list, read [references/extensions.md](references/extensions.md). Read only the section you need; it is grouped by topic.
3. If the source and this catalog disagree, the source wins: open the file named in the section heading. The catalog is generated from the source, so refresh it (see "Keeping this catalog current") instead of trusting a stale entry.
4. Prefer the exact receiver type. `Show()` exists for `GameObject`, `Transform` and `Component`; you rarely need `.gameObject` first.

## Quick map: "I was about to write ..." -> use

| Instead of writing | Use |
| --- | --- |
| `go.SetActive(true/false)` with a null check | `go.Show()`, `go.Hide()`, `go.SetActiveSafe(bool)` (warns on null; `isConcurrent: true` queues to the main thread) |
| `go.activeInHierarchy` with a null check | `go.IsActive()` |
| `Random.Range(0, list.Count)` then index | `list.GetRandomElement()` (array and `IList`), `enumerable.GetRandom()`, `GetRandom(count)` |
| a Fisher-Yates loop | `list.Shuffle()` |
| `transform.Find` recursion / child loops | `FindChildRecursive(name)`, `FindDirectChild(name)`, `HasDirectChild(name)`, `FindDirectChildren(name)` |
| `foreach (Transform c in t)` with an action | `t.ForChild(...)`, `t.ForChildReverse(...)`, `t.GetChildren()` |
| `new Vector3(v.x, newY, v.z)` | `v.WithY(newY)`; also `WithX`, `WithZ`, `AddX/AddY/AddZ` |
| `new Vector3(x, y, z)` | `VectorExtensions.Create3D(x, y, z)` (same for `Create`, `Create2D`) |
| `color.a = a` copy | `color.WithAlpha(a)`, `color.ToHex()`, `"66331A".ToColor()` |
| manual "K/M/B" number text | `n.ToAbbreviatedString()` (int, `BigInteger`), `"123456789".ToAbbreviatedNumberString()` |
| `mm:ss` formatting | `seconds.ToMinuteSecondString()` (int or float; negatives and NaN become `00:00`) |
| remap / lerp / clamp one-liners | `value.Remap(a, b, c, d)`, `a.Lerp(b, t)`, `i.Clamp(min, max)`, `range.Clamp(v)` |
| appending to an array | `array.Append(x)`, `AddRange`, `AddIfMissing`, `ContainsItem` |
| rotating an array | `MoveFirstToLast()`, `MoveLastToFirst()` (mutate in place) |
| `"Foo Bar"` from `"FooBar"` | `s.AddSpaceBeforeUppercase()`; `s.ToTitleCase()`; `s.ToEnum(default)` |
| `RectTransform` anchor/pivot/size code | `rt.SetAnchor(min, max)`, `rt.SetPivot(p)`, `rt.SetSize(size)` |
| tweening a number or slider | `tmp.DONumber(to, duration, ...)`, `slider.DOSlider(to, duration)` |

## Behaviour that surprises people

- **Null handling differs on purpose.** `Show`/`Hide`/`SetActiveSafe` log a warning on a null receiver and return. `FindChildRecursive`, `FindDirectChild` and most finders return `null`. `GetRandom` returns `default`. Do not add your own null guard before calling them.
- **`T[]` helpers return a new array** (`Append`, `AddRange`, `AddIfMissing`) and leave the input alone, except `MoveFirstToLast`/`MoveLastToFirst`, which rotate in place. `AddIfMissing` returns the same array when the item is already there.
- **`ForEach` on `T[]` skips null elements; `ForEach` on `IEnumerable<T>` does not.** Pick based on whether null items may exist.
- **`GetRandom(count)` throws `InvalidOperationException` immediately** when the sequence has fewer than `count` items.
- **Vector helpers return copies.** `v.WithX(1)` does not change `v`; assign the result.
- **Quaternion `WithX/WithY/WithZ/AddX...` round-trip through Euler angles.** They are fine for simple rotations but not for composing arbitrary orientations.
- **`isConcurrent: true`** needs a `Concurrency` object in the scene, otherwise `Concurrency.Instance()` throws.

## Deprecated: do not use in new code

These compile with an `[Obsolete]` warning and exist only for old call sites:

| Deprecated | Use |
| --- | --- |
| `GetChilds()` | `GetChildren()` |
| `GetChildTransform(i)` | `GetChildAt(i)` |
| `GetRandomOrDefault()` | `GetRandom()` |
| `SimpleForEach(...)` | `ForEach(...)` |

## When to add a new extension

Add one only when the same non-trivial snippet would be written in a third place, or when it must be null-safe in a consistent way. Put it in the file whose topic fits (see the table in the reference), keep the overload set small (one core on `Transform`, thin `GameObject`/`Component` forwarders), and add a line to the catalog. Do not add an overload that duplicates a built-in Unity or .NET method.

## Keeping this catalog current

`references/extensions.md` is generated from the source. After adding, renaming or removing an extension, regenerate it with `python .claude/skills/gf-extensions/scripts/generate_catalog.py` and review the diff. This keeps the skill from drifting out of date, which is the main way a reference like this becomes harmful.
