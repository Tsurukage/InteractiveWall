using System;
using System.Collections.Generic;

namespace GMVC.Core
{
    /// <summary>
    /// 逻辑控制器, 主要控制与模型和数据的交互
    /// </summary>
    public interface IController 
    {
    }
    /// <summary>
    /// 基于<see cref="IController"/>的DI容器
    /// </summary>
    public class ControllerServiceContainer
    {
        Dictionary<object, IController> Container { get; set; } = new Dictionary<object, IController>();

        public T Get<T>() where T : class, IController
        {
            var type = typeof(T);
            if (!TryGet<T>(type, out var obj))
                throw new NotImplementedException($"{type.Name} hasn't register!");
            return obj;
        }

        public bool TryGet<T>(Type type, out T c) where T : class, IController
        {
            if(!Container.TryGetValue(type.Name, out var obj))
            {
                c = default;
                return false;
            }
            c = obj as T;
            return true;
        }

        public void Reg<T>(T controller) where T : class, IController
        {
            Container.Add(controller.GetType().Name, controller);
        }
    }
}