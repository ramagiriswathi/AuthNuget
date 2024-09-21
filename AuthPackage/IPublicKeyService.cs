using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AuthPackage
{
    public interface IPublicKeyService
    {
        //  Task<JsonWebKeySet> GetJwks();
        Task<IEnumerable<SecurityKey>> GetSigningKeysFromJwkAsync(string publicKeyServiceUrl);
    }
}
