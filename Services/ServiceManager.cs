using System.Collections.Concurrent;

namespace My_Dashboard.Services
{
    public class ServiceManager
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _services = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _restartServices = new();
        private readonly ConcurrentDictionary<string,Func<CancellationTokenSource>> _updateRts = new ();




        public void RegisterService(string name, CancellationTokenSource stopToken, Func<CancellationTokenSource> updateRts)
        {
            _services[name] = stopToken;
            _updateRts[name] = updateRts;
            _restartServices[name] = _updateRts[name]();
        }

        public void RestartService(string name)
        {
            if (_restartServices.TryGetValue(name, out var rts))
            {
                rts.Cancel();
            }
        }

        public void UpdateRestartService(string name)
        {
            //if (_restartServices.TryGetValue(name, out var rts))
            //{
            _restartServices[name] = _updateRts[name](); // calling delegated method
            //}
        }
        public void StopService(string name)
        {
            if (_services.TryGetValue(name, out var cts))
            {
                cts.Cancel();
            }

            if (_restartServices.TryGetValue(name, out var rts))
            {
                rts.Cancel();
            }
        }

    }
}
    