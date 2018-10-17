namespace Freedom.Cryptography
{
    public interface IPasswordHash
    {
        string ComputePasswordHash(string password);
        bool VerifyPasswordHash(string password, string passwordHash);
    }
}