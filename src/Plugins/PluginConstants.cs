namespace LiteMonitor.src.Plugins
{
    /// <summary>
    /// 插件系统常量定义
    /// </summary>
    public static class PluginConstants
    {
        /// <summary>
        /// 插件监控项的前缀 Key
        /// </summary>
        public const string DASH_PREFIX = "DASH.";

        /// <summary>
        /// 默认刷新间隔 (秒)
        /// </summary>
        public const int DEFAULT_INTERVAL = 1;

        /// <summary>
        /// 错误状态显示文本
        /// </summary>
        public const string STATUS_ERROR = "❌";

        /// <summary>
        /// 加载中状态显示文本
        /// </summary>
        public const string STATUS_LOADING = "...";

        /// <summary>
        /// 未知状态显示文本 (解析失败/路径错误)
        /// </summary>
        public const string STATUS_UNKNOWN = "❓";

        /// <summary>
        /// 空值状态显示文本 (值为空)
        /// </summary>
        public const string STATUS_EMPTY = "⛔";

        /// <summary>
        /// 插件配置文件扩展名
        /// </summary>
        public const string CONFIG_EXT = "*.json";

        /// <summary>
        /// 失败重试间隔 (毫秒)
        /// </summary>
        public const int RETRY_INTERVAL_MS = 5000;
    }

    /// <summary>
    /// 插件定时星期逻辑 (1=周一 ... 7=周日；空字符串表示每天)
    /// </summary>
    public static class PluginSchedule
    {
        /// <summary>
        /// 获取今天对应的星期序号 (1=周一 ... 7=周日)
        /// </summary>
        public static int GetWeekdayIndex(DateTime now)
        {
            return now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
        }

        /// <summary>
        /// 解析逗号分隔的星期字符串，仅保留 1-7 的有效数字并去重升序
        /// </summary>
        public static List<int> ParseDays(string value)
        {
            var days = new List<int>();
            if (string.IsNullOrWhiteSpace(value)) return days;

            foreach (var part in value.Split(','))
            {
                string text = part.Trim();
                if (!int.TryParse(text, out int day)) continue;
                if (day < 1 || day > 7) continue;
                if (!days.Contains(day)) days.Add(day);
            }

            days.Sort();
            return days;
        }

        /// <summary>
        /// 规范化星期配置为 "1,3,7" 形式；无有效值时返回空字符串
        /// </summary>
        public static string Normalize(string value)
        {
            var days = ParseDays(value);
            return days.Count == 0 ? "" : string.Join(",", days);
        }

        /// <summary>
        /// 判断今天是否处于插件生效星期。空配置或 1-7 全选表示每天启用。
        /// </summary>
        public static bool IsActiveToday(string value)
        {
            var days = ParseDays(value);
            if (days.Count == 0 || days.Count >= 7) return true;
            return days.Contains(GetWeekdayIndex(DateTime.Now));
        }

        /// <summary>
        /// 判断指定监控项是否因插件定时星期配置而需要隐藏。
        /// 仅处理 DASH.&lt;插件实例Id&gt;[.xxx] 形式的插件监控项，内置看板项不受影响。
        /// </summary>
        public static bool IsPluginItemHidden(string itemKey, Settings cfg)
        {
            if (string.IsNullOrEmpty(itemKey) || cfg?.PluginInstances == null) return false;
            if (!itemKey.StartsWith(PluginConstants.DASH_PREFIX, StringComparison.OrdinalIgnoreCase)) return false;

            foreach (var inst in cfg.PluginInstances)
            {
                if (string.IsNullOrEmpty(inst.Id)) continue;

                string prefix = PluginConstants.DASH_PREFIX + inst.Id;
                if (itemKey.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                    itemKey.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
                {
                    return !IsActiveToday(inst.ScheduleWeekdays);
                }
            }

            return false;
        }
    }
}
