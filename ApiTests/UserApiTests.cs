using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiLib.DataCheck;
using WebApiLib.DataStore.Entity;

namespace ApiTests
{
    [TestFixture]
    public class UserApiTests
    {
        [Test]
        public void AuthentificateUserNullTest()
        {
            UserEntity user = null;

            Assert.IsNull(user);
        }

        [Test]
        public void CheckWrongPassword()
        {
            Assert.IsTrue(!Password.Check("12345"));
        }
        [Test]
        public void CheckCorrectPassword()
        {
            Assert.IsTrue(Password.Check("Pa55w0rd"));
        }

        [Test]
        public void CheckInCorrectEmail()
        {
            Assert.IsTrue(!UserName.Check("test"));
        }
        [Test]
        public void CheckCorrectEmail()
        {
            Assert.IsTrue(UserName.Check("correct@gmail.com"));
        }


    }
}
