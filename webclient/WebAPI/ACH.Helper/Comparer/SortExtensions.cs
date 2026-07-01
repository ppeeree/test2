using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.Helper.Comparer
{
    public static class SortExtensions
    {
        /// <summary>
        /// 字典容器：集中管理所有自定义排序规则
        /// </summary>
        private static readonly Dictionary<EnumSortType, Dictionary<string, int>> SortDictionaries;

        // 静态构造函数初始化
        static SortExtensions()
        {
            SortDictionaries = CreatesSortDictionary();
        }


        /// <summary>
        /// 生成自定义排序规则
        /// </summary>
        /// <returns></returns>
        private static Dictionary<EnumSortType, Dictionary<string, int>> CreatesSortDictionary()
        {
            // 读取json文件，获取自定义排序顺序
            string filePath = "SortDic.json";
            string jsonString = System.IO.File.ReadAllText(filePath);
            SortDictionary data = JsonConvert.DeserializeObject<SortDictionary>(jsonString);

            if (data == null)
            {
                return new Dictionary<EnumSortType, Dictionary<string, int>>()
                {
                    [EnumSortType.StationName] = new Dictionary<string, int>(),
                    [EnumSortType.PageCompName] = new Dictionary<string, int>(),
                    [EnumSortType.CompName] = new Dictionary<string, int>(),
                    [EnumSortType.MeaslocName] = new Dictionary<string, int>()
                };
            }

            Dictionary<EnumSortType, Dictionary<string, int>> res = new Dictionary<EnumSortType, Dictionary<string, int>>()
            {
                [EnumSortType.StationName] = data.stationList.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal),

                [EnumSortType.PageCompName] = data.pageCompList.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal),

                [EnumSortType.CompName] = data.compList.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal),

                [EnumSortType.MeaslocName] = data.measlocList.Select((name, index) => (name, index)).ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal)
            };
            return res;
        }

        /// <summary>
        /// 对 List<T> 进行原地排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">原始数组</param>
        /// <param name="ascending">true：降序，false：升序</param>
        /// <param name="dictType">排序类型</param>
        /// <returns></returns>
        public static List<T> SortByName<T>(this List<T> list, bool ascending = true, EnumSortType dictType = EnumSortType.Alphabetical) where T : ISortable
        {
            // 根据传参选定使用什么比较器
            var comparer = CreateComparer<T>(ascending, dictType);
            list.Sort(comparer);
            return list;
        }

        /// <summary>
        /// LINQ扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="ascending">控制升序/降序</param>
        /// <param name="useCustomOrder">切换排序模式：true按照字典定义排序，false按照字母顺序排序</param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByName<T>(this IEnumerable<T> source, bool ascending = true, EnumSortType dictType = EnumSortType.Alphabetical) where T : ISortable
        {
            var comparer = CreateComparer<T>(ascending, dictType);
            return ascending
                ? source.OrderBy(x => x, comparer)
                : source.OrderByDescending(x => x, comparer);
        }


        /// <summary>
        /// 比较器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ascending">控制升序/降序</param>
        /// <param name="useCustomOrder">切换排序模式：true按照字典定义排序，false按照字母顺序排序</param>
        /// <returns></returns>
        private static IComparer<T> CreateComparer<T>(bool ascending, EnumSortType dictType) where T : ISortable
        {
            IComparer<T> baseComparer = dictType == EnumSortType.Alphabetical
                ? new AlphabeticalComparer<T>()
                : (IComparer<T>)new CustomPriorityComparer<T>(dictType);
            return ascending ? baseComparer : new ReverseComparer<T>(baseComparer);
        }


        /// <summary>
        /// 比较器类：按照指定顺序排序，数组下标越小越靠前
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class CustomPriorityComparer<T> : IComparer<T> where T : ISortable
        {
            private readonly EnumSortType _dictType;

            public CustomPriorityComparer(EnumSortType dictType)
            {
                _dictType = dictType;
            }

            // 默认升序
            public int Compare(T x, T y)
            {
                string nameX = x?.GetSortableName() ?? string.Empty;
                string nameY = y?.GetSortableName() ?? string.Empty;

                var orderDict = SortDictionaries[_dictType];

                int px = GetPriorityByContains(nameX, orderDict);
                int py = GetPriorityByContains(nameY, orderDict);

                if (px != py) return px.CompareTo(py);
                return string.Compare(nameX, nameY, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// 核心方法：通过包含匹配查找优先级
        /// </summary>
        private static int GetPriorityByContains(string str, Dictionary<string, int> dict)
        {
            if (string.IsNullOrEmpty(str)) return int.MaxValue;

            // 遍历查找所有匹配的键，选择最高优先级（值最小）
            int? minPriority = dict
                .Where(kvp => str.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => (int?)kvp.Value)
                .Min(); // 空序列时返回 null

            return minPriority ?? int.MaxValue;
        }

        /// <summary>
        /// 比较器类：按照从a到z排序排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class AlphabeticalComparer<T> : IComparer<T> where T : ISortable
        {
            public int Compare(T x, T y)
            {
                string nameX = x?.GetSortableName() ?? string.Empty;
                string nameY = y?.GetSortableName() ?? string.Empty;
                int result = string.Compare(nameX, nameY, StringComparison.OrdinalIgnoreCase);
                return result;
            }
        }


        /// <summary>
        /// 倒序包装器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class ReverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _inner;
            public ReverseComparer(IComparer<T> inner) => _inner = inner;
            public int Compare(T x, T y) => _inner.Compare(y, x);
        }
    }
}
