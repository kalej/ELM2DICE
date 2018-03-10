using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Globalization;

namespace ELM2DICEGUI
{
    public class ELM327 : SerialPort
    {
        public ELM327(string portName)
        {
            this.PortName = portName;
            this.BaudRate = 38400;
            this.DataBits = 8;
            this.Parity = Parity.None;
            this.StopBits = StopBits.One;
            this.WriteBufferSize = 1024;
            this.ReadBufferSize = 1024;
            this.ReadTimeout = 3000;
            this.WriteTimeout = 1000;
        }

        public Boolean getVersion(out string version)
        {
            Write("ATI\r");
            version = ReadTo(">");
            return version.Contains("ELM");
        }

        private Boolean sendATCommand(string req)
        {
            Write(req);
            string reply = ReadTo(">");
            return reply.Contains("OK");
        }

        public Boolean addHeaders(Boolean addKWPHeadersToSerialReply)
        {
            return sendATCommand("ATH" + (addKWPHeadersToSerialReply ? "1" : "0") + "\r");
        }

        public Boolean enableEcho(Boolean enable)
        {
            return sendATCommand("ATE" + (enable ? "1" : "0") + "\r");
        }

        public Boolean allowLongMessages(Boolean allow)
        {
            return sendATCommand("AT" + (allow ? "A" : "N") + "L\r");
        }

        public Boolean addLfeed(Boolean addLineFeed)
        {
            return sendATCommand("ATL" + (addLineFeed ? "1" : "0") + "\r");
        }

        public Boolean setProtocol(int protocolId)
        {
            return sendATCommand("ATSP" + protocolId + "\r");
        }

        public Boolean setSpeed(int bauds)
        {
            string command;
            switch (bauds)
            {
                case 10400:
                    command = "ATIB10\r";
                    break;
                case 9600:
                    command = "ATIB96\r";
                    break;
                case 4800:
                    command = "ATIB48\r";
                    break;
                default:
                    return false;
            }
            return sendATCommand(command);
        }

        public Boolean setHeader(string header)
        {
            return sendATCommand("ATSH" + header + "\r");
        }

        public Boolean setWakeupMessage(string message)
        {
            return sendATCommand("ATWM" + message + "\r");
        }

        public Boolean setWakeupPeriod(int milliseconds)
        {
            return sendATCommand("ATSW" + (milliseconds / 20).ToString("X2") + "\r");
        }

        public Boolean adapterInit()
        {
            string dummy;
            return getVersion(out dummy)
                && addHeaders(false)
                && allowLongMessages(true)
                && addLfeed(true)
                && enableEcho(false)
                && setProtocol(5)
                && setSpeed(10400)
                && setHeader("8041F1");
        }

        public Boolean fastInit()
        {
            return sendATCommand("ATSW00\r");
        }

        public Boolean startConnection()
        {
            string resp;
            return fastInit() &&
                doRequest("81", out resp) &&
                resp.StartsWith("C1");
        }

        public Boolean closeConnection()
        {
            string resp;
            return doRequest("82", out resp) &&
                resp.StartsWith("C2");
        }

        public Boolean getDataByLocalId(int id, out byte[] data)
        {
            string resp;
            //resp = "61 01 00 00 00 00 00 00 00 FF 01 00 02 00 02 71 7A 23 12 00 00 FD 8F 40 00 02 02 42 00";
            if (doRequest("21" + id.ToString("X02"), out resp))
            //if ( true )
            {
                data = HexUtils.hexStringToByteArray(resp);
            }
            else
            {
                data = null;
            }
            return resp.StartsWith("61");
        }

        public Boolean readMemory(int address, int length, int chunksize, out byte[] data)
        {
            data = new byte[length];
            string resp;
            for (int offset = 0; offset < length; offset += chunksize)
            {
                if (doRequest("23" + (address + offset).ToString("X6") + chunksize.ToString("X2"), out resp) &&
                    resp.StartsWith("63"))
                {
                    byte[] rba = HexUtils.hexStringToByteArray(resp);
                    Array.Copy(rba, 1, data, offset, rba.Length - 1);
                }
                else
                {
                    data = null;
                    return false;
                }
            }

            return true;
        }

        public Boolean doRequest(string req, out string result)
        {
            bool reconnected = false;
            string rcvd;
            result = null;
            do
            {
                Write(req + '\r');
                rcvd = ReadTo(">");

                if (rcvd.Contains("?"))
                {
                    return false;
                }

                if (rcvd.Contains("ERROR"))
                {
                    return false;
                }

                if (rcvd.Contains("NO DATA"))
                {
                    if (reconnected)
                        return false;

                    startConnection();
                    reconnected = true;
                    continue;
                }

                break;
            } while (true);

            rcvd = rcvd.Trim();
            if (rcvd.Length == 0)
            {
                return false;
            }

            result = rcvd;
            return true;
        }

        public Boolean doRequest(byte[] req, out byte[] result)
        {
            string res;
            result = null;
            if (!doRequest(HexUtils.byteArrayToHexString(req), out res))
                return false;

            result = HexUtils.hexStringToByteArray(res);
            return true;
        }
    }
}
