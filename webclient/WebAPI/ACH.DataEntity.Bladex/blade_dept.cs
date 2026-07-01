namespace ACH.DataEntity.Bladex
{
    public class blade_dept
    {
        public long id { get; set; }
        public string tenant_id { get; set; }
        public long parent_id { get; set; }
        public string ancestors { get; set; }
        public int dept_category { get; set; }
        public string region_code { get; set; }
        public string dept_name { get; set; }
        public string dept_code { get; set; }
        public string full_name { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public string elevation { get; set; }
        public int sort { get; set; }
        public string remark { get; set; }
        public int is_deleted { get; set; }
    }
}
