using System.Text;
using System.Xml;

string rutaCsv = "NetflixOriginals.csv";
string rutaXml = "NetflixOriginals.xml";
int opcion;
do{
    MostrarMenu();
    opcion = LeerEntero("Elige una opción: ");
    Console.WriteLine();
    switch (opcion){
        case 1: rutaCsv = PedirRuta("CSV", rutaCsv); break;
        case 2: rutaXml = PedirRuta("XML", rutaXml); break;
        case 3: Convertir(true); break;
        case 4: Convertir(false); break;
        case 5: Console.WriteLine("Cerrando la aplicación. ¡Hasta pronto!"); break;
        default: Console.WriteLine("Opción no válida."); break;
    }
    if (opcion != 5){
        Console.WriteLine("\nPulsa una tecla para continuar...");
        Console.ReadKey();
    }
} 
while (opcion != 5);
void MostrarMenu(){
    Console.Clear();
    Console.WriteLine("===== FLUJO DE FICHEROS =====");
    Console.WriteLine($"Ruta CSV: {Path.GetFullPath(rutaCsv)}");
    Console.WriteLine($"Ruta XML: {Path.GetFullPath(rutaXml)}");
    Console.WriteLine("---------------------------------");
    Console.WriteLine("1. Elegir la ruta del fichero CSV");
    Console.WriteLine("2. Elegir la ruta del fichero XML");
    Console.WriteLine("3. Convertir de CSV a XML");
    Console.WriteLine("4. Convertir de XML a CSV");
    Console.WriteLine("5. Cerrar la aplicación");
    Console.WriteLine("---------------------------------");
}
string PedirRuta(string tipo, string actual){
    Console.WriteLine($"Ruta actual del {tipo}: {Path.GetFullPath(actual)}");
    Console.Write($"Nueva ruta del {tipo} (Enter para mantener la actual): ");
    string nueva = Console.ReadLine().Trim();
    if (string.IsNullOrEmpty(nueva)){
        return actual;
    }
    Console.WriteLine($"Ruta del {tipo} actualizada: {Path.GetFullPath(nueva)}");
    return nueva;
}
void Convertir(bool csvAXml){
    string origen;
    string destino;
    if (csvAXml){
        origen = rutaCsv;
        destino = rutaXml;
    }
    else{
        origen = rutaXml;
        destino = rutaCsv;
    }
    if (!File.Exists(origen)){
        Console.WriteLine($"No existe el fichero de origen: {Path.GetFullPath(origen)}");
        return;
    }
    List<Pelicula> peliculas;
    try{
        if (csvAXml){
            peliculas = LeerCsv(origen);
        }
        else{
            peliculas = LeerXml(origen);
        }
    }
    catch (Exception ex){
        Console.WriteLine("Error inesperado: " + ex.Message);
        return;
    }
    if (peliculas.Count == 0){
        Console.WriteLine("El fichero de origen no contiene ninguna película.");
        return;
    }
    Console.WriteLine($"Origen:  {Path.GetFullPath(origen)} ({peliculas.Count} películas)");
    Console.WriteLine($"Destino: {Path.GetFullPath(destino)}");
    Console.WriteLine("1. Exportar todo el documento");
    Console.WriteLine("2. Exportar por género");
    Console.WriteLine("3. Exportar por año (ese año y superiores)");
    Console.WriteLine("4. Exportar por lenguaje");
    Console.WriteLine("5. Volver");
    int modo = LeerEntero("Elige una opción: ");
    List<Pelicula> seleccion = Filtrar(peliculas, modo);
    if (seleccion == null){
        return;
    }
    if (seleccion.Count == 0){
        Console.WriteLine("No hay ninguna película que cumpla ese filtro. No se exporta nada.");
        return;
    }
    try{
        string rutaCarpeta = Path.GetDirectoryName(Path.GetFullPath(destino));
        bool existiaAntes = Directory.Exists(rutaCarpeta);
        DirectoryInfo carpeta = Directory.CreateDirectory(rutaCarpeta);
        if (existiaAntes){
            Console.WriteLine($"La carpeta ya existía: {carpeta.Name}");
        }
        else{
            Console.WriteLine($"Carpeta creada: {carpeta.Name}");
        }
        if (File.Exists(destino)){
            Console.WriteLine("El fichero de destino ya existía y se sobrescribirá.");
        }
        else{
            using (FileStream archivo = File.Create(destino)) { }
            Console.WriteLine("Archivo creado exitosamente.");
        }
        if (csvAXml){
            EscribirXml(destino, seleccion);
        }
        else{
            EscribirCsv(destino, seleccion);
        }
        Console.WriteLine($"Se han exportado {seleccion.Count} películas.");
        Console.WriteLine($"Ruta completa: {Path.GetFullPath(destino)}");
    }
    catch (Exception ex){
        Console.WriteLine("Error inesperado: " + ex.Message);
    }
}
List<Pelicula> Filtrar(List<Pelicula> peliculas, int modo){
    switch (modo){
        case 1:
            return peliculas;
        case 2:
            List<string> generos = new List<string>();
            foreach (Pelicula p in peliculas){
                if (!string.IsNullOrWhiteSpace(p.Genre) && !generos.Contains(p.Genre)){
                    generos.Add(p.Genre);
                }
            }
            generos.Sort();
            string genero = ElegirDeLista("género", generos);
            List<Pelicula> porGenero = new List<Pelicula>();
            foreach (Pelicula p in peliculas){
                if (string.Equals(p.Genre, genero, StringComparison.OrdinalIgnoreCase)){
                    porGenero.Add(p);
                }
            }
            return porGenero;
        case 3:
            int anio = LeerEntero("Año (se exportan las películas de ese año y posteriores): ");
            List<Pelicula> porAnio = new List<Pelicula>();
            foreach (Pelicula p in peliculas){
                if (ObtenerAnio(p.Premiere) >= anio){
                    porAnio.Add(p);
                }
            }
            return porAnio;
        case 4:
            List<string> idiomas = new List<string>();
            foreach (Pelicula p in peliculas){
                if (!string.IsNullOrWhiteSpace(p.Language) && !idiomas.Contains(p.Language)){
                    idiomas.Add(p.Language);
                }
            }
            idiomas.Sort();
            string idioma = ElegirDeLista("lenguaje", idiomas);
            List<Pelicula> porIdioma = new List<Pelicula>();
            foreach (Pelicula p in peliculas){
                if (string.Equals(p.Language, idioma, StringComparison.OrdinalIgnoreCase)){
                    porIdioma.Add(p);
                }
            }
            return porIdioma;

        case 5:
            return null;

        default:
            Console.WriteLine("Opción no válida.");
            return null;
    }
}
string ElegirDeLista(string nombreCampo, IEnumerable<string> valores){   
   List<string> distintos = new List<string>();
    foreach (string v in valores){
        if (string.IsNullOrWhiteSpace(v)){
            continue;
        }
        bool yaEsta = false;
        foreach (string d in distintos){
            if (string.Equals(d, v, StringComparison.OrdinalIgnoreCase)){
                yaEsta = true;
                break;
            }
        }
        if (!yaEsta){
            distintos.Add(v);
        }
    }
    distintos.Sort();
    Console.WriteLine($"Valores disponibles de {nombreCampo}:");
    for (int i = 0; i < distintos.Count; i++){
        Console.WriteLine($"{i + 1}. {distintos[i]}");
    }
    int numero = LeerEntero($"Elige un {nombreCampo} (1-{distintos.Count}): ");
    while (numero < 1 || numero > distintos.Count){
        numero = LeerEntero($"Número fuera de rango. Elige entre 1 y {distintos.Count}: ");
    }
    return distintos[numero - 1];
}
int ObtenerAnio(string premiere){
    string[] partes = premiere.Split(' ');
    string ultimo = partes[partes.Length - 1];
    int anio;
    if (int.TryParse(ultimo, out anio)){
        return anio;
    }
    return 0;
}
List<Pelicula> LeerCsv(string ruta){
    List<Pelicula> lista = new List<Pelicula>();
    string[] lineas = File.ReadAllLines(ruta, Encoding.UTF8);
    Console.WriteLine($"ReadAllLines: {lineas.Length} líneas. La primera (cabecera) es: {lineas[0]}");
    for (int i = 1; i < lineas.Length; i++){
        List<string> c = ParsearLineaCsv(lineas[i]);
        if (c.Count == 6){
            lista.Add(new Pelicula(c[0], c[1], c[2], c[3], c[4], c[5]));
        }
    }
    return lista;
}
List<string> ParsearLineaCsv(string linea){
    List<string> campos = new List<string>();
    StringBuilder actual = new StringBuilder();
    bool entreComillas = false;
    for (int i = 0; i < linea.Length; i++){
        char c = linea[i];
        if (entreComillas){
            if (c == '"'){
                if (i + 1 < linea.Length && linea[i + 1] == '"'){
                    actual.Append('"');
                    i++;
                }
                else{
                    entreComillas = false;
                }
            }
            else{
                actual.Append(c);
            }
        }
        else{
            if (c == '"'){
                entreComillas = true;
            }
            else if (c == ','){
                campos.Add(actual.ToString().Trim());
                actual.Clear();
            }
            else{
                actual.Append(c);
            }
        }
    }
    campos.Add(actual.ToString().Trim());
    return campos;
}
void EscribirCsv(string ruta, List<Pelicula> peliculas){
    List<string> lineas = new List<string>();
    lineas.Add("Title,Genre,Premiere,Runtime,IMDB Score,Language");
    foreach (Pelicula p in peliculas){
        lineas.Add(string.Join(",",
            EscaparCsv(p.Title), EscaparCsv(p.Genre), EscaparCsv(p.Premiere),
            EscaparCsv(p.Runtime), EscaparCsv(p.ImdbScore), EscaparCsv(p.Language)));
    }
    File.WriteAllLines(ruta, lineas, Encoding.UTF8);
}
string EscaparCsv(string campo){
    if (campo.Contains(',') || campo.Contains('"') || campo.Contains('\n') || campo.Contains('\r')){
        return "\"" + campo.Replace("\"", "\"\"") + "\"";
    }
    return campo;
}
List<Pelicula> LeerXml(string ruta){
    List<Pelicula> lista = new List<Pelicula>();
    XmlDocument xmlDoc = new XmlDocument();
    xmlDoc.Load(ruta);
    XmlNodeList nodosPelicula = xmlDoc.SelectNodes("/Peliculas/Pelicula");
    foreach (XmlNode nodoPelicula in nodosPelicula){
        string title = "", genre = "", premiere = "", runtime = "", imdbScore = "", language = "";
        foreach (XmlNode campo in nodoPelicula.ChildNodes){
            switch (campo.Name){
                case "Title": title = campo.InnerText.Trim(); break;
                case "Genre": genre = campo.InnerText.Trim(); break;
                case "Premiere": premiere = campo.InnerText.Trim(); break;
                case "Runtime": runtime = campo.InnerText.Trim(); break;
                case "IMDBScore": imdbScore = campo.InnerText.Trim(); break;
                case "Language": language = campo.InnerText.Trim(); break;
            }
        }
        lista.Add(new Pelicula(title, genre, premiere, runtime, imdbScore, language));
    }
    return lista;
}
void EscribirXml(string ruta, List<Pelicula> peliculas){
    XmlWriterSettings ajustes = new XmlWriterSettings();
    ajustes.Indent = true;
    ajustes.Encoding = Encoding.UTF8;
    using (XmlWriter escritor = XmlWriter.Create(ruta, ajustes)){
        escritor.WriteStartDocument();           
        escritor.WriteStartElement("Peliculas");
        foreach (Pelicula p in peliculas){
            escritor.WriteStartElement("Pelicula");
            escritor.WriteElementString("Title", p.Title);
            escritor.WriteElementString("Genre", p.Genre);
            escritor.WriteElementString("Premiere", p.Premiere);
            escritor.WriteElementString("Runtime", p.Runtime);
            escritor.WriteElementString("IMDBScore", p.ImdbScore);
            escritor.WriteElementString("Language", p.Language);
            escritor.WriteEndElement();
        }
        escritor.WriteEndElement();
        escritor.WriteEndDocument();
        escritor.Close();
    }
}
int LeerEntero(string mensaje){
    int valor;
    Console.Write(mensaje);
    while (!int.TryParse(Console.ReadLine(), out valor)){
        Console.Write("Valor no válido. Introduce un número entero: ");
    }
    return valor;
}
record Pelicula(string Title, string Genre, string Premiere, string Runtime, string ImdbScore, string Language);