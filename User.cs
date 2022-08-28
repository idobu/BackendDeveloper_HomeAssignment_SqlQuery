using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendDeveloperTest
{
    class User
    {
        public static int currId=0; // background data
        public int userID; // background data
        public string Email;
        public string FullName;
        public int Age;

        public User(string email, string fullname, int age)
        {
            this.userID = currId++;
            this.Email = email;
            this.Age = age;
            this.FullName = fullname;
        }
    }

    
}
