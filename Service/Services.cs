using Repository;
using Domain;

namespace Service
{
    public class Services
    {
        private readonly repositories _repository;

        public Services(repositories repository)
        {
            _repository = repository;
        }

        public User Create(User todo)
        {
            return _repository.Create(todo);
        }

        public User GetById(int id)
        {
            return _repository.GetById(id);
        }

        public User GetByUsername(string username)
        {
            return _repository.GetByUsername(username);
        }

        public IEnumerable<User> GetAll()
        {
            return _repository.GetAll();
        }
    }
}
