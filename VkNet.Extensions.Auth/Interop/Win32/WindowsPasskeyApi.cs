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
    
    public Task<string> RequestPasskeyAsync(string passkeyData, string origin)
    {
        return Task.Run(() =>
        {
            CancelCurrentOperationIfAny();

            BeginPasskey(passkeyData, origin, out var authenticatorData, out var signature, out var userHandle,
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
    
    private unsafe void BeginPasskey(string passkeyData, string origin, out byte[] authenticatorData, out byte[] signature,
        out byte[] userHandle, out string clientDataJson, out byte[] usedCredential)
    {
        var data = JsonSerializer.Deserialize<PasskeyDataResponse>(passkeyData, _jsonSerializerOptions)!;

        HRESULT hResult;
        WEBAUTHN_ASSERTION* assertion;
        fixed (char* publicKeyPtr = &PInvoke.WebauthnCredentialTypePublicKey.GetPinnableReference())
        {
            clientDataJson =
                JsonSerializer.Serialize(new PInvoke.SecurityKeyClientData(PInvoke.SecurityKeyClientData.GetAssertion,
                    data.Challenge, origin));

            var clientDataJsonPtr = Utf8StringMarshaller.ConvertToUnmanaged(clientDataJson);
            ReadOnlySpan<WEBAUTHN_CREDENTIAL> credList = [];
            try
            {
                credList = CreateWebauthnCredentials(data, publicKeyPtr);

                fixed (char* sha256Ptr = &"SHA-256".GetPinnableReference())
                fixed (WEBAUTHN_CREDENTIAL* credListPtr = &credList.GetPinnableReference())
                    hResult = PInvoke.WebAuthNAuthenticatorGetAssertion(
                        new(Process.GetCurrentProcess().MainWindowHandle), data.RpId, new()
                        {
                            pbClientDataJSON = clientDataJsonPtr,
                            cbClientDataJSON = (uint)Encoding.UTF8.GetByteCount(clientDataJson),
                            dwVersion = 2,
                            pwszHashAlgId = sha256Ptr,
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
                    Marshal.FreeHGlobal((nint)credential.pbId);
                }
            }
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

    private static unsafe ReadOnlySpan<WEBAUTHN_CREDENTIAL> CreateWebauthnCredentials(PasskeyDataResponse data, char* publicKeyPtr)
    {
        var dwVersion = PInvoke.WebAuthNGetApiVersionNumber();
        var credentials = new List<WEBAUTHN_CREDENTIAL>();
        foreach (var credential in data.AllowCredentials
                     .Where(b => b.Type == PInvoke.WebauthnCredentialTypePublicKey))
        {
            var id = credential.Id.Base64UrlDecode();
            var ptr = Marshal.AllocHGlobal(id.Length);
            Marshal.Copy(id, 0, ptr, id.Length);

            credentials.Add(new WEBAUTHN_CREDENTIAL
            {
                dwVersion = dwVersion,
                pwszCredentialType = publicKeyPtr,
                pbId = (byte*)ptr,
                cbId = (uint)id.Length
            });
        }
        
        return CollectionsMarshal.AsSpan(credentials);
    }
}

public class WebAuthNException(int hResult) : System.Exception(new(PInvoke.WebAuthNGetErrorName(new(hResult)).AsSpan()), Marshal.GetExceptionForHR(hResult));