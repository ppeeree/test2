using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMSFramework.BusinessEntity;

namespace WindCMS.IAnalyzerDomain
{
    /// <summary>
    /// 登录系统接口
    /// </summary>
    public interface ILoginSystem
    {
        User VibAnalyer_Login(string _userName, string _passWord);        
    }
}
