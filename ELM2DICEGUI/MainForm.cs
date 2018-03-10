using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Management;
using System.IO;
using System.ComponentModel.Design;
using Be.Windows.Forms;
using System.Threading;

namespace ELM2DICEGUI
{
    public partial class MainForm : Form
    {
        HexBox hexBox;
        Thread loggerThread;
        Logger loggerObject;
        ELM327 elm;
        
        private byte[] storedEEPROM = new byte[0x100];
        private void PopulateCOMPortList()
        {
            portCombo.Items.Clear();
           
            foreach ( string port in SerialPort.GetPortNames() )
            {
                portCombo.Items.Add(port);
            }

            if (portCombo.Items.Count > 0)
            {
                portCombo.SelectedIndex = 0;
            }
        }

        private string GetCOMPort()
        {
            string line = portCombo.SelectedItem.ToString();
            return line;
        }

        public MainForm()
        {
            int spacing = 0;

            InitializeComponent();
            PopulateCOMPortList();
            spacing = label2.Location.X;
            
            hexBox = new HexBox 
            {
                Width = progBar.Location.X + progBar.Size.Width - label2.Location.X,
                Height = 250,
                StringViewVisible = true,
                Location = new Point(
                    label2.Location.X,
                    label2.Location.Y + label2.Size.Height + spacing
                ),
                ByteProvider = new DynamicByteProvider(new byte[256]),
                UseFixedBytesPerLine = true,
                HexCasing = HexCasing.Upper,
                BytesPerLine = 16,
                ColumnInfoVisible = true,
                LineInfoVisible = true
            };
            int moveRight = hexBox.Location.X + hexBox.Size.Width -
                progBar.Location.X - progBar.Size.Width;

            int moveDown = hexBox.Location.Y + hexBox.Size.Height + spacing -
                progBar.Location.Y;

            progBar.Location = new Point(progBar.Location.X, progBar.Location.Y + moveDown);
            progBar.Size = new Size(progBar.Size.Width + moveRight, progBar.Size.Height);

            this.Size = new Size(this.Size.Width + moveRight, this.Size.Height + moveDown);
            
            Controls.Add(hexBox);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private bool readDataWithProgress(int address, int size, out byte[] data)
        {
            byte[] dump = new byte[size];
            bool success = false;
            data = null;
                
            ELM327 elm = new ELM327(GetCOMPort());
            elm.Open();
            if (elm.adapterInit() &&
                elm.startConnection())
            {
                for (int chunksize = 128; (chunksize > 4) && !success; chunksize >>= 1)
                {
                    bool attemptFailed = false;
                    for (int offset = 0; (offset < size) && !attemptFailed; offset += chunksize)
                    {
                        string resp;
                        if (elm.doRequest("23" + (address + offset).ToString("X6") + chunksize.ToString("X2"), out resp) &&
                            resp.StartsWith("63"))
                        {
                            byte[] rba = HexUtils.hexStringToByteArray(resp);
                            Array.Copy(rba, 1, dump, offset, rba.Length - 1);
                            progBar.Value = 100 * (offset + chunksize) / size;
                        }
                        else
                        {
                            attemptFailed = true;
                        }
                    }

                    success = !attemptFailed;
                }

                if (success)
                {
                    data = dump;
                }
                elm.closeConnection();
            }
            elm.Close();

            return success;
        }

        private void fullDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] dump;
            if (readDataWithProgress(0, 16 * 1024, out dump))
            {
                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "Bin files (*.bin)|*.bin|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        myStream.Write(dump, 0, dump.Length);
                        myStream.Flush();
                        myStream.Close();
                    }
                }
            }
        }

        private void downloadEEPROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] dump;
            if (readDataWithProgress(0x100, storedEEPROM.Length, out dump))
            {
                hexBox.ByteProvider = new DynamicByteProvider(dump);
                Array.Copy(dump, storedEEPROM, storedEEPROM.Length);
            }
        }

        private void uploadEEPROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ELM327 elm = new ELM327(GetCOMPort());
            elm.Open();
            if (elm.adapterInit() &&
                elm.startConnection())
            {
                //byte[] newEEPROM = byteViewer.GetBytes();
                byte[] newEEPROM = ((DynamicByteProvider)hexBox.ByteProvider).Bytes.ToArray();
                for ( int offset = 0; offset < storedEEPROM.Length; offset++)
                {
                    if ( storedEEPROM[offset] != newEEPROM[offset])
                    {
                        string resp;
                        do
                        {
                            if (elm.doRequest("3D" + (0x100 + offset).ToString("X6") + "01" + storedEEPROM[offset].ToString("X2"), out resp) &&
                                resp.StartsWith("7D"))
                            {
                                storedEEPROM[offset] = newEEPROM[offset];
                            }
                        } while (
                            !(elm.doRequest("23" + (0x100 + offset).ToString("X6") + "01", out resp) &&
                            resp.StartsWith("63") && HexUtils.hexStringToByteArray(resp)[1] == newEEPROM[offset])
                        );
                    }

                    progBar.Value = 100 * offset / storedEEPROM.Length;
                }
                //byteViewer.SetBytes(storedEEPROM);
                hexBox.ByteProvider = new DynamicByteProvider(storedEEPROM);

                elm.closeConnection();
            }
            elm.Close();
        }

        private void readEEPROMFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Bin files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            byte[] buffer = new byte[0x100];
                            if (myStream.Read(buffer, 0, buffer.Length) == 0x100)
                            {
                                //byteViewer.SetBytes(buffer);
                                hexBox.ByteProvider = new DynamicByteProvider(buffer);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void saveEEPROMToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Bin files (*.bin)|*.bin|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    byte[] buffer = ((DynamicByteProvider)hexBox.ByteProvider).Bytes.ToArray();
                    myStream.Write(buffer, 0, buffer.Length);
                    myStream.Flush();
                    myStream.Close();
                }
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if ( elm == null )
            {
                elm = new ELM327(GetCOMPort());
            }
            
            if ( !elm.IsOpen )
            {
                elm.Open();
            }

            if (!(elm.adapterInit() && elm.startConnection()))
            {
                MessageBox.Show("Could not initialize connection", "Logging failed");
                return;
            }
            
            Logger.LogValues[] logValues = new Logger.LogValues[]
            {
                Logger.LogValues.IGNITION_54,
                Logger.LogValues.IGNITION_15,
                Logger.LogValues.ENGINE_RUNNING_FROM_TRIONIC,
                Logger.LogValues.ACC_IN_BUS_FROM_ACC,
                Logger.LogValues.EVAPORATOR_TEMPERATURE,
                Logger.LogValues.AC_PRESSURE,
                Logger.LogValues.VEHICLE_SPEED,
                Logger.LogValues.COOLANT_TEMPERATURE,
                Logger.LogValues.COOLING_FAN_HIGH_RELAY_2,
                Logger.LogValues.COOLING_FAN_HIGH_RELAY_1,
                Logger.LogValues.COOLING_FAN_LOW_RELAY,
                Logger.LogValues.OUTSIDE_TEMPERATURE
            };

            loggerObject = new Logger(elm, 800, logValues);
            loggerThread = new Thread(loggerObject.DoLogging);
            loggerThread.Start();
            while (!loggerThread.IsAlive) ;
            Thread.Sleep(1);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loggerObject.RequestStop();
            MessageBox.Show("Finished logging", "Logging stopped");
            loggerThread.Join();    
        }

    }

    public class Logger
    {
        ELM327 elm;
        int period;
        private volatile bool shouldStop;
        private LogValues[] logValues;
        private StreamWriter writer;
        private int[] currentState;
        public enum LogValues 
        {
            REAR_FOG_LIGHT_FEED,
            MAP_AND_INTERIOR_LIGHT_FEED,
            DOOR_LIGHT_FEED,
            DIRECTION_INDICATOR_FEED,
            IGNITION_B,
            IGNITION_54,
            IGNITION_15,
            RIGHT_DIRECTION_INDICATOR_SWITCH,
            LEFT_DIRECTION_INDICATOR_SWITCH,
            HIGH_AND_LOW_BEAM_SWITCH,
            HAZARD_SWITCH,
            REAR_FOG_LIGHT_SWITCH,
            FRONT_FOG_LIGHT_SWITCH,
            LIGHT_SWITCH_POSITION_2_1,
            LIGHT_SWITCH_POSITION_0,
            ACC_IN_WIRE_FROM_MCC,
            REAR_HEATED_WINDOW_WIRE_FROM_MCC,
            FRONT_WIPER_PARK_POSITION,
            REAR_WINDOW_WASHER_SWITCH,
            REAR_WINDOW_WIPER_SWITCH,
            FRONT_WASH_WIPE_SWITCH,
            FRONT_INTERMITTENT_WIPE,
            DRIVER_DOOR_OPEN_FROM_TWICE,
            ENGINE_RUNNING_FROM_TRIONIC,
            REAR_HEATED_WINDOW_BUS_FROM_ACC,
            ACC_IN_BUS_FROM_ACC,
            REVERSE_GEAR_SWITCH,
            NIGHT_PANEL_FROM_SID,
            HAZARD_REQUEST_FROM_TWICE,
            TRUNK_OPEN_FROM_TWICE,
            DOOR_REAR_RIGHT_FROM_TWICE,
            DOOR_REAR_LEFT_FROM_TWICE,
            DOOR_PASSENGER_FROM_TWICE,
            EVAPORATOR_TEMPERATURE,
            BATTERY_VOLTAGE,
            WIPER_INTERMITTENT_TIME,
            AC_PRESSURE,
            VEHICLE_SPEED,
            RHEOSTAT_VALUE,
            RHEOSTAT_NIGHT_PANEL,
            COOLANT_TEMPERATURE,
            RIGHT_DIRECTION_INDICATOR_OUT,
            LEFT_DIRECTION_INDICATOR_OUT,
            REAR_HEATED_WINDOW_RELAY,
            FRONT_WIPER_RELAY,
            RELAY_HEADLAMP_WIPER_REAR_WIPER,
            COOLING_FAN_HIGH_RELAY_2,
            COOLING_FAN_HIGH_RELAY_1,
            COOLING_FAN_LOW_RELAY,
            MAIN_RELAY,
            SHIFT_BEAM_RELAY,
            FRONT_FOG_LIGHT_RELAY,
            REAR_FOG_LIGHT_OUT,
            COURTESY_LIGHTS_OUTPUT,
            TRUNK_LIGHT_OUT,
            MAP_LIGHT_OUT,
            INTERIOR_LIGHT_OUT,
            OUTSIDE_TEMPERATURE
        }

        public Logger(ELM327 elm, int period, LogValues[] logValues)
        {
            this.elm = elm;
            this.period = period;
            this.logValues = logValues;
            currentState = new int[logValues.Length];

            for (int idx = 0; idx < logValues.Length; idx++)
                currentState[idx] = -1;
        }

        public void DoLogging()
        {
            writer = new StreamWriter(String.Format("diceval_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
            for (int idx = 0; idx < logValues.Length; idx++)
                writer.Write(String.Format("{0}, ", logValues[idx].ToString()));
            writer.WriteLine("TIME");

            while (!shouldStop)
            {
                byte[] logdata;
                if (elm.getDataByLocalId(1, out logdata))
                {
                    int[] values = FilterValues(ParseLiveData(logdata));
                    bool stateChanged = false;
                    for (int idx = 0; idx < values.Length; idx++ )
                        if (currentState[idx] != values[idx])
                        {
                            currentState[idx] = values[idx];
                            stateChanged = true;
                        }


                    if (stateChanged)
                    {
                        for (int idx = 0; idx < values.Length; idx++)
                            writer.Write(String.Format("{0}, ", values[idx]));
                        writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                    }
                }
                Thread.Sleep(period);
            }
            writer.Flush();
            writer.Close();
        }

        public void RequestStop()
        {
            shouldStop = true;
        }

        private int[] FilterValues(Dictionary<LogValues, int> data)
        {
            int[] retVal = new int[logValues.Length];

            for (int idx = 0; idx < logValues.Length; idx++)
            {
                retVal[idx] = -1;
                data.TryGetValue(logValues[idx], out retVal[idx]);
            }

            return retVal;
        }

        private Dictionary<LogValues, int> ParseLiveData(byte[] data)
        {
            Dictionary<LogValues, int> retval = new Dictionary<LogValues, int>();
            int bgn;// = data.Length - 21;
            bgn = 9;
            
            retval.Add(LogValues.REAR_FOG_LIGHT_FEED, (data[bgn + 0x00] & 0x80) !=0 ? 1 : 0);
            retval.Add(LogValues.MAP_AND_INTERIOR_LIGHT_FEED, (data[bgn + 0x00] & 0x40) != 0 ? 1 : 0);
            retval.Add(LogValues.DOOR_LIGHT_FEED, (data[bgn + 0x00] & 0x20) != 0 ? 1 : 0);
            retval.Add(LogValues.DIRECTION_INDICATOR_FEED, (data[bgn + 0x00] & 0x10) != 0 ? 1 : 0);
            retval.Add(LogValues.IGNITION_B, (data[bgn + 0x00] & 0x08) != 0 ? 1 : 0);
            retval.Add(LogValues.IGNITION_54, (data[bgn + 0x00] & 0x04) != 0 ? 1 : 0);
            retval.Add(LogValues.IGNITION_15, (data[bgn + 0x00] & 0x02) != 0 ? 1 : 0);

            retval.Add(LogValues.RIGHT_DIRECTION_INDICATOR_SWITCH, (data[bgn + 0x01] & 0x80) != 0 ? 1 : 0);
            retval.Add(LogValues.LEFT_DIRECTION_INDICATOR_SWITCH, (data[bgn + 0x01] & 0x40) != 0 ? 1 : 0);
            retval.Add(LogValues.HIGH_AND_LOW_BEAM_SWITCH, (data[bgn + 0x01] & 0x20) != 0 ? 1 : 0);
            retval.Add(LogValues.HAZARD_SWITCH, (data[bgn + 0x01] & 0x10) != 0 ? 1 : 0);
            retval.Add(LogValues.REAR_FOG_LIGHT_SWITCH, (data[bgn + 0x01] & 0x08) != 0 ? 1 : 0);
            retval.Add(LogValues.FRONT_FOG_LIGHT_SWITCH, (data[bgn + 0x01] & 0x04) != 0 ? 1 : 0);
            retval.Add(LogValues.LIGHT_SWITCH_POSITION_2_1, (data[bgn + 0x01] & 0x02) != 0 ? 1 : 0);
            retval.Add(LogValues.LIGHT_SWITCH_POSITION_0, (data[bgn + 0x01] & 0x01) != 0 ? 1 : 0);

            retval.Add(LogValues.ACC_IN_WIRE_FROM_MCC, (data[bgn + 0x02] & 0x80) != 0 ? 1 : 0);
            retval.Add(LogValues.REAR_HEATED_WINDOW_WIRE_FROM_MCC, (data[bgn + 0x02] & 0x40) != 0 ? 1 : 0);
            retval.Add(LogValues.FRONT_WIPER_PARK_POSITION, (data[bgn + 0x02] & 0x10) != 0 ? 1 : 0);
            retval.Add(LogValues.REAR_WINDOW_WASHER_SWITCH, (data[bgn + 0x02] & 0x08) != 0 ? 1 : 0);
            retval.Add(LogValues.REAR_WINDOW_WIPER_SWITCH, (data[bgn + 0x02] & 0x04) != 0 ? 1 : 0);
            retval.Add(LogValues.FRONT_WASH_WIPE_SWITCH, (data[bgn + 0x02] & 0x02) != 0 ? 1 : 0);
            retval.Add(LogValues.FRONT_INTERMITTENT_WIPE, (data[bgn + 0x02] & 0x01) != 0 ? 1 : 0);

            retval.Add(LogValues.DRIVER_DOOR_OPEN_FROM_TWICE, (data[bgn + 0x03] & 0x20) != 0 ? 1 : 0);
            retval.Add(LogValues.ENGINE_RUNNING_FROM_TRIONIC, (data[bgn + 0x03] & 0x08) != 0 ? 1 : 0);
            retval.Add(LogValues.REAR_HEATED_WINDOW_BUS_FROM_ACC, (data[bgn + 0x03] & 0x04) != 0 ? 1 : 0);
            retval.Add(LogValues.ACC_IN_BUS_FROM_ACC, (data[bgn + 0x03] & 0x02) != 0 ? 1 : 0);
            retval.Add(LogValues.REVERSE_GEAR_SWITCH, (data[bgn + 0x03] & 0x01) != 0 ? 1 : 0);

            retval.Add(LogValues.NIGHT_PANEL_FROM_SID, (data[bgn + 0x04] & 0x20) != 0 ? 1 : 0);
            retval.Add(LogValues.HAZARD_REQUEST_FROM_TWICE, (data[bgn + 0x04] & 0x10) != 0 ? 1 : 0);
            retval.Add(LogValues.TRUNK_OPEN_FROM_TWICE, (data[bgn + 0x04] & 0x08) != 0 ? 1 : 0);
            retval.Add(LogValues.DOOR_REAR_RIGHT_FROM_TWICE, (data[bgn + 0x04] & 0x04) != 0 ? 1 : 0);
            retval.Add(LogValues.DOOR_REAR_LEFT_FROM_TWICE, (data[bgn + 0x04] & 0x02) != 0 ? 1 : 0);
            retval.Add(LogValues.DOOR_PASSENGER_FROM_TWICE, (data[bgn + 0x04] & 0x01) != 0 ? 1 : 0);

            retval.Add(LogValues.EVAPORATOR_TEMPERATURE, data[bgn + 0x05] * 256 + data[bgn + 0x06]);
            retval.Add(LogValues.BATTERY_VOLTAGE, data[bgn + 0x07]);
            retval.Add(LogValues.WIPER_INTERMITTENT_TIME, data[bgn + 0x08]);
            retval.Add(LogValues.AC_PRESSURE, data[bgn + 0x09]);
            retval.Add(LogValues.VEHICLE_SPEED, data[bgn + 0x0A] * 256 + data[bgn + 0x0B]);
            retval.Add(LogValues.RHEOSTAT_VALUE, data[bgn + 0x0C]);
            retval.Add(LogValues.RHEOSTAT_NIGHT_PANEL, data[bgn + 0x0D]);
            retval.Add(LogValues.COOLANT_TEMPERATURE, data[bgn + 0x0E]);


            retval.Add(LogValues.RIGHT_DIRECTION_INDICATOR_OUT, (data[bgn + 0x0F] & 0x80) != 0 ? 1 : 0);
            retval.Add(LogValues.LEFT_DIRECTION_INDICATOR_OUT, (data[bgn + 0x0F] & 0x40) != 0 ? 1 : 0);
            retval.Add(LogValues.REAR_HEATED_WINDOW_RELAY, (data[bgn + 0x0F] & 0x20) != 0 ? 1 : 0);
            retval.Add(LogValues.FRONT_WIPER_RELAY, (data[bgn + 0x0F] & 0x10) != 0 ? 1 : 0);
            retval.Add(LogValues.RELAY_HEADLAMP_WIPER_REAR_WIPER, (data[bgn + 0x0F] & 0x08) != 0 ? 1 : 0);
            retval.Add(LogValues.COOLING_FAN_HIGH_RELAY_2, (data[bgn + 0x0F] & 0x04) != 0 ? 1 : 0);
            retval.Add(LogValues.COOLING_FAN_HIGH_RELAY_1, (data[bgn + 0x0F] & 0x02) != 0 ? 1 : 0);
            retval.Add(LogValues.COOLING_FAN_LOW_RELAY, (data[bgn + 0x0F] & 0x01) != 0 ? 1 : 0);

            retval.Add(LogValues.MAIN_RELAY, (data[bgn + 0x10] & 0x80) != 0 ? 1 : 0);
            retval.Add(LogValues.SHIFT_BEAM_RELAY, (data[bgn + 0x10] & 0x40) != 0 ? 1 : 0);
            retval.Add(LogValues.FRONT_FOG_LIGHT_RELAY, (data[bgn + 0x10] & 0x20) != 0 ? 1 : 0);
            retval.Add(LogValues.REAR_FOG_LIGHT_OUT, (data[bgn + 0x10] & 0x10) != 0 ? 1 : 0);
            retval.Add(LogValues.COURTESY_LIGHTS_OUTPUT, (data[bgn + 0x10] & 0x08) != 0 ? 1 : 0);
            retval.Add(LogValues.TRUNK_LIGHT_OUT, (data[bgn + 0x10] & 0x04) != 0 ? 1 : 0);
            retval.Add(LogValues.MAP_LIGHT_OUT, (data[bgn + 0x10] & 0x02) != 0 ? 1 : 0);
            retval.Add(LogValues.INTERIOR_LIGHT_OUT, (data[bgn + 0x10] & 0x01) != 0 ? 1 : 0);

            retval.Add(LogValues.OUTSIDE_TEMPERATURE, data[bgn + 0x11] * 256 + data[bgn + 0x12]);
            
            return retval;
        }
    }
}
