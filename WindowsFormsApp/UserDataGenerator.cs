using Business.Conrete;

using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp
{
    public class UserDataGenerator
    {
        public static List<User> CreateData()
        {
            UserManager _userManager = new UserManager();
            List<User> Users = new List<User>();
            Users = _userManager.GetAll();
            return Users;
        }
    }
}
