using Domain;

namespace Repository
{
    public class repositories
    {
        private readonly List<Domain.User> _todos = new List<Domain.User>();
        private int _nextId = 1;
    }
}
