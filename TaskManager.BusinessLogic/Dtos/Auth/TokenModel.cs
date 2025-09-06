using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.BusinessLogic.Dtos.Auth;

public class TokenModel
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
}
