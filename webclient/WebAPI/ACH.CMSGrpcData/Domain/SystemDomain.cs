using CMSFramework.BusinessEntity;

namespace wtphm.gRPCServer.Domain
{
    public class SystemDomain : ILoginSystem
    {
        public User VibAnalyer_Login(string _userName, string _passWord)
        {
            return new User { UserName = _userName, PassWord = _passWord };
        }
    }
}
