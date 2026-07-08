using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using FirebaseAdmin.Auth;

namespace SMNETCORE.BaseAPI.Configs
{
    internal class FirebaseConfigs
    {
        public static void Configs(WebApplicationBuilder builder)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(
                    "firebase-service-account.json")
            });
            Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            "firebase-service-account.json");
        }
    }



    public class FirebaseAuthService
    {
        public async Task<FirebaseToken> VerifyAsync(string jwt)
        {
            return await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(jwt);
        }
    }
}
