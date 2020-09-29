using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Factories
{
    class UsersFactory
    {
        public static User CreateUser(string username, string password)
        {
            
            var user = new User()
            {
                UserName = username,
                Email = password,
            };
            
            user.MembershipUser = new MembershipUser()
            {
                LoweredEmail = user.Email.ToLower(),
                LoweredUserName = user.UserName.ToLower(),
                CreateDate=DateTime.UtcNow,
                FailedPasswordAnswerAttemptCount=0,
                FailedPasswordAnswerAttemptWindowStart=null,
                FailedPasswordAttemptCount=0,
                FailedPasswordAttemptWindowStart=null,
                IsApproved=true,
                IsLockedOut=false,
                LastLockoutDate=null,
                LastLoginDate=null,
                LastPasswordChangedDate=null,
                
            };
            
            return user;
        }
    }
}
