namespace ACH.CMSWebClient.ControllerModel.BladeSystem
{
    public class SystemToken
    {
        public string access_token { get; set; }
        public string account { get; set; }
        public string avatar { get; set; }
        public string deptCode { get; set; }
        public string dept_id { get; set; }
        public string dept_name { get; set; }
        public SystemTokenDeatil detail { get; set; }
        public string elevation { get; set; }
        public string expires_in { get; set; }
        public string latitude { get; set; }
        public string license { get; set; }
        public string longitude { get; set; }
        public string nick_name { get; set; }
        public string oauth_id { get; set; }
        public string post_id { get; set; }
        public string refresh_token { get; set; }
        public string role_id { get; set; }
        public string role_name { get; set; }
        public string tenant_id { get; set; }
        public string token_type { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
    }

    public class SystemTokenDeatil
    {
        public string ext { get; set; }
        public string type { get; set; }
    }

    public class SystemErrorToken
    {
        public int error_code { get; set; }
        public string error_description { get; set; }
    }
}
