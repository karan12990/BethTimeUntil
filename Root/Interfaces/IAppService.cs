using Root.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root.Interfaces
{
    public interface IAppService
    {
        public Task<MainResponse> AuthenticateUser(LoginModel loginModel);
    }
}
