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
        
#if UNITY_STANDALONE_WIN
        private delegate bool WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }
        
        private const int GWLP_WNDPROC = -4;
        private const uint WM_SIZING = 0x0214;
        private const uint WM_EXITSIZEMOVE = 0x0232;
        
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;
        
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        
        private static IntPtr _hWnd;
        private static IntPtr _oldWndProc;
        private static WndProcDelegate _wndProcDelegate;
        private static float _staticAspectRatio;
        
        private void Awake()
        {
            _staticAspectRatio = _aspectRatio;
            _wndProcDelegate = WndProc;
            
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
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            }
            
#if UNITY_STANDALONE_WIN
            _hWnd = GetActiveWindow();
            _oldWndProc = SetWindowLongPtr(_hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
#endif
        }
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        
        private static bool WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_SIZING && Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                int edge = wParam.ToInt32();
                RECT rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                
                int newWidth;
                int newHeight;
                
                switch (edge)
                {
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        newWidth = width;
                        newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                        rect.Bottom = rect.Top + newHeight;
                        break;
                        
                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        newHeight = height;
                        newWidth = Mathf.RoundToInt(height * _staticAspectRatio);
                        rect.Right = rect.Left + newWidth;
                        break;
                        
                    case WMSZ_TOPLEFT:
                        newWidth = width;
                        newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                        rect.Top = rect.Bottom - newHeight;
                        break;
                        
                    case WMSZ_TOPRIGHT:
                        newWidth = width;
                        newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                        rect.Top = rect.Bottom - newHeight;
                        break;
                        
                    case WMSZ_BOTTOMLEFT:
                    case WMSZ_BOTTOMRIGHT:
                        newWidth = width;
                        newHeight = Mathf.RoundToInt(width / _staticAspectRatio);
                        rect.Bottom = rect.Top + newHeight;
                        break;
                }
                
                Marshal.StructureToPtr(rect, lParam, true);
                return true;
            }
            
            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam) != IntPtr.Zero;
        }
        
        private void OnDestroy()
        {
#if UNITY_STANDALONE_WIN
            if (_hWnd != IntPtr.Zero && _oldWndProc != IntPtr.Zero)
            {
                SetWindowLongPtr(_hWnd, GWLP_WNDPROC, _oldWndProc);
            }
#endif
        }
#endif
    }
}