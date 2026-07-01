namespace ACH.DBRepository.DBSelect
{
    /// <summary>
    /// 风场ID+时间对象
    /// </summary>
    public class StationTimeSelect
    {
        /// <summary>
        /// 风场ID
        /// </summary>
        public string StationID { get; set; }

        /// <summary>
        /// 时间节点：时间节点前数据在TheWeave数据库，时间节点后的数据在DAT数据库
        /// </summary>
        public string Time { get; set; }
    }
}
