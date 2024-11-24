using System;

namespace GMVC.Conditions {
    /// <summary>
    /// 游戏状态值基本接口，包括执行方法
    /// </summary>
    public interface IGameCondition : IConditionValue
    {
        string Name { get; }
        bool IsExhausted { get; }
        void Add(int value);
        /// <summary>
        /// 榨取并扣除值，(有可能获取不足预设)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        int Squeeze(int value);
        void Set(int value);
        void SetMax(int max, bool alignValue = true);
        void SetFix(int fix);
        void AddMax(int value, bool alignValue = true);
        void Clone(IConditionValue con);
    }

    public class ConValue : IGameCondition
    {
        public int Max { get; private set; }
        public int Value { get; private set; }
        public int Fix { get; private set; }
        public double MaxRatio => 1d * Max / Fix;
        public double ValueFixRatio => 1d * Value / Fix;
        public double ValueMaxRatio => 1d * Value / Max;
        public string Name { get; }
        public bool IsExhausted => Value <= 0;

        public ConValue(string name)
        {
            Name = name;
        }
        public ConValue(string name, int fix, int max, int value)
        {
            Max = max < 0 ? fix : max;
            Value = value < 0 ? Max : value;
            Fix = fix;
            Name = name;
        }

        public ConValue(string name, int max) : this(name, max, max, max)
        {

        }

        public ConValue(int max, int value, string name) : this(name, max, max, value)
        {
        }

        public void Add(int value)
        {
            if (Max <= 0) return;
            Value += value;
            ClampValue();
        }

        public int Squeeze(int value)
        {
            if (Value > value)
            {
                Add(-value);
                return value;
            }
            value = Value;
            Value = 0;
            return value;
        }

        public void Set(int value) => Value = value;

        public void SetMax(int max, bool alignValue = true)
        {
            Max = max;
            if (alignValue) ClampValue();
        }

        public void SetFix(int fix) => Fix = fix;
        public void AddFix(int fix) => Fix += fix;
        public void AddMax(int value, bool alignValue = true)
        {
            Max += value;
            if (alignValue) ClampValue();
        }

        private void ClampValue()
        {
            Max = Math.Clamp(Max, 0, Fix);//先锁定最大值
            Value = Math.Clamp(Value, 0, Max);//后锁定状态值
        }

        public void Clone(IConditionValue con)
        {
            Max = con.Max;
            Value = con.Value;
            Fix = con.Fix;
        }

        private const char LBracket = '[';
        private const char RBracket = ']';
        public override string ToString() => $"{LBracket}{Value}/{Max}({Fix}){RBracket}";
    }
}