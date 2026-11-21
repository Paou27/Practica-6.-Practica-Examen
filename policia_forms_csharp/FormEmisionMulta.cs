using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

// Si agregas Web Reference del SEGIP como 'SegipRef', puedes descomentar el uso directo de SegipRef.SegipServiceSoapClient.
// Aquí también se incluye llamada SOAP XML directa por HttpClient para que funcione sin depender del nombre del proxy.

namespace PoliciaTransitoForms
{
    public partial class FormEmisionMulta : Form
    {
        private TextBox txtCi;
        private TextBox txtNombre;
        private TextBox txtInfraccion;
        private TextBox txtLugar;
        private TextBox txtMonto;
        private TextBox txtFecha;
        private Button btnEmitir;
        private RichTextBox txtResultado;

        private const string SegipSoapUrl = "http://localhost:5001/SegipService.asmx";
        private const string RestBaseUrl = "http://localhost:8000/api";
        private static readonly HttpClient http = new HttpClient();
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();

        public FormEmisionMulta()
        {
            Text = "Policía de Tránsito - Emisión de Boletas (SIS 258)";
            Width = 540;
            Height = 460;
            StartPosition = FormStartPosition.CenterScreen;

            Label lCi = new Label { Left = 20, Top = 20, Width = 140, Text = "CI Conductor:" };
            txtCi = new TextBox { Left = 170, Top = 18, Width = 320, Text = "1234567" };

            Label lNom = new Label { Left = 20, Top = 55, Width = 140, Text = "Nombre Conductor:" };
            txtNombre = new TextBox { Left = 170, Top = 53, Width = 320, Text = "Juan Carlos Mamani" };

            Label lInf = new Label { Left = 20, Top = 90, Width = 140, Text = "Tipo Infracción:" };
            txtInfraccion = new TextBox { Left = 170, Top = 88, Width = 320, Text = "Cruce con semáforo en rojo" };

            Label lLug = new Label { Left = 20, Top = 125, Width = 140, Text = "Lugar:" };
            txtLugar = new TextBox { Left = 170, Top = 123, Width = 320, Text = "Av. América y Heroínas" };

            Label lMon = new Label { Left = 20, Top = 160, Width = 140, Text = "Monto (Bs.):" };
            txtMonto = new TextBox { Left = 170, Top = 158, Width = 320, Text = "200.00" };

            Label lFec = new Label { Left = 20, Top = 195, Width = 140, Text = "Fecha:" };
            txtFecha = new TextBox { Left = 170, Top = 193, Width = 320, Text = DateTime.Now.ToString("yyyy-MM-dd") };

            btnEmitir = new Button { Left = 170, Top = 232, Width = 320, Height = 36, Text = "Validar SEGIP y Emitir Boleta" };
            btnEmitir.Click += BtnEmitir_Click;

            txtResultado = new RichTextBox { Left = 20, Top = 280, Width = 470, Height = 120, ReadOnly = true };

            Controls.AddRange(new Control[] { lCi, txtCi, lNom, txtNombre, lInf, txtInfraccion, lLug, txtLugar, lMon, txtMonto, lFec, txtFecha, btnEmitir, txtResultado });
        }

        private bool VerificarCiEnSegipSoap(string ci)
        {
            string soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <verificarCI xmlns=""http://segip.gob.bo/"">
      <ci>{System.Security.SecurityElement.Escape(ci)}</ci>
    </verificarCI>
  </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", ""http://segip.gob.bo/verificarCI"");

            var resp = http.PostAsync(SegipSoapUrl, content).Result;
            resp.EnsureSuccessStatusCode();
            string xml = resp.Content.ReadAsStringAsync().Result;
            return xml.IndexOf("<verificarCIResult>true</verificarCIResult>", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string ObtenerTokenRest()
        {
            var loginObj = new { email = "policia@transito.bo", password = "123456" };
            var content = new StringContent(js.Serialize(loginObj), Encoding.UTF8, "application/json");
            var resp = http.PostAsync($"{RestBaseUrl}/login", content).Result;
            resp.EnsureSuccessStatusCode();
            var dict = js.Deserialize<System.Collections.Generic.Dictionary<string, object>>(resp.Content.ReadAsStringAsync().Result);
            return dict["token"].ToString();
        }

        private void BtnEmitir_Click(object sender, EventArgs e)
        {
            try
            {
                string ci = txtCi.Text.Trim();
                bool valido = VerificarCiEnSegipSoap(ci);
                if (!valido)
                {
                    MessageBox.Show("El CI no fue validado por el SEGIP.", "Validación SEGIP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtResultado.Text = $"RECHAZADO: El CI {ci} no es válido en el SEGIP.";
                    return;
                }

                string token = ObtenerTokenRest();
                var payload = new
                {
                    ci = ci,
                    nombre_conductor = txtNombre.Text.Trim(),
                    fecha = txtFecha.Text.Trim(),
                    tipo_infraccion = txtInfraccion.Text.Trim(),
                    lugar = txtLugar.Text.Trim(),
                    monto = double.Parse(txtMonto.Text.Trim(), System.Globalization.CultureInfo.InvariantCulture)
                };

                var req = new HttpRequestMessage(HttpMethod.Post, $"{RestBaseUrl}/multa");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent(js.Serialize(payload), Encoding.UTF8, "application/json");

                var postResp = http.SendAsync(req).Result;
                string body = postResp.Content.ReadAsStringAsync().Result;

                if (postResp.IsSuccessStatusCode)
                {
                    txtResultado.Text = "BOLETA REGISTRADA EN CENTRAL (REST):
" + body;
                    MessageBox.Show("Boleta emitida correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtResultado.Text = $"ERROR REST ({(int)postResp.StatusCode}):
" + body;
                }
            }
            catch (Exception ex)
            {
                txtResultado.Text = "ERROR: " + ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new FormEmisionMulta());
        }
    }
}
