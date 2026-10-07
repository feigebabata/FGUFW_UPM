using System;
using System.Threading;
using FGUFW;
using UnityEngine;

// 本程序集没有任何类型被游戏代码直接引用，只靠下面的 [RuntimeInitializeOnLoadMethod] 自注册。
// IL2CPP + Managed Stripping 会把“没人引用”的程序集整包裁掉，真机启动时 fg.save 抛
// "ISaveService is not registered. Install one compatible service package..."。
// 用 AlwaysLinkAssembly 保证它一定参与托管代码裁剪流程、不被整包删除。
[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace FGUFW.LocalSave
{
    public static class RuntimeInitializeOnLoad
    {
        private static LocalSaveService service;
        private static int finalSaveStarted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            UnregisterEvents();
            service = null;
            finalSaveStarted = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterService()
        {
#if !DisableLocalSaveServiceSDS
            service = new LocalSaveService();
            fg.RegisterSave(service);
            RegisterEvents();
#endif
        }

        private static void RegisterEvents()
        {
            Application.wantsToQuit += OnWantsToQuit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            fg.messenger.Add<bool>(ApplicationLifecycle.Msg_OnApplicationPause, OnApplicationPause);
        }

        private static void UnregisterEvents()
        {
            Application.wantsToQuit -= OnWantsToQuit;
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            fg.messenger.Remove<bool>(ApplicationLifecycle.Msg_OnApplicationPause, OnApplicationPause);
        }

        private static bool OnWantsToQuit()
        {
            SaveFinal("应用正常退出");
            return true;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            SaveFinal("发生未处理异常");
        }

        private static void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveSafely("应用进入后台");
            }
        }

        private static void SaveFinal(string reason)
        {
            if (Interlocked.Exchange(ref finalSaveStarted, 1) != 0)
            {
                return;
            }

            SaveSafely(reason);
        }

        private static void SaveSafely(string reason)
        {
            if (service == null)
            {
                return;
            }

            try
            {
                service.SaveImmediately();
            }
            catch (Exception exception)
            {
                Debug.LogError($"LocalSave在{reason}时保存失败:\n{exception}");
            }
        }

    }
}
