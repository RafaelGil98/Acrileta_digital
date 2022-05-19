using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Text.RegularExpressions;

namespace Acrileta_Digital
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string docstring = "\n";
        int Importep;
        float Importef;

        private void Terminar_Conteo(object sender, RoutedEventArgs e)
        {
            Aceptar_conteo.Visibility = Visibility.Visible;
            Resumen_final.Visibility = Visibility.Visible;
            TextBlock1.Visibility = Visibility.Hidden;
            impini.Text =Importep.ToString();
            impfin.Text = Importef.ToString();
            if ((float)Importep == Importef)
            {
                sobrante.Text = "0";
                faltante.Text = "0";
            }
            else if ((float)Importep > Importef)
            {
                sobrante.Text = "0";
                var fal = (float)Importep - Importef;
                faltante.Text = fal.ToString();
            }
            else if ((float)Importep < Importef)
            {
                faltante.Text = "0";
                var sobr = Importef - (float)Importep;
                sobrante.Text = sobr.ToString();
            }
        }
        private void Agregar_Comentario(object sender, RoutedEventArgs e)
        {
            Wcomentario.Visibility = Visibility.Visible;
            Aceptar_conteo.Visibility = Visibility.Hidden;
            Resumen_final.Visibility = Visibility.Hidden;
            TextBlock2.Text = "";
        }
        private void Aceptar_Comentario(object sender, RoutedEventArgs e)
        {
            Aceptar_conteo.Visibility = Visibility.Visible;
            Resumen_final.Visibility = Visibility.Visible;
            Wcomentario.Visibility = Visibility.Hidden;
            TextBlock1.Text += "\n" + TextBlock2.Text;
            //MessageBox.Show("Conteo finalizado");
        }
        private void Aceptar_Conteo(object sender, RoutedEventArgs e)
        {
            string datos = "Usuario: "+Usuario.Text+"\nCliente: "+Cliente.Text+"\nOlomo: "+Plomo.Text+"\nFolio: "+Folio.Text+ "\nImporte: " + Importe.Text+"\n" + TextBlock1.Text;
            File.AppendAllText("Parciales.txt", "\n" + datos);
            Aceptar_conteo.Visibility = Visibility.Hidden;
            Resumen_final.Visibility = Visibility.Hidden;
            TextBlock1.Visibility = Visibility.Visible;
            //MessageBox.Show("Conteo finalizado");
            TextBlock1.Text = "Esperando nuevo conteo";
        }
        private void Captura_Manual(object sender, RoutedEventArgs e)
        {
            TextBlock1.Visibility = Visibility.Hidden;
            Manual.Visibility = Visibility.Visible;
        }
        private void Aceptar_Captura(object sender, RoutedEventArgs e)
        {

            string[] dmonedas = { m20.Text, m10.Text, m5.Text, m2.Text, m1.Text, c50.Text, c20.Text };
            float[] deno = { 20, 10, 5, 2, 1, 0.5f, 0.2f };
            string desglose ="\nMonedas manual\nDenom  Pcs.  Monto\n";
            int ind = 0;
            foreach (string moneda in dmonedas)
            {
                if (int.TryParse(moneda, out int cmoneda))
                {
                    desglose += $"${deno[ind]}\t{cmoneda}\t${cmoneda*deno[ind]}\n";
                    Importef += cmoneda * deno[ind];
                }
                ind += 1;
            }
            if (desglose != "\nMonedas manual\nDenom  Pcs.  Monto\n")
            {
                TextBlock1.Text += "\n" + desglose;
            }
            Monedas.Visibility = Visibility.Hidden;
            TextBlock1.Visibility = Visibility.Visible;
            Aceptar_captura.Visibility = Visibility.Hidden;
        }
        private void Aceptar_CapturaD(object sender, RoutedEventArgs e)
        {
            if (concepto.Text!="" && cantidad.Text != "")
            {
                if (int.TryParse(cantidad.Text, out int cantidaf))
                {
                    docstring += $"Concepto: {concepto.Text}\nMonto: {cantidad.Text}\n";
                    Importef += (float)cantidaf;
                }
                else
                {
                    MessageBox.Show("Introduzca solo cantidades numericas en el monto");
                }
            }
            TextBlock1.Text += docstring;
            Documentos.Visibility = Visibility.Hidden;
            TextBlock1.Visibility = Visibility.Visible;
        }
        private void Agregar_CapturaD(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(cantidad.Text,out int cantidaf))
            {
                docstring += $"Concepto: {concepto.Text}\nMonto: {cantidad.Text}\n";
                Importef += (float)cantidaf;
            }
            else
            {
                MessageBox.Show("Introduzca solo cantidades numericas en el monto");
            }
            concepto.Text = "";
            cantidad.Text = "";
        }
        private void Aceptar_Moneda(object sender, RoutedEventArgs e)
        {
            Manual.Visibility = Visibility.Hidden;
            Monedas.Visibility = Visibility.Visible;
            TextBlock1.Visibility = Visibility.Hidden;
            Aceptar_captura.Visibility = Visibility.Visible;
        }
        private void Aceptar_Documentos(object sender, RoutedEventArgs e)
        {
            Manual.Visibility = Visibility.Hidden;
            Monedas.Visibility = Visibility.Hidden;
            TextBlock1.Visibility = Visibility.Hidden;
            Documentos.Visibility = Visibility.Visible;
        }
        private void Iniciar_Conteo(object sender, RoutedEventArgs e)
        {
            if (Usuario.Text != "" && Cliente.Text != "" && Plomo.Text != "" && Folio.Text != "" && Importe.Text != "" && int.TryParse(Importe.Text,out Importep))
            {
                TextBlock1.Clear();
                var ObjS = new Serial();
                string lectura = ObjS.Escribir();
                //AGREGAR REGEX PARA BCS160
                //
                //
                // 
                string lectura1="Sin lectura, reinicie el conteo";
                int indicef = 0;
                string salida = "";
                int total = 0; 
                Regex pat = new Regex(@"(.+Batch Report.+Counted Result(.+\s+\d+ X\s+\d+ =.+\d+)+.+SubTotal)",RegexOptions.Singleline);
                Regex pat1 = new Regex(@"CUSTOMER ID:.+Denom\..+1000\s+(\d)+\s+\d+\s+500\s+(\d)+\s+\d+\s+200\s+(\d)+\s+\d+\s+100\s+(\d)+\s+\d+\s+50\s+(\d)+\s+\d+\s+20\s+(\d)+\s+\d+\s+", RegexOptions.Singleline);
                Regex patre;
                string patrep="";
                int[] den = { 1000, 500, 200, 100, 50, 20 };
                if (pat.IsMatch(lectura))
                {
                    //patron en python:  w=re.compile(r"(.+Batch Report.+Counted Result(.+\d{2,4} X\s+\d+ =.+\d+)+.+SubTotal)",re.S)
                    Match m = pat.Match(lectura);
                    foreach (Group i in m.Groups)
                    {
                        lectura1 = i.Value;
                    }
                    string[] vecs = { @".+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)", @".+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)", @".+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)", @".+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)", @".+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)", @".+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)\s+.+\*!.+\s+(\d+) X\s+(\d+) =.+\s+(\d+)" };
                    for (int w = 6; w > 0; w--)
                    {
                        string patron = vecs[w - 1];
                        patre = new Regex(patron);
                        if (patre.IsMatch(lectura1))
                        {
                            indicef = w;
                            patrep = patron;
                            break;
                        }
                    }
                    Regex repatre = new Regex(patrep);
                    if (repatre.IsMatch(lectura1))
                    {
                        Match n = repatre.Match(lectura1);
                        for (int k = 0; k < indicef; k++)
                        {
                            salida += n.Groups[1 + k * 3].ToString() + " X " + n.Groups[2 + k * 3].ToString() + " = " + n.Groups[3 + k * 3].ToString() + "\n";
                            total += Int32.Parse(n.Groups[3 + k * 3].ToString());
                        }
                        TextBlock1.Text = salida + "Total: $" + total.ToString();
                        Importef += (float)total;
                    }
                    else
                    {
                        TextBlock1.Text = lectura1;
                    }
                }
                else if (pat1.IsMatch(lectura))
                {
                    TextBlock1.Text = "Esto es 160\n" + lectura;
                    Match bcs160 = pat1.Match(lectura);
                    int a = 0;
                    foreach (Group m in bcs160.Groups)
                    {
                        if (a > 6) break;
                        if (a > 0)
                        {
                            if (m.Value != "0")
                            {
                                salida += den[a - 1].ToString() + " X " + m.Value + $" = {den[a - 1] * int.Parse(m.Value)}\n";
                                total += den[a - 1] * Int32.Parse(m.Value);
                            }
                        }
                        a++;
                    }
                    TextBlock1.Text = salida + "Total: $" + total.ToString();
                    Importef += (float)total;
                }
                
            }
            else
            {
                MessageBox.Show("Introduzca todos los datos solicitados");
            }
        }
    }
    public class Serial
    {
        static SerialPort serial; 
        
        public string Escribir()
        {
            string puerto = "5"; //elección provisional de puerto
            SerialPort serial = new SerialPort("COM" + puerto, 115200, Parity.None, 8, StopBits.One);

            serial.Open();
            string s = "";
            while (true)
            {
                int b = serial.ReadByte();
                s += (char)b + "";
                if (s.Contains("Reject"))
                {
                    serial.Close();
                    break;
                }
                else if (s.Contains("Total:"))
                {
                    serial.Close();
                    break;
                }
            }
            return s;
        }
    }
}
