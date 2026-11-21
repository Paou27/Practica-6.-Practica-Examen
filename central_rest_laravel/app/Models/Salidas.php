<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Salidas extends Model
{
    /** @use HasFactory<\Database\Factories\SalidasFactory> */
    use HasFactory;
    protected $fillable = ['destino','fecha','hora','bus','capacidad'];

}
