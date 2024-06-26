﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proyecto1_completo_grupo4.Entities;

namespace Proyecto1_completo_grupo4
{
    internal class Program
    {

        static string cadena; //Variable global para almacenar la cadena ingresada por el usuario
        static List<string> recorrido = new List<string>(); //guardda cada estado recorrido 
        static List<string> recorridoFallido = new List<string>(); //cuarda la union de los estados recorridos si este falla, para luego comprar cual ruta ya se ha realizado
        static bool AFN = false;


        // ------LECTURA ARCHIVO E INVOCACIÓN DE FUNCIONES PARA LOS TRES TIPOS DE ARCHIVOS-----
        public static void LeerArchivos()
        {
            const int maxIntentos = 2; //Intentos maximos para ingresar el path
            int intentos = 0; //Contador de intentos
            bool archivoAceptado = false; //Se establece en true cuando se procesa con éxito un archivo.
                                          //Se utiliza para salir del loop una vez que se ha procesado un archivo.

            //Bucle para solicitar el path del archivo y procesarlo
            while (intentos <= maxIntentos && !archivoAceptado)
            {
                Console.WriteLine("Ingrese el path del archivo:  (copiar y pegar como la ruta de acceso que brinda su dispositivo)");
                string path = Console.ReadLine();
                string trimmedPath = path.Trim('"'); //Se guarda el path sin las comillas, si se guarda con comillas no funciona.

                //Valida el path ingresado
                if (string.IsNullOrEmpty(trimmedPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red; 
                    Console.WriteLine("ERROR: El path no puede esta vacío.\n");
                    Console.ResetColor();
                    intentos++;
                    continue;
                }

                if (!File.Exists(trimmedPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: El archivo {trimmedPath} no existe.\n");
                    Console.ResetColor();
                    intentos++;
                    continue;
                }

                //Obtiene la extension del path
                string extension = Path.GetExtension(trimmedPath);

                switch (extension.ToLower())
                {
                    case ".txt":
                        LeerArchivoTxt(trimmedPath, extension);
                        archivoAceptado = true;
                        break;

                    case ".csv":
                        LeerArchivoCsv(trimmedPath, extension);
                        archivoAceptado = true;
                        break;

                    case ".json":
                        LeerArchivoJson(trimmedPath, extension);
                        archivoAceptado = true;
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: La aplicación no soporta la extensión.");
                        Console.ResetColor();
                        intentos++;
                        break;

                }
            }

            if (!archivoAceptado)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nNúmero de intentos exedido.");
                Console.ResetColor();
            }

            Console.WriteLine("\nPresione cualquier tecla para regresar al menú...");
            Console.ReadKey();
            Console.Clear();
        }

        static AutomataEntity LeerArchivoTxt(string filePath, string fileExtension)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nArchivo de extensión " + fileExtension + "\n");
                Console.ResetColor();

                List<string> lineasTxt = File.ReadAllLines(filePath).ToList(); //Lee todas las líneas

                //Asegurar de que el archivo tiene al menos 4 líneas
                if (lineasTxt.Count < 3)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: El archivo {filePath} no tiene el formato esperado.");
                    Console.ResetColor();
                    return null;
                }


                //Crea una nueva instancia de AutomataEntity
                AutomataEntity Automata = new AutomataEntity();
                {
                    Automata.Estados = lineasTxt[0];
                    Automata.EstadoInicial = lineasTxt[1].Split(',');
                    Automata.EstadosFinales = lineasTxt[2].Split(',');
                    Automata.Transiciones = new List<TransicionEntity>();
                };


                //Asignar las transiciones de la instacia del automata
                for (int i = 3; i < lineasTxt.Count; i++)
                {
                    string[] transicionDatos = lineasTxt[i].Split(',');


                    if (transicionDatos.Length != 3)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"Advertencia: El formato de la transición en la línea {i + 1} del archivo {filePath} no es válido.");
                        Console.ResetColor();
                    }
                    Automata.Transiciones.Add(new TransicionEntity
                    {
                        EstadoOrigen = transicionDatos[0].Trim(),
                        Simbolo = ConvertirEpsilon(transicionDatos[1].Trim()),
                        EstadoDestino = transicionDatos[2].Trim()
                    });
                }

                ImprimirAutomata(Automata);
                ConsultarCadena(Automata);
                return Automata;
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: No se puede leer el archivo {filePath}. {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (FormatException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Formato de los datos en el archivo {filePath} no es válido. {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        public static AutomataEntity LeerArchivoCsv(string filePath, string fileExtension)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nArchivo de extensión " + fileExtension + "\n");
                Console.ResetColor();

                List<string> lineasCsv = File.ReadAllLines(filePath).ToList(); //Lee todas las líneas

                //Valida que el archivo tenga al menos las primeras 3 líneas necesarias
                if (lineasCsv.Count < 3)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: El archivo {filePath} no tiene el formato esperado.");
                    Console.ResetColor();
                    return null;
                }

                AutomataEntity Automata = new AutomataEntity();
                {
                    Automata.Estados = lineasCsv[0].Trim('"');
                    Automata.EstadoInicial = lineasCsv[1].Trim('"').Split(',').Select(est => est.Trim()).ToArray();
                    Automata.EstadosFinales = lineasCsv[2].Trim('"').Split(',').Select(est => est.Trim()).ToArray();
                    Automata.Transiciones = new List<TransicionEntity>();
                };

                //Asigna las transiciones a la instancia del autómata
                for (int i = 3; i < lineasCsv.Count; i++)
                {
                    string[] transicionDatos = lineasCsv[i].Split(',').Select(dato => dato.Trim()).ToArray();

                    if (transicionDatos.Length != 3)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"Advertencia: El formato de la transición en la línea {i + 1} del archivo {filePath} no es válido.");
                        Console.ResetColor();
                        continue;
                    }

                    Automata.Transiciones.Add(new TransicionEntity
                    {
                        EstadoOrigen = transicionDatos[0].Trim('"'),
                        Simbolo = ConvertirEpsilon(transicionDatos[1].Trim()),
                        EstadoDestino = transicionDatos[2].Trim('"')
                    });
                }
                ImprimirAutomata(Automata);
                ConsultarCadena(Automata);
                Console.Clear();
                return Automata;
            }
            //Valida la lectura del archivo
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: No se puede leer el archivo {filePath}. {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (FormatException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Formato de los datos en el archivo {filePath} no es válido. {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        public static AutomataEntity LeerArchivoJson(string trimmedPath, string fileExtension)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nArchivo de extensión " + fileExtension + "\n");
                Console.ResetColor();

                string jsonData;
                AutomataEntity Automata = null;

                //Lee el archivo JSON
                using (StreamReader leerJson = new StreamReader(trimmedPath))
                {
                    jsonData = leerJson.ReadToEnd();

                    //Intentar deserializar el JSON a una instancia de AutomataEntity
                    try
                    {
                        Automata = JsonConvert.DeserializeObject<AutomataEntity>(jsonData);
                    }
                    catch (JsonException)
                    {
                        Automata = new AutomataEntity();
                        Automata = JsonConvert.DeserializeObject<AutomataEntity>(jsonData, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Error
                        });
                    }
                    //Imprimir el autómata y consultar la cadena
                    if (Automata != null)
                    {
                        ImprimirAutomata(Automata);
                        ConsultarCadena(Automata);
                        return Automata;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"ERROR: No se pudo deserializar el archivo {trimmedPath}");
                        Console.ResetColor();
                    }
                }
            }

            //Valida la lectura del archivo
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: No se puede leer el archivo {trimmedPath}. {ex.Message}");
                Console.ResetColor();
            }

            //Valida el formato json
            catch (JsonException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: El archivo {trimmedPath} contiene un .json inválido.");
                Console.ResetColor();
                MostrarEstructuraJson();
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        static void MostrarEstructuraJson() //Funcion para mostrar la estructura JSON esperada
        {
            string jsonString = @"
                {
                    ""estados"": 6,
                    ""estadoInicial"": [""0""],
                    ""estadosFinales"": [""4""],
                    ""transiciones"": [
                        {
                            ""estadoOrigen"": ""0"",
                            ""simbolo"": ""0"",
                            ""estadoDestino"": ""E""
                        },
                        {
                            ""estadoOrigen"": ""0"",
                            ""simbolo"": ""1"",
                            ""estadoDestino"": ""1""
                        },
                        {
                            ""estadoOrigen"": ""1"",
                            ""simbolo"": ""0"",
                            ""estadoDestino"": ""2""
                        }
                    ]
                }";
            Console.WriteLine("El .json debe seguir la siguiente estructura:");
            Console.WriteLine(jsonString);
        }



        // -----FUNCIONES DEL AUTOMATA-----
        //Funcion recursiva para recorrer el autómata con una cadena
        static void RecorrerAF(string estActual, string cadena, int contador, AutomataEntity automata)
        {
            string[] caracteres = cadena.ToCharArray().Select(c => c.ToString()).ToArray(); //separa la cadena ingresada en un arreglo string

            if (contador == caracteres.Length)
            {
                VerifEstFinal(estActual, automata.EstadosFinales, automata);
            }
            else
            {
                string sigEstado = SigEstado(caracteres[contador], estActual, automata);
                ImprimirPaso(estActual, caracteres[contador], sigEstado, contador, cadena, automata);
            }
        }


        //Funcion para obtener el siguiente estado en el autómata
        static string SigEstado(string caracter, string estado, AutomataEntity automata)
        {
            foreach (var transicion in automata.Transiciones)//recorre toda la matriz
            {
                if (estado.Equals(transicion.EstadoOrigen))//se compara estado recibido con la primera posicion de cada fila(estado actual)
                {
                    if (caracter.Equals(transicion.Simbolo))//Si coincide se compara con la segunda posición el cual es "la letra"
                    {
                        return transicion.EstadoDestino;//Si ambas coinciden se retorna el siguiente estado
                    }
                }
            }

            if (estado.ToUpper() == "E")
            {
                return "E";
            }
            else
            {
                return ("no se encontró el estado " + estado);

            }
        }


        //Funcion para imprimir el paso actual del recorrido
        static void ImprimirPaso(string estActual, string caracter, string sigEstado, int contador, string cadena, AutomataEntity automata)
        {
            int longitudCad = cadena.Length;
            if (contador < longitudCad)
            {
                Console.WriteLine(estActual + " -> " + caracter + " -> " + sigEstado);
                contador++;//aumenta el contador para seguir recorriendo la cadena
                estActual = sigEstado;//siguiente estado pasa a ser el actual para volver a llamar RecorrerAF

                RecorrerAF(estActual, cadena, contador, automata);
            }
        }


        //Funcion para verificar si el estado actual es final o no
        static void VerifEstFinal(string estActual, string[] EstadosFinales, AutomataEntity automata)
        {
            //Limpiar el estado actual
            string estAcualLimpio = estActual.Trim().ToUpper();

            //Limpiar los estados finales
            string[] estadosFinalesLimpios = EstadosFinales.Select(ef => ef.Trim().ToUpper()).ToArray();

            bool esEstadoFinal = estadosFinalesLimpios.Contains(estAcualLimpio);

            if (esEstadoFinal)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("El recorrido dirige al estado '" + estActual + "' que si es un estado final, la palabra SI es aceptable.");
                Console.ResetColor();
                ContinuarValidando(automata);
            }
            else if (estActual == "E")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("El recorrido dirige a Epsilon. \nEl estado '" + estActual + "' no está permitido, la palabra NO es aceptable.");
                Console.ResetColor();
                ContinuarValidando(automata);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("El recorrido dirige al estado '" + estActual + "' que no es un estado final, la palabra NO es aceptable.");
                Console.ResetColor();
                ContinuarValidando(automata);
            }
        }




        // -----FUNCIONES PARA MOSTRAR Y CONSULTAR-----
        //Funcion para imprimir los detalles del autómata
        static void ImprimirAutomata(AutomataEntity automata)
        {
            try
            {
                //Comprueba la lectura de los archivos 
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"DATOS DEL AUTOMATA: ");
                Console.WriteLine("---------------------");
                Console.ResetColor();
                Console.WriteLine($"Estados: {automata.Estados}");
                Console.WriteLine($"Estados iniciales: {string.Join(", ", automata.EstadoInicial)}");
                Console.WriteLine($"Estados finales: {string.Join(", ", automata.EstadosFinales)}");

                foreach (var transicion in automata.Transiciones)
                {
                    Console.WriteLine($"{transicion.EstadoOrigen}, {transicion.Simbolo}, {transicion.EstadoDestino}");
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR al imprimir automata: {ex.Message}");
                Console.ResetColor();
            }
        }

        //Funcion para consultar y validar una cadena en el autómata
        static void ConsultarCadena(AutomataEntity automata)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("\n\nCONSULTA DE CADENA: ");
                Console.WriteLine("----------------------");
                Console.WriteLine("Ingrese su cadena de caracteres:");
                Console.ResetColor();
                cadena = Console.ReadLine();

                Console.WriteLine();
                Animacion();

                //Verifica que la cadena no esté vacía
                if (!string.IsNullOrEmpty(cadena))
                {
                    Console.WriteLine("\nRecorrido:");
                    
                    if (AFN)
                    {
                        RecorrerAFN(automata.EstadoInicial[0], cadena, 0, automata);
                    }
                    else
                    {
                        RecorrerAF(automata.EstadoInicial[0], cadena, 0, automata);
                    }
                }
                else
                {
                    Console.WriteLine("El string está vacío.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR al mostrar recorrido: {ex.Message}");
                Console.ResetColor();
            }
        }

        //Funcion para continuar validando cadenas o volver al menu
        static void ContinuarValidando(AutomataEntity automata)
        {
            string respuesta;
            recorrido.Clear();
            recorridoFallido.Clear();

            do
            {
                Console.WriteLine($"\nDesea seguir validando cadenas? (SI o NO):");
                respuesta = Console.ReadLine();

                if (respuesta.ToUpper() == "SI")
                {
                    ConsultarCadena(automata);
                    recorridoFallido.Clear();
                    recorrido.Clear();
                    break;
                }
                else if (respuesta.ToUpper() == "NO")
                {
                    Console.Clear();
                    MostrarMenu();
                    break;
                }
                else
                {
                    Console.Write($"\nEntrada no válida. Por favor ingrese 'SI' O ''NO'");
                }

            } while (respuesta != "SI" && respuesta != "NO");
        }


    

        // -----FUNCIONES DE MENÚ-----
        static void MostrarMenu()
        {
            try
            {
                bool op = true;

                while (op == true)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("MENÚ DE OPCIONES");
                    Console.WriteLine("-----------------");
                    Console.ResetColor();
                    Console.WriteLine("1. Automata finito determinista");
                    Console.WriteLine("2. Automata finito no determinista");
                    Console.WriteLine("3. Salir");
                    Console.WriteLine("\nDigite su opcción: ");

                    string opcion = Console.ReadLine();

                    switch (opcion)
                    {
                        case "1":
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine("AUTÓMATAS FINITOS DETERMINISTAS");
                            Console.WriteLine("-------------------------------");
                            Console.ResetColor();
                            MostrarOpcion1();
                            break;

                        case "2":
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine("AUTÓMATAS FINITOS NO DETERMINISTAS");
                            Console.WriteLine("----------------------------------");
                            Console.ResetColor();
                            MostrarOpcion2();
                            break;

                        case "3":
                            Environment.Exit(0); //Sale del programa
                            break;

                        default:
                            MostrarOpcionInvalida();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR con el menú: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void MostrarOpcion1()
        {
            bool archivoLeido = false;
            try
            {
                if (!archivoLeido)
                {
                    AFN = false;
                    LeerArchivos(); //se manda false para indicar que no es AFN
                    archivoLeido = true;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al leer archivo: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                MostrarMenu();
            }
        }

        static void MostrarOpcion2()
        {
            bool archivoLeido = false;
            try
            {
                if (!archivoLeido)
                {
                    AFN = true;
                    LeerArchivos(); //se manda true para indicar que si es AFN
                    archivoLeido = true;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al leer archivo: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                MostrarMenu();
            }
        }

        static void MostrarOpcionInvalida()
        {
            Console.WriteLine("Opción inválida.");
            MostrarRegresarMenu();
        }

        static void MostrarRegresarMenu()
        {
            Console.WriteLine("\nPresione cualquier tecla para regresar al menú...");
            Console.ReadKey();
            MostrarMenu();
        }

        static void MostrarBienvenida()
        {
            Console.Beep();
            Console.ForegroundColor = ConsoleColor.DarkCyan; 

            Console.WriteLine("**********************************************");
            Console.WriteLine("*     BIENVENIDO/A AL PROGRAMA QUE LEE Y     *");
            Console.WriteLine("*          VALIDA AUTÓMATAS FINITOS          *");
            Console.WriteLine("*                                            *");
            Console.WriteLine("*     Este programa realiza operaciones      *");
            Console.WriteLine("*     relacionadas con autómatas finitos     *");
            Console.WriteLine("*          y cadenas de caracteres.          *");
            Console.WriteLine("**********************************************");
            Console.ResetColor();
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
            Animacion(); 
        }

        static void Animacion()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.Write("\rCargando... " + i * 10 + "%");
                System.Threading.Thread.Sleep(150); //150 milisegundos
            }
            Console.WriteLine("\r\nCargado completo!     ");
        }


        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; //para que muestre epsilon en consola
            MostrarBienvenida();
            MostrarMenu();
        }



        // -----FUNCIONES DEL AUTOMATA NO DETERMINISTA-----
        //Funcion recursiva para recorrer el autómata con una cadena
        static void RecorrerAFN(string estActual, string cadena, int contador, AutomataEntity automata)
        {
            string[] caracteres = cadena.ToCharArray().Select(c => c.ToString()).ToArray(); //separa la cadena ingresada en un arreglo string

            if (contador == caracteres.Length)
            {
                recorrido.Add(estActual);
                VerifEstFinalN(estActual, automata.EstadosFinales, automata);
            }
            else
            {
                recorrido.Add(estActual);
                string sigEstado = SigEstadoN(ConvertirEpsilon(caracteres[contador]), estActual, automata);

                string[] partes = sigEstado.Split(',');
                sigEstado = partes[1];

                if (partes[0]== "ε")
                {
                    ImprimirPasoN(estActual, "ε", sigEstado, contador-1, cadena, automata);
                }
                else
                {
                    ImprimirPasoN(estActual, caracteres[contador], sigEstado, contador, cadena, automata);
                }
                
            }
        }


        //Funcion para obtener el siguiente estado en el autómata
        static string SigEstadoN(string caracter, string estado, AutomataEntity automata)
        {
            foreach (var transicion in automata.Transiciones)//recorre toda la lista (se mantiene en AFN)
            {
                if (estado.Equals(transicion.EstadoOrigen))//se compara estado recibido con la primera posicion de cada fila(estado actual) (Se mantiene en AFN)
                {
                    if (caracter.Equals(transicion.Simbolo))//Si coincide se compara con la segunda posición el cual es "la letra"
                    {
                        return " ,"+transicion.EstadoDestino;//Si ambas coinciden se retorna el siguiente estado
                    }
                    else if ("ɛ"==transicion.Simbolo) //si no coincide la cadena con ninguna se verifica si existe ruta de Epsilon
                    {
                        return "ε,"+transicion.EstadoDestino;
                    }
                }
            }

            if (estado.ToUpper() == "E")
            {
                return ",E";
            }
            else
            {
                //return ("no se encontró el estado " + estado); //SOLUCIÓN FUERA DE MATRIZ EN FALLIDOS
                return ",fallido";

            }
        }


        //Funcion para imprimir el paso actual del recorrido
        static void ImprimirPasoN(string estActual, string caracter, string sigEstado, int contador, string cadena, AutomataEntity automata)
        {
            int longitudCad = cadena.Length;
            if (sigEstado.Equals("fallido"))
            {
                VerifEstFinalN(sigEstado, automata.EstadosFinales, automata);
            }
            else if (contador < longitudCad)
            {
                Console.WriteLine(estActual + " -> " + caracter + " -> " + sigEstado);
                contador++;//aumenta el contador para seguir recorriendo la cadena
                estActual = sigEstado;//siguiente estado pasa a ser el actual para volver a llamar RecorrerAF

                RecorrerAFN(estActual, cadena, contador, automata);
            }
        }


        //Funcion para verificar si el estado actual es final o no
        static void VerifEstFinalN(string estActual, string[] EstadosFinales, AutomataEntity automata)
        {
            //Limpiar el estado actual
            string estAcualLimpio = estActual.Trim().ToUpper();

            //Limpiar los estados finales
            string[] estadosFinalesLimpios = EstadosFinales.Select(ef => ef.Trim().ToUpper()).ToArray();
            
            bool esEstadoFinal = estadosFinalesLimpios.Contains(estAcualLimpio);

            if (esEstadoFinal)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("El recorrido dirige al estado '" + estActual + "' que si es un estado final, la palabra SI es aceptable.");
                Console.ResetColor();
                ContinuarValidando(automata);
            }
            else if (estActual == "E")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("El recorrido dirige a Epsilon. \nEl estado '" + estActual + "' no está permitido, la palabra NO es aceptable.");
                Console.ResetColor();
                ContinuarValidando(automata);
            }else if(estActual.Equals("fallido"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("El estado actual no cuenta con una ruta diferente a la anterior con el simbolo utilizado");
                Console.ResetColor();
                Console.WriteLine();
                recorridoFallido.Add(string.Join(",", recorrido));
                recorrido.Clear();
                otrasRutasAFN(automata);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("El recorrido dirige al estado '" + estActual + "' que no es un estado final, la palabra NO es aceptable.");
                Console.ResetColor();
                Console.WriteLine();
                //ContinuarValidando(automata);
                recorridoFallido.Add(string.Join(",", recorrido));
                recorrido.Clear();
                otrasRutasAFN(automata);

            }
        }

        //Funcion para convertir espacios(" ") en epsilon
        static string ConvertirEpsilon(string caracter)
        {
            if ( caracter == " " || caracter=="")
            {
                return "ɛ";
            }

            return caracter;
        }


        //AFN: Se usan del segundo intento en adelante
        //utiliza listas que guardan el último recorrido, se llama al método donde ya se utilizan esas nuevas condiciones
        static void otrasRutasAFN(AutomataEntity automata)
        {
            int cantRecorridos = recorridoFallido.Count-1; //obtiene cantidad de items en lista, es decir, candidad de intentos o rutas.
            string[] ultRecorrido = recorridoFallido[cantRecorridos].Split(','); //separa los estados del recorrido anterior en un arreglo
            int longRecorrido = ultRecorrido.Length; //obtiene la cantidad de estados del recorrido. Esto para no llegar al último y cambiar de decisión.
            if (longRecorrido==1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ninguna ruta acepta la cadena ingresada.");
                Console.ResetColor();
                ContinuarValidando(automata);
            }
            string ultEstados = ultRecorrido[longRecorrido - 1]; //aquí se guardaran los últimos estados donde ha fallado recorrido en los intentos anteriores. Separados por comas
            string[] pivoteRecorrido;

            while (cantRecorridos>=0)
            {
                cantRecorridos--;

                if (cantRecorridos >=0)
                {
                    pivoteRecorrido = recorridoFallido[cantRecorridos].Split(','); //se obtiene el el recorrido anterior 

                    if (longRecorrido == pivoteRecorrido.Length)//si la longitud del ultimo recorrido es igual a la del que se está comparando, significa que se quedan en el mismo estado
                    {//esto se hace para guardar las posibles decisiones de ese estado, en caso que se tengan más de dos caminos por estado
                        ultEstados = ultEstados + "," + pivoteRecorrido[longRecorrido - 1];
                    }
                }
  
            }

            RecorrerAFN(automata.EstadoInicial[0], cadena, 0, automata,ultEstados,longRecorrido);
        }

        ////Funcion recursiva para recorrer el autómata con una cadena 
        static void RecorrerAFN(string estActual, string cadena, int contador, AutomataEntity automata, string ultEstados, int longRecorrido)
        {
            string[] caracteres = cadena.ToCharArray().Select(c => c.ToString()).ToArray(); //separa la cadena ingresada en un arreglo string
            string sigEstado = "";
            if (contador == caracteres.Length)
            {
                recorrido.Add(estActual);
                VerifEstFinalN(estActual, automata.EstadosFinales, automata);
            }
            else if (longRecorrido == 1 && contador >= caracteres.Length)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ninguna ruta acepta la cadena ingresada.");
                Console.ResetColor();
                ContinuarValidando(automata);
            }
            else if (contador == longRecorrido-1)
            {
                //recorrido.Add(estActual);
                sigEstado = SigEstadoN(ConvertirEpsilon(caracteres[contador]), estActual, automata, ultEstados);

                string[] partes = sigEstado.Split(',');
                sigEstado = partes[1];

                if (partes[0] == "ε")
                {
                    ImprimirPasoN(estActual, "ε", sigEstado, contador - 1, cadena, automata,ultEstados,longRecorrido);
                }
                else
                {
                    ImprimirPasoN(estActual, caracteres[contador], sigEstado, contador, cadena, automata, ultEstados, longRecorrido);
                }
            }
            else if (recorrido.Count == longRecorrido-2 /*&& recorrido.Count!=0*/) //aquí se llama la función para cambiar el último estado fallido
            {
                
                if (contador < caracteres.Length)
                {
                    recorrido.Add(estActual);
                    sigEstado = SigEstadoN(ConvertirEpsilon(caracteres[contador]), estActual, automata, ultEstados);

                    string[] partes = sigEstado.Split(',');
                    sigEstado = partes[1];

                    if (partes[0] == "ε")
                    {
                        ImprimirPasoN(estActual, "ε", sigEstado, contador - 1, cadena, automata, ultEstados, longRecorrido);
                    }
                    else
                    {
                        ImprimirPasoN(estActual, caracteres[contador], sigEstado, contador, cadena, automata, ultEstados, longRecorrido);
                    }

                }
            }
            else //acá sigue ruta que ya se ha seguido antes porque se cambia hasta la última elección antes de que falle
            {
                
                if (contador < caracteres.Length)
                {
                    recorrido.Add(estActual);
                    sigEstado = SigEstadoN(ConvertirEpsilon(caracteres[contador]), estActual, automata); //obtiene el siguiente estado (ε,ESTADO) o (,ESTADO)

                    string[] partes = sigEstado.Split(','); //Lo separa para verificar si en la primera posición está epsilon
                    sigEstado = partes[1];

                    if (partes[0] == "ε")//si en el primero está epsilon, se imprime esa ruta y se regresa el contador para no perder un caracter
                    {
                        ImprimirPasoN(estActual, "ε", sigEstado, contador - 1, cadena, automata, ultEstados, longRecorrido);
                    }
                    else// si no, imprime ruta normal
                    {
                        ImprimirPasoN(estActual, caracteres[contador], sigEstado, contador, cadena, automata, ultEstados, longRecorrido);
                    }

                }
            }



        }
        
        //Funcion para imprimir el paso actual del recorrido
        static void ImprimirPasoN(string estActual, string caracter, string sigEstado, int contador, string cadena, AutomataEntity automata, string ultEstados, int longRecorrido)
        {
            int longitudCad = cadena.Length;
            if (contador < longitudCad)
            {
                Console.WriteLine(estActual + " -> " + caracter + " -> " + sigEstado);
                contador++;//aumenta el contador para seguir recorriendo la cadena

                if (sigEstado.Equals("fallido"))
                {
                    VerifEstFinalN(sigEstado, automata.EstadosFinales, automata);
                }
                else
                {
                    estActual = sigEstado;//siguiente estado pasa a ser el actual para volver a llamar RecorrerAF
                    RecorrerAFN(estActual, cadena, contador, automata, ultEstados, longRecorrido);
                } 
            }
        }

        //en proceso 2
        //Funcion para obtener el siguiente estado en el autómata. SE USA PARA NO REPETIR LA ÚLTIMA ELECCIÓN FALLIDA
        static string SigEstadoN(string caracter, string estado, AutomataEntity automata, string ultEstados)
        {
            string[] ultiEstados = ultEstados.Split(',');
            string estadoDest="";
            bool verif= false;

            foreach (var transicion in automata.Transiciones)//recorre toda la lista (se mantiene en AFN)
            {
                if (estado.Equals(transicion.EstadoOrigen))//se compara estado recibido con la primera posicion de cada fila(estado actual) (Se mantiene en AFN)
                {
                    if (caracter.Equals(transicion.Simbolo))//Si coincide se compara con la segunda posición el cual es "la letra"
                    {

                        //return " ," + transicion.EstadoDestino;//Si ambas coinciden se retorna el siguiente estado
                        //CAMBIAR  tengo que validar todas las rutas existentes y todas las rutas pasadas al mismo tiempo
                        foreach (var item in ultiEstados)
                        {
                            if (item.Equals(transicion.EstadoDestino))
                            {
                                verif = false;
                                break;
                            }
                            else
                            {
                                verif=true;
                                estadoDest= " ," + transicion.EstadoDestino;//Si ambas coinciden se retorna el siguiente estado
                            }

                        }

                    }
                    else if ("ɛ" == transicion.Simbolo) //si no coincide la cadena con ninguna se verifica si existe ruta de Epsilon
                    {
                        //return "ε," + transicion.EstadoDestino;
                        foreach (var item in ultiEstados)
                        {
                            if (item.Equals(transicion.EstadoDestino))
                            {
                                verif = false;
                                break;
                            }
                            else
                            {
                                verif = true;
                                estadoDest = "ε," + transicion.EstadoDestino;//Si ambas coinciden se retorna el siguiente estado
                            }

                        }
                    }
                }
            }
            if (verif)
            {
               return estadoDest;
            }
            else //sin es falso significa que ya no quedan rutas
            {                       
               return ",fallido";

            }

        }
    }
}
