<?php

namespace App\Http\Controllers;

use App\Models\Salidas;
use Illuminate\Http\Request;

class SalidasController extends Controller
{
    /**
     * Display a listing of the resource.
     */
    public function index(Request $request)
    {
        $consulta = Salidas::query();
        if($request->has('id')){
            $consulta->where('id',$request->ci);
        }
        return response()->json($consulta->get(),200);
    }

    /**
     * Store a newly created resource in storage.
     */
    public function store(Request $request)
    {
       $request->validate([
           'id' => 'required',
           'destino' => 'required',
           'fecha' => 'required',
           'hora' => 'required',
           'bus' => 'required',
           'capacidad'=> 'required|numeric'
       ]);

       $salidas = Salidas::create($request->all());
       return response()->json($salidas, 201);
    }

    /**
     * Display the specified resource.
     */
    public function show(Salidas $salidas)
    {
        return response()->json($salidas,200);
    }

    /**
     * Update the specified resource in storage.
     */
    public function update(Request $request, Salidas $salidas)
    {
        $salidas->update($request->all());
        return response()->json($salidas, 200);
    }

    /**
     * Remove the specified resource from storage.
     */
    public function destroy(Salidas $salidas)
    {
        //
    }
}
