namespace Radical.Security.Identity
{
    public interface IMemberSecurity
    {
        string CreateToken(MemberIdentity member);

        MemberIdentity GetByToken(string token);

        MemberIdentity GetByUserId(string userId);

        bool Register(MemberIdentity memberIdentity, bool encoded = false);

        bool Register(string name, string key, out MemberIdentity di, string ip = "");

        bool Register(string token, out MemberIdentity di, string ip = "");

        bool VerifyIdentity(MemberIdentity member, string checkPasswd);

        bool VerifyToken(MemberIdentity member, string checkToken);
    }
}
