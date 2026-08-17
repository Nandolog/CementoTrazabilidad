using System;

namespace CementoTrazabilidad.Blazor.Services
{
    public class AndenesStateService
    {
        public event Action? OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}