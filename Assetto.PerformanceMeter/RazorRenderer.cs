using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Assetto.PerformanceMeter;

public class RazorRenderer
{
    private readonly HtmlRenderer _renderer;

    public RazorRenderer(HtmlRenderer renderer)
    {
        _renderer = renderer;
    }

    public async Task<string> RenderToStringAsync<TComponent>(object? parameters) where TComponent : ComponentBase
    {
        return await _renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameterView = ParameterView.FromDictionary(ToDictionary(parameters));
            var output = await _renderer.RenderComponentAsync<TComponent>(parameterView);

            return output.ToHtmlString();
        });
    }

    private static Dictionary<string, object?> ToDictionary(object? obj)
    {
        if (obj == null) return new Dictionary<string, object?>();
        
        var props = obj.GetType().GetProperties();
        return props.ToDictionary(x => x.Name, x => x.GetValue(obj));
    }
}
