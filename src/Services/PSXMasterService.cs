using System.Net;
using PSXMaster.Core;

namespace PSXMaster.Services;
public partial class PSXMasterService : IPSXMasterService
{
    private HttpListenerHelp? _listener;

    public async Task Connect(IPAddress ip, int port, bool isConnected, UpdataUrlLog urlLog)
    {
        if (_listener != null)
        {
            _listener.Dispose();
            _listener = null;
        }
        else
        {
            _listener = new(ip, port, urlLog);
            _listener.Start();
        }

        await Task.CompletedTask;
    }
}
