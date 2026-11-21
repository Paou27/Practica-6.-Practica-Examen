<?php
namespace App\Http\Controllers;
use Illuminate\Http\Request;
use App\Models\User;
use Illuminate\Support\Facades\Hash;
use Firebase\JWT\JWT;
class LoginController extends Controller
{
public function login(Request $request)
{
    $usuario = User::where('email', $request->email)->first();

    if ($usuario == null || !Hash::check($request->password, $usuario->password)) {
      return response()->json(['mensaje' => 'Credenciales invalidas'], 401);
}
    $key = env('JWT_SECRET');

        $algoritmo = env('JWT_ALGORITHM');
        $time = time();
        $payload = array(
            'iat' => $time,
            'exp' => $time + (60 * 60),
            'data' => [
                'user_id' => $usuario->id,
                'email' => $usuario->email,
                'name' => $usuario->name,
                'role' => $usuario->role
            ]
        );
        $jwt = JWT::encode($payload, $key, $algoritmo);
        return response()->json([
            'mensaje' => 'Se logro autenticar al usuario',
            'token' => $jwt,
            'type' => 'bearer',
            'usuario' => $usuario
        ], 200);
    }
}