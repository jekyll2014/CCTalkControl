using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

public class ParseEscPos
{
    //INTERFACES
    //source of the data to parce
    //public static string sourceData = ""; //in Init()
    public static List<byte> sourceData = new List<byte>(); //in Init()
    //source of the command description (DataTable)
    public static DataTable commandDataBase = new DataTable(); //in Init()
    public static byte deviceAddress = 0; //in Init()
    public static byte hostAddress = 0; //in Init()

    //INTERNAL VARIABLES
    //Command list preselected
    //private static Dictionary<int, string> _commandList = new Dictionary<int, string>(); //in Init()

    public const byte ackSign = 0x00;
    public const byte nackSign = 0x05;
    public const byte busySign = 0x05;
    public static bool itIsReply = false;
    public static bool itIsBroadcast = false;
    public static bool itIsReplyACK = false;
    public static bool itIsReplyNACK = false;
    public static bool crcFailed = false;
    public static bool lengthIncorrect = false;

    //RESULT VALUES
    public static int dataFrameLength;

    //place of the frame start in the text
    public static int commandFramePosition; //in findCommand()
    //Command text
    public static string commandName; //in findCommand()
    //Command desc
    public static string commandDesc; //in findCommand()
    //string number of the command found
    public static int commandDbLineNum; //in findCommand()
    //height of the command
    public static int commandDbHeight; //in findCommand()

    //string number of the command found
    public static List<int> commandParamDbLineNum = new List<int>(); //in findCommand()
    //list of command parameters real sizes
    public static List<int> commandParamSize = new List<int>(); //in findCommand()
    //list of command parameters sizes defined in the database
    public static List<string> commandParamSizeDefined = new List<string>(); //in findCommand()
    //command parameter description
    public static List<string> commandParamDesc = new List<string>(); //in findCommand()
    //command parameter type
    public static List<string> commandParamType = new List<string>(); //in findCommand()
    //command parameter RAW value
    public static List<List<byte>> commandParamRAWValue = new List<List<byte>>(); //in findCommand()
    //command parameter value
    public static List<string> commandParamValue = new List<string>(); //in findCommand()

    //Length of command+parameters text
    public static int commandBlockLength = 0;

    public class CSVColumns
    {
        public static int CommandName { get; set; } = 0;
        public static int CommandDescription { get; set; } = 1;
        public static int CommandParameterSize { get; set; } = 2;
        public static int CommandParameterType { get; set; } = 3;
        public static int CommandParameterValue { get; set; } = 4;
        public static int ReplyParameterSize { get; set; } = 5;
        public static int ReplyParameterType { get; set; } = 6;
        public static int ReplyParameterValue { get; set; } = 7;
        public static int ReplyDescription { get; set; } = 8;
    }

    public class DataTypes
    {
        public static string String { get; set; } = "string";
        public static string Number { get; set; } = "number";
        public static string Data { get; set; } = "data";
        public static string Bitfield { get; set; } = "bitfield";
    }

    //lineNum = -1 - искать во всех командах
    //lineNum = x - искать в команде на определенной стоке базы
    public static bool FindCommand(int _pos, byte command, int lineNum = -1)
    {
        //reset all result values
        ClearCommand();

        if (sourceData.Count < _pos + 4) return false;
        //check if it's a command or reply
        if (sourceData[_pos] == 0) itIsBroadcast = true;
        if (sourceData[_pos + 2] == hostAddress && sourceData[_pos + 3] != 0)
        {
            itIsReply = false;
            CSVColumns.CommandDescription = 1;
            CSVColumns.CommandParameterSize = 2;
            CSVColumns.CommandParameterType = 3;
            CSVColumns.CommandParameterValue = 4;            
        }
        else if (sourceData[_pos + 2] == deviceAddress && sourceData[_pos + 3] == 0)
        {
            itIsReply = true;
            CSVColumns.CommandParameterSize = 5;
            CSVColumns.CommandParameterType = 6;
            CSVColumns.CommandParameterValue = 7;
            CSVColumns.CommandDescription = 8;
        }
        else return false;
        //select data frame
        dataFrameLength = sourceData[_pos + 1];

        _pos += 3;

        //check if "commandFrameLength" less than "sourcedata". note the last byte of "sourcedata" is CRC.
        if (_pos + dataFrameLength + 1 >= sourceData.Count)
        {
            dataFrameLength = sourceData.Count - _pos;
            lengthIncorrect = true;
        }

        //find command
        int i = 0;
        if (lineNum != -1) i = lineNum;
        for (; i < commandDataBase.Rows.Count; i++)
        {
            if (commandDataBase.Rows[i][CSVColumns.CommandName].ToString() != "")
            {
                if (command == Accessory.ConvertHexToByte(commandDataBase.Rows[i][CSVColumns.CommandName].ToString())) //if command matches
                {
                    if (lineNum < 0 || lineNum == i) //if string matches
                    {
                        commandName = commandDataBase.Rows[i][CSVColumns.CommandName].ToString();
                        commandDbLineNum = i;
                        commandDesc = commandDataBase.Rows[i][CSVColumns.CommandDescription].ToString();
                        commandFramePosition = _pos;
                        //get CRC of the frame
                        //check length of sourceData
                        int calculatedCRC = SimpleCRC(sourceData.GetRange(_pos - 3, dataFrameLength + 4).ToArray(), dataFrameLength + 4);
                        int sentCRC = sourceData[_pos + dataFrameLength + 1];
                        if (calculatedCRC != sentCRC) crcFailed = true;
                        else crcFailed = false;
                        //check command height - how many rows are occupated
                        int i1 = 0;
                        while ((commandDbLineNum + i1 + 1) < commandDataBase.Rows.Count && commandDataBase.Rows[commandDbLineNum + i1 + 1][CSVColumns.CommandName].ToString() == "")
                        {
                            i1++;
                        }
                        commandDbHeight = i1;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static bool FindCommandParameter()
    {
        ClearCommandParameters();
        //collect parameters from database
        int _stopSearch = commandDbLineNum + 1;
        while (_stopSearch < commandDataBase.Rows.Count && commandDataBase.Rows[_stopSearch][CSVColumns.CommandName].ToString() == "") _stopSearch++;
        for (int i = commandDbLineNum + 1; i < _stopSearch; i++)
        {
            if (commandDataBase.Rows[i][CSVColumns.CommandParameterSize].ToString() != "")
            {
                commandParamDbLineNum.Add(i);
                commandParamSizeDefined.Add(commandDataBase.Rows[i][CSVColumns.CommandParameterSize].ToString());
                if (commandParamSizeDefined.Last() == "?")
                {
                    commandParamSize.Add(dataFrameLength);
                    for (int i1 = 0; i1 < commandParamSize.Count - 1; i1++) commandParamSize[commandParamSize.Count - 1] -= commandParamSize[i1];
                    if (commandParamSize[commandParamSize.Count - 1] < 0) commandParamSize[commandParamSize.Count - 1] = 0;
                }
                else
                {
                    int v = 0;
                    int.TryParse(commandParamSizeDefined.Last(), out v);
                    commandParamSize.Add(v);
                }
                commandParamDesc.Add(commandDataBase.Rows[i][CSVColumns.CommandDescription].ToString());
                commandParamType.Add(commandDataBase.Rows[i][CSVColumns.CommandParameterType].ToString());
            }
        }

        int commandParamPosition = commandFramePosition + 1;
        //process each parameter
        for (int parameter = 0; parameter < commandParamDbLineNum.Count; parameter++)
        {
            //collect predefined RAW values
            List<string> predefinedParamsRaw = new List<string>();
            int j = commandParamDbLineNum[parameter] + 1;
            while (j < commandDataBase.Rows.Count && commandDataBase.Rows[j][CSVColumns.CommandParameterValue].ToString() != "")
            {
                predefinedParamsRaw.Add(commandDataBase.Rows[j][CSVColumns.CommandParameterValue].ToString());
                j++;
            }

            //Calculate predefined params
            List<int> predefinedParamsVal = new List<int>();
            foreach (string formula in predefinedParamsRaw)
            {
                int val = 0;
                if (!int.TryParse(formula.Trim(), out val)) val = 0;
                predefinedParamsVal.Add(val);
            }

            //get parameter from text
            bool errFlag = false;  //Error in parameter found
            string errMessage = "";

            string _prmType = commandDataBase.Rows[(int)commandParamDbLineNum[parameter]][CSVColumns.CommandParameterType].ToString().ToLower();
            if (parameter != 0) commandParamPosition = commandParamPosition + commandParamSize[parameter - 1];
            List<byte> _raw = new List<byte>();
            string _val = "";

            if (_prmType == DataTypes.String)
            {
                if (commandParamPosition + commandParamSize[parameter] <= sourceData.Count - 1)
                {
                    _raw = sourceData.GetRange(commandParamPosition, commandParamSize[parameter]);
                    _val = RawToString(_raw.ToArray(), (byte)commandParamSize[parameter]);
                }
                else
                {
                    errFlag = true;
                    errMessage = "!!!ERR: Out of data bound!!!";
                    if (commandParamPosition <= sourceData.Count - 1) _raw = sourceData.GetRange(commandParamPosition, sourceData.Count - 1 - commandParamPosition);
                }
            }
            else if (_prmType == DataTypes.Number)
            {
                double l = 0;
                if (commandParamPosition + commandParamSize[parameter] <= sourceData.Count - 1)
                {
                    _raw = sourceData.GetRange(commandParamPosition, commandParamSize[parameter]);
                    l = RawToNumber(_raw.ToArray());
                    _val = l.ToString();
                }
                else
                {
                    errFlag = true;
                    errMessage = "!!!ERR: Out of data bound!!!";
                    if (commandParamPosition <= sourceData.Count - 1) _raw = sourceData.GetRange(commandParamPosition, sourceData.Count - 1 - commandParamPosition);
                }
            }
            else if (_prmType == DataTypes.Data)
            {
                if (commandParamPosition + (commandParamSize[parameter]) <= sourceData.Count - 1)
                {
                    _raw = sourceData.GetRange(commandParamPosition, commandParamSize[parameter]);
                    _val = RawToData(_raw.ToArray());
                }
                else
                {
                    errFlag = true;
                    errMessage = "!!!ERR: Out of data bound!!!";
                    if (commandParamPosition <= sourceData.Count - 1) _raw = sourceData.GetRange(commandParamPosition, sourceData.Count - 1 - commandParamPosition);
                }
            }
            else if (_prmType == DataTypes.Bitfield)
            {
                double l = 0;
                if (commandParamPosition + (commandParamSize[parameter]) <= sourceData.Count - 1)
                {
                    _raw = sourceData.GetRange(commandParamPosition, commandParamSize[parameter]);
                    l = RawToBitfield(_raw[0]);
                    _val = l.ToString();
                }
                else
                {
                    errFlag = true;
                    errMessage = "!!!ERR: Out of data bound!!!";
                    if (commandParamPosition <= sourceData.Count - 1) _raw = sourceData.GetRange(commandParamPosition, sourceData.Count - 1 - commandParamPosition);
                }
            }
            else
            {
                //flag = true;
                errFlag = true;
                errMessage = "!!!ERR: Incorrect parameter type!!!";
                if (commandParamPosition + (commandParamSize[parameter]) <= sourceData.Count - 1)
                {
                    _raw = sourceData.GetRange(commandParamPosition, commandParamSize[parameter]);
                    //_val = Accessory.ConvertHexToString(_raw, CustomFiscalControl.Properties.Settings.Default.CodePage);
                }
                else
                {
                    errFlag = true;
                    errMessage = "!!!ERR: Out of data bound!!!";
                    if (commandParamPosition <= sourceData.Count - 1) _raw = sourceData.GetRange(commandParamPosition, sourceData.Count - 1 - commandParamPosition);
                }
            }
            commandParamRAWValue.Add(_raw);
            commandParamValue.Add(_val);

            bool predefinedFound = false; //Matching predefined parameter found and it's number is in "predefinedParameterMatched"
            if (errFlag) commandParamDesc[parameter] += errMessage + "\r\n";

            //compare parameter value with predefined values to get proper description
            int predefinedParameterMatched = 0;
            for (int i1 = 0; i1 < predefinedParamsVal.Count; i1++)
            {
                if (commandParamValue[parameter] == predefinedParamsVal[i1].ToString())
                {
                    predefinedFound = true;
                    predefinedParameterMatched = i1;
                }
            }
            commandParamDesc[parameter] += "\r\n";
            if ((commandParamDbLineNum[parameter] + predefinedParameterMatched + 1) < commandDbLineNum + commandDbHeight && predefinedFound == true)
            {
                commandParamDesc[parameter] += commandDataBase.Rows[commandParamDbLineNum[parameter] + predefinedParameterMatched + 1][CSVColumns.CommandDescription].ToString();
            }
        }
        ResultLength();
        return true;
    }

    internal static void ClearCommand()
    {
        itIsReply = false;
        itIsReplyNACK = false;
        crcFailed = false;
        lengthIncorrect = false;
        commandFramePosition = -1;
        commandDbLineNum = -1;
        commandDbHeight = -1;
        commandName = "";
        commandDesc = "";

        commandParamSize.Clear();
        commandParamDesc.Clear();
        commandParamType.Clear();
        commandParamValue.Clear();
        commandParamRAWValue.Clear();
        commandParamDbLineNum.Clear();
        commandBlockLength = 0;
    }

    internal static void ClearCommandParameters()
    {
        commandParamSize.Clear();
        commandParamDesc.Clear();
        commandParamType.Clear();
        commandParamValue.Clear();
        commandParamRAWValue.Clear();
        commandParamDbLineNum.Clear();
        commandParamSizeDefined.Clear();
        commandBlockLength = 0;
    }

    internal static int ResultLength()  //Calc "CommandBlockLength" - length of command text in source text field
    {
        //[device] + [dataLength] + [host] + [command] + [data] + [CRC]
        commandBlockLength = 4 + dataFrameLength + 1;
        return commandBlockLength;
    }

    public static byte SimpleCRC(byte[] data, int length)
    {
        byte sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += data[i];
        }
        return (byte)(256 - sum);
    }

    public static string RawToString(byte[] b, byte n)
    {
        if (Accessory.PrintableByteArray(b))
        {
            string outStr = Encoding.GetEncoding(CCTalkControl.Properties.Settings.Default.CodePage).GetString(b);
            if (outStr.Length > n) outStr = outStr.Substring(0, n);
            return outStr;
        }
        else return ("");
    }

    public static double RawToNumber(byte[] b)
    {
        double l = 0;
        for (int n = 0; n < b.Length; n++)
        {
            l += (b[n] * Math.Pow(256, n));
        }
        return l;
    }

    public static string RawToData(byte[] b)
    {
        if (Accessory.PrintableByteArray(b)) return ("\"" + Encoding.GetEncoding(CCTalkControl.Properties.Settings.Default.CodePage).GetString(b) + "\"");
        else return ("");
    }

    public static double RawToBitfield(byte b)
    {
        return b;
    }

    public static string StringToRaw(string s, byte n)
    {
        //while (s.Length < n) s += "\0";
        //return Accessory.ConvertStringToHex(s, CustomFiscalControl.Properties.Settings.Default.CodePage).Substring(0, n * 3);
        string outStr = Accessory.ConvertStringToHex(s.Substring(1, s.Length - 2), CCTalkControl.Properties.Settings.Default.CodePage);
        if (outStr.Length > n * 3) outStr = outStr.Substring(0, n * 3);
        while (outStr.Length < n * 3) outStr += "00 ";
        return outStr;
    }

    public static string NumberToRaw(string s, byte n)
    {
        double d = 0;
        if (s != "") double.TryParse(s, out d);
        if (d < 0) d = Math.Pow(2, n * 8) - Math.Abs(d);
        byte[] b = new byte[n];
        for (int i = n - 1; i >= 0; i--)
        {
            b[i] += (byte)(d / Math.Pow(256, i));
            d -= (b[i] * Math.Pow(256, i));
        }
        string str = "";
        for (int i = 0; i < n; i++) str += Accessory.ConvertByteToHex(b[i]);
        return str;
    }

    public static string DataToRaw(string s, byte n)
    {
        string outStr = "";
        if (s.Substring(0, 1) == "[") outStr = s.Substring(1, s.Length - 2);
        else if (s.Substring(0, 1) == "\"") outStr = Accessory.ConvertStringToHex(s.Substring(1, s.Length - 2), CCTalkControl.Properties.Settings.Default.CodePage);
        else return ("");
        if (outStr.Length > n * 3) outStr = outStr.Substring(0, n * 3);
        while (outStr.Length < n * 3) outStr += "00 ";
        return outStr;
    }

    public static string BitfieldToRaw(string s)
    {
        byte l = 0;
        if (s != "") byte.TryParse(s, out l);
        string str = "";
        str += Accessory.ConvertByteToHex(l);
        return str;
    }
}