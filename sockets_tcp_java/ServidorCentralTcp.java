import java.io.*;
import java.net.ServerSocket;
import java.net.Socket;
import java.sql.*;

// Requiere driver JDBC SQLite en classpath (ej: sqlite-jdbc-3.46.1.3.jar)
// Compilar: javac -cp ".;sqlite-jdbc.jar" ServidorCentralTcp.java
// Ejecutar: java -cp ".;sqlite-jdbc.jar" ServidorCentralTcp

public class ServidorCentralTcp {
    private static final int PUERTO = 9090;
    private static final String DB_URL = "jdbc:sqlite:../central_rest_laravel/database/database.sqlite";

    public static void main(String[] args) {
        System.out.println("Servidor Socket TCP (Central de Registro) escuchando en puerto " + PUERTO + "...");
        try (ServerSocket serverSocket = new ServerSocket(PUERTO)) {
            while (true) {
                Socket cliente = serverSocket.accept();
                new Thread(() -> atenderCliente(cliente)).start();
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private static void atenderCliente(Socket socket) {
        try (
            BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
            PrintWriter out = new PrintWriter(socket.getOutputStream(), true)
        ) {
            String linea = in.readLine();
            if (linea == null || linea.trim().isEmpty()) {
                out.println("ERROR: Código de boleta vacío");
                return;
            }

            // Formato esperado: PAGAR|<codigo>|<ci>
            String[] partes = linea.trim().split("\|");
            String codigo = partes[partes.length > 1 ? 1 : 0].trim();
            String ciEsperado = partes.length > 2 ? partes[2].trim() : null;

            String respuesta = procesarPagoBoleta(codigo, ciEsperado);
            out.println(respuesta);
            System.out.println("Solicitud [" + linea + "] -> " + respuesta);
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            try { socket.close(); } catch (IOException ignored) {}
        }
    }

    private static String procesarPagoBoleta(String codigo, String ciEsperado) {
        try {
            Class.forName("org.sqlite.JDBC");
        } catch (ClassNotFoundException e) {
            return "ERROR: Falta driver JDBC SQLite en classpath";
        }

        String selectSql = "SELECT ci, estado, monto FROM multas WHERE codigo = ?";
        String updateSql = "UPDATE multas SET estado = 'pagada', updated_at = datetime('now') WHERE codigo = ?";

        try (Connection conn = DriverManager.getConnection(DB_URL);
             PreparedStatement psSelect = conn.prepareStatement(selectSql)) {

            psSelect.setString(1, codigo);
            try (ResultSet rs = psSelect.executeQuery()) {
                if (!rs.next()) {
                    return "ERROR: La boleta " + codigo + " no existe";
                }

                String ciBd = rs.getString("ci");
                String estado = rs.getString("estado");
                double monto = rs.getDouble("monto");

                if (ciEsperado != null && !ciEsperado.isEmpty() && !ciEsperado.equals(ciBd)) {
                    return "ERROR: La boleta " + codigo + " no pertenece al CI " + ciEsperado;
                }

                if ("pagada".equalsIgnoreCase(estado)) {
                    return "ERROR: La boleta " + codigo + " ya fue pagada previamente";
                }

                if ("anulada".equalsIgnoreCase(estado)) {
                    return "ERROR: La boleta " + codigo + " está anulada";
                }

                try (PreparedStatement psUpdate = conn.prepareStatement(updateSql)) {
                    psUpdate.setString(1, codigo);
                    psUpdate.executeUpdate();
                }

                return "OK: Pago registrado en tiempo real para boleta " + codigo + " (Monto: Bs. " + monto + ", Estado: pagada)";
            }
        } catch (SQLException e) {
            return "ERROR_BD: " + e.getMessage();
        }
    }
}
