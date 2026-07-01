using CMSFramework.BusinessEntity;

namespace wtphm.gRPCServer.Domain
{
    public interface ILoginSystem
    {
        User VibAnalyer_Login(string _userName, string _passWord);
    }
}
