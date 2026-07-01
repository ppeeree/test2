namespace ACH.DataEntity.Bladex
{
    public class blade_menu
    {
        public long id { get; set; }
        public long parent_id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string alias { get; set; }
        public string path { get; set; }
        public string source { get; set; }
        public int sort { get; set; }
        public int category { get; set; }
        public int action { get; set; }
        public int is_open { get; set; }
        public string component { get; set; }
        public string remark { get; set; }
        public int is_deleted { get; set; }
    }
}
