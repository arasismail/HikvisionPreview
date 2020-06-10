using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Abstract
{
    public interface IUserDal
    {
        List<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(int Id);
    }
}
