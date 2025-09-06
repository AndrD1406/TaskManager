using System;
using System.Collections.Generic;
namespace TaskManager.DataAccess.Models;

public class User : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string UserName { get; set; } 

    public string Email { get; set; }
    
    public string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual List<AppTask> Tasks { get; set; } = new List<AppTask>();

}
