
using ACH.Helper.Others.Entity;
using System;
using System.Collections.Generic;

namespace ACH.Helper.Others
{
    public class LogStore
    {
        private readonly LogsInfo _logs = new LogsInfo
        {
            logs = new List<string>(),
            path = string.Empty
        };
        private readonly object _lock = new object();
        private const int MaxEntries = 1000;
        private static LogStore _instance;
        private static readonly object instanceLock = new object();

        /// <summary>
        /// 单例对象
        /// </summary>
        public static LogStore Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new LogStore();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 新增日志
        /// </summary>
        /// <param name="msg"></param>
        public void AddLog(string msg)
        {
            lock (_lock)
            {
                if (_logs.logs.Count == MaxEntries)
                    _logs.logs.RemoveAt(_logs.logs.Count - 1);

                // 把最新的插到最前面
                _logs.logs.Insert(0, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff}] {msg}");
            }
        }

        /// <summary>
        /// 更新路径
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(string path)
        {
            lock (_lock)
            {
                _logs.path = path ?? string.Empty;
            }
        }


        /// <summary>
        /// 返回日志
        /// </summary>
        /// <returns></returns>
        public LogsInfo Snapshot()
        {
            lock (_lock)
            {
                return new LogsInfo
                {
                    path = _logs.path,
                    logs = new List<string>(_logs.logs)   // 深拷贝
                };
            }
        }



        /// <summary>
        /// 清空日志
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _logs.logs.Clear();
                _logs.path = string.Empty;
            }
        }
    }
}
