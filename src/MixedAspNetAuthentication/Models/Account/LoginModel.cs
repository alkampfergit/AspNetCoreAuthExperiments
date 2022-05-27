namespace MixedAspNetAuthentication.Models.Account
{
    public class LoginModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// Is different from null if we have some login error
        /// </summary>
        public string LoginError { get; set; }
    }
}
