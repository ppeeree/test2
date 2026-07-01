using System;

namespace ACH.DataEntity.Bladex
{
    public class blade_user
    {
        public long id { get; set; }
        public string tenant_id { get; set; }
        public string code { get; set; }
        public int user_type { get; set; }
        public string account { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string real_name { get; set; }
        public string avatar { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public DateTime birthday { get; set; }
        public int sex { get; set; }
        public string role_id { get; set; }
        public string dept_id { get; set; }
        public string post_id { get; set; }
        public long create_user { get; set; }
        public long create_dept { get; set; }
        public DateTime create_time { get; set; }
        public long update_user { get; set; }
        public DateTime update_time { get; set; }
        public int status { get; set; }
        public int is_deleted { get; set; }
    }
}
