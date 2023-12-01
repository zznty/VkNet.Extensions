using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WindowsWebServices;
using VkNet.Extensions.Auth.Abstractions.Interop;
using VkNet.Extensions.Auth.Extensions;
using VkNet.Extensions.Auth.Models.Auth;

namespace VkNet.Extensions.Auth.Interop.Win32;

public class WindowsPasskeyApi : IPlatformPasskeyApi
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    internal static bool IsSupported
    {
        get
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362)) // Windows 10 version 1903
                return false;
            
            var hResult = PInvoke.WebAuthNIsUserVerifyingPlatformAuthenticatorAvailable(out var authenticatorAvailable);
            
            return hResult.Succeeded && authenticatorAvailable && PInvoke.WebAuthNGetApiVersionNumber() > 4;
        }
    }
    
    public Task<string> RequestPasskeyAsync(string passkeyData)
    {
        return Task.Run(() =>
        {
            CancelCurrentOperationIfAny();

            BeginPasskey(passkeyData, out var authenticatorData, out var signature, out var userHandle,
                out var clientDataJson, out var usedCredential);
        
            var json = new JsonObject
            {
                ["response"] = new JsonObject
                {
                    ["authenticatorData"] = authenticatorData.Base64UrlEncode(),
                    ["signature"] = signature.Base64UrlEncode(),
                    ["userHandle"] = userHandle.Base64UrlEncode(),
                    ["clientDataJson"] = clientDataJson.Base64UrlEncode()
                },
                ["id"] = usedCredential.Base64UrlEncode(),
                ["rawId"] = usedCredential.Base64UrlEncode(),
                ["type"] = PInvoke.WebauthnCredentialTypePublicKey,
                ["clientExtensionResults"] = new JsonArray()
            };

            return json.ToJsonString();
        });
    }
    
    private static void CancelCurrentOperationIfAny()
    {
        var hResult = PInvoke.WebAuthNGetCancellationId(out var cancellationId);
        if (hResult.Failed) return;
        
        hResult = PInvoke.WebAuthNCancelCurrentOperation(cancellationId);
        if (hResult.Failed)
            throw new WebAuthNException(hResult);
    }
    
    private unsafe void BeginPasskey(string passkeyData, out byte[] authenticatorData, out byte[] signature,
        out byte[] userHandle, out string clientDataJson, out byte[] usedCredential)
    {
        var data = JsonSerializer.Deserialize<PasskeyDataResponse>(passkeyData, _jsonSerializerOptions);

        var dwVersion = PInvoke.WebAuthNGetApiVersionNumber();

        var publicKeyPtr = Marshal.StringToHGlobalUni(PInvoke.WebauthnCredentialTypePublicKey);

        var credList = data!.AllowCredentials
            .Where(b => b.Type == PInvoke.WebauthnCredentialTypePublicKey)
            .Select(b =>
            {
                var id = b.Id.Base64UrlDecode();
                var ptr = Marshal.AllocHGlobal(id.Length);
                Marshal.Copy(id, 0, ptr, id.Length);
            
                return new WEBAUTHN_CREDENTIAL
                {
                    dwVersion = dwVersion,
                    pwszCredentialType = (char*)publicKeyPtr,
                    pbId = (byte*)ptr,
                    cbId = (uint)id.Length
                };
        }).ToArray();

        clientDataJson =
            JsonSerializer.Serialize(new PInvoke.SecurityKeyClientData(PInvoke.SecurityKeyClientData.GetAssertion,
                data.Challenge, "https://id.vk.ru"));
        
        var clientDataJsonPtr = Utf8StringMarshaller.ConvertToUnmanaged(clientDataJson);
        var sha256Ptr = Marshal.StringToHGlobalUni("SHA-256");

        HRESULT hResult;
        WEBAUTHN_ASSERTION* assertion;
        try
        {
            fixed (WEBAUTHN_CREDENTIAL* credListPtr = &credList[0])
                hResult = PInvoke.WebAuthNAuthenticatorGetAssertion(new(Process.GetCurrentProcess().MainWindowHandle), data.RpId, new()
                {
                    pbClientDataJSON = clientDataJsonPtr,
                    cbClientDataJSON = (uint)Encoding.UTF8.GetByteCount(clientDataJson),
                    dwVersion = 2,
                    pwszHashAlgId = (char*)sha256Ptr,
                }, new WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS
                {
                    dwVersion = 4,
                    dwTimeoutMilliseconds = (uint)data.Timeout,
                    CredentialList = new()
                    {
                        cCredentials = (uint)credList.Length,
                        pCredentials = credListPtr
                    },
                    dwUserVerificationRequirement = data.UserVerification == "required" ? 1u : 0,
                    dwAuthenticatorAttachment = PInvoke.WebauthnAuthenticatorAttachmentCrossPlatformU2FV2
                }, out assertion);
        }
        finally
        {
            Utf8StringMarshaller.Free(clientDataJsonPtr);
            
            foreach (var credential in credList)
            {
                Marshal.FreeHGlobal((IntPtr)credential.pbId);
            }
            
            Marshal.FreeHGlobal(sha256Ptr);
            Marshal.FreeHGlobal(publicKeyPtr);
        }

        if (hResult.Failed)
            throw new WebAuthNException(hResult);

        authenticatorData = new ReadOnlySpan<byte>(assertion->pbAuthenticatorData, (int)assertion->cbAuthenticatorData)
            .ToArray();
        signature = new ReadOnlySpan<byte>(assertion->pbSignature, (int)assertion->cbSignature)
            .ToArray();
        userHandle = new ReadOnlySpan<byte>(assertion->pbUserId, (int)assertion->cbUserId)
            .ToArray();
        usedCredential = new ReadOnlySpan<byte>(assertion->Credential.pbId, (int)assertion->Credential.cbId)
            .ToArray();
        
        PInvoke.WebAuthNFreeAssertion(assertion);
    }
}

public class WebAuthNException(int hResult) : System.Exception(new(PInvoke.WebAuthNGetErrorName(new(hResult)).AsSpan()), Marshal.GetExceptionForHR(hResult));