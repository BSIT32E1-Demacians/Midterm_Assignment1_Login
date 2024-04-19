using Domain;

namespace Repository
{
    public class repositories
    {
        private readonly List<Domain.User> _todos = new List<Domain.User>();
        private int _nextId = 1;

        public Domain.User Create(Domain.User user)
        {
            user.Id = _nextId++;
            _todos.Add(user);
            return user;
        }

        public Domain.User GetById(int id)
        {
            return _todos.FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<Domain.User> GetAll()
        {
            return _todos.ToList();
        }

    }
}
