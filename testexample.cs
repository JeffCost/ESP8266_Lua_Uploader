using System;
using System.IO.Ports;
 
public class SerialPortTest
{
    public static void Main(string[] args)
    {
        SerialPortTest myTest = new SerialPortTest();
        myTest.Test();
    }
 
    private SerialPort mySerial;
 
    // Constructor
    public SerialPortTest()
    {
    }
 
    public void Test()
    {
        if (mySerial != null)
            if (mySerial.IsOpen)
                mySerial.Close();
 
        mySerial = new SerialPort("/dev/ttyUSB0", 9600);
        mySerial.Open();
        mySerial.ReadTimeout = 400;
        SendData("ATI3\r");
 
                // Should output some information about your modem firmware
        Console.WriteLine(ReadData());
    }
 
    public string ReadData()
    {
        byte tmpByte;
        string rxString = "";
 
        tmpByte = (byte) mySerial.ReadByte();
 
        while (tmpByte != 255) {
            rxString += ((char) tmpByte);
            tmpByte = (byte) mySerial.ReadByte();
        }
 
        return rxString;
    }
 
    public bool SendData(string Data)
    {
        mySerial.Write(Data);
        return true;
    }
}
