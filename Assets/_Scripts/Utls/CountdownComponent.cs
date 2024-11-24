using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Utls
{
    /// <summary>
    /// 用协程实现的倒计时组件，别轻易调用StopAllCoroutines()，否则会导致所有协程停止
    /// </summary>
    public abstract class CountdownComponent : MonoBehaviour
    {
        /// <summary>
        /// 跳动次数
        /// </summary>
        protected abstract int PulseTimes { get; }
        /// <summary>
        /// 总持续时间
        /// </summary>
        protected abstract float Duration { get; }

        public readonly UnityEvent<int> OnPulseTrigger = new();
        public readonly UnityEvent OnCountdownComplete = new();
        
        int remainTimes;
        Coroutine countdownCo;
        public void StartCountdown() => StartCountdown(false);
        public void StartCountdown(bool reset)
        {
            if (PulseTimes <= 0) throw new NotImplementedException("跳动次数至少要一次!");
            if (Duration <= 0) throw new NotImplementedException("持续市场不可以负数!");
            if (reset) StopCountdown();
            if (countdownCo!=null) return;
            countdownCo = StartCoroutine(Execute());
        }
        public void StopCountdown()
        {
            if(countdownCo!=null)
            {
                StopCoroutine(countdownCo);
                countdownCo = null;
            }
            remainTimes = 0;
        }

        IEnumerator Execute()
        {
            var interval = Duration / PulseTimes;
            remainTimes = PulseTimes;
            while (remainTimes > 0)
            {
                remainTimes--;
                OnPulseTrigger?.Invoke(remainTimes);
                OnPulse(remainTimes); // 调用可被子类重写的方法
                yield return new WaitForSeconds(interval);
            }
            // 倒计时完成后的回调
            OnCountdownComplete?.Invoke();
            StopCountdown();
        }

        // 可被子类重写的回调方法
        protected virtual void OnPulse(int remainingTimes){}
    }
}