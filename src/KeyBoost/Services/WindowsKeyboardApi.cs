using KeyBoost.Core.Models;
using KeyBoost.Core.Services;
using KeyBoost.Native;

namespace KeyBoost.Services;
internal sealed class WindowsKeyboardApi : IKeyboardApi
{
    public KeyboardSettings Read() => KeyboardNative.Read();
    public void Apply(KeyboardSettings settings) => KeyboardNative.Apply(settings);
}
