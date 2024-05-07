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
        Task<bool> RefreshToken();
        public Task<MainResponse> AuthenticateUser(LoginModel loginModel);
        Task<(bool IsSuccess, string ErrorMessage)> RegisterUser(RegistrationModel registerUser);
        Task<List<StudentModel>> GetAllStudents();
    }
}
