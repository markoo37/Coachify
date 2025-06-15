using System.Security.Claims;

namespace CoachCRM.Extensions
{
    public static class UserExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("userId")?.Value;

            if (userIdClaim == null)
            {
                throw new Exception("UserId claim not found");
            }

            return int.Parse(userIdClaim);
        }
    }
}