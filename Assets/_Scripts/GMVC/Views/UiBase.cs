using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;
using Utls;
using Object = UnityEngine.Object;

namespace GMVC.Views
{
    public abstract class UiBase : IUiBase
    {
        public GameObject GameObject { get; }
        public Transform Transform { get; }
        public RectTransform RectTransform { get; }
        IView _v;

        public UiBase(IView v, bool display = true)
        {
            _v = v ?? throw new ArgumentNullException($"{GetType().Name}: view = null!");
            GameObject = v.GameObject;
            Transform = v.GameObject.transform;
            RectTransform = v.RectTransform;
            GameObject.SetActive(display);
        }

        /// <summary>
        /// 当ui显示触发器
        /// </summary>
        protected virtual void OnUiShow() { }
        /// <summary>
        /// 当ui隐藏触发器
        /// </summary>
        protected virtual void OnUiHide() { }
        public View GetView() => _v.GetView();
        public event UnityAction OnDestroyEvent;
        public void Show() => Display(true);
        public void Hide() => Display(false);
        public void Display(bool display)
        {
            if (display) OnUiShow();
            else OnUiHide();
            GameObject.SetActive(display);
        }
        public virtual void ResetUi() { }

        public Coroutine StartCoroutine(IEnumerator enumerator) => _v.StartCo(enumerator);
        public void StopCoroutine(Coroutine coroutine) => _v.StopCo(coroutine);
        public void StopAllCoroutines() => _v.StopAllCo();

        public void Destroy() => Object.Destroy(GameObject);

        protected void Log(string msg = null) => Debug.Log($"{GameObject.name}: {msg}", GameObject);

        protected void LogEvent(string msg = null, [CallerMemberName] string methodName = null) =>
            Debug.Log($"{GameObject.name}.{methodName}() {msg}", GameObject);
        protected void LogError(string msg = null) => Debug.LogError($"{GameObject.name}: {msg}", GameObject);
        protected void LogWarning(string msg = null) => Debug.LogWarning($"{GameObject.name}: {msg}", GameObject);
        protected void LogException(Exception e) => Debug.LogException(e, GameObject);
    }

    /// <summary>
    /// 用于管理列表的ui, 但是不包含滚动条<br/>
    /// 要使用滚动条, 请使用<see cref="ListView_Scroll{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListView_Trans<T> : UiBase where T : UiBase
    {
        List<T> _list { get; } = new List<T>();
        public IReadOnlyList<T> List => _list;
        public View Prefab { get; }
        public Transform Content { get; }
        ObjectPool<View> pool;

        public ListView_Trans(IView v, View prefab, Transform content, bool display = true, bool usePool = true,
            bool hideChildrenViews = true) : base(v, display)
        {
            Prefab = prefab;
            Content = content;

            if (usePool)
            {
                pool = new ObjectPool<View>(
                    () => Object.Instantiate(Prefab, Content),
                    obj => obj.gameObject.SetActive(true),
                    obj => obj.gameObject.SetActive(false),
                    obj => Object.Destroy(obj.gameObject),
                    false, 10, 100 // 可根据需要设置容量
                );
            }

            if (hideChildrenViews) HideChildren();
        }

        public ListView_Trans(IView v, string prefabName, string contentName,bool display = true, bool usePool = true) 
            : this(v, v.Get<View>(prefabName), v.Get<Transform>(contentName), display,usePool) { }

        public ListView_Trans(IView v, string prefabName,bool display = true , bool usePool = true) 
            : this(v, v.Get<View>(prefabName), v.RectTransform, display,usePool) { }

        public void HideChildren()
        {
            foreach (Transform tran in Content)
                tran.gameObject.SetActive(false);
        }

        public T Instance(Func<View, T> func)
        {
            Func<View> onCreateView = pool != null ? pool.Get : () => Object.Instantiate(Prefab, Content);
            return Instance(onCreateView, func);
        }

        T Instance(Func<View> onCreateView, Func<View, T> func)
        {
            var obj = onCreateView();
            var ui = func.Invoke(obj);
            _list.Add(ui);
            return ui;
        }

        public void ClearList(Action<T> onRemoveFromList)
        {
            foreach (var ui in _list)
            {
                onRemoveFromList(ui);
                if (pool != null)
                    pool.Release(ui.GetView());
                else
                    Object.Destroy(ui.GetView().gameObject);
            }

            _list.Clear();
        }

        public void ClearPool(Action<T> onRemoveFromList)
        {
            if (pool == null)
            {
                "未使用对象池，无法清空对象池。".Log(GameObject);
                return;
            }

            foreach (var ui in _list)
            {
                onRemoveFromList(ui);
                pool.Release(ui.GetView());
            }

            _list.Clear();
        }

        public void Remove(T obj)
        {
            if (pool != null)
                pool.Release(obj.GetView());
            else
                Object.Destroy(obj.GetView().gameObject);
            _list.Remove(obj);
        }
    }

    /// <summary>
    /// 滚动条列表ui, 用于管理列表的ui, 包含滚动条<br/>
    /// 如果不用滚动条, 请使用<see cref="ListView_Trans{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListView_Scroll<T> : ListView_Trans<T> where T : UiBase
    {
        readonly ScrollRect _scrollRect;
        public ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                    throw new InvalidOperationException("如果要调用ScrollRect,请在构造的时候传入scrollrect控件");
                return _scrollRect;
            }
        }

        public ListView_Scroll(View prefab, ScrollRect scrollRect, IView v, bool displayParent,
            bool usePool = true,
            bool hideChildrenViews = true) : base(v, prefab, scrollRect.content, displayParent, usePool)
        {
            _scrollRect = scrollRect;
            if (hideChildrenViews) HideChildren();
        }

        public ListView_Scroll(IView v, string prefabName, string scrollRectName, bool displayParent,
            bool usePool = true,
            bool hideChildrenViews = true) : this(
            v.Get<View>(prefabName),
            v.Get<ScrollRect>(scrollRectName), v, 
            displayParent: displayParent, 
            usePool : usePool,
            hideChildrenViews: hideChildrenViews)
        {
        }

        public void SetVerticalScrollPosition(float value)
        {
            ScrollRect.verticalNormalizedPosition = value;
        }

        public void SetHorizontalScrollPosition(float value)
        {
            ScrollRect.horizontalNormalizedPosition = value;
        }

        public void ScrollRectSetSize(Vector2 size) => ((RectTransform)_scrollRect.transform).sizeDelta = size;

        public void ScrollRectSetSizeX(float x)
        {
            var rect = ((RectTransform)_scrollRect.transform);
            rect.sizeDelta = new Vector2(x, rect.sizeDelta.y);
        }

        public void ScrollRectSetSizeY(float y)
        {
            var rect = ((RectTransform)_scrollRect.transform);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, y);
        }

        public void HideOptions()
        {
            ScrollRect.gameObject.SetActive(false);
        }

        public void ShowOptions() => ScrollRect.gameObject.SetActive(true);
        public override void ResetUi() => HideOptions();
    }
}