using Business.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Conrete
{
    public class UserManager : IUserService
    {
        public  UserDal _userDal = new UserDal();
        public void Add(User user)
        {
            _userDal.Add(user);
        }

        public void Delete(int Id)
        {
            _userDal.Delete(Id);
        }

        public User GetByUserName(string userName)
        {
            return _userDal.GetAll().Find(x => x.UserName == userName);
        }

        public List<User> GetAll()
        {
            return _userDal.GetAll();
        }

        public void Update(User user)
        {
            _userDal.Update(user);
        }
        
    }
}
