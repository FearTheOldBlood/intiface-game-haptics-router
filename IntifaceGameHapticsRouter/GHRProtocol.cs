using System;
using System.IO;
using System.Xml.Serialization;

namespace IntifaceGameHapticsRouter
{
    [Serializable]
    public class GHRProtocolMessageContainer
    {
        public void SendSerialized(Stream aStream)
        {
            // We can't use BinaryFormatter, because we're merging DLLs for the mods and the assemblies won't match. Use XML instead.
            var _formatter = new XmlSerializer(typeof(GHRProtocolMessageContainer));
            // TODO Should figure out a better way to do this, otherwise we're going to create a ton of formatters and clog the GC.
            _formatter.Serialize(aStream, this);
        }

        public static GHRProtocolMessageContainer Deserialize(Stream aStream)
        {
            var _formatter = new XmlSerializer(typeof(GHRProtocolMessageContainer));
            var obj = _formatter.Deserialize(aStream);
            return (GHRProtocolMessageContainer) obj;
        }

        // Basically copying how protobuf deals with aggregate messages. Only one of these should be valid at any time.
        public Log Log;
        public Ping Ping;
        public XInputHaptics XInputHaptics;
    }

    [Serializable]
    public class Ping
    {
    }

    [Serializable]
    public class Log
    {
        public string Message;

        public Log()
        {

        }

        public Log(string aMsg)
        {
            Message = aMsg;
        }
    }

    public enum HandSpec
    {
        LEFT,
        RIGHT
    }

    [Serializable]
    public class XInputHaptics
    {
        public uint LeftMotor;
        public uint RightMotor;
        public int ControllerIndex;

        public XInputHaptics()
        {

        }

        public XInputHaptics(uint aLeft, uint aRight, int controllerIndex)
        {
            LeftMotor = aLeft;
            RightMotor = aRight;
            ControllerIndex = controllerIndex;
        }
    }

}
