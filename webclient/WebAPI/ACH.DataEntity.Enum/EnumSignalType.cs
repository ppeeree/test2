using System.ComponentModel;

namespace ACH.DataEntity.Enum
{
    /// <summary>
    /// 信号类型枚举
    /// </summary>
    public enum EnumSignalType
    {
        /// <summary>
        /// 加速度A
        /// </summary>
        [Description("加速度")]
        A = 0,
        /// <summary>
        /// 速度V
        /// </summary>
        [Description("速度")]
        V = 1,
        /// <summary>
        /// 位移S
        /// </summary>
        [Description("位移")]
        S = 2,
        /// <summary>
        /// 电压VT
        /// </summary>
        [Description("电压")]
        VT = 3,
        /// <summary>
        /// 电流I
        /// </summary>
        [Description("电流")]
        I = 4,
        /// <summary>
        /// 角度DEG
        /// </summary>
        [Description("角度")]
        DEG = 5,
        /// <summary>
        /// 温度
        /// </summary>
        [Description("温度")]
        T = 6,
    }
}
