using System.Linq;
using UnityEditor;
using FGUFW;
using FGUFW.Editor;

public static class DisableLitJsonServiceSDS
{
    private const string sdsMenu = "Conditional/DisableLitJsonServiceSDS";
    private const string sdsId = "DisableLitJsonServiceSDS";

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
