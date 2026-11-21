<?php

namespace Database\Seeders;

use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class SalidasSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Salidas::create(['destino' => 'Potosi', 'fecha' => '27/06/2026','hora' => '14:02','bus'=> 'MariaBus','capacidad'=> 2]);
       
    }
}
