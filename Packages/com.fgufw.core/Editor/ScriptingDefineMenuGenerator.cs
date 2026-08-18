#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace FGUFW.Editor
{
    public static class ScriptingDefineMenuGenerator
    {
        [MenuItem("Assets/Create/FGUFW/Scripting Define Menu", false, 80)]
        private static void Create()
        {
            var createPath = EditorUtil.GetSeleceFolderPath() + "/SDS.cs";
            var createScriptHelper = ScriptableObject.CreateInstance<CreateScriptHelper>();
            createScriptHelper.Callback = filePath =>
            {
                var scriptText =
@"using System.Linq;
using UnityEditor;
using FGUFW;
using FGUFW.Editor;

public static class |CLASS_NAME|
{
    private const string sdsMenu = ""Conditional/|SDS_ID|"";
    private const string sdsId = ""|SDS_ID|"";

    [MenuItem(sdsMenu)]
    public static void Toggle()
    {
        var enabled = Menu.GetChecked(sdsMenu);
        var defines = EditorUtil.GetScriptingDefineSymbols().ToList();

        if (enabled)
        {
            defines.Remove(sdsId);
        }
        else
        {
            defines.Add(sdsId);
        }

        EditorUtil.SetScriptingDefineSymbols(defines.ToArray());
        Menu.SetChecked(sdsMenu, !enabled);
    }

    [MenuItem(sdsMenu, true)]
    public static bool Validate()
    {
        var defines = EditorUtil.GetScriptingDefineSymbols();
        var enabled = defines.IndexOf(sdsId) != -1;
        Menu.SetChecked(sdsMenu, enabled);
        return true;
    }
}
";

                var sdsId = Path.GetFileNameWithoutExtension(filePath);
                scriptText = scriptText.Replace("|CLASS_NAME|", sdsId);
                scriptText = scriptText.Replace("|SDS_ID|", sdsId);
                return scriptText;
            };

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                createScriptHelper,
                createPath,
                null,
                null);
        }
    }
}

#endif
