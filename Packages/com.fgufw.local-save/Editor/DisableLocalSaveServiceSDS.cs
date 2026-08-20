using System.Linq;
using FGUFW;
using FGUFW.Editor;
using UnityEditor;

public static class DisableLocalSaveServiceSDS
{
    private const string sdsMenu =
        "Conditional/DisableLocalSaveServiceSDS";

    private const string sdsId = "DisableLocalSaveServiceSDS";

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
