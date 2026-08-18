using System;
using UnityEditor;
using UnityEngine;

namespace FGUFW.Editor
{
    public sealed class TextInputWindow : EditorWindow
    {
        private const string InputControlName = "FGUFW.TextInputWindow.Input";

        private string value = string.Empty;
        private Action<string> onConfirm;
        private Action onCancel;
        private bool shouldFocusInput = true;
        private bool completed;

        public static void Open(string title, string defaultValue, Action<string> onConfirm, Action onCancel = null)
        {
            var window = CreateInstance<TextInputWindow>();
            window.titleContent = new GUIContent(title ?? string.Empty);
            window.value = defaultValue ?? string.Empty;
            window.onConfirm = onConfirm;
            window.onCancel = onCancel;

            var size = new Vector2(320f, 88f);
            window.minSize = size;
            window.maxSize = size;
            window.ShowUtility();
            window.Focus();
        }

        private void OnGUI()
        {
            if (HandleKeyboardInput())
            {
                return;
            }

            GUILayout.Space(6f);

            GUI.SetNextControlName(InputControlName);
            value = EditorGUILayout.TextField(value);
            FocusAndSelectInput();

            GUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel", GUILayout.Height(24f)))
                {
                    Cancel();
                }

                if (GUILayout.Button("Confirm", GUILayout.Height(24f)))
                {
                    Confirm();
                }
            }
        }

        private void OnDisable()
        {
            if (!completed)
            {
                var callback = onCancel;
                Complete();
                callback?.Invoke();
            }
        }

        private void FocusAndSelectInput()
        {
            if (!shouldFocusInput)
            {
                return;
            }

            EditorGUI.FocusTextInControl(InputControlName);
            var editor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
            editor?.SelectAll();
            shouldFocusInput = false;
        }

        private bool HandleKeyboardInput()
        {
            var currentEvent = Event.current;
            if (currentEvent.type != EventType.KeyDown)
            {
                return false;
            }

            if (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter)
            {
                Confirm();
                currentEvent.Use();
                return true;
            }

            if (currentEvent.keyCode == KeyCode.Escape)
            {
                Cancel();
                currentEvent.Use();
                return true;
            }

            return false;
        }

        private void Confirm()
        {
            if (completed)
            {
                return;
            }

            var callback = onConfirm;
            Complete();
            Close();
            callback?.Invoke(value);
        }

        private void Cancel()
        {
            if (completed)
            {
                return;
            }

            var callback = onCancel;
            Complete();
            Close();
            callback?.Invoke();
        }

        private void Complete()
        {
            completed = true;
            ClearCallbacks();
        }

        private void ClearCallbacks()
        {
            onConfirm = null;
            onCancel = null;
        }
    }
}
