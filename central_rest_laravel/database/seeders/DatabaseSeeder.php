<?php

namespace Database\Seeders;

use App\Models\User;
use App\Models\Multa;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        User::updateOrCreate(
            ['email' => 'policia@transito.bo'],
            [
                'name' => 'Oficial de Tránsito',
                'password' => Hash::make('123456'),
            ]
        );

        Multa::updateOrCreate(
            ['codigo' => 'MLT-1001'],
            [
                'ci' => '1234567',
                'nombre_conductor' => 'Juan Carlos Mamani',
                'fecha' => date('Y-m-d'),
                'tipo_infraccion' => 'Exceso de velocidad',
                'lugar' => 'Av. Circunvalación',
                'monto' => 150.00,
                'estado' => 'pendiente',
            ]
        );

        Multa::updateOrCreate(
            ['codigo' => 'MLT-1002'],
            [
                'ci' => '7654321',
                'nombre_conductor' => 'María Elena Flores',
                'fecha' => date('Y-m-d'),
                'tipo_infraccion' => 'Estacionar en zona prohibida',
                'lugar' => 'Plaza 25 de Mayo',
                'monto' => 100.00,
                'estado' => 'pagada',
            ]
        );
    }
}
