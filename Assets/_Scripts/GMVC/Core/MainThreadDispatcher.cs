using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;

namespace GMVC.Core
{
    public interface IMainThreadDispatcher
    {
        void Enqueue(Action action);
    }

    public class MainThreadDispatcher : MonoBehaviour, IMainThreadDispatcher
    {
        static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        static MainThreadDispatcher _instance;
        public bool UseCustomExceptionHandler;
        public event UnityAction<Exception> CustomExceptionHandler;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Enqueue(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
#if UNITY_EDITOR
            action(); // 在编辑器模式下直接执行
#else
        _executionQueue.Enqueue(action);
#endif
        }

        void Update()
        {
            while (_executionQueue.TryDequeue(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        void HandleException(Exception ex)
        {
            // 调用自定义异常处理程序（如果设置了）
            if (UseCustomExceptionHandler && CustomExceptionHandler != null)
            {
                CustomExceptionHandler.Invoke(ex);
            }
            else
            {
                Debug.LogException(ex);
                // 停止游戏逻辑
                PauseGame();
                // 显示错误信息
                ShowErrorMessage($"An unexpected error occurred: {ex.Message}");
            }
        }

        void PauseGame()
        {
            // 暂停游戏，停止时间流动
            Time.timeScale = 0;
        }

        void ShowErrorMessage(string message)
        {
            // 显示错误信息，例如通过 Unity 的 UI 系统
            // 这里可以使用 UnityEngine.UI.Text 或弹出对话框的方式
            GameObject errorPopup = new GameObject("ErrorPopup");
            var canvas = errorPopup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var textObj = new GameObject("ErrorText");
            textObj.transform.SetParent(errorPopup.transform);
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.red;
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;

            var rectTransform = text.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(600, 200);
            rectTransform.anchoredPosition = Vector2.zero;

            // 你也可以添加一个按钮让玩家退出或重启游戏
        }

        public void ResumeGame()
        {
            // 恢复游戏的时间流动
            Time.timeScale = 1;
        }
    }
}