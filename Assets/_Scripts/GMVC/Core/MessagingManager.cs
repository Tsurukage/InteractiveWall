using System;
using System.Collections.Generic;
using GMVC.Utls;
using UnityEngine;

namespace GMVC.Core
{
    /// <summary>
    /// 事件消息系统
    /// </summary>
    public class MessagingManager
    {
        Dictionary<string, Dictionary<string, Action<string>>> EventMap { get; set; } =
            new Dictionary<string, Dictionary<string, Action<string>>>();

        /// <summary>
        /// 发送事件, 所有物件将以<see cref="DataBag"/>序列化
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="objs"></param>
        public void Send(string eventName, params object[] objs) => SendSerialized(eventName, DataBag.Serialize(objs));
        /// <summary>
        /// 发送事件, 参数为<see cref="string"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="dataBag"></param>
        public void SendSerialized(string eventName, string dataBag)
        {
            if (EventMap.ContainsKey(eventName))
                foreach (var (_, action) in EventMap[eventName])
                    action?.Invoke(string.IsNullOrEmpty(dataBag) ? string.Empty : dataBag);
            else
            {
                Debug.LogWarning($"{eventName} 没有注册事件!");
            }
        }

        string RegEvent(string eventName, Action<string> action)
        {
            if (!EventMap.ContainsKey(eventName))
            {
                EventMap.Add(eventName, new Dictionary<string, Action<string>>());
            }
            var key = Guid.NewGuid().ToString();
            EventMap[eventName].Add(key, action);
            return key;
        }
        /// <summary>
        /// 注册<see cref="DataBag"/>事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public string RegEvent(string eventName, Action<DataBag> action)
        {
            return RegEvent(eventName, ObjBagSerialize);
            void ObjBagSerialize(string arg) => action?.Invoke(DataBag.Deserialize(arg));
        }
        /// <summary>
        /// 删除事件方法(仅仅是删除一个事件方法, 其余的监听方法依然有效)
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="key"></param>
        public void RemoveEvent(string eventName, string key)
        {
            if (EventMap[eventName].ContainsKey(key))
                EventMap[eventName].Remove(key);
        }
    }
} 