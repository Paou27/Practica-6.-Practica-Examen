<?php

namespace App\Http\Controllers;

use App\Models\Multa;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;

class MultaController extends Controller
{
    public function index(Request $request)
    {
        $query = Multa::query();

        if ($request->filled('ci')) {
            $query->where('ci', $request->input('ci'));
        }
        if ($request->filled('codigo')) {
            $query->where('codigo', $request->input('codigo'));
        }
        if ($request->filled('estado')) {
            $query->where('estado', $request->input('estado'));
        }

        return response()->json($query->orderByDesc('id')->get(), 200);
    }

    public function show($codigo)
    {
        $multa = Multa::where('codigo', $codigo)->first();
        if (!$multa) {
            return response()->json(['mensaje' => 'Boleta de infracción no encontrada'], 404);
        }

        return response()->json($multa, 200);
    }

    public function store(Request $request)
    {
        $validator = Validator::make($request->all(), [
            'codigo' => 'nullable|string|max:50|unique:multas,codigo',
            'ci' => 'required|string|max:20',
            'nombre_conductor' => 'nullable|string|max:120',
            'fecha' => 'nullable|string|max:30',
            'tipo_infraccion' => 'required|string|max:150',
            'lugar' => 'required|string|max:150',
            'monto' => 'required|numeric|min:0.5',
            'estado' => 'nullable|string|in:pendiente,pagada,anulada',
        ]);

        if ($validator->fails()) {
            return response()->json([
                'mensaje' => 'Datos de la boleta inválidos',
                'errores' => $validator->errors(),
            ], 422);
        }

        $data = $validator->validated();
        if (empty($data['codigo'])) {
            $data['codigo'] = 'MLT-' . date('YmdHis') . '-' . rand(100, 999);
        }
        if (empty($data['fecha'])) {
            $data['fecha'] = date('Y-m-d H:i:s');
        }
        if (empty($data['estado'])) {
            $data['estado'] = 'pendiente';
        }

        $multa = Multa::create($data);
        return response()->json($multa, 201);
    }
}
