public interface ISsoService
{
    Task<bool> ValidateToken(string token);
}