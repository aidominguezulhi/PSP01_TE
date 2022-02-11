using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace pipeServidor
{
    class PipeServidor
    {
        static void Main(string[] args)
        {
            var cuento = string.Empty;
            
            string s1 = string.Empty;
            string s2 = string.Empty;
            List<String> list = new List<String>();


            try
            {
                //creamos la conexion
                var server = new NamedPipeServerStream("PSP01_UD01_Pipes");
                server.WaitForConnection();
                Console.WriteLine("Conexión a servidor establecida.");
                Console.WriteLine("Pipe Servidor esperando datos.");

                //Creamos los buffer de entrada y de salida
                StreamReader reader = new StreamReader(server);
                StreamWriter writer = new StreamWriter(server);
                while (true)
                {
                    //recibimos el cuento
                    var line = reader.ReadLine();
                    Console.WriteLine("Pipe Servidor recibiendo datos: '{0}'", line);
                    //recibimos el nombre del cuento en formato N nombredelcuento y solo necesitamos el nombre del cuento,
                    //eliminamos los dos primeros caracteres N y espacio
                    cuento = line.Remove(0, 2);
                    cuento = cuento + ".txt";
                    
                    Console.WriteLine("Apertura del fichero: {0}", cuento);
                    File.WriteAllText(@"../../../../cuentos/resultado.txt", string.Empty);

               
                    //abrimos el fichero
                    try
                    {
                        // abrimos el fichero para leer
                        using (StreamReader sr = new StreamReader(@"../../../../cuentos/" + cuento))
                        {
                            Console.WriteLine("Fichero abierto. {0}", cuento);
                            string fileLine;
                            // leemos las lineas del fichero una a una
                            while ((fileLine = sr.ReadLine()) != null)
                            {
                                list.Clear();
                                Console.WriteLine("Tubo servidor procesando datos: {0}", fileLine);
                                var words = fileLine.Split(' ');
                                //procesar la linea del fichero
                                foreach (var word in words)
                                {
                                    if (word.StartsWith("<"))
                                    {
                                        s1 = word.Remove(word.IndexOf("<"), 1);
                                        s2 = s1.Remove(s1.IndexOf(">"), 1);
                                        Console.WriteLine("Tubo servidor emitiendo datos: {0}", s2);
                                        writer.WriteLine(s2);
                                        writer.Flush();
                                        var line1 = reader.ReadLine();
                                        Console.WriteLine("Tubo Servidor recibiendo datos: '{0}'", line1);
                                        var palabra = line1.Remove(0, 2);
                                        list.Add(palabra);
                                    }
                                    else
                                    {
                                        list.Add(word);
                                    }
                                }

                                var newLine = String.Join(" ", list.ToArray());
                                using (StreamWriter sw = File.AppendText(@"../../../../cuentos/resultado.txt"))
                                {
                                    sw.WriteLine(newLine);
                                }
                            }
                            writer.WriteLine(" ");
                            writer.Flush();
                            Console.WriteLine("Tubo cliente emitiendo datos cuento");
                            extraerCuento(writer);
                            writer.WriteLine("$$finDeFichero");
                            writer.Flush();
                        }
                    }
                    catch (Exception e)
                    {
                        // Let the user know what went wrong.
                        Console.WriteLine("El fichero no existe");
                        Console.WriteLine(e.Message);
                        writer.WriteLine("El cuento no existe");
                        writer.Flush();
                        
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}  Apangado servidor por error", e.Message);
            }
        }
        private static void extraerCuento(StreamWriter writer)
        {
            string cuento = string.Empty;
            string line = string.Empty;
            try
            {
                StreamReader sr = new StreamReader(@"../../../../cuentos/resultado.txt");
                line = sr.ReadLine();
                while (line != null)
                {
                    Console.WriteLine(line);
                    writer.WriteLine(line);
                    writer.Flush();
                    line = sr.ReadLine();
                    
                }
                //close the file
                
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}