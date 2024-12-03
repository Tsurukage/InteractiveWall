using System;
using System.Collections;
using GMVC.Utls;
using UnityEngine;

namespace GMVC.Core
{
    public class App
    {
        public const string Game_Save = "Game_Save";
        public const string Game_Start = "Game_Start";
        public const string DrawingBoard_Align = "DrawingBoard_Align";
        public const string DrawingBoard_Activation = "DrawingBoard_Activation";
        public const string Pref_Settings = "Pref_Settings";
        static MonoService _monoService;
        static bool IsRunning { get; set; }
        static ControllerServiceContainer ServiceContainer { get; set; }

        public static T GetController<T>() where T : class, IController => ServiceContainer.Get<T>();
        public static MessagingManager MessagingManager { get; private set; } 
        public static MainThreadDispatcher MainThread { get; private set; }
        public static AudioManager AudioManager { get; private set; }
        public static MonoService MonoService
        {
            get
            {
                if (_monoService == null)
                    _monoService = new GameObject("MonoService").AddComponent<MonoService>();
                return _monoService;
            }
        }
        public static AppSetting Setting { get; private set; }
        public static World World { get; set; }
        public static void End()
        {
            IsRunning = false;
        }

        public static void Run(Action onGameStartAction, World world, AudioManager audioManager,
            MainThreadDispatcher mainThreadDispatcher, AppSetting settings, float startAfter = 0.5f)
        {
            if (IsRunning) throw new NotImplementedException("App is running!");
            IsRunning = true;
            MessagingManager = new MessagingManager();
            Setting = settings;
            World = world;
            MainThread = mainThreadDispatcher;
            AudioManager = audioManager;
            ControllerReg();
            RegEvents();
            MonoService.StartCoroutine(StartAfterSec(startAfter));
            return;

            void ControllerReg()
            {
                ServiceContainer = new ControllerServiceContainer();
                ServiceContainer.Reg(new GameController());
            }

            void RegEvents()
            {

            }

            IEnumerator StartAfterSec(float delay)
            {
                yield return new WaitForSeconds(delay);
                onGameStartAction?.Invoke();
            }
        }

        public static void SendEvent(string eventName, DataBag bag) => MessagingManager.Send(eventName, bag);
        public static void SendEvent(string eventName, params object[] args)
        {
            args ??= Array.Empty<object>();
            MessagingManager.Send(eventName, args);
        }
        public static string RegEvent(string eventName, Action<DataBag> callbackAction) =>
            MessagingManager.RegEvent(eventName, callbackAction);
        public static void RemoveEvent(string eventName, string key) => MessagingManager.RemoveEvent(eventName, key);
        public static void PlayBGM(AudioClip clip) => AudioManager.Play(AudioManager.Types.BGM,clip);
        public static void PlaySFX(AudioClip clip) => AudioManager.Play(AudioManager.Types.SFX, clip);

        /// <summary>
        /// 停止协程服务
        /// </summary>
        /// <param name="coroutine"></param>
        public static void StopCoService(Coroutine coroutine)
        {
            if (coroutine == null) return;
            _monoService.StopCoroutine(coroutine);
        }
        public static Coroutine StartCoService(IEnumerator coroutine) => _monoService.StartCoroutine(coroutine);

        public static void Pause(bool pause)
        {
            if (pause)
            {
                Time.timeScale = 0;
                AudioManager.MuteAll(true);
            }
            else
            {
                Time.timeScale = 1;
                AudioManager.MuteAll(false);
            }
        }
    }
}
