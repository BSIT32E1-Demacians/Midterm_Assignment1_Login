using Domain;

namespace Repository
{
    public interface IUserRepository
    {
        User Create(User user);
        User GetById(int id);
        User GetByUsername(string username);
        IEnumerable<User> GetAll();
    }
}