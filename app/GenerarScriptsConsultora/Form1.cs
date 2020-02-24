using GenerarScriptsConsultora.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GenerarScriptsConsultora
{
    public partial class Form1 : Form
    {
        protected UtilFile util;
        public Form1()
        {
            util = new UtilFile();
            InitializeComponent();
        }

        private List<string> GetPaises()
        {
            var i = tcBD.SelectedIndex;
            TabControl tab = new TabControl();
            List<string> paises = new List<string>();

            if (i == 0) tab = tcTiposBD57;

            foreach (Control control in tab.SelectedTab.Controls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.CheckBox")
                {
                    CheckBox chkPais = (CheckBox)control;
                    if (chkPais.Checked)
                    {
                        // para ods
                        var siglaPais = chkPais.Name.Replace("chb", "");
                        paises.Add(chkPais.Tag.ToString());
                        paises.Add("ODS_" + siglaPais);
                    }
                }
            }

            return paises;
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            var i = tcBD.SelectedIndex;
            TabControl tab = new TabControl();
            List<string> paises = new List<string>();

            if (i == 0) tab = tcTiposBD57;

            var scriptIni = new StringBuilder();
            //int cantVacio = 0;
            string lineaTxt = "";
            foreach (Control control in tab.SelectedTab.Controls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.CheckBox")
                {
                    CheckBox chkPais = (CheckBox)control;
                    if (chkPais.Checked)
                    {
                        paises.Add(chkPais.Tag.ToString());
                    }
                    continue;
                }

                if (control.GetType().ToString() == "System.Windows.Forms.TextBox")
                {
                    TextBox txtScript = (TextBox)control;
                    if (txtScript.Tag != null && txtScript.Tag.ToString() == "txtScript")
                    {
                        //script = txtScript.Text.Trim();

                        if (txtScript.Lines.Length > 0)
                            foreach (var linea in txtScript.Lines)
                            {
                                lineaTxt = linea.TrimEnd();

                                scriptIni.Append(lineaTxt);
                                scriptIni.AppendLine();
                            }
                    }
                }
            }

            var script = scriptIni.ToString();
            if (script == "") return;

            //Formatear(script);

            StringBuilder scriptGenerado = new StringBuilder();
            scriptGenerado.Append("GO");
            scriptGenerado.AppendLine();
            foreach (string pais in paises)
            {
                scriptGenerado.Append("USE " + pais);
                scriptGenerado.AppendLine();
                scriptGenerado.Append("GO");
                scriptGenerado.AppendLine();
                scriptGenerado.Append(script);
                scriptGenerado.AppendLine();
                scriptGenerado.Append("GO");
                scriptGenerado.AppendLine();
            }

            txtResultado.Text = scriptGenerado.ToString();

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (txtResultado.Text.Trim() == "")
            {
                return;
            }

            var i = tcBD.SelectedIndex;
            TabControl tab = new TabControl();
            if (i == 0) tab = tcTiposBD57;

            foreach (Control control in tab.SelectedTab.Controls)
            {
                if (control.GetType().ToString() == "System.Windows.Forms.TextBox")
                {
                    TextBox txtResult = (TextBox)control;
                    if (txtResult.Tag != null && txtResult.Tag.ToString() == "txtResultado")
                    {
                        Clipboard.SetDataObject(txtResult.Text);
                        if (txtDireccion.Text.Trim() == "")
                        {
                            MessageBox.Show("Se copió al portapapeles.", "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        break;
                    }
                }
            }

            if (txtDireccion.Text.Trim() != "")
            {
                ExportarScriptEnDireccion();
            }

        }

        private void ExportarScriptEnDireccion()
        {
            try
            {

                var direc = txtDireccion.Text.Trim();
                if (File.Exists(direc))
                {
                    MessageBox.Show("Ya existe un archivo con el mismo nombre \n " + direc, "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Create the file.
                    using (FileStream fs = File.Create(direc))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(txtResultado.Text);
                        fs.Write(info, 0, info.Length);
                    }

                    MessageBox.Show("Ya esta creado el archivo \n " + direc, "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {
            }
        }

        // SaveFile(Encoding.UTF8);
        // no se utiliza es una demo
        private static void SaveFile(Encoding encoding)
        {
            Console.WriteLine("Encoding: {0}", encoding.EncodingName);
            string filename = string.Concat(@"c:\file-", encoding.EncodingName, ".txt");
            StreamWriter streamWriter = new StreamWriter(filename, false, encoding);
            streamWriter.WriteLine("I am feeling great.");
            streamWriter.Close();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty; txtResultado.Text = string.Empty;
        }

        #region Pase
        private void PaseBtnGenerar_Click(object sender, EventArgs e)
        {
            List<string> paises = GetPaises();

            string siglasEquipo = paseSiglasEquipo.Text.Trim();
            string urlScript = paseUrlScript.Text.Trim();

            if (siglasEquipo == "" || paises.Count == 0 || urlScript == "")
            {
                MessageBox.Show("Debe tener \n Seleccionar Paises \n Siglas del Equipo \n Ruta Script del Pase", "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            RecorridoFileSql(urlScript, paises);
            //RecorridoFileCs(urlScript, paises);


        }

        private void RecorridoFileSql(string urlScript, List<string> paises)
        {
            foreach (string file in Directory.EnumerateFiles(urlScript, "*.sql"))
            {
                ReadLine(file, paises);
            }

        }

        private void ReadLine(string direc, List<string> paises)
        {
            if (!File.Exists(direc))
                return;

            string siglasEquipo = paseSiglasEquipo.Text.Trim();
            siglasEquipo = siglasEquipo[0] == '_' ? siglasEquipo : ("_" + siglasEquipo);

            string paisPase = "";
            bool addPais = false;

            StringBuilder scriptGeneradoPase = new StringBuilder();

            using (StreamReader sr = new StreamReader(direc))
            {
                while (sr.Peek() >= 0)
                {
                    var linea = sr.ReadLine();
                    linea = linea.TrimEnd();
                    var lineaAux = linea.Trim().ToLower();
                    var pais = "";
                    if (lineaAux.Contains("use "))
                    {
                        pais = lineaAux.Split(' ')[1];
                        paisPase = GetPaisPase(paises, pais);
                        if (paisPase != "")
                        {
                            addPais = true;
                        }
                    }

                    if (paisPase != "")
                    {
                        if (addPais)
                        {
                            scriptGeneradoPase.Append("GO");
                            scriptGeneradoPase.AppendLine();
                            scriptGeneradoPase.Append("USE " + paisPase + siglasEquipo);
                            scriptGeneradoPase.AppendLine();
                            scriptGeneradoPase.Append("GO");
                            scriptGeneradoPase.AppendLine();
                            addPais = false;
                            continue;
                        }

                        scriptGeneradoPase.Append(linea);
                        scriptGeneradoPase.AppendLine();
                    }
                }
            }

            if (scriptGeneradoPase.ToString() == "")
                return;

            direc = NuevaRuta(direc, siglasEquipo);
            ExportarScriptEnDireccion(direc, scriptGeneradoPase);
        }

        private string GetPaisPase(List<string> paises, string paisbuscar)
        {
            string paisPase = "";
            foreach (var pais in paises)
            {
                var paistrim = pais.Trim().ToLower();
                paisbuscar = paisbuscar.Trim().ToLower();
                if (paisbuscar == paistrim
                    || paisbuscar == paistrim + ";"
                    || paisbuscar == "[" + paistrim + "]"
                    || paisbuscar == "[" + paistrim + "];"
                )
                {
                    paisPase = paistrim;
                    break;
                }
            }
            return paisPase;
        }

        private string NuevaRuta(string direc, string siglas)
        {
            var list = direc.Split('\\');
            var nombre = list[list.Length - 1];
            var carpetaOri = direc.Substring(0, direc.Length - nombre.Length);
            var carpeta = paseUrlGenerado.Text.Trim();

            if (carpeta == "" || (carpeta == carpetaOri && carpetaOri != ""))
            {
                carpeta = carpetaOri + siglas;
            }

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }
            paseUrlGenerado.Text = carpeta;

            direc = carpeta + "\\" + nombre;
            return direc;
        }

        private void ExportarScriptEnDireccion(string direc, StringBuilder script)
        {
            try
            {
                if (File.Exists(direc))
                {
                    File.Delete(direc);
                }

                using (FileStream fs = File.Create(direc))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(script.ToString());
                    fs.Write(info, 0, info.Length);
                }

            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region Extra

        private void RecorridoFileCs(string urlScript, List<string> paises)
        {
            int cant = 0;
            foreach (string file in Directory.EnumerateFiles(urlScript, "*.cs", SearchOption.AllDirectories))
            {
                //if (!(file.Contains("BEPaisCampana.cs")
                //    ))
                //{
                //    continue;
                //}

                if (!File.Exists(file))
                    continue;

                if (file.Contains("BEUsuario.cs")
                    || file.Contains("BEConsultoraCUV.cs")
                    || file.Contains("BEConsultoraDD.cs")
                    || file.Contains("BEConsultoraTop")
                    || file.Contains("BEPedidoDD.cs")
                    || file.Contains("BESolicitudCredito.cs")
                )
                {
                    continue;
                }

                //LimpiarArchivo(file, 1);
                //LimpiarArchivo(file, 2);
                //LimpiarArchivo(file, 4);
                //LimpiarArchivo(file, 3);
                LimpiarArchivo(file, 5);
                //FormatoArchivo(file);
                cant++;
            }

            paseUrlGenerado.Text = "Termino conforme \n Total archivos " + cant;

            MessageBox.Show("Termino conforme \n Total archivos " + cant, "Generar Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LimpiarArchivo(string direc, int tipo)
        {
            var nombreArchivo = GetNombreArchivo(direc);
            var nombreMetodo = GetNombreMetodo(nombreArchivo);
            var nombreLector = "";
            var espacio = "";
            var dentroMetodo = false;
            var lineasFinal = new List<string>();
            var cambioMetodo = false;
            var lineaSigAux = "";

            int cont = 0;
            int contMax;
            int.TryParse(paseSiglasEquipo.Text, out contMax);
            string[] lineas = File.ReadAllLines(direc);
            for (int i = 0; i < lineas.Length; i++)
            {
                var linea = lineas[i].TrimEnd();
                var lineaAux = linea.Trim();

                if (tipo == 3) // If HasColumn + Convert.ToX
                {
                    if (cont < contMax)
                    {
                        cont++;

                        if (cont + 2 > lineas.Length)
                        {
                            lineasFinal.Add(linea);
                            continue;
                        }

                        if (lineaSigAux != "" && lineaSigAux != "-" && lineaSigAux != "--")
                        {
                            linea = lineaSigAux;
                            lineaSigAux = "";
                        }
                        else if (lineaSigAux == "" || lineaSigAux == "--")
                        {
                            linea = LimpiarHasColumnYConvertPorToX(linea, lineas[i + 1].TrimEnd(), nombreLector, out lineaSigAux);
                            if (linea == "")
                            {
                                linea = lineas[i];
                            }

                        }
                    }
                    cambioMetodo = true;
                    lineasFinal.Add(linea);
                    continue;
                }
                else if (tipo == 5) // valor  = Convert.ToX
                {
                    linea = LimpiarConvertTox(linea, "row");
                    if (linea != lineas[i].TrimEnd())
                    {
                        cambioMetodo = true;
                    }
                    lineasFinal.Add(linea);
                    continue;
                }

                if (lineaAux.StartsWith(nombreMetodo))
                {
                    nombreLector = getNombreLector(linea);
                    if (nombreLector == "")
                    {
                        break;
                    }

                    espacio = GetEspaciado(linea);
                    dentroMetodo = true;
                    lineasFinal.Add(linea);
                    continue;
                }

                if (lineaAux == "}")
                {
                    dentroMetodo = false;
                }

                if (dentroMetodo)
                {
                    if (tipo == 1)
                    {
                        linea = FormatoCampoToTipo(linea);
                    }
                    else if (tipo == 2)
                    {
                        linea = LimpiarDBNull(linea);
                        linea = LimpiarDBNullPorHasColumn(linea);
                    }
                    else if (tipo == 3)
                    {

                    }
                    else if (tipo == 4)
                    {
                        if (linea.Trim() == "")
                        {
                            cambioMetodo = true;
                            continue;
                        }

                        linea = linea.Replace(lineaAux, lineaAux.Replace("  ", " "));
                    }

                    if (linea.Trim() != lineaAux)
                    {
                        cambioMetodo = true;
                    }
                }

                lineasFinal.Add(linea);
            }

            if (cambioMetodo)
                File.WriteAllLines(direc, lineasFinal.ToArray(), Encoding.UTF8);
        }

        private string FormatoCampoToTipo(string linea)
        {
            if (!linea.EndsWith("].ToString();"))
            {
                return linea;
            }

            var lineaAux = "";
            var lista = linea.Split('=');
            foreach (var parte in lista)
            {
                if (parte.Trim().EndsWith("].ToString();"))
                {
                    lineaAux = parte.Trim();
                }
            }

            lista = lineaAux.Split('.');
            if (lista.Length != 2)
            {
                return linea;
            }

            var lineaFinal = "Convert.ToString(" + lista[0].Trim() + ");";
            lineaFinal = linea.Replace(lineaAux, lineaFinal);

            //linea = lista[0].TrimEnd() + " = Convert.ToString(" + lista[1].Trim() + ");";

            return lineaFinal;
        }

        private string LimpiarDBNull(string linea)
        {
            var nombreCampo = GetNombreCampo(linea);
            var lineaAux = linea.Trim();
            if (!(lineaAux.StartsWith("if (DataRecord.HasColumn(") && lineaAux.Contains(" && ") && lineaAux.EndsWith("] != DBNull.Value)")))
            {
                return linea;
            }

            lineaAux = "";
            var lista = linea.Split('&');
            foreach (var parte in lista)
            {
                if (parte.Trim().EndsWith("[" + '"' + nombreCampo + '"' + "] != DBNull.Value)"))
                {
                    lineaAux = " && " + parte.Trim();
                }
            }

            lista = lineaAux.Split('!');
            if (lista.Length != 2)
            {
                return linea;
            }

            var lineaFinal = linea.Replace(lineaAux, ")");

            return lineaFinal;
        }

        private string LimpiarDBNullPorHasColumn(string linea)
        {
            var lineaAux = linea.Trim();
            if (!lineaAux.Contains("DBNull.Value"))
            {
                return linea;
            }

            var nombreCampo = GetNombreCampo(linea);
            if (lineaAux != ("if (row[" + '"' + nombreCampo + '"' + "] != DBNull.Value)"))
            {
                return linea;
            }

            lineaAux = lineaAux.Split('(')[1].Split(')')[0];

            var lineaFinal = linea.Replace(lineaAux, "DataRecord.HasColumn(row, " + '"' + nombreCampo + '"' + ")");
            return lineaFinal;
        }

        private string LimpiarHasColumnYConvertPorToX(string linea, string lineaSig, string nombreLector, out string lineaSigFinal)
        {
            var espacio = GetEspaciado(linea);
            lineaSigFinal = "";
            var lineaAux = lineaSig.Trim();

            if (lineaAux == "" || lineaAux.StartsWith("else ") || lineaAux.StartsWith("{"))
            {
                return linea;
            }

            lineaAux = linea.Trim();

            if (lineaAux == "" || lineaAux.StartsWith("else ") || lineaAux.StartsWith("{"))
            {
                return linea;
            }

            if (!lineaAux.StartsWith("if"))
            {
                return linea;
            }

            if (nombreLector == "")
            {
                nombreLector = "row";
            }

            if ((lineaAux.StartsWith("if (DataRecord.HasColumn(" + nombreLector + ",")
                || lineaAux.StartsWith("if (" + nombreLector + ".HasColumn("))
                && lineaSig.Trim().Contains(" = Convert.To"))
            {
                var nombreCampo = GetNombreCampo(linea);
                if (lineaAux == ("if (DataRecord.HasColumn(" + nombreLector + ", " + '"' + nombreCampo + '"' + "))")
                    || lineaAux == ("if (" + nombreLector + ".HasColumn(" + '"' + nombreCampo + '"' + "))"))
                {
                    lineaAux = lineaSig.Trim();

                    var lista = lineaAux.Split('=');
                    if (lista.Length == 2)
                    {
                        var nombreT = lista[0].Trim();
                        var nombreConvert = GetNombreConvert(lineaAux);

                        if (lineaAux == nombreT + " = Convert." + nombreConvert + "(" + nombreLector + "[" + '"' + nombreCampo + '"' + "]);")
                        {
                            linea = espacio;
                            lineaSigFinal = espacio + "    " + lineaAux
                                .Replace("(" + nombreLector + "[", "(")
                                .Replace("]);", ");")
                                .Replace("= Convert.", "= row.")
                                .Replace("=Convert.", "= row.");
                        }
                    }
                }
            }

            if (lineaSigFinal == "" && (lineaAux.StartsWith("if (DataRecord.HasColumn(" + nombreLector + ",")
                || lineaAux.StartsWith("if (" + nombreLector + ".HasColumn("))
                && lineaAux.Contains(" = Convert.") && lineaAux.EndsWith("]);"))
            {
                var lista = lineaAux.Split('=');
                if (lista.Length == 2)
                {
                    var nombreCampo = GetNombreCampo(lineaAux);
                    var nombreT = lista[0].Split(')')[2].Trim();
                    lineaAux = nombreT + " = " + lista[1].Trim();
                    lista = lineaAux.Split('.');
                    var totipo = "-";
                    if (lista.Length == 2)
                    {
                        lista = lista[1].Split('(');
                        if (lista.Length == 2)
                        {
                            totipo = lista[0].Trim();
                        }
                    }

                    if (lineaAux == (nombreT + " = " + "Convert." + totipo + "(" + nombreLector + "[" + '"' + nombreCampo + '"' + "]);"))
                    {
                        lineaSigFinal = "--";
                        linea = espacio + lineaAux
                            .Replace("(" + nombreLector + "[", "(")
                            .Replace("]);", ");")
                            .Replace("Convert.", "row.");
                    }

                }
            }
            return linea;
        }

        private string LimpiarConvertTox(string linea, string nombreLector)
        {
            if (!linea.Contains("="))
            {
                return linea;
            }

            if (!linea.Contains("= Convert.To"))
            {
                return linea;
            }

            var lineaAux = linea.Trim();
            var nombreCampoAsignar = GetNombreCampoEntidad(lineaAux);
            var nombreConvertToX = GetNombreConvert(lineaAux);
            var nombreCampoDb = GetNombreCampoDBToConvert(lineaAux);
            if (lineaAux == nombreCampoAsignar + " = Convert." + nombreConvertToX + "(" + nombreLector + "[" + '"' + nombreCampoDb + '"' + "]);")
            {
                linea = linea
                    .Replace("(" + nombreLector + "[", "(")
                    .Replace("]);", ");")
                    .Replace("= Convert.", "= row.");
            }

            return linea;
        }

        private void FormatoArchivo(string direc)
        {
            var nombreArchivo = GetNombreArchivo(direc);
            var nombreMetodo = GetNombreMetodo(nombreArchivo);
            var dentroMetodo = false;
            var cantidadLlave = 0;

            var txtBuild = new StringBuilder();
            var espacio = "";

            var lineasFinal = new List<string>();

            string[] lineas = File.ReadAllLines(direc);
            for (int i = 0; i < lineas.Length; i++)
            {
                var linea = lineas[i].TrimEnd();
                var lineaAux = linea.Trim();

                if (lineaAux == nombreMetodo)
                {
                    espacio = GetEspaciado(linea);
                    dentroMetodo = true;
                    continue;
                }

                if (lineaAux == "}")
                {
                    dentroMetodo = false;
                }

                if (dentroMetodo)
                {
                    if (lineaAux == "{")
                    {
                        cantidadLlave++;
                        if (cantidadLlave == 1)
                        {
                            continue;
                        }
                    }

                    txtBuild.Append(linea);
                    continue;
                }
                else
                {
                    var txtMetodo = txtBuild.ToString();
                    if (txtMetodo == "")
                    {
                        lineasFinal.Add(linea);
                    }
                    if (txtMetodo != "")
                    {
                        txtMetodo = Formatear(txtMetodo);
                        txtBuild.Clear();

                        lineasFinal.Add(espacio + nombreMetodo);
                        lineasFinal.Add(espacio + "{");
                        foreach (var item in txtMetodo.Split(';'))
                        {
                            if (item.Trim() == "")
                            {
                                lineasFinal.Add("");
                                continue;
                            }
                            lineasFinal.Add(espacio + "    " + item + ";");
                        }

                        lineasFinal.Add(espacio + "}");

                    }
                }

                //}
            }

            File.WriteAllLines(direc, lineasFinal.ToArray(), Encoding.UTF8);

        }

        private string GetNombreArchivo(string direc)
        {
            var list = direc.Split('.');
            list = list[list.Length - 2].Split('\\');
            direc = list[list.Length - 1];
            return direc;
        }

        private string GetNombreMetodo(string nombre)
        {
            nombre = nombre.Replace(" ", "");
            return "public " + nombre + "(IDataRecord ";
        }

        private string getNombreLector(string linea)
        {
            var lista = linea.Split('(');
            if (lista.Length == 2)
            {
                lista = lista[1].Split(' ');

                if (lista.Length >= 2)
                {
                    lista = lista[1].Split(')');
                    lista = lista[0].Split(',');
                    return lista[0];
                }
            }
            return "";
        }

        private string GetNombreCampo(string linea)
        {
            var lineaAux = "";
            if (linea.Contains(".HasColumn("))
            {
                var lista = linea.Split('"');
                for (int i = 0; i < lista.Length; i++)
                {
                    if (lista[i].Contains(".HasColumn") && i < lista.Length)
                    {
                        lineaAux = lista[i + 1];
                    }
                }

            }
            else if (linea.Contains("if (row["))
            {
                var lista = linea.Split('"');
                if (lista.Length > 2)
                {
                    lineaAux = lista[1].Trim();
                }

            }

            return lineaAux;
        }

        private string GetEspaciado(string linea)
        {
            var espacio = "";
            foreach (char caracter in linea)
            {
                if (caracter == ' ')
                {
                    espacio += caracter.ToString();
                }
                else
                {
                    break;
                }
            }

            return espacio;
        }

        private string GetNombreCampoEntidad(string linea)
        {
            var nombre = "";
            var lista = linea.Split('=');
            if (lista.Length == 2)
            {
                nombre = lista[0].Trim();
            }
            return nombre;
        }

        private string GetNombreConvert(string linea)
        {
            var nombreConvert = "";
            var lista = linea.Split('=');
            if (lista.Length == 2)
            {
                linea = lista[1].Trim();
                lista = linea.Split('.');
                if (lista.Length == 2)
                {
                    lista = lista[1].Split('(');
                    if (lista.Length == 2)
                    {
                        nombreConvert = lista[0].Trim();
                    }
                }

            }

            return nombreConvert;

        }

        private string GetNombreCampoDBToConvert(string linea)
        {
            var nombre = "";
            var lista = linea.Split('=');
            if (lista.Length == 2)
            {
                lista = lista[1].Split('"');
                if (lista.Length == 3)
                {
                    nombre = lista[1].Trim();
                }
            }
            return nombre;
        }

        private string Formatear(string cadena)
        {
            var txtBuild = new StringBuilder();

            cadena = Limpiar(cadena);
            var lista = cadena.Split(';');

            foreach (var fila in lista)
            {
                if (fila.Contains("||"))
                {
                    continue;
                }

                var lista2 = fila.Replace("))", "||").Split('|');
                foreach (var campo in lista2)
                {
                    if (campo == "" || campo.Contains(".HasColumn("))
                    {
                        continue;
                    }
                    txtBuild.Append(Remplazar(campo + ";"));
                    //txtBuild.AppendLine();
                }
            }

            return txtBuild.ToString();
        }

        private string Limpiar(string cadena)
        {
            cadena = cadena
                .Replace(" this.", "")
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "");

            if (cadena.Contains(" "))
            {
                cadena = Limpiar(cadena);
            }
            return cadena;
        }
        private string Remplazar(string cadena)
        {
            //cadena = cadena
            cadena = cadena.Replace("(row[", "(");
            cadena = cadena.Replace("]);", ");");
            cadena = cadena.Replace("= Convert.", " = row.");
            cadena = cadena.Replace("=Convert.", " = row.");

            return cadena;
        }
        #endregion

        #region  Group File
        private void btnGroupFile_Click(object sender, EventArgs e)
        {
            string txtExtension = txtFileExtension.Text.Trim();
            string txtOrigin = txtDirectoryOrigin.Text.Trim();
            string txtFin = txtDirectoryEnd.Text.Trim();

            string mensaje = GroupFile_ValidarParametros(txtExtension, txtOrigin, txtFin);

            if (mensaje != "")
            {
                MessageBox.Show(mensaje, "Juntar archivos en una sola carpeta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            mensaje = util.GroupFileToDirectory(txtExtension, txtOrigin, txtFin);
            if (mensaje != "")
            {
                MessageBox.Show(mensaje, "Juntar archivos en una sola carpeta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Termino el proceso exitosamente", "Juntar archivos en una sola carpeta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        private string GroupFile_ValidarParametros(string txtExtension, string txtOrigin, string txtFin)
        {
            if (txtExtension == "")
            {
                return "Poner Extensión de archivos, en caso sea todos poner *.*";
            }

            if (!util.DirectoryExists(txtOrigin))
            {
                return "Directoriio de Origen No Existe";
            }

            util.CreateDirectory(txtFin);
            if (!util.DirectoryExists(txtFin))
            {
                return "Directoriio de Origen No Existe";
            }

            return "";
        }

        #endregion
    }
}
