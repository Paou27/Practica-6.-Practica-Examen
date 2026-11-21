import java.io.*;
import java.net.HttpURLConnection;
import java.net.Socket;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

// Compilar: javac ClienteEntidadRecaudadora.java
// Ejecutar: java ClienteEntidadRecaudadora

public class ClienteEntidadRecaudadora {
    private static final String GRAPHQL_URL = "http://localhost:4000/graphql";
    private static final String TCP_HOST = "localhost";
    private static final int TCP_PORT = 9090;

    public static void main(String[] args) {
        Scanner sc = new Scanner(System.in);
        System.out.println("=== ENTIDAD RECAUDADORA (Consulta GraphQL + Cobro Socket TCP) ===");
        System.out.print("Ingrese CI del conductor: ");
        String ci = sc.nextLine().trim();
        System.out.print("Ingrese código de boleta (ej. MLT-1001): ");
        String codigo = sc.nextLine().trim();

        String gqlJson = consultarGraphQl(codigo);
        System.out.println("
Respuesta GraphQL:
" + gqlJson);

        if (gqlJson.contains("\"multa\":null") || !gqlJson.contains("\"codigo\":")) {
            System.out.println("RESULTADO: La boleta no existe en el sistema.");
            return;
        }
        if (!gqlJson.contains("\"ci\":\"" + ci + "\"")) {
            System.out.println("RESULTADO: La boleta no corresponde al CI indicado.");
            return;
        }
        if (!gqlJson.contains("\"estado\":\"pendiente\"")) {
            System.out.println("RESULTADO: La boleta no está pendiente de pago (puede estar ya pagada o anulada).");
            return;
        }

        System.out.println("Boleta habilitada para pago. Enviando confirmación por Socket TCP...");
        String respuestaTcp = enviarPagoPorSocket(codigo, ci);
        System.out.println("Respuesta Central (TCP): " + respuestaTcp);
    }

    private static String consultarGraphQl(String codigo) {
        try {
            URL url = new URL(GRAPHQL_URL);
            HttpURLConnection con = (HttpURLConnection) url.openConnection();
            con.setRequestMethod("POST");
            con.setRequestProperty("Content-Type", "application/json; charset=UTF-8");
            con.setDoOutput(true);

            String query = "{\"query\": \"query { multa(codigo: \\\"" + codigo + "\\\") { codigo ci nombre_conductor fecha tipo_infraccion lugar monto estado } }\"}";
            try (OutputStream os = con.getOutputStream()) {
                os.write(query.getBytes(StandardCharsets.UTF_8));
            }

            InputStream is = con.getResponseCode() >= 400 ? con.getErrorStream() : con.getInputStream();
            try (BufferedReader br = new BufferedReader(new InputStreamReader(is, StandardCharsets.UTF_8))) {
                StringBuilder sb = new StringBuilder();
                String line;
                while ((line = br.readLine()) != null) sb.append(line);
                return sb.toString();
            }
        } catch (Exception e) {
            return "{\"error\": \"" + e.getMessage() + "\"}";
        }
    }

    private static String enviarPagoPorSocket(String codigo, String ci) {
        try (
            Socket socket = new Socket(TCP_HOST, TCP_PORT);
            PrintWriter out = new PrintWriter(socket.getOutputStream(), true);
            BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream()))
        ) {
            out.println("PAGAR|" + codigo + "|" + ci);
            String resp = in.readLine();
            return resp != null ? resp : "Sin respuesta del servidor TCP";
        } catch (IOException e) {
            return "Error de conexión Socket TCP: " + e.getMessage();
        }
    }
}
