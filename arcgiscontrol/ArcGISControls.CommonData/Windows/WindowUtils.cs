
namespace ArcGISControls.CommonData.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    public static class WindowUtils
    {
        /// <summary>
        /// Windows의 z 깊이 순서를 이용해 listToSort를 정렬한다.
        /// </summary>
        /// <typeparam name="T">정렬할 데이터의 타입</typeparam>
        /// <param name="windows">정렬할 기준</param>
        /// <param name="listToSort">정렬할 대상 리스트</param>
        /// <returns>졍렬된 대상의 결과(새 리스트)</returns>
        public static List<T> SortFromTopToBottom<T>(IList<PresentationSource> windows, IList<T> listToSort)
        {
            var hwndSourceLookup = new Dictionary<IntPtr, List<int>>();

            for (var index = 0; index < windows.Count; index++)
            {
                var presentationSource = windows[index];
                if (presentationSource == null)
                    continue;

                var handle = ((HwndSource)presentationSource).Handle;

                if (hwndSourceLookup.ContainsKey(handle))
                {
                    hwndSourceLookup[handle].Add(index);
                }
                else
                {
                    hwndSourceLookup[handle] = new List<int> { index };
                }
            }

            var usedMarker = new bool[windows.Count];
            var result = new List<T>();

            for (var currentHandle = GetTopWindow(IntPtr.Zero);
                currentHandle != IntPtr.Zero;
                currentHandle = GetWindow(currentHandle, GetWindow_Cmd.GW_HWNDNEXT))
            {
                if (!hwndSourceLookup.ContainsKey(currentHandle))
                    continue;

                foreach (var index in hwndSourceLookup[currentHandle])
                {
                    usedMarker[index] = true;
                    result.Add(listToSort[index]);
                }
            }

            for (var index = 0; index < windows.Count; index++)
            {
                if (!usedMarker[index])
                {
                    result.Add(listToSort[index]);
                }
            }

            return result;
        }

        /// <summary>
        /// Windows의 z 깊이 순서를 이용해 정렬한다.
        /// </summary>
        /// <typeparam name="T">정렬할 데이터의 타입</typeparam>
        /// <param name="windows">정렬할 기준과 내용이 들어간 KVPair 목록</param>
        /// <returns>졍렬된 대상의 결과(새 리스트)</returns>
        public static List<KeyValuePair<PresentationSource, T>> SortFromTopToBottom<T>(IList<KeyValuePair<PresentationSource, T>> windows)
        {
            var hwndSourceLookup = new Dictionary<IntPtr, List<int>>();

            for (var index = 0; index < windows.Count; index++)
            {
                var presentationSource = windows[index].Key;
                if (presentationSource == null)
                    continue;

                var handle = ((HwndSource)presentationSource).Handle;

                if (hwndSourceLookup.ContainsKey(handle))
                {
                    hwndSourceLookup[handle].Add(index);
                }
                else
                {
                    hwndSourceLookup[handle] = new List<int> { index };
                }
            }

            var usedMarker = new bool[windows.Count];
            var result = new List<KeyValuePair<PresentationSource, T>>();

            for (var currentHandle = GetTopWindow(IntPtr.Zero);
                currentHandle != IntPtr.Zero;
                currentHandle = GetWindow(currentHandle, GetWindow_Cmd.GW_HWNDNEXT))
            {
                if (!hwndSourceLookup.ContainsKey(currentHandle))
                    continue;

                foreach (var index in hwndSourceLookup[currentHandle])
                {
                    usedMarker[index] = true;
                    result.Add(windows[index]);
                }
            }

            for (var index = 0; index < windows.Count; index++)
            {
                if (!usedMarker[index])
                {
                    result.Add(windows[index]);
                }
            }

            return result;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        private enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
    }
}
