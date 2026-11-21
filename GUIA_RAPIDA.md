# Guía Rápida - Sistema de Gestión de Multas de Tránsito (SIS 258)

## 1. Central de Registro REST (Laravel -> Puerto 8000)
```bash
cd central_rest_laravel
php artisan migrate:fresh --seed
php artisan serve --port=8000
```
- Credenciales de prueba Policía: `policia@transito.bo` / `123456`
- Boletas iniciales: `MLT-1001` (CI `1234567`, estado `pendiente`) y `MLT-1002` (CI `7654321`, estado `pagada`).

## 2. Central de Registro GraphQL (Node.js -> Puerto 4000)
```bash
cd central_graphql_node
npm install
npm start
```
- Usa la misma base SQLite `central_rest_laravel/database/database.sqlite`.

## 3. Servidor Socket TCP (Java -> Puerto 9090)
```bash
cd sockets_tcp_java
javac -cp ".;sqlite-jdbc.jar" ServidorCentralTcp.java
java -cp ".;sqlite-jdbc.jar" ServidorCentralTcp
```

## 4. SEGIP SOAP (C# ASMX -> Puerto 5001)
- Publica `segip_soap_csharp/SegipService.asmx` en IIS/IIS Express en `http://localhost:5001/SegipService.asmx` (`verificarCI(string ci)`).

## 5. Módulo Policía de Tránsito (Windows Forms C#)
- Compila y ejecuta `policia_forms_csharp/FormEmisionMulta.cs` (referenciando `System.Web.Extensions` y `System.Net.Http`).

## 6. Cliente Entidad Recaudadora (Java)
```bash
cd sockets_tcp_java
javac ClienteEntidadRecaudadora.java
java ClienteEntidadRecaudadora
```

## 7. Cliente Web Ciudadano (HTML + JS)
- Abre `cliente_web_ciudadano/index.html` en tu navegador.
