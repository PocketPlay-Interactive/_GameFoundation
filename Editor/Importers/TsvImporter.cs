using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "tsv")]
public class TsvImporter : ScriptedImporter
{
    private static readonly Encoding TsvEncoding = new UTF8Encoding(false, true);

    public override void OnImportAsset(AssetImportContext ctx)
    {
        string text;

        try
        {
            text = File.ReadAllText(ctx.assetPath, TsvEncoding);
        }
        catch (IOException exception)
        {
            ctx.LogImportError($"Failed to read TSV file '{ctx.assetPath}': {exception.Message}");
            return;
        }
        catch (DecoderFallbackException exception)
        {
            ctx.LogImportError($"TSV file '{ctx.assetPath}' must be valid UTF-8 text: {exception.Message}");
            return;
        }

        TextAsset textAsset = new TextAsset(text)
        {
            name = Path.GetFileNameWithoutExtension(ctx.assetPath)
        };

        ctx.AddObjectToAsset("main", textAsset);
        ctx.SetMainObject(textAsset);
    }
}
