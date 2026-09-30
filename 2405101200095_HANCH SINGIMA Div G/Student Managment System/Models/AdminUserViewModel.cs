using System.Collections.Generic;

namespace StudentManagementSystem.Models
{
    public class AdminUserViewModel
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public IList<string>? Roles { get; set; }
    }
}
