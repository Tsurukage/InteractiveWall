using System.Collections.Generic;
using System.Linq;

namespace Utls
{
    public interface IWeightElement
    {
        int Weight { get; }
    }

    public static class WeightElementExtension
    {
        /// <summary>
        /// 根据权重从列表中随机选择一个元素。如果权重为0，将选择保底值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T WeightPick<T>(this IEnumerable<T> list) where T : IWeightElement
        {
            // 筛选权重大于 0 的元素
            var weightedList = list.Where(w => w.Weight > 0).ToList();

            if (!weightedList.Any())
                return list.FirstOrDefault(w => w.Weight == 0); // 返回权重为 0 的保底值

            // 计算总权重
            var totalWeight = weightedList.Sum(w => w.Weight);

            // 随机数生成
            var randomValue = Sys.Random.Next(1, totalWeight + 1);

            // 按累计权重查找
            var cumulativeWeight = 0;
            foreach (var element in weightedList)
            {
                cumulativeWeight += element.Weight;
                if (randomValue <= cumulativeWeight)
                    return element;
            }

            return default; // 理论上不会到达此处
        }

        /// <summary>
        /// 从列表中随机选择指定数量的元素，根据权重决定出现概率。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public static IEnumerable<T> WeightTake<T>(this IEnumerable<T> list, int take) where T : IWeightElement
        {
            var availableList = list.ToList(); // 复制列表
            var result = new List<T>();
            var random = Sys.Random;

            for (var i = 0; i < take && availableList.Any(); i++)
            {
                // 计算总权重
                var totalWeight = availableList.Sum(w => w.Weight);

                if (totalWeight == 0)
                    break;

                // 随机数生成
                var randomValue = random.Next(1, totalWeight + 1);

                // 按累计权重查找
                var cumulativeWeight = 0;
                T selectedElement = default;
                foreach (var element in availableList)
                {
                    cumulativeWeight += element.Weight;
                    if (randomValue <= cumulativeWeight)
                    {
                        selectedElement = element;
                        break;
                    }
                }

                // 添加到结果并移除选中元素
                if (selectedElement != null)
                {
                    result.Add(selectedElement);
                    availableList.Remove(selectedElement);
                }
            }

            return result;
        }
    }
}