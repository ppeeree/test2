using System;

namespace ACH.DataEntity.Bladex
{
    public class account_space
    {
        public int id { get; set; }
        public string space_guid { get; set; }
        public string model_guid { get; set; }
        public string model_code { get; set; }
        public string space_name { get; set; }
        public string space_code { get; set; }
        public int node_type { get; set; }
        public string path { get; set; }
        public string link_guid { get; set; }
        public string space_parent_guid { get; set; }
        public string create_user { get; set; }
        public string create_dept { get; set; }
        public DateTime create_time { get; set; }
        public string update_user { get; set; }
        public DateTime update_time { get; set; }
        public int is_deleted { get; set; }
        public int sort { get; set; }
        public string dept_code { get; set; }


    }
}
