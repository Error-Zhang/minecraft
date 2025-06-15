#if ANDROID
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Engine.Input;
using Org.Libsdl.App;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl.Android;
using Debug = System.Diagnostics.Debug;

namespace Engine
{

    public class EngineActivity : SilkActivity
    {
        internal static EngineActivity m_activity;

        public event Action Paused;

        public event Action Resumed;

        public event Action Destroyed;

        public event Action<Intent> NewIntent;

        public event Func<KeyEvent, bool> OnDispatchKeyEvent;
        
        public static string BasePath = RunPath.AndroidFilePath;
        public static string ConfigPath = RunPath.AndroidFilePath;

        public EngineActivity()
        {
            m_activity = this;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TranslucentStatus | WindowManagerFlags.TranslucentNavigation);
            EnableImmersiveMode();
            VolumeControlStream = Android.Media.Stream.Music;
            RequestedOrientation = ScreenOrientation.SensorLandscape;
        }

        public void Vibrate(long ms)
        {
            Vibrator vibrator = (Vibrator)GetSystemService("vibrator");
            vibrator.Vibrate(VibrationEffect.CreateOneShot(ms, VibrationEffect.DefaultAmplitude));
        }
        public void OpenLink(string link)
        {
            StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(link)));
        }


        protected override void OnPause()
        {
            base.OnPause();
            Paused?.Invoke();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Resumed?.Invoke();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            NewIntent?.Invoke(intent);
        }

        protected override void OnRun()
        {
        }

        protected override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                Destroyed?.Invoke();
            }
            finally
            {
                Thread.Sleep(250);
                System.Environment.Exit(0);
            }
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            Touch.HandleTouchEvent(e);
            return true;
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            System.Diagnostics.Debug.WriteLine($"[DispatchKeyEvent]action:{e.Action} keyCode:{e.KeyCode} unicodeChar:{e.UnicodeChar} flags:{e.Flags} metaState:{e.MetaState} source:{e.Source} deviceId:{e.DeviceId}");
            bool handled = false;
            var invocationList = OnDispatchKeyEvent?.GetInvocationList();
            if (invocationList != null)
            {
                foreach (var invocation in invocationList)
                {
                    handled |= (bool)invocation.DynamicInvoke([e])!;
                }
            }
            if (!handled)
            {
                handled = e.Action switch
                {
                    KeyEventActions.Down => OnKeyDown(e.KeyCode, e),
                    KeyEventActions.Up => OnKeyUp(e.KeyCode, e),
                    _ => false
                };
            }

            return true;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.VolumeUp:
                    ((AudioManager)Context?.GetSystemService("audio"))?.AdjustStreamVolume(Android.Media.Stream.Music, Adjust.Raise, VolumeNotificationFlags.ShowUi);
                    EnableImmersiveMode();
                    break;
                case Keycode.VolumeDown:
                    ((AudioManager)Context?.GetSystemService("audio"))?.AdjustStreamVolume(Android.Media.Stream.Music, Adjust.Lower, VolumeNotificationFlags.ShowUi);
                    EnableImmersiveMode();
                    break;
            }
            if ((e.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad || (e.Source & InputSourceType.Joystick) == InputSourceType.Joystick)
            {
                GamePad.HandleKeyEvent(e);
            }
            else
            {
                Keyboard.HandleKeyEvent(e);
            }
            return true;
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if ((e.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad || (e.Source & InputSourceType.Joystick) == InputSourceType.Joystick)
            {
                GamePad.HandleKeyEvent(e);
            }
            else
            {
                Keyboard.HandleKeyEvent(e);
            }
            return true;
        }

        public override bool DispatchGenericMotionEvent(MotionEvent e)
        {
            System.Diagnostics.Debug.WriteLine($"[OnGenericMotionEvent]source:{e.Source} action:{e.Action}");
            if (((e.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad || (e.Source & InputSourceType.Joystick) == InputSourceType.Joystick) && e.Action == MotionEventActions.Move)
            {
                GamePad.HandleMotionEvent(e);
            }
            if ((e.Source & InputSourceType.Mouse) == InputSourceType.Mouse || (e.Source & InputSourceType.ClassPointer) == InputSourceType.ClassPointer || (e.Source & InputSourceType.MouseRelative) == InputSourceType.MouseRelative)
            {
                Mouse.HandleMotionEvent(e);
                return true;
            }
            return true;
        }

        public void EnableImmersiveMode()
        {
            if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)6150;
                Window.DecorView.SystemUiFlags = SystemUiFlags.Fullscreen | SystemUiFlags.HideNavigation | SystemUiFlags.Immersive | SystemUiFlags.ImmersiveSticky;
            }
        }
    }
}
#endif
