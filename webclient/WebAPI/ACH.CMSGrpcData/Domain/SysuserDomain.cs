using CMSFramework.BusinessEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindCMS.IAnalyzerDomain;

namespace WindCMS.GRPCWPServerImp.Domain
{
    internal class SysuserDomain : ILoginSystem
    {
        public User VibAnalyer_Login(string _userName, string _passWord)
        {
            // 暂时使用固定密码 ADMIN 和 123#@！
            if (_userName == "ADMIN" && _passWord == "123#@!")
            {
                User user = new User();
                user.UserName = _userName;
                return user;
            }
            return null;
            //User user = DataDbContext.QueryFirstOrDefault<User>($"SELECT account AS 'UserID', `name` AS 'UserName' FROM blade_user WHERE account = '{_userName}' LIMIT 1;");

            //if (user == null)
            //{
            //    return null;
            //}
            //return user;
        }
    }
}
