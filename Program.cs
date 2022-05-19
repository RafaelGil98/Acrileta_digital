using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.IO;

namespace Acrileta_Digital
{
    class Program
    {
        static void _Main()
        {
            string input = "FE08EF19555344";
            var bytes = HexToBytes(input);
            string hex = Crc16.ComputeChecksum(bytes).ToString("x2");
            Console.WriteLine(hex); //c061
        }
        static byte[] HexToBytes(string input)
        {
            byte[] result = new byte[input.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
            }
            return result;
        }

        public static class Crc16
        {
            const int polynomial = 0x18005;
            static readonly ushort[] table = new ushort[256];

            public static ushort ComputeChecksum(byte[] bytes)
            {
                ushort crc = 0;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    byte index = (byte)(crc ^ bytes[i]);
                    crc = (ushort)((crc >> 8) ^ table[index]);
                }
                return crc;
            }

            static Crc16()
            {
                ushort value;
                ushort temp;
                for (ushort i = 0; i < table.Length; ++i)
                {
                    value = 0;
                    temp = i;
                    for (byte j = 0; j < 8; ++j)
                    {
                        if (((value ^ temp) & 0x0001) != 0)
                        {
                            value = (ushort)((value >> 1) ^ polynomial);
                        }
                        else
                        {
                            value >>= 1;
                        }
                        temp >>= 1;
                    }
                    table[i] = value;
                }
            }
        }


        static void Comando(byte[] Txbuff, int l)
        {
            Txbuff[(l - 1)] = Calcbcc(Txbuff, 20); //l-1=19 en la última pos
            try
            {
                serial.Write(Txbuff, 0, l);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static byte Calcbcc(byte[] buff, int l)
        {
            byte num = 0;
            var num4 = (l - 2);
            var i = 1;
            do
            {
                num = Convert.ToByte((buff[i] ^ num)); //'hasta la pos 18, la penúltima
                i += 1;
            }
            while (i <= num4);
            return num;
        }
        static SerialPort serial; //= new SerialPort(SerialPort.GetPortNames()[0], 19200, Parity.Odd, 8, StopBits.One);
        static void Leer()
        {
            while (true)
            {
                byte gxbyte = (byte)serial.ReadByte();
                Console.Write(gxbyte.ToString("X") + " ");
                File.AppendAllText("bytecitos.txt", gxbyte.ToString("X") + " ");
            }
        }

        static string Escribir(string[] args)
        {
            Console.WriteLine("Escribe num puerto");
            string puerto = Console.ReadLine();
            SerialPort serial = new SerialPort("COM"+puerto, 115200, Parity.None, 8, StopBits.One);

            serial.Open();

            
            byte checksum = 0x7E + 0x04 + 0x4C + 0x00 + 0x00; 
            Console.WriteLine();
            Console.Clear();
            string s = "";
            Console.WriteLine("leyendo");
            while (true)
            {
                int b = serial.ReadByte();
                s += (char)b + "";
                //Console.Write((char)b+"");
            }
            byte[] bytes2 = { 0xFE, 0x08, 0xEF, 0x19, 0x55, 0x53, 0x44, 0x53, 0xFE, 0x10 };
            Thread hilo = new Thread(new ThreadStart(Leer));
            hilo.Start();

            for (int i = 634; i >= 1; i++)
            {

                byte[] txbuff = new byte[] { 0xFF, 1, 0x11, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                txbuff[4] = Convert.ToByte((i >> 8) & 0xFF);
                txbuff[5] = Convert.ToByte((i & 0xFF));
                txbuff[6] = 0;
                Comando(txbuff, 20);
                Thread.Sleep(5000);
            }
            return s;
            int gxcounter = 0;
        }
    }
}


