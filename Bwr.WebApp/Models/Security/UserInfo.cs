namespace Bwr.WebApp.Models.PageModels.Security
{
    public class UserInfo
    {
        public UserInfo()
        {
            //Roles = new List<Role>();
            //Permissions = new List<Permission>();
        }
        
        public string FullName { get; set; }

        public string UserName { get; set; }

        //public string ImageUrl { get; set; }

        //public IList<Role> Roles { get; set; }

        //public IList<Permission> Permissions { get; set; }
    }
}