using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Wpf.Ui.Abstractions.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Views;

public partial class BrowserCaptchaPage : INavigableView<BrowserCaptchaViewModel>
{
    public BrowserCaptchaPage(BrowserCaptchaViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public BrowserCaptchaViewModel ViewModel { get; }

    private void WebView_OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;

        var settings = WebView.CoreWebView2.Settings;

        settings.AreBrowserAcceleratorKeysEnabled = false;
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreDefaultScriptDialogsEnabled = false;
#if !DEBUG
        settings.AreDevToolsEnabled = false;
        settings.IsBuiltInErrorPageEnabled = false;
#endif
        settings.IsGeneralAutofillEnabled = false;
        settings.IsPasswordAutosaveEnabled = false;
        settings.IsStatusBarEnabled = false;
        settings.IsWebMessageEnabled = true; // Включаем WebMessage
        settings.IsZoomControlEnabled = false;
        settings.UserAgent = "Mozilla/5.0 (Linux; Android 14; MusicX Build/UE1A.230829.036.A2; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/113.0.5672.136 Mobile Safari/537.36";

        WebView.CoreWebView2.WebResourceRequested += CoreWebView2OnWebResourceRequested;
        WebView.CoreWebView2.WebResourceResponseReceived += CoreWebView2OnWebResourceResponseReceived;
        WebView.CoreWebView2.WebMessageReceived += CoreWebView2OnWebMessageReceived;
        WebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All,
            CoreWebView2WebResourceRequestSourceKinds.All);

        // Инжектируем скрипт для перехвата XHR ответов
        WebView.CoreWebView2.DOMContentLoaded += async (s, args) =>
        {
            await WebView.CoreWebView2.ExecuteScriptAsync(@"
                (function() {
                    const originalFetch = window.fetch;
                    window.fetch = function(...args) {
                        return originalFetch.apply(this, args).then(async response => {
                            if (response.url.includes('captchaNotRobot.check')) {
                                const clone = response.clone();
                                try {
                                    const data = await clone.json();
                                    if (data.response && data.response.status === 'OK' && data.response.success_token) {
                                        window.chrome.webview.postMessage(JSON.stringify({
                                            type: 'captcha_success',
                                            success_token: data.response.success_token
                                        }));
                                    }
                                } catch (e) {
                                    console.error('Failed to parse captcha response:', e);
                                }
                            }
                            return response;
                        });
                    };

                    const originalXhrOpen = XMLHttpRequest.prototype.open;
                    const originalXhrSend = XMLHttpRequest.prototype.send;
                    XMLHttpRequest.prototype.open = function(method, url) {
                        this._method = method;
                        this._url = url;
                        return originalXhrOpen.apply(this, arguments);
                    };
                    XMLHttpRequest.prototype.send = function() {
                        this.addEventListener('load', function() {
                            if (this._url && this._url.includes('captchaNotRobot.check')) {
                                try {
                                    const data = JSON.parse(this.responseText);
                                    if (data.response && data.response.status === 'OK' && data.response.success_token) {
                                        window.chrome.webview.postMessage(JSON.stringify({
                                            type: 'captcha_success',
                                            success_token: data.response.success_token
                                        }));
                                    }
                                } catch (e) {
                                    console.error('Failed to parse XHR response:', e);
                                }
                            }
                        });
                        return originalXhrSend.apply(this, arguments);
                    };
                })();
            ");
        };
    }

    private void CoreWebView2OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            Debug.WriteLine($"WebMessage received: {e.WebMessageAsJson}");
            var node = JsonNode.Parse(e.WebMessageAsJson);
            if (node?["type"]?.ToString() == "captcha_success" &&
                node?["success_token"]?.ToString() is { } successToken)
            {
                Debug.WriteLine($"Captcha solved! Token: {successToken}");
                _ = Dispatcher.InvokeAsync(() => ViewModel.OnCaptchaSolved(successToken));
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Error processing WebMessage: {exception}");
        }
    }

    private async void CoreWebView2OnWebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        try
        {
            Debug.WriteLine(
                $"Response {e.Request.Method} {e.Request.Uri} - {e.Response.StatusCode} {e.Response.ReasonPhrase}");
            if (e.Request.Uri.Contains("captchaNotRobot.check", StringComparison.OrdinalIgnoreCase))
            {
                // Пытаемся прочитать ответ через Content
                try
                {
                    await using var content = await e.Response.GetContentAsync();
                    if (content is not null && content.CanRead)
                    {
                        using var reader = new StreamReader(content);
                        var responseText = await reader.ReadToEndAsync();
                        Debug.WriteLine($"Captcha response: {responseText}");

                        var node = JsonNode.Parse(responseText)?["response"];
                        if (node?["status"]?.ToString() == "OK" &&
                            node?["success_token"]?.ToString() is { } successToken)
                        {
                            Debug.WriteLine($"Captcha solved via response! Token: {successToken}");
                            _ = Dispatcher.InvokeAsync(() => ViewModel.OnCaptchaSolved(successToken));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to read response content: {ex}");
                }
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private static void CoreWebView2OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
#if DEBUG
        if (e.ResourceContext == CoreWebView2WebResourceContext.XmlHttpRequest && e.Request.Content is not null)
        {
            using var reader = new StreamReader(e.Request.Content, leaveOpen: true);
            Debug.WriteLine($"Request {e.Request.Method} {e.Request.Uri} -- {reader.ReadToEnd()}");
        }
        else Debug.WriteLine($"Request {e.Request.Method} {e.Request.Uri}");
#endif
        e.Request.Headers.SetHeader("X-Requested-With", "com.vkontakte.android");
    }

    private void WebView_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        LoadingRing.Visibility = Visibility.Collapsed;
    }
}