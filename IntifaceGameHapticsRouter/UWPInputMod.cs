using System;
using GHRXInputModInterface;

namespace IntifaceGameHapticsRouter
{
    class UWPInputMod : EasyHookMod
    {
        private Vibration _lastVibration = new Vibration();

        public UWPInputMod()
        {
            GHRXInputModInterface.GHRXInputModInterface.VibrationCommandReceived += OnVibrationCommand;
            GHRXInputModInterface.GHRXInputModInterface.VibrationPingMessageReceived += base.OnVibrationPingMessage;
            GHRXInputModInterface.GHRXInputModInterface.VibrationExceptionReceived += base.OnVibrationException;
            GHRXInputModInterface.GHRXInputModInterface.VibrationExitReceived += base.OnVibrationExit;
            GHRXInputModInterface.GHRXInputModInterface.VibrationLogMessageReceived += base.OnVibrationLogMessage;
        }

        public override void Attach(int aProcessId)
        {
            _log.Info("Attaching UWP Mod Payload");
            Attach<GHRUwpGamingInputPayload.GHRUwpGamingInputPayload>(aProcessId, "GHRUwpGamingInputPayload.dll");
        }

        protected override void OnVibrationCommand(object aObj, Vibration aVibration)
        {
            if (aVibration == _lastVibration)
            {
                return;
            }

            _lastVibration = aVibration;
            MessageReceivedHandler?.Invoke(this, new GHRProtocolMessageContainer { XInputHaptics = new XInputHaptics(aVibration.LeftMotorSpeed, aVibration.RightMotorSpeed, aVibration.ControllerIndex)});
        }

    }
}
