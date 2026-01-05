using System.Net;
using PSXMaster.Core;

namespace PSXMaster.Services;
public interface IPSXMasterService
{
    Task Connect(IPAddress ip, int port, bool isConnected, UpdataUrlLog urlLog);
}
