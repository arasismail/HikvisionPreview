using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IUserService
    {
        List<User> GetAll();
        User GetByUserName(string userName);
        void Add(User user);
        void Update(User user);
        void Delete(int Id);
    }
}
