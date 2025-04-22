using Microsoft.JSInterop;

namespace TaskManager.Web.Services;

public class ToastService : IToastService
{
    private readonly IJSRuntime _jsRuntime;

    public ToastService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async void ShowSuccess(string message)
    {
        await _jsRuntime.InvokeVoidAsync("showToast", "success", message);
    }

    public async void ShowError(string message)
    {
        await _jsRuntime.InvokeVoidAsync("showToast", "error", message);
    }

    public async void ShowWarning(string message)
    {
        await _jsRuntime.InvokeVoidAsync("showToast", "warning", message);
    }

    public async void ShowInfo(string message)
    {
        await _jsRuntime.InvokeVoidAsync("showToast", "info", message);
    }
} 