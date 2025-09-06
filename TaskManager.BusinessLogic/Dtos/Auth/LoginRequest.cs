using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.BusinessLogic.Dtos.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Enter name or email")]
    public string UserNameOrEmail { get; set; }

    [Required(ErrorMessage = "Password can't be blank")]
    public string Password { get; set; } 
}
