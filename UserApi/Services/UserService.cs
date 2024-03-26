using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiLib;
using WebApiLib.Abstraction;
using WebApiLib.DataStore.Entity;
using WebApiLib.Responce;

namespace UserApi.Services;

public class UserService : IUserService
{
    public readonly Func<AppDbContext> _context;
    private readonly Account _account;
    private readonly IMapper _mapper;

    public UserService(Func<AppDbContext> context, Account account, IMapper mapper)
    {
        _context = context;
        _account = account;
        _mapper = mapper;
    }

    public UserResponce UserAdd(LoginModel model)
    {
        var users = new List<UserEntity>();
        var responce = UserResponce.Ok();
        using (var context = _context())
        {

            var userExist = context.Users.Any(x => x.UserName!.ToLower().Equals(model.Name.ToLower()));
            users = context.Users.ToList();            
            if (userExist)
            {
                return UserResponce.UserExist(); ;

            }
            else
            {

                var entity = _mapper.Map<UserEntity>(model);
                entity.RoleType = new RoleEntity { Role = UserRole.User };

                context.Add(entity);
                context.SaveChanges();
                responce.UserId = entity.Id;
                return responce;
            }
        }
    }
    public Guid AdminAdd(LoginModel model)
    {
        using (var context = _context())
        {
            var userExist = context.Users.Any(x => x.UserName!.ToLower().Equals(model.Name.ToLower()));
            if (userExist)
            {
                return default;
            }

            var adminExists = context.Users.Any(u => u.RoleType!.Role == UserRole.Admin);
            if (adminExists)
            {
                return default;
            }

            var entity = _mapper.Map<UserEntity>(model);
            entity.RoleType = new RoleEntity
            {
                Role = UserRole.Admin
            };

            context.Users.Add(entity);
            context.SaveChanges();
            return entity.Id;

        }

    }


    public bool Delete(string userToDeleteName)
    {

        using (var context = _context())
        {
            var userToDelete = context.Users
                .Include(x => x.RoleType)
                .FirstOrDefault(x => x.UserName == userToDeleteName);

            if (userToDelete == null || userToDelete.RoleType!.Role == UserRole.Admin)
            {
                return false;
            }

            context.Users.Remove(userToDelete);
            context.SaveChanges();
            return true;
        }

    }

    public UserResponce Authentification(LoginModel model)
    {
        UserEntity user = null;
        using (var context = _context())
        {
            user = context.Users.Include(x => x.RoleType)
                .FirstOrDefault(x => x.UserName == model.Name);
            if (user is null)
            {
                return UserResponce.UserNotFound();
            }
            if (PasswordValidation(model.Password, user.Password!)) 
            {
                var responce = UserResponce.Ok();
                responce.Users.Add(_mapper.Map<UserModel>(user));
                return responce;
            }

            return UserResponce.WrongPassword();
        }
    }

    public List<UserModel> GetUsers()
    {

        using (var context = _context())
        {
            if (_account.Role.Equals(UserRole.Admin))
            {
                return context.Users
                    .Include(x => x.RoleType)
                    .Select(x => _mapper.Map<UserModel>(x))
                    .ToList();
            }
            else
            {
                return context.Users
                    .Select(x => new UserModel { UserName = x.UserName })
                    .ToList();
            }
        }

    }

    public UserEntity GetUser(Guid? userId, string? email)
    {
        var user = new UserEntity();
        using (var context = _context())
        {
            var query = context.Users.Include(x => x.RoleType).AsQueryable();
            if (!string.IsNullOrEmpty(email))
                query = query.Where(x => x.UserName == email);
            if (userId.HasValue)
                query = query.Where(x => x.Id == userId);

            user = query.FirstOrDefault();
        }
        if (user == null)
            return null!;
        if (_account.Role == UserRole.Admin || _account.Id == userId)
            return user;
        return null!;
    }

    private static bool PasswordValidation(string password1, string password2)
    {
        return password1 == password2;
    }


}

