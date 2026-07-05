using System.ComponentModel;

namespace SMNETCORE.Common.Enums
{
    public enum ExternalAuthType
    {
        Google = Globals.Authentication.ExternalAuthType.Google,
        Facebook = Globals.Authentication.ExternalAuthType.Facebook,
        Twitter = Globals.Authentication.ExternalAuthType.Twitter,
        AzureAD = Globals.Authentication.ExternalAuthType.AzureAD,
        Okta = Globals.Authentication.ExternalAuthType.Okta,
        SpecificClientAD = Globals.Authentication.ExternalAuthType.SpecificClientAD
    }

    public enum ExternalAuthParamType
    {
        ResponseParam = Globals.Authentication.ExternalAuthParamType.ResponseParam,
        RequestParameter = Globals.Authentication.ExternalAuthParamType.RequestParameter

    }

    public enum ExternalRequestAndResponseAuthenticationType
    {
        JSON = Globals.Authentication.RequestAndResponsetype.JSONPayloads,
        JSONUrl = Globals.Authentication.RequestAndResponsetype.JSONUrl,
        SAML = Globals.Authentication.RequestAndResponsetype.SAML
    }
}
