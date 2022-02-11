using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace pipeCliente
{
    class PipeCliente
    {

        static void Main(string[] args)
        {
            Process p;
            StartServer(out p);
            Task.Delay(1000).Wait();
            //Preparar conexion del cliente
            var client = new NamedPipeClientStream("PSP01_UD01_Pipes");
            client.Connect();
            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);
            var line = String.Empty;

            while (true)
            {
                //solicitar el nombre del cuento
                line = SolictarCuento(reader, writer);

                //trabajamos la entrada y salida de datos
                while (!line.Equals(" "))
                {
                    Console.WriteLine("{0}:", line);
                    var line1 = Console.ReadLine();
                    var palabra = "P " + line1;
                    Console.WriteLine("Tubo cliente procesando datos: {0}", palabra);
                    writer.WriteLine(palabra);
                    writer.Flush();
                    line = reader.ReadLine();
                }
                //mostramos el cuento completo
                mostrarCuento(reader);
                
            }
            //Parar pipes
            if (p != null && !p.HasExited)
            {
                p.Kill();
                p = null;
            }
        }

        static Process StartServer(out Process p1)
        {
            // iniciar un proceso con el servidor y devolver
            ProcessStartInfo info = new ProcessStartInfo(@"..\..\..\..\pipeServidor\bin\Release\net6.0\win-x64\pipeServidor.exe");
            //ProcessStartInfo info = new ProcessStartInfo(@"C:\Users\Aitor\PSP7\TareaEvaluativa01\Solution1\PipeServidor\bin\Debug\net6.0\PipeServidor.exe");

            // su valor por defecto el false, si se establece a true no se "crea" ventana
            info.CreateNoWindow = false;
            info.WindowStyle = ProcessWindowStyle.Normal;
            // indica si se utiliza el cmd para lanzar el proceso
            info.UseShellExecute = true;
            p1 = Process.Start(info);
            return p1;
        }

        //Sacamos por la consola lo que nos envia el servidor linea a linea hasta que nos llega un caracter vacio
        private static void mostrarCuento(StreamReader reader)
        {
            Console.WriteLine("\n***************************");
            Console.WriteLine("\n    el cuento creado es"    );
            Console.WriteLine("\n***************************");
            var line = reader.ReadLine();
            while (!line.Equals("$$finDeFichero"))
            {
                Console.WriteLine(line);
                line = reader.ReadLine();
                
            }
        }

        
        private static string SolictarCuento(StreamReader reader, StreamWriter writer)
        {
            var line = string.Empty;
            do
            {
                //Solicitamos elnombre del cuento por consola y le damos formato N + nombreFichero
                Console.WriteLine("indica el nombre del cuento");
                string cuento = Console.ReadLine();
                cuento = "N " + cuento;

                //Enviamos el dato al servidor
                Console.WriteLine("Tubo Cliente procesando datos: {0}", cuento);
                writer.WriteLine(cuento);
                writer.Flush();


                //Procesar huecos a rellenar. Recibimos una palabra desde el servidor y devolvemos otra en formato P + palabra
                //Si entra un caracter vacio paramos pasamos a mostrar el cuento completo
                line = reader.ReadLine();
                Console.WriteLine("prueba {0}", line);
            }
            //mientras no exista el cuento nos quedamos bloqueados en este bucle
            while (line == "El cuento no existe");
            return line;
        }
            
    }
}
