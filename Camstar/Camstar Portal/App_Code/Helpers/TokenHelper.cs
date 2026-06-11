// Copyright Siemens 2025
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for TokenHelper

// </summary>
namespace Camstar.WebPortal.Helpers
{
    public class TokenHelper
    {
        public TokenHelper()
        {
        }

        public TokenHelper(string jwtToken)
        {
        }

        public JwtSecurityToken DecodeJwt(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            return token;
        }
    }

}

