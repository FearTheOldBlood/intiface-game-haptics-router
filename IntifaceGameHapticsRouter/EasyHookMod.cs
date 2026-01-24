using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using EasyHook;
using GHRXInputModInterface;
using NLog;

namespace IntifaceGameHapticsRouter
{
    abstract class EasyHookMod
    {
        public EventHandler<GHRProtocolMessageContainer> MessageReceivedHandler;
        private IpcServerChannel _hookServer;
        private string _channelName;
        protected Logger _log;

        public EasyHookMod()
        {
            _log = LogManager.GetCurrentClassLogger();
        }

        public abstract void Attach(int aProcessId);

        protected void Attach<T>(int aProcessId, string payloadName)
        {
            try
            {
                _hookServer = RemoteHooking.IpcCreateServer<GHRXInputModInterface.GHRXInputModInterface>(
                    ref _channelName,
                    WellKnownObjectMode.Singleton);
                var dllFile = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(typeof(T).Assembly.Location),
                    payloadName);

                _log.Info($"Beginning process injection on {aProcessId}...");
                _log.Info($"Injecting DLL {dllFile}");

                RemoteHooking.Inject(
                    aProcessId,
                    InjectionOptions.Default,
                    dllFile,
                    dllFile,
                    // the optional parameter list...
                    _channelName);
                _log.Info($"Finished process injection on {aProcessId}...");
            }
            catch (Exception ex)
            {
                Detach();
                _log.Error(ex);
            }
        }

        public void Detach()
        {
            GHRXInputModInterface.GHRXInputModInterface.Detach();
            _channelName = null;
            _hookServer = null;
        }

        protected abstract void OnVibrationCommand(object aObj, Vibration aVibration);

        protected void OnVibrationException(object aObj, Exception aEx)
        {
            _log.Error($"Remote Exception: {aEx}");
            Detach();
        }

        protected void OnVibrationLogMessage(object aObj, string aMsg)
        {
            _log.Info($"EasyHookMod: {aMsg}");
        }

        protected void OnVibrationPingMessage(object aObj, EventArgs aIgnored)
        {

        }

        protected void OnVibrationExit(object aObj, EventArgs aIgnored)
        {
            Detach();
        }
    }
}
