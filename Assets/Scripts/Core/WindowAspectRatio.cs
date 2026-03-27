using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GreedDungeon.Core
{
    public class WindowAspectRatio : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _aspectRatio = 16f / 9f;
        [SerializeField] private bool _startFullscreen = true;
        [SerializeField] private int _minWidth = 1280;
        
#if UNITY_STANDALONE_WIN
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }
        
        private const int GWLP_WNDPROC = -4;
        private const uint WM_SIZING = 0x0214;
        
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;
        
        private static IntPtr _hWnd;
        private static IntPtr _oldWndProc;
        private static WndProcDelegate _wndProcDelegate;
        private static float _staticAspectRatio;
        private static bool _isFullscreen;
        private static int _staticMinWidth;
        private static int _staticMinHeight;
        
        private void Awake()
        {
            _staticAspectRatio = _aspectRatio;
            _wndProcDelegate = WndProc;
            _isFullscreen = _startFullscreen;
            _staticMinWidth = _minWidth;
            _staticMinHeight = Mathf.RoundToInt(_minWidth / _aspectRatio);
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            if (_startFullscreen)
            {
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
            }
            else
            {
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            }
            
            _hWnd = GetActiveWindow();
            
            if (IntPtr.Size == 8)
                _oldWndProc = SetWindowLongPtr64(_hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
            else
                _oldWndProc = SetWindowLong32(_hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
        }
        
        private void Update()
        {
            _isFullscreen = Screen.fullScreen;
        }
        
        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_SIZING && !_isFullscreen)
            {
                int edge = wParam.ToInt32();
                RECT rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                
                // 최소 크기 체크
                if (width < _staticMinWidth) width = _staticMinWidth;
                if (height < _staticMinHeight) height = _staticMinHeight;
                
                switch (edge)
                {
                    case WMSZ_LEFT:
                        {
                            int newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                            rect.Bottom = rect.Top + newHeight;
                            rect.Left = rect.Right - width;
                            break;
                        }
                    case WMSZ_RIGHT:
                        {
                            int newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                            rect.Bottom = rect.Top + newHeight;
                            rect.Right = rect.Left + width;
                            break;
                        }
                        
                    case WMSZ_TOP:
                        {
                            int newWidth = Mathf.RoundToInt(height * _staticAspectRatio);
                            rect.Right = rect.Left + newWidth;
                            rect.Top = rect.Bottom - height;
                            break;
                        }
                    case WMSZ_BOTTOM:
                        {
                            int newWidth = Mathf.RoundToInt(height * _staticAspectRatio);
                            rect.Right = rect.Left + newWidth;
                            rect.Bottom = rect.Top + height;
                            break;
                        }
                        
                    case WMSZ_TOPLEFT:
                        {
                            int newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                            rect.Top = rect.Bottom - newHeight;
                            rect.Left = rect.Right - width;
                            break;
                        }
                    case WMSZ_TOPRIGHT:
                        {
                            int newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                            rect.Top = rect.Bottom - newHeight;
                            rect.Right = rect.Left + width;
                            break;
                        }
                        
                    case WMSZ_BOTTOMLEFT:
                        {
                            int newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                            rect.Bottom = rect.Top + newHeight;
                            rect.Left = rect.Right - width;
                            break;
                        }
                    case WMSZ_BOTTOMRIGHT:
                        {
                            int newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                            rect.Bottom = rect.Top + newHeight;
                            rect.Right = rect.Left + width;
                            break;
                        }
                }
                
                Marshal.StructureToPtr(rect, lParam, true);
                return IntPtr.Zero;
            }
            
            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }
        
        private void OnDestroy()
        {
            if (_hWnd != IntPtr.Zero && _oldWndProc != IntPtr.Zero)
            {
                if (IntPtr.Size == 8)
                    SetWindowLongPtr64(_hWnd, GWLP_WNDPROC, _oldWndProc);
                else
                    SetWindowLong32(_hWnd, GWLP_WNDPROC, _oldWndProc);
            }
        }
#endif
    }
}