using System;

namespace ACH.DataEntity.Bladex
{
    public class account_space_basic
    {
        public int id { get; set; }
        public string space_basic_guid { get; set; }
        public string space_guid { get; set; }
        public string space_basic_code { get; set; }
        public string space_basic_name { get; set; }
        public string space_basic_value { get; set; }
        public long create_user { get; set; }
        public DateTime create_time { get; set; }
        public long update_user { get; set; }
        public DateTime update_time { get; set; }
        public int is_deleted { get; set; }
        public int sort { get; set; }
        public long create_dept { get; set; }
        public string type { get; set; }
        public int modifiable { get; set; }
        public string dept_code { get; set; }
    }
}
