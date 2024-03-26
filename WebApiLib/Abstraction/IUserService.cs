

using WebApiLib.DataStore.Entity;
using WebApiLib.Responce;

namespace WebApiLib.Abstraction
{
    public interface IUserService
    {
        public UserResponce UserAdd(LoginModel model);
        public bool Delete(string userToDeleteName);
        public Guid AdminAdd(LoginModel model);
        public UserResponce Authentification(LoginModel model);
        public List<UserModel> GetUsers();
        public UserEntity GetUser(Guid? userId, string? email);


    }
}

